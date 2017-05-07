using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Quarry {

  public class Item_Conglomerate : ThingWithComps {

    // Rock types allowed to spawn in the current map
    private ThingDef chunk;

    // Reference to the quarry manager
    private QuarryManager mgr;

    // Reference to the resources file
    private QuarryResourceDef resourceDef = DefDatabase<QuarryResourceDef>.GetNamed("Resources");

    // What type of item is this? For quarry tracking
    private QuarryItemType type = QuarryItemType.None;

    private Map mapRef = null;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      mapRef = map;
      chunk = Find.World.NaturalRockTypesIn(map.Tile).RandomElement().building.mineableThing;
      mgr = map.GetComponent<QuarryManager>();

      GenProduct();
      Destroy(DestroyMode.Vanish);
    }


    // Randomly determine the resource, then tell 
    // SpawnProduct() what to spawn and how much
    public void GenProduct() {

      if (mgr.resources == null) {
        Log.Warning("Trying to spawn resources with no resource list! Is the quarry missing?");
        mgr.FindResources();
        if (mgr.resources == null) {
          Log.Warning("Unable to find a resources list. Destroying output.");
          Destroy();
        }
      }
      List<QuarryResource> resources = mgr.resources;

      Random rand = new Random();
      int junkChance = rand.Next(100);
      int chunkChance = rand.Next(100);

      if (junkChance < resourceDef.JunkChance) {
        if (chunkChance < resourceDef.ChunkChance) {
          SpawnProduct(chunk, 1, false); 
        }
        else {
          SpawnProduct(ThingDefOf.RockRubble, 1, false, true);
        }
      }
      else { 
        int maxProb = resources.Sum(c => c.Probability);
        int choice = rand.Next(maxProb);
        int sum = 0;

        foreach (QuarryResource resource in resources) {
          for (int i = sum; i < resource.Probability + sum; i++) {
            if (i >= choice) {
              SpawnProduct(resource.ThingDef, resource.StackCount, resource.LargeVein);
              goto Done;
            }
          }
          sum += resource.Probability;
        }

        QuarryResource first = resources.First();
        SpawnProduct(first.ThingDef, first.StackCount, first.LargeVein); 
      }
      Done:;
    }


    // Spawn the resource
    public void SpawnProduct(ThingDef product, int stack, bool largeVein, bool failed = false) {
      Thing placedProduct = ThingMaker.MakeThing(product);
      placedProduct.stackCount = stack;

      GenPlace.TryPlaceThing(placedProduct, Position, mapRef, ThingPlaceMode.Direct);

      // Handle text motes
      if (largeVein) {
        MoteMaker.ThrowText(placedProduct.DrawPos, mapRef, "QRY_TextMote_LargeVein".Translate(), 3f);
      }
      if (failed) {
        MoteMaker.ThrowText(placedProduct.DrawPos, mapRef, "QRY_TextMote_MiningFailed".Translate(), 3f);
      }

      if (mgr.Base != null) {
        // If a haulable (chunk or slag) was spawned, mark it as haulable (if the player allows it)
        if (product.designateHaulable) {
          if (mgr.Base.autoHaul) {
            mapRef.designationManager.AddDesignation(new Designation(placedProduct, DesignationDefOf.Haul));
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
