using System;

using RimWorld;
using Verse;

namespace Quarry {

  public class Item_BlockSpawner : ThingWithComps {

    // Rock types allowed to spawn in the current map
    private string rockType;

    // Reference to the quarry manager
    private QuarryManager mgr;

    // Reference to the resources file
    private QuarryResourceDef resourceDef = DefDatabase<QuarryResourceDef>.GetNamed("Resources");

    private Map mapRef = null;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      mapRef = map;
      mgr = map.GetComponent<QuarryManager>();
      rockType = mgr.RockTypes.RandomElement().ToString().Replace("Chunk", "");

      GenProduct();
      Destroy(DestroyMode.Vanish);
    }


    // Randomly determine the blocks, then tell 
    // SpawnProduct() what to spawn and how much
    public void GenProduct() {

      Random rand = new Random();
      int amount = rand.Next(10, 30);
      int chunkChance = rand.Next(100);

      if (chunkChance < resourceDef.ChunkChance) {
        // If there aren't blocks for this type of rock, default to granite
        if (DefDatabase<ThingDef>.GetNamed("Blocks" + rockType, false) == null) {
          SpawnProduct(ThingDefOf.BlocksGranite, amount);
          return;
        }

        SpawnProduct(ThingDef.Named("Blocks" + rockType), amount);
      }
      else {
        SpawnProduct(ThingDefOf.RockRubble, 1, false);
      }
    }


    // Spawn the resource
    public void SpawnProduct(ThingDef product, int stack, bool blocksSpawned = true) {
      Thing placedProduct = ThingMaker.MakeThing(product);
      placedProduct.stackCount = stack;

      GenPlace.TryPlaceThing(placedProduct, Position, mapRef, ThingPlaceMode.Direct);

      if (!blocksSpawned) {
        MoteMaker.ThrowText(placedProduct.DrawPos, mapRef, "QRY_TextMote_MiningFailed".Translate(), 3f);
      }

      if (mgr.Base != null && blocksSpawned) {
        mgr.Base.ResourceMined(QuarryItemType.Block, stack);
      }
    }
  }
}
