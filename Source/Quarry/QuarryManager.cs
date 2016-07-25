using System;
using System.Collections.Generic;

using Verse;

namespace Quarry {

  public class QuarryManager : MapComponent {

    public Building_QuarryBase Base;
    public List<Building_Quarry> Quads = new List<Building_Quarry>();

    // Is there currently a quarry spawned?
    public bool Spawned = false;

    // Resolve issues with prior versions
    private delegate IntVec3 Del_Offset(IntVec3 newLoc);
    private static IntVec3 offsetUL(IntVec3 basePos) { return basePos + new IntVec3(-3, 0, 3); }
    private static IntVec3 offsetUR(IntVec3 basePos) { return basePos + new IntVec3(3, 0, 3); }
    private static IntVec3 offsetLL(IntVec3 basePos) { return basePos + new IntVec3(-3, 0, -3); }
    private static IntVec3 offsetLR(IntVec3 basePos) { return basePos + new IntVec3(3, 0, -3); }


    public override void ExposeData() {
      base.ExposeData();

      Scribe_References.LookReference(ref Base, "QRY_QuarryManager_Base");
      Scribe_Values.LookValue(ref Spawned, "QRY_QuarryManager_Spawned", false);

      // TODO: In A15, replace this with:
      //Scribe_Collections.LookList(ref Quads, "QRY_QuarryManager_Quads", LookMode.MapReference);
      // For now, each load has to regenerate the list of quadrants
      if (Scribe.mode == LoadSaveMode.LoadingVars && Quads == null) {
        // Setup offsets
        Del_Offset Del_UL = new Del_Offset(offsetUL);
        Del_Offset Del_UR = new Del_Offset(offsetUR);
        Del_Offset Del_LL = new Del_Offset(offsetLL);
        Del_Offset Del_LR = new Del_Offset(offsetLR);

        // Manually add the quadrants
        Quads.Add(Del_UL(Base.Position).GetEdifice() as Building_Quarry);
        Quads.Add(Del_UR(Base.Position).GetEdifice() as Building_Quarry);
        Quads.Add(Del_LL(Base.Position).GetEdifice() as Building_Quarry);
        Quads.Add(Del_LR(Base.Position).GetEdifice() as Building_Quarry);
      }
    }


    public void Register(Building_QuarryBase quarryBase, Building_Quarry ul, Building_Quarry ur, Building_Quarry ll, Building_Quarry lr) {
      if (Base != null) {
        Log.Warning("Trying to register a quarry when one already exists");
        return;
      }
      Base = quarryBase;
      Quads.Add(ul);
      Quads.Add(ur);
      Quads.Add(ll);
      Quads.Add(lr);
      Spawned = true;
    }


    public void DeconstructQuarry() {
      // Destroy all the quadrants
      foreach (Building_Quarry quad in Quads) {
        quad.Destroy();
      }
      // Deregister all pieces of the quarry
      Deregister();
    }


    public void Deregister() {
      Base = null;
      Quads.Clear();
      Spawned = false;
    }
  }
}
