using RimWorld;
using Verse;

namespace Quarry {

  public class Building_QuarrySpawner : Building {

    public override void SpawnSetup() {
      base.SpawnSetup();

      // Destroy the spawner, otherwise every load generates a new set of quarries
      Destroy();

      // Build all the quarry pieces

      // Handles the graphic, preventing the graphic from tearing while zooming
      // Also handles autohauling and resource statistics (can no longer be handled by quadrants without overriding a few core classes)
      Building_QuarryBase quarry = ThingMaker.MakeThing(ThingDef.Named("QRY_Quarry"), null) as Building_QuarryBase;
      quarry.SetFactionDirect(Faction.OfPlayer);

      // Upper-left work area
      Building_Quarry quarryUL = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryUL"), null) as Building_Quarry;
      IntVec3 vec = Position + new IntVec3(-3, 0, 3);
      quarryUL.SetFactionDirect(Faction.OfPlayer);
      
      // Upper-right work area
      Building_Quarry quarryUR = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryUR"), null) as Building_Quarry;
      IntVec3 vec2 = Position + new IntVec3(3, 0, 3);
      quarryUR.SetFactionDirect(Faction.OfPlayer);

      // Lower-left work area
      Building_Quarry quarryLL = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryLL"), null) as Building_Quarry;
      IntVec3 vec3 = Position + new IntVec3(-3, 0, -3);
      quarryLL.SetFactionDirect(Faction.OfPlayer);

      // Lower-right work area
      Building_Quarry quarryLR = ThingMaker.MakeThing(ThingDef.Named("QRY_QuarryLR"), null) as Building_Quarry;
      IntVec3 vec4 = Position + new IntVec3(3, 0, -3);
      quarryLR.SetFactionDirect(Faction.OfPlayer);

      // Spawn all the quarry pieces

      GenSpawn.Spawn(quarry, Position);
      GenSpawn.Spawn(quarryUL, vec);
      GenSpawn.Spawn(quarryUR, vec2);
      GenSpawn.Spawn(quarryLL, vec3);
      GenSpawn.Spawn(quarryLR, vec4);

      // Register all the quarry pieces

      Find.Map.GetComponent<QuarryManager>().Register(quarry, quarryUL, quarryUR, quarryLL, quarryLR);
    }
  }
}
