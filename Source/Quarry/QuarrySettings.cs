using System.Collections.Generic;
using Verse;

namespace Quarry {

  public class QuarrySettings : ModSettings {

    internal static bool letterSent = false;
    internal static int quarryMaxHealth = 2000;
    internal static int junkChance = 70;
    internal static int chunkChance = 50;

    internal static List<ThingDef> database;
    internal static List<QuarryResource> resources;

    internal static int QuarryMaxHealth {
      get {
        if (quarryMaxHealth > 10000) {
          return int.MaxValue;
        }
        return quarryMaxHealth;
      }
    }


    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref letterSent, "QRY_letterSent", false);
      Scribe_Values.Look(ref quarryMaxHealth, "QRY_quarryMaxHealth", 2000);
      Scribe_Values.Look(ref junkChance, "QRY_junkChance", 70);
      Scribe_Values.Look(ref chunkChance, "QRY_chunkChance", 50);
    }
  }
}
