using System.Collections.Generic;
using System.Linq;

using Verse;

namespace Quarry {

  public class PlaceWorker_Quarry : PlaceWorker {

    List<IntVec3> occupiedCellsTemp = new List<IntVec3>();


		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null) {

      int occCells = 0;
      int rockCells = 0;
      foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size)) {
        occCells++;

        // Make sure the quarry is over sufficient rock
        // Gravel is an acceptable terrain since it normally borders stone
        if (QuarryPlacementUtility.validator(map.terrainGrid.TerrainAt(c))) {
          rockCells++;
        }
      }

      // Require at least 60% rocky terrain
      if ((float)(occCells - rockCells) / occCells > 0.4f) {
        return Static.ReportNotEnoughStone;
      }

      return true;
    }
	}
}
