using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace Quarry {


  public class WorkGiver_MineQuarry : WorkGiver_Scanner {

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
      Building_Quarry quarry = t as Building_Quarry;

      // Make sure a permitted quarry is found, and that it has resources, and does not have too many workers
      if (quarry == null || quarry.IsForbidden(pawn) || quarry.Depleted) {
        return null;
      }

			if (!quarry.Unowned && !quarry.AssignedPawns.Contains(pawn)) {
				return null;
			}

      // Find a cell within the middle of the quarry to mine at
      IntVec3 cell = IntVec3.Invalid;
      CellRect rect = quarry.OccupiedRect().ContractedBy(quarry.WallThickness);
      foreach (IntVec3 c in rect.Cells.InRandomOrder()) {
        if (pawn.Map.reservationManager.CanReserve(pawn, c, 1)) {
          cell = c;
          break;
        }
      }
      // If a cell wasn't found, fail
      if (!cell.IsValid) {
        return null;
      }

      return new Job(QuarryDefOf.QRY_MineQuarry, cell);
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
      return	pawn.Map.listerBuildings.AllBuildingsColonistOfDef(QuarryDefOf.QRY_Quarry).Cast<Thing>().Concat(
							pawn.Map.listerBuildings.AllBuildingsColonistOfDef(QuarryDefOf.QRY_MiniQuarry).Cast<Thing>());
    }
  }
}

