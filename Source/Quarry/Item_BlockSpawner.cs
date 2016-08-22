using System;

using RimWorld;
using Verse;

namespace Quarry {

  public class Item_BlockSpawner : ThingWithComps {

    // Rock types allowed to spawn in the current map
    private string rockType = Find.World.NaturalRockTypesIn(Find.Map.WorldCoords).RandomElement().building.mineableThing.ToString().Replace("Chunk", "");

    // Reference to the quarry manager
    private QuarryManager mgr = Find.Map.GetComponent<QuarryManager>();


    public override void SpawnSetup() {
      base.SpawnSetup();

      GenProduct();
      Destroy(DestroyMode.Vanish);
    }


    // Randomly determine the blocks, then tell 
    // SpawnProduct() what to spawn and how much
    public void GenProduct() {

      Random rand = new Random();
      int amount = rand.Next(10, 30);

      // If there aren't blocks for this type of rock, default to granite
      if (DefDatabase<ThingDef>.GetNamed("Blocks" + rockType, false) == null) {
        SpawnProduct(ThingDefOf.BlocksGranite, amount);
        return;
      }

      SpawnProduct(ThingDef.Named("Blocks" + rockType), amount);
    }


    // Spawn the resource
    public void SpawnProduct(ThingDef product, int stack) {
      Thing placedProduct = ThingMaker.MakeThing(product);
      placedProduct.stackCount = stack;

      GenPlace.TryPlaceThing(placedProduct, Position, ThingPlaceMode.Direct);


      if (mgr.Base != null) {
        mgr.Base.ResourceMined(QuarryItemType.Block);
      }
    }
  }
}
