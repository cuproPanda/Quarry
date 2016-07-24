using RimWorld;
using System;
using System.Collections.Generic;
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
      int tmp = Rand.RangeInclusive(1, 150);
      if (tmp < 40) {
        isChunk = true;
        SpawnProduct(chunk, 1);
        return;
      }


      if (tmp >= 40 && tmp < 45) {// 5
        if (DefDatabase<ThingDef>.GetNamed("CP_Copper", false) != null) {
          SpawnProduct(ThingDef.Named("CP_Copper"), 10);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 45 && tmp < 50) {// 5
        if (DefDatabase<ThingDef>.GetNamed("CP_Quartz", false) != null) {
          SpawnProduct(ThingDef.Named("CP_Quartz"), 10);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp == 50) {// 1
        if (DefDatabase<ThingDef>.GetNamed("CAL_RoseGold", false) != null) {
          SpawnProduct(ThingDef.Named("CAL_RoseGold"), 5);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 51 && tmp < 55) {// 4
        if (DefDatabase<ThingDef>.GetNamed("POW_Coldstone", false) != null) {
          SpawnProduct(ThingDef.Named("POW_Coldstone"), 1);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 55 && tmp < 60) {// 5
        if (DefDatabase<ThingDef>.GetNamed("Aluminium", false) != null) {
          SpawnProduct(ThingDef.Named("Aluminium"), 10);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 60 && tmp < 65) {// 5
        if (DefDatabase<ThingDef>.GetNamed("Copper", false) != null) {
          SpawnProduct(ThingDef.Named("Copper"), 10);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 65 && tmp < 70) {// 5
        if (DefDatabase<ThingDef>.GetNamed("MD2Coal", false) != null) {
          SpawnProduct(ThingDef.Named("MD2Coal"), 5);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 70 && tmp < 74) {// 4
        if (DefDatabase<ThingDef>.GetNamed("POW_Glowstone", false) != null) {
          SpawnProduct(ThingDef.Named("POW_Glowstone"), 1);
          return;
        }
        else {
          isChunk = true;
          SpawnProduct(chunk, 1);
          return;
        }
      }
      if (tmp >= 74 && tmp < 86) {// 12
        SpawnProduct(ThingDefOf.Steel, 15);
        return;
      }
      if (tmp >= 86 && tmp < 92) {// 6
        SpawnProduct(ThingDefOf.Silver, 15);
        return;
      }
      if (tmp >= 92 && tmp < 95) {// 3
        SpawnProduct(ThingDef.Named("Jade"), 10);
        return;
      }
      if (tmp >= 95 && tmp < 98) {// 3
        SpawnProduct(ThingDefOf.Gold, 10);
        return;
      }
      if (tmp == 98 || tmp == 99) {// 2
        SpawnProduct(ThingDef.Named("Uranium"), 5);
        return;
      }
      if (tmp == 100 ) {// 1
        SpawnProduct(ThingDefOf.Plasteel, 5);
        return;
      }
      if (tmp >= 101 && tmp < 104) {// 3
        SpawnProduct(ThingDefOf.Component, 2);
        return;
      }
      else { // Allow for increasing chunk odds without needing to refactor the others
        isChunk = true;
        SpawnProduct(chunk, 1);
        return;
      }
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
