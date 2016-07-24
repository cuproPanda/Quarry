using System.Collections.Generic;

using Verse;

namespace Quarry {

  public class QuarryManager : MapComponent {

    public Building_QuarryBase Base;
    public List<Building_Quarry> Quads = new List<Building_Quarry>();

    // Is there currently a quarry spawned?
    public bool Spawned = false;

    public override void ExposeData() {
      base.ExposeData();
      Scribe_References.LookReference(ref Base, "QRY_QuarryManager_Base");
      Scribe_Collections.LookList(ref Quads, "QRY_QuarryManager_Quads", LookMode.Deep);
      Scribe_Values.LookValue(ref Spawned, "QRY_QuarryManager_Spawned", false);
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
