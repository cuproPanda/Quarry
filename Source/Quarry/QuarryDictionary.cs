using System.Collections.Generic;

using Verse;

namespace Quarry {

  public interface IResource {
    int Probability { get; set; }
    int StackCount { get; set; }
  }


  public struct QResource : IResource {
    public int Probability { get; set; }
    public int StackCount { get; set; }
  }


  public class QuarryDictionary : Dictionary<ThingDef, QResource> {

    QResource qr;


    public void Add(ThingDef tDef, int probability, int stackCount) {
      qr.Probability = probability;
      qr.StackCount = stackCount;
      Add(tDef, qr);
    }
  }
}
