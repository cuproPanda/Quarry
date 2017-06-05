using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;

namespace Quarry {
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

    // Create a list of mineable rocks
    // This allows for a weighted list, so if the quarry is only built on 1
    // tile of sandstone, mining sandstone will be very rare
    private List<ThingDef> rockTypes = new List<ThingDef>();
    public List<ThingDef> RockTypes {
      get {
        if (rockTypes.Count == 0) {
          return null;
        }
        return rockTypes;
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

      yield return new Command_Toggle() {
        icon = blockTex,
        defaultLabel = "QRY_DesignatorMineBlocks".Translate(),
        defaultDesc = "QRY_DesignatorMineBlocksDesc".Translate(),
        hotKey = KeyBindingDefOf.Misc11,
        activateSound = SoundDefOf.Click,
        isActive = () => quarryBlocks,
        toggleAction = () => { quarryBlocks = !quarryBlocks; },
      };

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

      if (firstSpawn) {
        database = DefDatabase<ThingDef>.AllDefsListForReading;

        foreach (IntVec3 c in GenAdj.CellsOccupiedBy(this)) {
          List<Thing> thingsInCell = new List<Thing>();
          bool usableCell = true;

          // What type of rock are we over?
          string rockType = c.GetTerrain(Map).label.Split(' ').Last().CapitalizeFirst();
          // If there isn't a known chunk for this, it probably isn't a rock type
          // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
          ThingDef chunk = database.Find(t => t.defName == "Chunk" + rockType);
          if (chunk != null) {
            rockTypes.Add(chunk);
          }

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
            int filthChance = Rand.RangeInclusive(1, 100);
            // Check for rock rubble
            if (filthChance < 60) {
              GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RockRubble), c, Map);
            }
            // Check for chunks
            if (filthChance < 20 && chunk != null) {
              GenSpawn.Spawn(ThingMaker.MakeThing(chunk), c, Map);
            }
            // Check for dirt filth
            if (filthChance > 60) {
              GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FilthDirt), c, Map);
            }
          }
        }
      }
    }


    public QuarryResource GenProduct() {
      if (Rand.Chance(QuarryDefOf.Resources.JunkChance)) {
        if (Rand.Chance(QuarryDefOf.Resources.ChunkChance)) {
          return new QuarryResource() {
            thingDef = RockTypes.RandomElement(),
            stackCount = 1
          };
        }
        else {
          return new QuarryResource() {
            thingDef = ThingDefOf.RockRubble,
            stackCount = 1
          };
        }
      }
      else {
        System.Random rand = new System.Random();
        int maxProb = QuarryMod.Resources.Sum(c => c.probability);
        int choice = rand.Next(maxProb);
        int sum = 0;

        foreach (QuarryResource resource in QuarryMod.Resources) {
          for (int i = sum; i < resource.probability + sum; i++) {
            if (i >= choice) {
              return resource;
            }
          }
          sum += resource.probability;
        }

        return QuarryMod.Resources.First();
      }
    }
  }
}
