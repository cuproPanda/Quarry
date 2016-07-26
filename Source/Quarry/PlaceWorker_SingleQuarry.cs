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

      // Don't allow placing on steam geysers - obviously
      occupiedCellsTemp.Clear();
      foreach (IntVec3 current in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size)) {
        occupiedCellsTemp.Add(current);
      }

      for (int i = 0; i < occupiedCellsTemp.Count; i++) {
        IntVec3 c = occupiedCellsTemp[i];
        Thing geyser = Find.ThingGrid.ThingAt(c, ThingDefOf.SteamGeyser);
        if (geyser != null) {
          return false;
        }
      }

      return true;
    }
  }
}
