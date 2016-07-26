using RimWorld;
using Verse;

namespace Quarry {

  public class QuarryResources {

    public QuarryDictionary Resources;

    
    public void BuildBaseDictionary() {
      Resources = new QuarryDictionary();
      Resources.Add(ThingDefOf.Steel, 12, 10);
      Resources.Add(ThingDefOf.Silver, 6, 15);
      Resources.Add(ThingDef.Named("Jade"), 3, 10);
      Resources.Add(ThingDefOf.Gold, 3, 8);
      Resources.Add(ThingDefOf.Component, 3, 2);
      Resources.Add(ThingDefOf.ChunkSlagSteel, 3, 1);
      Resources.Add(ThingDef.Named("Uranium"), 2, 5);
      Resources.Add(ThingDefOf.Plasteel, 1, 5);
    }


    public void Add (ThingDef tDef, int probability, int stackCount) {
      Resources.Add(tDef, probability, stackCount);
    }
  }
}
