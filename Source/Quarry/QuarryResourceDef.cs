using System.Collections.Generic;

using Verse;

namespace Quarry {

  public class QuarryResourceDef : Def {

    public List<SimpleQuarryResource> Resources;


    public IEnumerator<SimpleQuarryResource> GetEnumerator() {
      return Resources.GetEnumerator();
    }
  }
}
