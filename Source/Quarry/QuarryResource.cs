using Verse;

namespace Quarry {

  public class QuarryResource {

    public ThingDef thingDef;
    public int probability;
    public int stackCount;


    public QuarryResource() {

    }


    public QuarryResource(ThingDef thingDef, int stackCount) {
      this.thingDef = thingDef;
      this.stackCount = stackCount;
    }


    public QuarryResource(ThingDef thingDef, int probability, int stackCount) {
      this.thingDef = thingDef;
      this.probability = probability;
      this.stackCount = stackCount;
    }


    public Thing ToThing() {
      Thing t = ThingMaker.MakeThing(thingDef);
      t.stackCount = stackCount;
      return t;
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
