using System.Linq;

using Verse;
using Verse.AI;
using RimWorld;

namespace Quarry {


  public class WorkGiver_MineQuarry : WorkGiver_Scanner {

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
      Building_Quarry quarry = t as Building_Quarry;

      if (quarry == null || (!quarry.quarryResources && !quarry.quarryBlocks)) {
        return null;
      }

      IntVec3 cell = IntVec3.Invalid;
      CellRect rect = quarry.OccupiedRect().ContractedBy(2);
      foreach (IntVec3 c in rect.Cells.InRandomOrder()) {
        if (pawn.Map.reservationManager.CanReserve(pawn, c, 1)) {
          cell = c;
          break;
        }
      }

      if (!cell.IsValid) {
        return null;
      }

      return new Job(QuarryDefOf.QRY_MineQuarry, quarry, cell);
    }

    public override System.Collections.Generic.IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
      return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(QuarryDefOf.QRY_Quarry).Cast<Thing>();
    }
  }
}

