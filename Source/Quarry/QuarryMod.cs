using System.Collections.Generic;

using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod {

    private List<ThingDef> database;
    private List<QuarryResource> resources;
    private int vanillaTracker = 0;
    private int moddedTracker = 0;

    public static List<QuarryResource> Resources {
      get { return Instance.resources; }
    }
    public static List<ThingDef> Database {
      get { return Instance.database; }
    }

    private static QuarryMod Instance { get; set; }


    public QuarryMod(ModContentPack mcp) : base(mcp) {
      Instance = this;
      LongEventHandler.ExecuteWhenFinished(BuildResourceList);
      LongEventHandler.ExecuteWhenFinished(Echo);
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
  }
}
