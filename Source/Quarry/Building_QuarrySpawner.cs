using System;
using System.Linq;

using RimWorld;
using Verse;

namespace Quarry {
  // TODO: A15 - Add rotations, spawning platform, etc.
  public class Building_QuarrySpawner : Building {

    public override void SpawnSetup() {
      base.SpawnSetup();

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
      GenSpawn.Spawn(quarry, Position);
      GenSpawn.Spawn(quarryUL, vec);
      GenSpawn.Spawn(quarryUR, vec2);
      GenSpawn.Spawn(quarryLL, vec3);
      GenSpawn.Spawn(quarryLR, vec4);

      // Register the quarry
      Find.Map.GetComponent<QuarryManager>().Register(quarry);

      // Create filth from digging the quarry
      Random rand = new Random();
      foreach (IntVec3 c in GenAdj.CellsOccupiedBy(quarry)) {

        int junkChance = rand.Next(100);

        // Check for dirt filth before checking for chunks,
        // since chunks can skip the current iteration
        if (junkChance < 60) {
          GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FilthDirt), c);
        }

        // Check for chunks
        if (junkChance < 20) {
          // What type of rock are we over?
          string rockType = c.GetTerrain().label.Split(' ').Last().CapitalizeFirst();
          // If rockType doesn't return a known value, skip to the next tile
          // This could be from a modded rock type, or from a terrain that isn't rock
          if (rockType != "Sandstone" && rockType != "Granite" && rockType != "Limestone" && rockType != "Slate" && rockType != "Marble") {
            continue;
          }
          GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("Chunk" + rockType)), c);
        }

        // Check for rock rubble
        if (junkChance > 60) {
          GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.RockRubble), c); 
        }
      }
    }
  }
}
