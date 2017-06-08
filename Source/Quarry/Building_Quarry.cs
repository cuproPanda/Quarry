using System.Collections.Generic;

using UnityEngine;
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
        if (QuarryMod.QuarryMaxHealth == int.MaxValue) {
          return 100f;
        }
        return (quarryHealth * 100f) / QuarryMod.QuarryMaxHealth;
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
        quarryHealth = QuarryMod.QuarryMaxHealth;

        CellRect rect = this.OccupiedRect();
        // First pass to populate rockTypesUnder
        SetupFirstPass(rect);
        // Second pass to change the terrain to quarried stone
        SetupSecondPass(rect);
        // Third pass to spawn filth and also set terrain back to quarried stone where the ladders are
        SetupThirdPass(rect);
      }

      if (QuarryMod.SettingsChanged) {
        quarryHealth = QuarryMod.QuarryMaxHealth;
      }
    }


    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
      foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
        // Change the terrain here back to quarried stone, removing the walls
        Map.terrainGrid.SetTerrain(c, QuarryDefOf.QRY_QuarriedGround);
      }
      if (!QuarryMod.LetterSent) {
        Find.LetterStack.ReceiveLetter(Static.LetterLabel, Static.LetterText, QuarryDefOf.CuproLetter, new RimWorld.Planet.GlobalTargetInfo(Position, Map));
        QuarryMod.Instance.Notify_LetterSent();
      }
      base.Destroy(mode);
    }


    private void SetupFirstPass(CellRect rect) {
      foreach (IntVec3 c in rect) {
        // What type of rock are we over?
        string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
        // If there isn't a known chunk for this, it probably isn't a rock type and wouldn't work for spawning anyways
        // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
        ThingDef chunkTest = QuarryMod.Database.Find(t => t.defName == "Chunk" + rockType);
        if (chunkTest != null) {
          rockTypesUnder.Add(rockType);
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
              ThingDef chunk = QuarryMod.Database.Find(t => t.defName == "Chunk" + chunkType);
              GenSpawn.Spawn(ThingMaker.MakeThing(chunk), c, Map);
            }
          }
        }
      }
    }


    public Thing GiveResources(ResourceRequest req, out MoteType mote) {
      mote = MoteType.None;

      // Decrease the amount this quarry can be mined, eventually depleting it
      if (QuarryMod.QuarryMaxHealth != int.MaxValue) {
        quarryHealth--; 
      }

      // Cache values since this process is convoluted and the values need to remain the same
      bool cachedJunkChance = Rand.Chance(QuarryMod.JunkChance);

      // Check for blocks first to prevent spawning chunks (these would just be cut into blocks)
      if (req == ResourceRequest.Blocks) {
        if (!cachedJunkChance) {
          string blockType = RockTypesUnder.RandomElement();
          return new QuarryResource(QuarryMod.Database.Find(t => t.defName == "Blocks" + blockType), Rand.RangeInclusive(5, 10)).ToThing();
        }
        // The rock didn't break into a usable size, spawn rubble
        mote = MoteType.Failure;
        return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
      }

      // Try to give junk before resources. This simulates only mining chunks or useless rubble
      if (cachedJunkChance) {
        if (Rand.Chance(QuarryMod.ChunkChance)) {
          return new QuarryResource(QuarryMod.Database.Find(t => t.defName == "Chunk" + RockTypesUnder.RandomElement()), 1).ToThing();
        }
        else {
          mote = MoteType.Failure;
          return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
        }
      }

      System.Random rand = new System.Random();

      // Try to give resources
      if (req == ResourceRequest.Resources) {
        int maxProb = QuarryMod.Resources.Sum(c => c.probability);
        int choice = rand.Next(maxProb);
        int sum = 0;

        foreach (QuarryResource resource in QuarryMod.Resources) {
          for (int i = sum; i < resource.probability + sum; i++) {
            if (i >= choice) {
              if (resource.largeVein) {
                mote = MoteType.LargeVein;
              }
              return resource.ToThing();
            }
          }
          sum += resource.probability;
        }
        QuarryResource qr = QuarryMod.Resources.First();
        if (qr.largeVein) {
          mote = MoteType.LargeVein;
        }
        return qr.ToThing();
      }
      // The quarry was most likely toggled off while a pawn was still working. Give junk
      else {
        return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
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
