using System.Collections.Generic;

using RimWorld;
using Verse;
using System.Linq;

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

    private int quarryHealth;
    private bool firstSpawn = false;
    private CompAffectedByFacilities facilityComp;
    private List<string> rockTypesUnder = new List<string>();

    public float QuarryPercent {
      get {
        if (QuarrySettings.QuarryMaxHealth == int.MaxValue) {
          return 100f;
        }
        return (quarryHealth * 100f) / QuarrySettings.QuarryMaxHealth;
      }
    }

    public bool Depleted {
      get { return QuarryPercent <= 0; }
    }

    public List<string> RockTypesUnder {
      get {
        if (rockTypesUnder.Count == 0) {
          return null;
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
      Scribe_Values.Look(ref quarryHealth, "QRY_quarryHealth", 2000);
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
        quarryHealth = QuarrySettings.QuarryMaxHealth;

        CellRect rect = this.OccupiedRect();
        // First pass to populate rockTypesUnder
        SetupFirstPass(rect);
        // Second pass to change the terrain to quarried stone
        SetupSecondPass(rect);
        // Third pass to spawn filth and also set terrain back to quarried stone where the ladders are
        SetupThirdPass(rect);
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


    private void SetupFirstPass(CellRect rect) {
      foreach (IntVec3 c in rect) {
        // What type of rock are we over?
        string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
        // If there isn't a known chunk for this, it probably isn't a rock type and wouldn't work for spawning anyways
        // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
        ThingDef chunkTest = QuarrySettings.database.Find(t => t.defName == "Chunk" + rockType);
        // Add a second scan for blocks matching this stone name to prevent errors
        ThingDef blocksTest = QuarrySettings.database.Find(t => t.defName == "Blocks" + rockType);
        if (chunkTest != null && blocksTest != null) {
          rockTypesUnder.Add(rockType);
        }
        if (rockTypesUnder.Count <= 0) {
          // If the quarry was just built over gravel (no stone types), add a random stone (or two) from the map
          string weightedStone = Find.World.NaturalRockTypesIn(Map.Tile).RandomElement().building.mineableThing.ToString().Replace("Chunk", "");
          for (int i = 0; i < 2; i++) {
            // Add this stone twice so there won't be a perfect 50/50 split
            rockTypesUnder.Add(weightedStone);
          }
          // Try to add another stone type. This may return the same stone type, but it may also get a different one. Either way works
          rockTypesUnder.Add(Find.World.NaturalRockTypesIn(Map.Tile).RandomElement().building.mineableThing.ToString().Replace("Chunk", ""));
        }
        // Change the terrain here to be quarried stone wall
        Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGroundWall);
      }
    }


    private void SetupSecondPass(CellRect rect) {
      foreach (IntVec3 c in rect.ContractedBy(2)) {
        Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGround);
      }
    }


    private void SetupThirdPass(CellRect rect) {
      foreach (IntVec3 c in rect) {
        if (c == Position + Static.LadderOffset1.RotatedBy(Rotation) || c == Position + Static.LadderOffset2.RotatedBy(Rotation) || c == Position + Static.LadderOffset3.RotatedBy(Rotation) || c == Position + Static.LadderOffset4.RotatedBy(Rotation)) {
          Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGround);
        }

        List<Thing> thingsInCell = new List<Thing>();
        bool usableCell = true;
        // Skip this cell if it is occupied by a placed object
        // This is to avoid save compression errors
        thingsInCell = Map.thingGrid.ThingsListAtFast(c);
        for (int t = 0; t < thingsInCell.Count; t++) {
          if (thingsInCell[t].def.saveCompressible) {
            usableCell = false;
            break;
          }
        }

        if (usableCell) {
          int filthAmount = Rand.RangeInclusive(1, 100);
          // Check for dirt filth
          if (filthAmount > 20 && filthAmount <= 40) {
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FilthDirt), c, Map);
          }
          // Check for rock rubble
          else if (filthAmount > 40) {
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RockRubble), c, Map);
            // Check for chunks
            if (filthAmount > 80) {
              string chunkType = RockTypesUnder.RandomElement();
              ThingDef chunk = QuarrySettings.database.Find(t => t.defName == "Chunk" + chunkType);
              GenSpawn.Spawn(ThingMaker.MakeThing(chunk), c, Map);
            }
          }
        }
      }
    }


    public ThingDef GiveResources(ResourceRequest req, out MoteType mote, out bool singleSpawn) {
      mote = MoteType.None;
      singleSpawn = true;

      // Decrease the amount this quarry can be mined, eventually depleting it
      if (QuarrySettings.QuarryMaxHealth != int.MaxValue) {
        quarryHealth--; 
      }

      // Cache values since this process is convoluted and the values need to remain the same
      bool cachedJunkChance = Rand.Chance(QuarrySettings.junkChance / 100f);

      // Check for blocks first to prevent spawning chunks (these would just be cut into blocks)
      if (req == ResourceRequest.Blocks) {
        if (!cachedJunkChance) {
          singleSpawn = false;
          string blockType = RockTypesUnder.RandomElement();
          return QuarrySettings.database.Find(t => t.defName == "Blocks" + blockType);
        }
        // The rock didn't break into a usable size, spawn rubble
        mote = MoteType.Failure;
        return ThingDefOf.RockRubble;
      }

      // Try to give junk before resources. This simulates only mining chunks or useless rubble
      if (cachedJunkChance) {
        if (Rand.Chance(QuarrySettings.chunkChance / 100f)) {
          return QuarrySettings.database.Find(t => t.defName == "Chunk" + RockTypesUnder.RandomElement());
        }
        else {
          mote = MoteType.Failure;
          return ThingDefOf.RockRubble;
        }
      }

      System.Random rand = new System.Random();

      // Try to give resources
      if (req == ResourceRequest.Resources) {
        singleSpawn = false;
        return OreDictionary.From(QuarryMod.oreDictionary).TakeOne();
      }
      // The quarry was most likely toggled off while a pawn was still working. Give junk
      else {
        return ThingDefOf.RockRubble;
      }
    }


    public bool TryFindBestStoreCellFor(Thing t, Pawn carrier, Map map, Faction faction, out IntVec3 foundCell) {
      List<Thing> facilities = facilityComp.LinkedFacilitiesListForReading;
      for (int f = 0; f < facilities.Count; f++) {
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
