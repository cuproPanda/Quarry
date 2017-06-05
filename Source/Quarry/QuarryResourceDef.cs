using System.Collections.Generic;

using UnityEngine;
using Verse;

namespace Quarry {

  public class QuarryResourceDef : Def {

    private float pctJunk = 0.6f;
    public float JunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp01(pctJunk);
      }
    }

    private float pctChunks = 0.5f;
    public float ChunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp01(pctChunks);
      }
    }

    public List<SimpleQuarryResource> resources;


    public IEnumerator<SimpleQuarryResource> GetEnumerator() {
      return resources.GetEnumerator();
    }
  }
}
