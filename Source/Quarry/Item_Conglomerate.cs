using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Quarry {

  public class Item_Conglomerate : ThingWithComps {

    // Rock types allowed to spawn in the current map
    private ThingDef chunk = Find.World.NaturalRockTypesIn(Find.Map.WorldCoords).RandomElement().building.mineableThing;

    // Reference to the quarry manager
    private QuarryManager mgr = Find.Map.GetComponent<QuarryManager>();

    // Reference to the resources file
    private QuarryResourceDef resourceDef = DefDatabase<QuarryResourceDef>.GetNamed("Resources");

    // What type of item is this? For quarry tracking
    private QuarryItemType type = QuarryItemType.None;


    public override void SpawnSetup() {
      base.SpawnSetup();

      GenProduct();
      Destroy(DestroyMode.Vanish);
    }


    // Randomly determine the resource, then tell 
    // SpawnProduct() what to spawn and how much
    public void GenProduct() {

      if (mgr.Resources == null) {
        Log.Warning("Trying to spawn resources with no resource list! Is the quarry missing?");
        mgr.FindResources();
        if (mgr.Resources == null) {
          Log.Warning("Unable to find a resources list. Destroying output.");
          Destroy();
        }
      }
      List<QuarryResource> resources = mgr.Resources;

      Random rand = new Random();
      int junkChance = rand.Next(100);
      int chunkChance = rand.Next(100);

      if (junkChance < resourceDef.JunkChance) {
        if (chunkChance < resourceDef.ChunkChance) {
          SpawnProduct(chunk, 1); 
        }
        else {
          SpawnProduct(ThingDefOf.RockRubble, 1);
        }
      }
      else { 
        int maxProb = resources.Sum(c => c.Probability);
        int choice = rand.Next(maxProb);
        int sum = 0;

        foreach (QuarryResource resource in resources) {
          for (int i = sum; i < resource.Probability + sum; i++) {
            if (i >= choice) {
              SpawnProduct(resource.ThingDef, resource.StackCount);
              goto Done;
            }
          }
          sum += resource.Probability;
        }

        QuarryResource first = resources.First();
        SpawnProduct(first.ThingDef, first.StackCount); 
      }
      Done:;
    }


    // Spawn the resource
    public void SpawnProduct(ThingDef product, int stack) {
      Thing placedProduct = ThingMaker.MakeThing(product);
      placedProduct.stackCount = stack;

      GenPlace.TryPlaceThing(placedProduct, Position, ThingPlaceMode.Direct);

      if (mgr.Base != null) {
        // If a haulable (chunk or slag) was spawned, mark it as haulable (if the player allows it)
        if (product.designateHaulable) {
          if (mgr.Base.AutoHaul) {
            Find.DesignationManager.AddDesignation(new Designation(placedProduct, DesignationDefOf.Haul));
          }

          // Mark this as a chunk
          if (product != ThingDefOf.ChunkSlagSteel) {
            type = QuarryItemType.Chunk; 
          }
        }

        // Mark this as a resource
        if ((!product.designateHaulable || product == ThingDefOf.ChunkSlagSteel) && product != ThingDefOf.RockRubble) {
          type = QuarryItemType.Resource;
        }

        // Tell the quarry what type of item was mined
        if (type != QuarryItemType.None) {
          mgr.Base.ResourceMined(type); 
        }
      }
    }
  }
}
