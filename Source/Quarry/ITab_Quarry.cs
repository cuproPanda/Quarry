using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Quarry {

  public class ITab_Quarry : ITab {

    private const float TopAreaHeight = 35f;
    private static readonly Vector2 WinSize = new Vector2(300f, 180f);

    public override bool IsVisible => true;

    public int MaxNumWorkers {
      get { return Quarry.maxNumWorkers; }
      set {
        Quarry.maxNumWorkers = Mathf.Clamp(value, 0, 32);
      }
    }

    private Building_Quarry Quarry {
      get { return (Building_Quarry)SelObject; }
    }
    

    public ITab_Quarry() {
      size = WinSize;
      labelKey = "Quarry";
    }


    protected override void FillTab() {
      Rect position = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
      Text.Font = GameFont.Small;
      Rect labelRect = new Rect(0f, 0f, 90f, 29f);
      Rect zeroRect = new Rect(130f, 0f, 25f, 25f);
      Rect lessRect = new Rect(155f, 0f, 25f, 25f);
      Rect numRect = new Rect(185f, 0f, 20f, 25f);
      Rect moreRect = new Rect(205f, 0f, 25f, 25f);
      Rect maxRect = new Rect(230f, 0f, 25f, 25f);

      GUI.BeginGroup(position);

      Widgets.Label(labelRect, "Max Workers:");

      if (Widgets.ButtonText(zeroRect, "<<", true, false, true)) {
        MaxNumWorkers = 0;
        SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
      }
      if (Widgets.ButtonText(lessRect, "<", true, false, true)) {
        MaxNumWorkers--;
        SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
      }

      Widgets.Label(numRect, MaxNumWorkers.ToString());

      if (Widgets.ButtonText(moreRect, ">", true, false, true)) {
        MaxNumWorkers++;
        SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
      }
      if (Widgets.ButtonText(maxRect, ">>", true, false, true)) {
        MaxNumWorkers = 32;
        SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
      }

      GUI.EndGroup();
    }
  }
}
