using System.Collections.Generic;

using UnityEngine;
using Verse;

namespace Quarry {

  public class QuarryResourceDef : Def {

    private int PctJunk = 60;
    public int JunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp(PctJunk, 0, 100);
      }
    }

    private int PctChunks = 50;
    public int ChunkChance {
      get {
        // This prevents a player from setting a value too high or too low, 
        // which would cause errors when trying to spawn resources
        return Mathf.Clamp(PctChunks, 0, 100);
      }
    }

    public List<SimpleQuarryResource> Resources;


    public IEnumerator<SimpleQuarryResource> GetEnumerator() {
      return Resources.GetEnumerator();
    }
  }
}
