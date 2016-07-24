using Verse;

namespace Quarry {

  public class PlaceWorker_SingleQuarry : PlaceWorker {

    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot) {

      if (Find.Map.GetComponent<QuarryManager>().Spawned) {
        return "QuarryAlreadyBuilt".Translate();
      }
      // This prevents placing multiple blueprints to get around the check
      if (Find.ListerThings.ThingsOfDef(ThingDef.Named("QRY_QuarrySpawner").blueprintDef).Count > 0){
        return "QuarryAlreadyBuilt".Translate();
      }
      return true;
    }
  }
}
