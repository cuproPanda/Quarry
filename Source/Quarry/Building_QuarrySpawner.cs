using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace Quarry {
  // TODO: A15 - Add rotations, spawning platform, etc.
  public class Building_QuarrySpawner : Building {

    private bool usableCell;
    private List<Thing> thingsInCell;

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      // Destroy the spawner, otherwise every load generates a new set of quarries
      Destroy();

      // Build all the quarry pieces

      // Handles the graphic, preventing the graphic from tearing while zooming
      // Also handles autohauling and resource statistics (can no longer be handled by quadrants without overriding a few core classes)
      Quarry_Base quarry = ThingMaker.MakeThing(ThingDef.Named("QRY_Quarry"), null) as Quarry_Base;
      quarry.SetFactionDirect(Faction.OfPlayer);

      // Upper-left work area
      Quarry_Quadrant quarryUL = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryUL"), null) as Quarry_Quadrant;
      IntVec3 vec = Position + new IntVec3(-3, 0, 3);
      quarryUL.SetFactionDirect(Faction.OfPlayer);
      
      // Upper-right work area
      Quarry_Quadrant quarryUR = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryUR"), null) as Quarry_Quadrant;
      IntVec3 vec2 = Position + new IntVec3(3, 0, 3);
      quarryUR.SetFactionDirect(Faction.OfPlayer);

      // Lower-left work area
      Quarry_Quadrant quarryLL = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryLL"), null) as Quarry_Quadrant;
      IntVec3 vec3 = Position + new IntVec3(-3, 0, -3);
      quarryLL.SetFactionDirect(Faction.OfPlayer);

      // Lower-right work area
      Quarry_Quadrant quarryLR = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryLR"), null) as Quarry_Quadrant;
      IntVec3 vec4 = Position + new IntVec3(3, 0, -3);
      quarryLR.SetFactionDirect(Faction.OfPlayer);

      // Spawn all the quarry pieces
      GenSpawn.Spawn(quarry, Position, map);
      GenSpawn.Spawn(quarryUL, vec, map);
      GenSpawn.Spawn(quarryUR, vec2, map);
      GenSpawn.Spawn(quarryLL, vec3, map);
      GenSpawn.Spawn(quarryLR, vec4, map);

      // Register the quarry
      map.GetComponent<QuarryManager>().Register(quarry);

      // Create filth from digging the quarry
      Random rand = new Random();

      // Create a list of mineable rocks to pass to the manager
      // This allows for a weighted list, so if the quarry is only built on 1
      // tile of sandstone, mining sandstone will be very rare
      List<ThingDef> rockTypes = new List<ThingDef>();

      foreach (IntVec3 c in GenAdj.CellsOccupiedBy(quarry)) {

        usableCell = true;
        ThingDef rock = null;

        // What type of rock are we over?
        string rockType = c.GetTerrain(map).label.Split(' ').Last().CapitalizeFirst();
        // If there isn't a known chunk for this, it probably isn't a rock type
        // This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
        if (DefDatabase<ThingDef>.GetNamed("Chunk" + rockType, false) != null) {
          rock = DefDatabase<ThingDef>.GetNamed("Chunk" + rockType);
        }

        // Skip this cell if it is occupied by a placed object
        // This is to avoid save compression errors
        thingsInCell = map.thingGrid.ThingsListAt(c);
        for (int t = 0; t < thingsInCell.Count; t++) {
          if (thingsInCell[t].def.saveCompressible) {
            usableCell = false;
            break;
          }
        }

        if (usableCell) {
          int filthChance = rand.Next(100);

          // Check for dirt filth before checking for chunks,
          // since chunks can skip the current iteration
          if (filthChance < 60) {
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FilthDirt), c, map);
          }

          // Check for chunks
          if (filthChance < 20 && rock != null) {
            GenSpawn.Spawn(ThingMaker.MakeThing(rock), c, map);
          }

          // Check for rock rubble
          if (filthChance > 60) {
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RockRubble), c, map);
          } 
        }

        // if there was a rock, add it to the list
        if (rock != null) {
          rockTypes.Add(rock);
        }
      }

      // After the foreach loop, send the list off to the manager
      map.GetComponent<QuarryManager>().RockTypes = rockTypes;
    }
  }
}
