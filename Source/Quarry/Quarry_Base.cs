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

    private static QuarryManager mgr = Find.Map.GetComponent<QuarryManager>();

    // autoHaul defaults to true
    public bool autoHaul = true;
    // Trackers for the resources gathered here
    public int chunkTracker;
    public int resourceTracker;
    public int blockTracker;

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
      Scribe_Values.LookValue(ref autoHaul, "QRY_autoHaul");
      Scribe_Values.LookValue(ref chunkTracker, "QRY_Chunks", 0);
      Scribe_Values.LookValue(ref resourceTracker, "QRY_Resources", 0);
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


    public override void SpawnSetup() {
      base.SpawnSetup();

      // Tell the quarry manager to find the available resources
      // This is called when a quarry is built, or on game load
      mgr.FindResources();
    }


    public void ResourceMined(QuarryItemType item) {
      if (item == QuarryItemType.Chunk) {
        chunkTracker++;
      }
      if (item == QuarryItemType.Resource) {
        resourceTracker++;
      }
      if (item == QuarryItemType.Block) {
        blockTracker++;
      }
    }


    public override void DeSpawn() {
      base.DeSpawn();

      // Give back the materials used for making the steps,
      // this doesn't support xml alteration
      Thing placedProduct = ThingMaker.MakeThing(ThingDefOf.WoodLog);
      placedProduct.stackCount = Random.Range(60, 81);
      GenPlace.TryPlaceThing(placedProduct, Position, ThingPlaceMode.Direct);

      // Tell the quarry manager to deconstruct the quarry
      mgr.DeconstructQuarry();
    }
  }
}
