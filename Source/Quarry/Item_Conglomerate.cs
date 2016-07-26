using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Quarry {

  public class Item_Conglomerate : ThingWithComps {

    // Rock types allowed to spawn in the current map
    private ThingDef chunk = Find.World.NaturalRockTypesIn(Find.Map.WorldCoords).RandomElement().building.mineableThing;

    // Reference to the quarry this was spawned at
    private Building_QuarryBase quarry = Find.Map.GetComponent<QuarryManager>().Base;

    // Is this a chunk? For auto-hauling
    private bool isChunk;

    // What type of item is this? For quarry tracking
    private QuarryItemType type;


    public override void SpawnSetup() {
      base.SpawnSetup();

      GenProduct();
      Destroy(DestroyMode.Vanish);
    }


    // Randomly determine the resource, then tell 
    // SpawnProduct() what to spawn and how much
    public void GenProduct() {

      if (Find.Map.GetComponent<QuarryManager>().Resources == null) {
        Log.Warning("Trying to spawn resources with no resource list! Is the quarry missing?");
      }
      List<QuarryResource> resources = Find.Map.GetComponent<QuarryManager>().Resources;

      Random rand = new Random();
      int chunkChance = rand.Next(100);

      if (chunkChance < DefDatabase<QuarryResourceDef>.GetNamed("Resources").ChunkChance) {
        isChunk = true;
        SpawnProduct(chunk, 1);
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

      if (quarry != null) {
        // If a chunk was spawned, mark it as haulable (if the player allows it)
        if (isChunk) {
          if (quarry.AutoHaul) {
            Find.DesignationManager.AddDesignation(new Designation(placedProduct, DesignationDefOf.Haul));
          }
          // Mark this as a chunk
          type = QuarryItemType.Chunk;
        }
        if (!isChunk) {
          // Mark this as a resource
          type = QuarryItemType.Resource;
        }
        // Tell the quarry what type of item was mined
        quarry.ResourceMined(type);
      }
    }
  }
}
