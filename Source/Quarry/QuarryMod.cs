using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod, IExposable {

    private List<ThingDef> database;
    private List<QuarryResource> resources;
    private int quarryMaxHealth = -1;
    private int cachedMaxHealth = -1;
    private int vanillaTracker = 0;
    private int moddedTracker = 0;
    private int junkChance = 70;
    private int chunkChance = 50;
    private bool letterSent = false;

    public static List<QuarryResource> Resources {
      get { return Instance.resources; }
    }
    public static List<ThingDef> Database {
      get { return Instance.database; }
    }

    public static bool LetterSent {
      get { return Instance.letterSent; }
    }

    public static int QuarryMaxHealth {
      get {
        if (Instance.quarryMaxHealth > 10000) {
          return int.MaxValue;
        }
        return Instance.quarryMaxHealth;
      }
    }

    public static bool SettingsChanged {
      get {
        if (Instance.cachedMaxHealth == -1) {
          return false;
        }
        if (Instance.cachedMaxHealth == Instance.quarryMaxHealth) {
          return false;
        }
        return true;
      }
    }

    public static float JunkChance {
      get { return Instance.junkChance / 100f; }
    }

    public static float ChunkChance {
      get { return Instance.chunkChance / 100f; }
    }

    public static QuarryMod Instance { get; private set; }


    public QuarryMod(ModContentPack mcp) : base(mcp) {
      Instance = this;
      quarryMaxHealth = (quarryMaxHealth == -1) ? 2000 : quarryMaxHealth;
      LongEventHandler.ExecuteWhenFinished(BuildResourceList);
      LongEventHandler.ExecuteWhenFinished(Echo);
    }


    public void ExposeData() {
      Scribe_Values.Look(ref letterSent, "QRY_letterSent", false);
      Scribe_Values.Look(ref quarryMaxHealth, "QRY_quarryMaxHealth", 2000);
      Scribe_Values.Look(ref junkChance, "QRY_junkChance", 70);
      Scribe_Values.Look(ref chunkChance, "QRY_chunkChance", 50);
    }


    private void BuildResourceList() {
      database = DefDatabase<ThingDef>.AllDefsListForReading;
      resources = new List<QuarryResource>();

      // Add vanilla resources
      foreach (SimpleQuarryResource resource in QuarryDefOf.MainResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          vanillaTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
      // Add resources from my mods
      foreach (SimpleQuarryResource resource in QuarryDefOf.CuproResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          moddedTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
      // Add other modded resources
      foreach (SimpleQuarryResource resource in QuarryDefOf.ModdedResources.Resources) {
        ThingDef tmpThing = database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          moddedTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
    }


    private void Echo() {
      // I'm keeping this since it might prove useful in the future for user errors
      Log.Message("Quarry:: Loaded " + vanillaTracker + " vanilla and " + moddedTracker + " modded entries into resource list.");
    }


    public void Notify_LetterSent() {
      Instance.letterSent = true;
    }


    public override string SettingsCategory() {
      return Static.Quarry;
    }


    public override void DoSettingsWindowContents(Rect rect) {
      Listing_Standard list = new Listing_Standard();
      cachedMaxHealth = quarryMaxHealth;
      list.ColumnWidth = rect.width;
      list.Begin(rect);
      list.Gap(10);
      {
        Rect fullRect = list.GetRect(Text.LineHeight);
        Rect leftRect = fullRect.LeftHalf().Rounded();
        Rect rightRect = fullRect.RightHalf().Rounded();

        if (quarryMaxHealth <= 10000) {
          Widgets.Label(leftRect, "QRY_DepletionLabel".Translate(quarryMaxHealth.ToString("N0")));
        }
        else {
          Widgets.Label(leftRect, "QRY_DepletionLabel".Translate("Infinite"));
        }

        //Increment timer value by -100 (button).
        if (Widgets.ButtonText(new Rect(rightRect.xMin, rightRect.y, rightRect.height, rightRect.height), "-", true, false, true)) {
          if (quarryMaxHealth >= 200) {
            quarryMaxHealth -= 100;
          }
        }

        quarryMaxHealth = RoundToAsInt(100, Widgets.HorizontalSlider(
          new Rect(rightRect.xMin + rightRect.height + 10f, rightRect.y, rightRect.width - ((rightRect.height * 2) + 20f),rightRect.height),
          quarryMaxHealth, 100f, 10100f, true));

        //Increment timer value by +100 (button).
        if (Widgets.ButtonText(new Rect(rightRect.xMax - rightRect.height, rightRect.y, rightRect.height, rightRect.height), "+", true, false, true)) {
          if (quarryMaxHealth < 10100) {
            quarryMaxHealth += 100;
          }
        }

        list.Gap(25);

        {
          Rect letterRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();

          Widgets.CheckboxLabeled(letterRect, Static.LetterSent, ref letterSent);
          if (Mouse.IsOver(letterRect)) {
            Widgets.DrawHighlight(letterRect);
          }
          TooltipHandler.TipRegion(letterRect, Static.ToolTipLetter);
        }

        list.Gap(25);

        {
          Rect junkRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();
          Rect junkSliderOffset = junkRect.RightHalf().Rounded().RightPartPixels(200);

          Widgets.Label(junkRect, "QRY_SettingsJunkChance".Translate(junkChance));
          junkChance = RoundToAsInt(5, Widgets.HorizontalSlider(
          junkSliderOffset,
          junkChance, 10f, 90f, true));
          if (Mouse.IsOver(junkRect)) {
            Widgets.DrawHighlight(junkRect);
          }
          TooltipHandler.TipRegion(junkRect, Static.ToolTipJunkChance);

        }

        list.Gap(25);

        {
          Rect chunkRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();
          Rect chunkSliderOffset = chunkRect.RightHalf().Rounded().RightPartPixels(200);

          Widgets.Label(chunkRect, "QRY_SettingsChunkChance".Translate(chunkChance));
          chunkChance = RoundToAsInt(5, Widgets.HorizontalSlider(
          chunkSliderOffset,
          chunkChance, 10f, 90f, true));
          if (Mouse.IsOver(chunkRect)) {
            Widgets.DrawHighlight(chunkRect);
          }
          TooltipHandler.TipRegion(chunkRect, Static.ToolTipChunkChance);

        }

        list.Gap(75);
        {
          Vector2 rbCenter = list.GetRect(Text.LineHeight).center;
          Rect rbRect = new Rect(rbCenter.x - 100, rbCenter.x + 100, 200, 30);
          // Only allows opening the resource folder on windows. I should be able to support linux and mac in the future
          if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && Widgets.ButtonText(rbRect, "Open Resource Defs Folder")) {
            Application.OpenURL(ModsConfig.ActiveModsInLoadOrder.Single(m => m.Name == "Quarry").RootDir.ToString() + "/Defs/QuarryResourceDefs");
          }

        }

        list.Gap(10);

        list.End();
      }
    }


    private int RoundToAsInt(int factor, float num) {
      return (int)(Math.Round(num / (double)factor, 0) * factor);
    }
  }
}
