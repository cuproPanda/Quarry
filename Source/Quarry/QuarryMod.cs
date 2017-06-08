using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod, IExposable {

    private float depletionPercentWhenQuarried = 0.05f;

    private List<ThingDef> database;
    private List<QuarryResource> resources;
    private int vanillaTracker = 0;
    private int moddedTracker = 0;
    private bool letterSent = false;

    public static List<QuarryResource> Resources {
      get { return Instance.resources; }
    }
    public static List<ThingDef> Database {
      get { return Instance.database; }
    }

    public static bool LetterSent {
      get { return Instance.letterSent; }
    }

    public static float DepletionPercentWhenQuarried {
      get { return Instance.depletionPercentWhenQuarried; }
    }

    public static QuarryMod Instance { get; private set; }


    public QuarryMod(ModContentPack mcp) : base(mcp) {
      Instance = this;
      LongEventHandler.ExecuteWhenFinished(BuildResourceList);
      LongEventHandler.ExecuteWhenFinished(Echo);
    }


    public void ExposeData() {
      Scribe_Values.Look(ref letterSent, "QRY_letterSent", false);
      Scribe_Values.Look(ref depletionPercentWhenQuarried, "QRY_depletionPercentWhenQuarried", 0.05f);
    }


    private void BuildResourceList() {
      database = DefDatabase<ThingDef>.AllDefsListForReading;
      resources = new List<QuarryResource>();

      // Add vanilla resources
      foreach (SimpleQuarryResource resource in QuarryDefOf.MainResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          vanillaTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
      // Add resources from my mods
      foreach (SimpleQuarryResource resource in QuarryDefOf.CuproResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          moddedTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
      // Add other modded resources
      foreach (SimpleQuarryResource resource in QuarryDefOf.ModdedResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          moddedTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
    }


    private void Echo() {
      // I'm keeping this since it might prove useful in the future for user errors
      Log.Message("Quarry:: Loaded " + vanillaTracker + " vanilla and " + moddedTracker + " modded entries into resource list.");
    }


    public void Notify_LetterSent() {
      Instance.letterSent = true;
    }


    public override string SettingsCategory() {
      return Static.Quarry;
    }


    public override void DoSettingsWindowContents(Rect rect) {
      Listing_Standard list = new Listing_Standard();
      list.ColumnWidth = rect.width;
      list.Begin(rect);
      list.Label(Static.SettingsDepletionPercent);
      depletionPercentWhenQuarried = list.Slider(GenMath.RoundedHundredth(depletionPercentWhenQuarried), 0f, 0.25f);
      if (depletionPercentWhenQuarried > 0.001f) {
        list.Label("QRY_DepletionLabel".Translate(Mathf.RoundToInt(100f / depletionPercentWhenQuarried)));
      }
      else {
        list.Label("QRY_DepletionLabel".Translate("Infinite"));
      }
      list.End();
    }
  }
}
