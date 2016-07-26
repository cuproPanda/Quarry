using Verse;

namespace Quarry {

  public class QuarryResource : IExposable {

    private QuarryResourceDef def;
    public QuarryResourceDef Def {
      get {
        if (def == null) {
          def = DefDatabase<QuarryResourceDef>.GetNamed("Resources");
        }
        return def;
      }
    }

    private ThingDef thingDef;
    public ThingDef ThingDef { get { return thingDef; } set { thingDef = value; } }

    private int probability;
    public int Probability { get { return probability; } set { probability = value; } }

    private int stackCount;
    public int StackCount { get { return stackCount; } set { stackCount = value; } }


    public QuarryResource() {

    }


    public QuarryResource(ThingDef thingDef, int probability, int stackCount) {
      this.thingDef = thingDef;
      this.probability = probability;
      this.stackCount = stackCount;
    }


    public void ExposeData() {
      Scribe_Defs.LookDef(ref def, "QRY_QuarryResourceDef");
    }
  }



  // SimpleQuarryResource uses strings instead of ThingDefs to prevent
  // errors on loading. The strings are later processed and it is 
  // determined if they match with a ThingDef currently in the game
  public class SimpleQuarryResource {

    public string thingDef;
    public int probability;
    public int stackCount;


    public SimpleQuarryResource() {

    }


    public SimpleQuarryResource(string thingDef, int probability, int stackCount) {
      this.thingDef = thingDef;
      this.probability = probability;
      this.stackCount = stackCount;
    }
  }
}
