using System.Collections.Generic;

using UnityEngine;
using Verse;

namespace Quarry {

  public class QuarryResourceDef : Def {

    public List<SimpleQuarryResource> Resources;

    // Default value to act as a fallback in case the entry is removed
    private float pctJunk = 0.75f;
    public float JunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp01(pctJunk);
      }
    }

    // Default value to act as a fallback in case the entry is removed
    private float pctChunks = 0.5f;
    public float ChunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp01(pctChunks);
      }
    }
  }
}
