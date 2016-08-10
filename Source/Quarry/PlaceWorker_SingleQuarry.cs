using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace Quarry {

  public class PlaceWorker_SingleQuarry : PlaceWorker {

    List<IntVec3> occupiedCellsTemp = new List<IntVec3>();


    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot) {

      if (Find.Map.GetComponent<QuarryManager>().Spawned) {
        return "QuarryAlreadyBuilt".Translate();
      }

      // This prevents placing multiple blueprints to get around the check
      if (Find.ListerThings.ThingsOfDef(ThingDef.Named("QRY_QuarrySpawner").blueprintDef).Count > 0){
        return "QuarryAlreadyBuilt".Translate();
      }

      // Don't allow placing on steam geysers - in dev mode
      occupiedCellsTemp.Clear();
      int occCells = 0;
      int rockCells = 0;
      foreach (IntVec3 current in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size)) {
        occupiedCellsTemp.Add(current);
        occCells++;
      }

      for (int i = 0; i < occupiedCellsTemp.Count; i++) {
        IntVec3 c = occupiedCellsTemp[i];

        // Try to find a geyser
        Thing geyser = Find.ThingGrid.ThingAt(c, ThingDefOf.SteamGeyser);
        if (geyser != null) {
          return false;
        }

        // Make sure the quarry is over sufficient rock
        Predicate<TerrainDef> validator = ((TerrainDef t) => t.defName.EndsWith("_Rough") || t.defName.EndsWith("_RoughHewn") || t.defName.EndsWith("_Smooth") || t == TerrainDefOf.Gravel);
        if (validator(c.GetTerrain())) {
          rockCells++;
        }
      }

      // Require at least 60% rocky terrain
      if ((float)(occCells - rockCells) / occCells > 0.4f) {
        return false;
      }

      return true;
    }
  }
}
