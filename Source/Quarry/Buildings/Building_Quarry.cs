using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace Quarry {

  public enum ResourceRequest {
    None,
    Resources,
    Blocks
  }

  public enum MoteType {
    None,
    LargeVein,
    Failure
  }



  [StaticConstructorOnStartup]
  public class Building_Quarry : Building {

    public bool autoHaul = true;
    public bool mineModeToggle = true;

    private float quarryPercent = 1f;
    private int jobsCompleted = 0;
    private bool firstSpawn = false;
    private CompAffectedByFacilities facilityComp;
    private List<string> rockTypesUnder = new List<string>();
    private List<ThingDef> blocksUnder = new List<ThingDef>();
    private List<ThingDef> chunksUnder = new List<ThingDef>();

		public virtual int WallThickness => 2;
    public bool Depleted => QuarryPercent <= 0;

    public float QuarryPercent {
      get {
        if (QuarrySettings.QuarryMaxHealth == int.MaxValue) {
          return 100f;
        }
        return quarryPercent * 100f;
      }
    }

		public bool HasConnectedPlatform {
			get { return !facilityComp.LinkedFacilitiesListForReading.NullOrEmpty(); }
		}

		public List<ThingDef> ChunksUnder {
			get {
				if (chunksUnder.Count <= 0) {
					MakeThingDefListsFrom(RockTypesUnder);
				}
				return chunksUnder;
			}
		}

		public List<ThingDef> BlocksUnder {
			get {
				if (blocksUnder.Count <= 0) {
					MakeThingDefListsFrom(RockTypesUnder);
				}
				return blocksUnder;
			}
		}

		protected virtual int QuarryDamageMultiplier => 1;
		protected virtual int SinkholeFrequency => 100;

		protected virtual List<IntVec3> LadderOffsets {
			get {
				return new List<IntVec3>() {
					Static.LadderOffset_Big1,
					Static.LadderOffset_Big2,
					Static.LadderOffset_Big3,
					Static.LadderOffset_Big4
				};
			}
		}

		private List<string> RockTypesUnder {
      get {
        if (rockTypesUnder.Count <= 0) {
					rockTypesUnder = RockTypesFromMap();
				}
        return rockTypesUnder;
      }
    }

    private string HaulDescription {
      get { return (autoHaul ? Static.LabelHaul : Static.LabelNotHaul); }
    }


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref autoHaul, "QRY_boolAutoHaul", true);
      Scribe_Values.Look(ref mineModeToggle, "QRY_mineMode", true);
      Scribe_Values.Look(ref quarryPercent, "QRY_quarryPercent", 1f);
      Scribe_Values.Look(ref jobsCompleted, "QRY_jobsCompleted", 0);
      Scribe_Collections.Look(ref rockTypesUnder, "QRY_rockTypesUnder", LookMode.Value);
    }


    public override IEnumerable<Gizmo> GetGizmos() {

      Command_Action mineMode = new Command_Action() {
        icon = (mineModeToggle ? Static.DesignationQuarryResources : Static.DesignationQuarryBlocks),
        defaultLabel = (mineModeToggle ? Static.LabelMineResources : Static.LabelMineBlocks),
        defaultDesc = (mineModeToggle ? Static.DescriptionMineResources : Static.DescriptionMineBlocks),
        hotKey = KeyBindingDefOf.Misc10,
        activateSound = SoundDefOf.Click,
        action = () => { mineModeToggle = !mineModeToggle; },
      };
      // Only allow this option if stonecutting has been researched
      // The default behavior is to allow resources, but not blocks
      if (QuarryDefOf.Stonecutting.IsFinished) {
        yield return mineMode;
      }

      yield return new Command_Toggle() {
        icon = Static.DesignationHaul,
        defaultLabel = Static.LabelHaulMode,
        defaultDesc = HaulDescription,
        hotKey = KeyBindingDefOf.Misc11,
        activateSound = SoundDefOf.Click,
        isActive = () => autoHaul,
        toggleAction = () => { autoHaul = !autoHaul; },
      };

      if (base.GetGizmos() != null) {
        foreach (Command c in base.GetGizmos()) {
          yield return c;
        }
      }
    }


    public override void PostMake() {
      base.PostMake();
      firstSpawn = true;
    }


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      facilityComp = GetComp<CompAffectedByFacilities>();

      if (firstSpawn) {
				// Set the initial quarry health
				quarryPercent = 1f;

        CellRect rect = this.OccupiedRect();
				// Remove this area from the quarry grid. Quarries can never be built here again
				map.GetComponent<QuarryGrid>().RemoveFromGrid(rect);

				foreach (IntVec3 c in rect) {
					// What type of terrain are we over?
					string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
					// If this is a valid rock type, add it to the list
					if (QuarryUtility.IsValidQuarryRock(rockType)) {
						rockTypesUnder.Add(rockType);
					}
					// Change the terrain here to be quarried stone					
					if (rect.ContractedBy(WallThickness).Contains(c)) {
						Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGround);
					}
					else {
						Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGroundWall);
					}
				}
				// Now that all the cells have been processed, create ThingDef lists
				MakeThingDefListsFrom(RockTypesUnder);
				// Spawn filth for the quarry
				foreach (IntVec3 c in rect) {
					SpawnFilth(c);
				}
				// Change the ground back to normal quarried stone where the ladders are
				// This is to negate the speed decrease and encourages pawns to use the ladders
				foreach (IntVec3 offset in LadderOffsets) {
					Map.terrainGrid.SetTerrain(Position + offset.RotatedBy(Rotation), QuarryDefOf.QRY_QuarriedGround);
				}
			}
		}


    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
      foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
        // Change the terrain here back to quarried stone, removing the walls
        Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGround);
      }
      if (!QuarrySettings.letterSent && !TutorSystem.AdaptiveTrainingEnabled) {
        Find.LetterStack.ReceiveLetter(Static.LetterLabel, Static.LetterText, QuarryDefOf.CuproLetter, new RimWorld.Planet.GlobalTargetInfo(Position, Map));
        QuarrySettings.letterSent = true;
      }
      if (TutorSystem.AdaptiveTrainingEnabled) {
        LessonAutoActivator.TeachOpportunity(QuarryDefOf.QRY_ReclaimingSoil, OpportunityType.GoodToKnow);
      }
      base.Destroy(mode);
    }


		private List<string> RockTypesFromMap() {
			// Try to add all the rock types found in the map
			List<string> list = new List<string>();
			List<string> tempRockTypesUnder = Find.World.NaturalRockTypesIn(Map.Tile).Select(r => r.LabelCap).ToList();
			foreach (string str in tempRockTypesUnder) {
				if (QuarryUtility.IsValidQuarryRock(str)) {
					list.Add(str);
				}
			}
			// This will cause an error if there still isn't a list, so make a new one using known rocks
			if (list.Count <= 0) {
				Log.Warning("Quarry:: No valid rock types were found in the map. Building list using vanilla rocks.");
				list = new List<string>() { "Sandstone", "Limestone", "Granite", "Marble", "Slate" };
			}
			return list;
		}


		private void MakeThingDefListsFrom(List<string> stringList) {
			chunksUnder = new List<ThingDef>();
			blocksUnder = new List<ThingDef>();
			foreach (string str in stringList) {
				if (QuarryUtility.IsValidQuarryChunk(str, out ThingDef chunk)) {
					chunksUnder.Add(chunk);
				}
				if (QuarryUtility.IsValidQuarryBlocks(str, out ThingDef blocks)) {
					blocksUnder.Add(blocks);
				}
			}
		}


		private void SpawnFilth(IntVec3 c) {
			List<Thing> thingsInCell = new List<Thing>();
			// Skip this cell if it is occupied by a placed object
			// This is to avoid save compression errors
			thingsInCell = Map.thingGrid.ThingsListAtFast(c);
			for (int t = 0; t < thingsInCell.Count; t++) {
				if (thingsInCell[t].def.saveCompressible) {
					return;
				}
			}

			int filthAmount = Rand.RangeInclusive(1, 100);
			// If this cell isn't filthy enough, skip it
			if (filthAmount <= 20) {
				return;
			}
			// Check for dirt filth
			if (filthAmount <= 40) {
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FilthDirt), c, Map);
			}
			else {
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RockRubble), c, Map);
				// Check for chunks
				if (filthAmount > 80) {
					GenSpawn.Spawn(ThingMaker.MakeThing(ChunksUnder.RandomElement()), c, Map);
				}
			}
		}


    public ThingDef GiveResources(ResourceRequest req, out MoteType mote, out bool singleSpawn, out bool eventTriggered) {
      // Increment the jobs completed
      jobsCompleted++;

      eventTriggered = false;
      mote = MoteType.None;
      singleSpawn = true;

      // Decrease the amount this quarry can be mined, eventually depleting it
      if (QuarrySettings.QuarryMaxHealth != int.MaxValue) {
        QuarryMined(); 
      }

      // Determine if the mining job resulted in a sinkhole event, based on game difficulty
      if (jobsCompleted % SinkholeFrequency == 0 && Rand.Chance(Find.Storyteller.difficulty.difficulty / 50f)) {
        eventTriggered = true;
				// The sinkhole damages the quarry a little
				QuarryMined(Rand.RangeInclusive(1, 3));
			}

      // Cache values since this process is convoluted and the values need to remain the same
      bool junkMined = Rand.Chance(QuarrySettings.junkChance / 100f);

      // Check for blocks first to prevent spawning chunks (these would just be cut into blocks)
      if (req == ResourceRequest.Blocks) {
				if (!junkMined) {
					singleSpawn = false;
					return BlocksUnder.RandomElement();
        }
        // The rock didn't break into a usable size, spawn rubble
        mote = MoteType.Failure;
        return ThingDefOf.RockRubble;
      }

      // Try to give junk before resources. This simulates only mining chunks or useless rubble
      if (junkMined) {
        if (Rand.Chance(QuarrySettings.chunkChance / 100f)) {
          return ChunksUnder.RandomElement();
        }
        else {
          mote = MoteType.Failure;
          return ThingDefOf.RockRubble;
        }
      }

      // Try to give resources
      if (req == ResourceRequest.Resources) {
        singleSpawn = false;
        return OreDictionary.TakeOne();
      }
      // The quarry was most likely toggled off while a pawn was still working. Give junk
      else {
        return ThingDefOf.RockRubble;
      }
    }


		private void QuarryMined(int damage = 1) {
			quarryPercent = ((QuarrySettings.quarryMaxHealth * quarryPercent) - (damage * QuarryDamageMultiplier)) / QuarrySettings.quarryMaxHealth;
		}


    public bool TryFindBestPlatformCell(Thing t, Pawn carrier, Map map, Faction faction, out IntVec3 foundCell) {
      List<Thing> facilities = facilityComp.LinkedFacilitiesListForReading;
      for (int f = 0; f < facilities.Count; f++) {
				if (facilities[f].GetSlotGroup() == null || !facilities[f].GetSlotGroup().Settings.AllowedToAccept(t)) {
					continue;
				}
        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(facilities[f])) {
          if (StoreUtility.IsGoodStoreCell(c, map, t, carrier, faction)) {
            foundCell = c;
            return true;
          }
        }
      }
      foundCell = IntVec3.Invalid;
      return false;
    }


    public override string GetInspectString() {
      return Static.InspectQuarryPercent + ": " + QuarryPercent.ToStringDecimalIfSmall() + "%";
    }
  }
}
