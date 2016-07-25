using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Quarry {

  public class QuarryManager : MapComponent {

    private delegate IntVec3 Del_Offset(IntVec3 newLoc);
    private static   IntVec3 offsetUL(IntVec3 basePos) { return basePos + new IntVec3(-3, 0,  3); }
    private static   IntVec3 offsetUR(IntVec3 basePos) { return basePos + new IntVec3( 3, 0,  3); }
    private static   IntVec3 offsetLL(IntVec3 basePos) { return basePos + new IntVec3(-3, 0, -3); }
    private static   IntVec3 offsetLR(IntVec3 basePos) { return basePos + new IntVec3( 3, 0, -3); }

    public bool Spawned = false;

    private Building_QuarryBase baseInt;
    public Building_QuarryBase Base {
      get {
        if (baseInt == null) {
          List<Thing> allThings = Find.ListerThings.AllThings;
          for (int i = 0; i < allThings.Count; i++) {
            if (allThings[i] is Building_QuarryBase) {
              baseInt = allThings[i] as Building_QuarryBase;
              break;
            }
          }
        }
        return baseInt;
      }
    }

    private List<Building_Quarry> quadsInt;
    public List<Building_Quarry> Quads {
      get {
        if (quadsInt == null) {
          quadsInt = new List<Building_Quarry>();
          // Setup offsets
          Del_Offset Del_UL = new Del_Offset(offsetUL);
          Del_Offset Del_UR = new Del_Offset(offsetUR);
          Del_Offset Del_LL = new Del_Offset(offsetLL);
          Del_Offset Del_LR = new Del_Offset(offsetLR);

          // Manually add the quadrants
          quadsInt.Add(Del_UL(Base.Position).GetEdifice() as Building_Quarry);
          quadsInt.Add(Del_UR(Base.Position).GetEdifice() as Building_Quarry);
          quadsInt.Add(Del_LL(Base.Position).GetEdifice() as Building_Quarry);
          quadsInt.Add(Del_LR(Base.Position).GetEdifice() as Building_Quarry);
        }
        return quadsInt;
      }
    }


    


    public override void ExposeData() {
      base.ExposeData();

      Scribe_References.LookReference(ref baseInt, "QRY_QuarryManager_Base");
      Scribe_Values.LookValue(ref Spawned, "QRY_QuarryManager_Spawned", false);
      Scribe_Collections.LookList(ref quadsInt, "QRY_QuarryManager_Quads", LookMode.MapReference);
    }


    public void Register(Building_QuarryBase quarryBase, Building_Quarry ul, Building_Quarry ur, Building_Quarry ll, Building_Quarry lr) {
      if (Base != null) {
        Log.Warning("Trying to register a quarry when one already exists");
        return;
      }
      baseInt = quarryBase;
      quadsInt.Add(ul);
      quadsInt.Add(ur);
      quadsInt.Add(ll);
      quadsInt.Add(lr);
      Spawned = true;
    }


    public void DeconstructQuarry() {
      // Destroy all the quadrants
      foreach (Building_Quarry quad in quadsInt) {
        quad.Destroy();
      }
      // Deregister all pieces of the quarry
      Deregister();
    }


    public void Deregister() {
      baseInt = null;
      quadsInt.Clear();
      Spawned = false;
    }
  }
}
