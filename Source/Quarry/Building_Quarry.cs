using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;

namespace Quarry {

  public enum ResourceRequest {
    None,
    Resources,
    Blocks,
    Random
  }



  [StaticConstructorOnStartup]
  public class Building_Quarry : Building {

    public bool quarryResources = true;
    public bool quarryBlocks;
    public bool autoHaul = true;

    public float QuarryPercent {
      get { return quarryPercent; }
    }

    private static Texture2D quarryTex = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/Quarry", false);
    private static Texture2D blockTex = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/QuarryBlocks", false);
    private static Texture2D haulTex = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/Haul", false);
    private static float percentDamagedWhenQuarried = 0.2f;

    private float quarryPercent = 100f;
    private List<ThingDef> database;
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
      get { return (autoHaul ? "QRY_Haul" : "QRY_NotHaul").Translate(); }
    }


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref quarryResources, "QRY_boolResources", true);
      Scribe_Values.Look(ref quarryBlocks, "QRY_boolBlocks", false);
      Scribe_Values.Look(ref autoHaul, "QRY_boolAutoHaul", true);
      Scribe_Values.Look(ref quarryPercent, "QRY_percentQuarried", 100f);
      Scribe_Collections.Look(ref rockTypesUnder, "QRY_rockTypesUnder", LookMode.Value);
    }


    public override IEnumerable<Gizmo> GetGizmos() {

      yield return new Command_Toggle() {
        icon = quarryTex,
        defaultLabel = "QRY_DesignatorMine".Translate(),
        defaultDesc = "QRY_DesignatorMineDesc".Translate(),
        hotKey = KeyBindingDefOf.Misc10,
        activateSound = SoundDefOf.DesignateMine,
        isActive = () => quarryResources,
        toggleAction = () => { quarryResources = !quarryResources; },
      };

      Command_Toggle blocks = new Command_Toggle() {
        icon = blockTex,
        defaultLabel = "QRY_DesignatorMineBlocks".Translate(),
        defaultDesc = "QRY_DesignatorMineBlocksDesc".Translate(),
        hotKey = KeyBindingDefOf.Misc11,
        activateSound = SoundDefOf.Click,
        isActive = () => quarryBlocks,
        toggleAction = () => { quarryBlocks = !quarryBlocks; },
      };
      // Only allow this option if stonecutting has been researched
      if (QuarryDefOf.Stonecutting.IsFinished) {
        yield return blocks;
      }

      yield return new Command_Toggle() {
        icon = haulTex,
        defaultLabel = "QRY_Mode".Translate(),
        defaultDesc = HaulDescription,
        hotKey = KeyBindingDefOf.Misc12,
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

      database = DefDatabase<ThingDef>.AllDefsListForReading;

      if (firstSpawn) {

        // First pass to populate rockTypesUnder
        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
          // What type of rock are we over?
          string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
          // If there isn't a known chunk for this, it probably isn't a rock type and wouldn't work for spawning anyways
          // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
          ThingDef chunkTest = database.Find(t => t.defName == "Chunk" + rockType);
          if (chunkTest != null) {
            rockTypesUnder.Add(rockType);
          }
        }

        // Second pass to spawn filth
        // This might cause minor fps hiccup, but it'll only be when the quarry is first built
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
                ThingDef chunk = database.Find(t => t.defName == "Chunk" + chunkType);
                GenSpawn.Spawn(ThingMaker.MakeThing(chunk), c, Map);
              }
            }
          }
        }
      }
    }


    public Thing GiveResources(ResourceRequest req) {
      // Try to give junk first
      if (Rand.Chance(QuarryDefOf.Resources.JunkChance)) {
        if (Rand.Chance(QuarryDefOf.Resources.ChunkChance)) {
          return new QuarryResource(database.Find(t => t.defName == "Chunk" + RockTypesUnder.RandomElement()), 1).ToThing();
        }
        else {
          return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
        }
      }

      System.Random rand = new System.Random();

      if (req == ResourceRequest.Resources || (req == ResourceRequest.Random && Rand.Chance(0.6f))) {
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
      else if (req == ResourceRequest.Blocks) {
        string blockType = RockTypesUnder.RandomElement();
        return new QuarryResource(database.Find(t => t.defName == "Blocks" + blockType), Rand.RangeInclusive(5, 10)).ToThing();
      }
      // The quarry was most likely toggled off while a pawn was still working. Give junk
      else {
        return new QuarryResource(ThingDefOf.RockRubble, 1).ToThing();
      }
    }
  }
}
