using System.Collections.Generic;

using UnityEngine;
using Verse;

namespace Quarry {

  public class QuarryResourceDef : Def {

    private int chunkChanceInt;
    public int ChunkChance {
      get {
        return chunkChanceInt;
      }
      set {
        chunkChanceInt = Mathf.Clamp(value, 0, 100);
      }
    }

    public List<SimpleQuarryResource> Resources;


    public IEnumerator<SimpleQuarryResource> GetEnumerator() {
      return Resources.GetEnumerator();
    }
  }
}
