using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;

namespace Quarry {
  // TODO: Change graphic based on resources mined, simulating digging deeper
  public class Quarry_Base : Building {

    private static QuarryManager mgr = Find.Map.GetComponent<QuarryManager>();

    // autoHaul defaults to true
    public bool AutoHaul = true;
    // Trackers for the resources gathered here
    public int ChunkTracker;
    public int ResourceTracker;

    private string description {
      get {
        if (AutoHaul) {
          return "QRY_Haul".Translate();
        }
        return "QRY_NotHaul".Translate();
      }
    }


    // Handle loading
    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.LookValue(ref AutoHaul, "QRY_autoHaul");
      Scribe_Values.LookValue(ref ChunkTracker, "QRY_Chunks", 0);
      Scribe_Values.LookValue(ref ResourceTracker, "QRY_Resources", 0);
    }


    public override IEnumerable<Gizmo> GetGizmos() {
      Command_Toggle haul = new Command_Toggle() {

        icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul", false),
        defaultDesc = description,
        hotKey = KeyBindingDefOf.Misc12,
        activateSound = SoundDef.Named("Click"),
        isActive = () => AutoHaul,
        toggleAction = () => { AutoHaul = !AutoHaul; },
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
        ChunkTracker++;
      }
      if (item == QuarryItemType.Resource) {
        ResourceTracker++;
      }
    }


    public override void DeSpawn() {
      base.DeSpawn();
      // Tell the quarry manager to deconstruct the quarry
      mgr.DeconstructQuarry();
    }
  }
}
