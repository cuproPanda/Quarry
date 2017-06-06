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



  [StaticConstructorOnStartup]
  public class Building_Quarry : Building {

    public bool autoHaul = true;
    public bool mineModeToggle = true;

    public float QuarryPercent {
      get { return quarryPercent; }
    }

    private static float percentDamagedWhenQuarried = 0.2f;

    private float quarryPercent = 100f;
    private bool firstSpawn = false;

    // Create a list of mineable rock types
    // This allows for a weighted list, so if the quarry is only built on 1
    // tile of sandstone, mining sandstone will be very rare
    private List<string> rockTypesUnder = new List<string>();
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
      Scribe_Values.Look(ref quarryPercent, "QRY_percentQuarried", 100f);
      Scribe_Collections.Look(ref rockTypesUnder, "QRY_rockTypesUnder", LookMode.Value);
    }


    public override IEnumerable<Gizmo> GetGizmos() {

      Command_Toggle mineMode = new Command_Toggle() {
        icon = (mineModeToggle ? Static.DesignationQuarryResources : Static.DesignationQuarryBlocks),
        defaultLabel = (mineModeToggle ? Static.LabelMineResources : Static.LabelMineBlocks),
        defaultDesc = (mineModeToggle ? Static.DescriptionMineResources : Static.DescriptionMineBlocks),
        hotKey = KeyBindingDefOf.Misc10,
        activateSound = SoundDefOf.Click,
        isActive = () => mineModeToggle,
        toggleAction = () => { mineModeToggle = !mineModeToggle; },
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

      if (firstSpawn) {

        // First pass to populate rockTypesUnder
        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
          // What type of rock are we over?
          string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
          // If there isn't a known chunk for this, it probably isn't a rock type and wouldn't work for spawning anyways
          // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
          ThingDef chunkTest = QuarryMod.Database.Find(t => t.defName == "Chunk" + rockType);
          if (chunkTest != null) {
            rockTypesUnder.Add(rockType);
          }
        }

        // Second pass to spawn filth
        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
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
    }


    public Thing GiveResources(ResourceRequest req) {
      // Cache values since this process is convoluted and the values need to remain the same
      bool cachedJunkChance = Rand.Chance(QuarryDefOf.Resources.JunkChance);

      // Check for blocka first to prevent spawning chunks (these would just be cut into blocks)
      if (req == ResourceRequest.Blocks) {
        if (!cachedJunkChance) {
          string blockType = RockTypesUnder.RandomElement();
          return new QuarryResource(QuarryMod.Database.Find(t => t.defName == "Blocks" + blockType), Rand.RangeInclusive(5, 10)).ToThing();
        }
        // The rock didn't break into a usable size, spawn rubble
        return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
      }

      // Try to give junk before resources. This simulates only mining chunks or useless rubble
      if (cachedJunkChance) {
        if (Rand.Chance(QuarryDefOf.Resources.ChunkChance)) {
          return new QuarryResource(QuarryMod.Database.Find(t => t.defName == "Chunk" + RockTypesUnder.RandomElement()), 1).ToThing();
        }
        else {
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
              return resource.ToThing();
            }
          }
          sum += resource.probability;
        }
        return QuarryMod.Resources.First().ToThing();
      }
      // The quarry was most likely toggled off while a pawn was still working. Give junk
      else {
        return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
      }
    }
  }
}
