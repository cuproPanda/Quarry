using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;

namespace Quarry {

  public enum QuarryItemType {
    None = 0,
    Chunk = 1,
    Resource = 2,
    Block = 4
  }



  public class Quarry_Base : Building {

    //private static QuarryManager mgr = Find.VisibleMap.GetComponent<QuarryManager>();
    private static QuarryManager mgr;

    // autoHaul defaults to true
    public bool autoHaul = true;
    // Trackers for the resources gathered here
    public int chunkTracker;
    public int resourceTracker;
    public int blockTracker;
    private Map map;

    private string Description {
      get {
        if (autoHaul) {
          return "QRY_Haul".Translate();
        }
        return "QRY_NotHaul".Translate();
      }
    }


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref autoHaul, "QRY_autoHaul");
      Scribe_Values.Look(ref chunkTracker, "QRY_Chunks", 0);
      Scribe_Values.Look(ref resourceTracker, "QRY_Resources", 0);
      Scribe_Values.Look(ref blockTracker, "QRY_Blocks", 0);
    }


    public override IEnumerable<Gizmo> GetGizmos() {
      Command_Toggle haul = new Command_Toggle() {

        icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul", false),
        defaultDesc = Description,
        hotKey = KeyBindingDefOf.Misc12,
        activateSound = SoundDef.Named("Click"),
        isActive = () => autoHaul,
        toggleAction = () => { autoHaul = !autoHaul; },
      };
      yield return haul;

      if (base.GetGizmos() != null) {
        foreach (Command c in base.GetGizmos()) {
          yield return c;
        }
      }
    }


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      this.map = map;
      mgr = map.GetComponent<QuarryManager>();

      // Tell the quarry manager to find the available resources
      // This is called when a quarry is built, or on game load
      mgr.FindResources();
    }


    public void ResourceMined(QuarryItemType item, int quantity = 1) {
      if (item == QuarryItemType.Chunk) {
        chunkTracker++;
      }
      if (item == QuarryItemType.Resource) {
        resourceTracker++;
      }
      if (item == QuarryItemType.Block) {
        blockTracker += quantity;
      }
    }


    public override void DeSpawn() {
      base.DeSpawn();

      // Give back the materials used for making the steps,
      // this doesn't support xml alteration
      Thing placedProduct = ThingMaker.MakeThing(ThingDefOf.WoodLog);
      placedProduct.stackCount = Random.Range(60, 81);
      GenPlace.TryPlaceThing(placedProduct, Position, map, ThingPlaceMode.Direct);

      // Tell the quarry manager to deconstruct the quarry
      mgr.DeconstructQuarry();
    }
  }
}
