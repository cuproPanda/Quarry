using Verse;

namespace Quarry {
  // TODO: Change graphic based on resources mined, simulating digging deeper
  public class Building_QuarryBase : Building {

    // autoHaul defaults to true
    public bool AutoHaul = true;
    // Trackers for the resources gathered here
    public int ChunkTracker;
    public int ResourceTracker;


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.LookValue(ref AutoHaul, "QRY_autoHaul");
      Scribe_Values.LookValue(ref ChunkTracker, "QRY_Chunks", 0);
      Scribe_Values.LookValue(ref ResourceTracker, "QRY_Resources", 0);
    }


    public void ResourceMined(QuarryItemType item) {
      if (item == QuarryItemType.Chunk) {
        ChunkTracker++;
      }
      if (item == QuarryItemType.Resource) {
        ResourceTracker++;
      }
    }


    public override void DeSpawn() {
      base.DeSpawn();
      // Tell the quarry manager to deconstruct the quarry
      Find.Map.GetComponent<QuarryManager>().DeconstructQuarry();
    }
  }
}
