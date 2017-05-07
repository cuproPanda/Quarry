using System.Collections.Generic;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace Quarry {

  public class Quarry_Quadrant : Building_WorkTable {

    // Only one quarry is allowed, so this will always return the correct quarry
    Quarry_Base parentInt;
    Quarry_Base Parent {
      get {
        if (parentInt == null) {
          parentInt = Map.GetComponent<QuarryManager>().Base;
        }
        return parentInt;
      }
    }


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_References.Look(ref parentInt, "parent");
    }


    public override IEnumerable<Gizmo> GetGizmos() {
      Command_Action parent = new Command_Action() {

        icon = ContentFinder<Texture2D>.Get("Cupro/Object/Quarry", false),
        defaultDesc = "QRY_SwitchToParent".Translate(),
        activateSound = SoundDef.Named("Click"),
        action = () => {
          Find.Selector.Deselect(this);
          Find.Selector.Select(Parent);
        },
      };
      yield return parent;

      if (base.GetGizmos() != null) {
        foreach (Command c in base.GetGizmos()) {
          yield return c;
        }
      }
    }


    public override string GetInspectString() {
      StringBuilder stringBuilder = new StringBuilder();

      // Display the chunks and resources mined here
      stringBuilder.AppendLine("QRY_ChunksMined".Translate() + ": " + Parent.chunkTracker.ToString("N0"));
      stringBuilder.AppendLine("QRY_ResourcesMined".Translate() + ": " + Parent.resourceTracker.ToString("N0"));
      stringBuilder.AppendLine("QRY_BlocksMined".Translate() + ": " + Parent.blockTracker.ToString("N0"));

      return stringBuilder.ToString().TrimEndNewlines();
    }
  }
}
