using System.Collections.Generic;

using Verse;

namespace Quarry {
  public sealed class QuarryMod : Mod {

    private List<ThingDef> database;
    private List<QuarryResource> resources;
    public static List<QuarryResource> Resources {
      get { return Resources; }
    }

    public QuarryMod(ModContentPack mcp) : base(mcp) {
      LongEventHandler.ExecuteWhenFinished(BuildResourceList);
    }


    private void BuildResourceList() {
      database = DefDatabase<ThingDef>.AllDefsListForReading;
      resources = new List<QuarryResource>();
      ThingDef tmpThing;

      foreach (SimpleQuarryResource resource in QuarryDefOf.Resources.resources) {
        tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          ThingDef resourceDef = tmpThing;

          resources.Add(new QuarryResource(
            resourceDef,
            resource.probability,
            resource.stackCount,
            resource.largeVein));
        }
      }
    }
  }
}
