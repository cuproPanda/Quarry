using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod {

    private int vanillaTracker = 0;
    private int moddedTracker = 0;


    public QuarryMod(ModContentPack mcp) : base(mcp) {
      GetSettings<QuarrySettings>();
      LongEventHandler.ExecuteWhenFinished(BuildResourceList);
      LongEventHandler.ExecuteWhenFinished(Echo);
    }


    public override void WriteSettings() {
      base.WriteSettings();
    }


    private void BuildResourceList() {
      QuarrySettings.database = DefDatabase<ThingDef>.AllDefsListForReading;
      List<QuarryResource> resources = new List<QuarryResource>();

      // Add vanilla resources
      foreach (SimpleQuarryResource resource in QuarryDefOf.MainResources.Resources) {
        ThingDef tmpThing = QuarrySettings.database.Find(t => t.defName == resource.thingDef);
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
        ThingDef tmpThing = QuarrySettings.database.Find(t => t.defName == resource.thingDef);
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
        ThingDef tmpThing = QuarrySettings.database.Find(t => t.defName == resource.thingDef);
        if (tmpThing != null) {
          moddedTracker++;
          resources.Add(new QuarryResource(
            tmpThing,
            resource.probability,
            resource.stackCount));
        }
      }
      QuarrySettings.resources = resources;
    }


    private void Echo() {
      // I'm keeping this since it might prove useful in the future for user errors
      Log.Message("Quarry:: Loaded " + vanillaTracker + " vanilla and " + moddedTracker + " modded entries into resource list.");
    }


    public override string SettingsCategory() {
      return Static.Quarry;
    }


    public override void DoSettingsWindowContents(Rect rect) {
      Listing_Standard list = new Listing_Standard();

      list.ColumnWidth = rect.width;
      list.Begin(rect);
      list.Gap(10);
      {
        Rect fullRect = list.GetRect(Text.LineHeight);
        Rect leftRect = fullRect.LeftHalf().Rounded();
        Rect rightRect = fullRect.RightHalf().Rounded();

        if (QuarrySettings.quarryMaxHealth <= 10000) {
          Widgets.Label(leftRect, "QRY_DepletionLabel".Translate(QuarrySettings.quarryMaxHealth.ToString("N0")));
        }
        else {
          Widgets.Label(leftRect, "QRY_DepletionLabel".Translate("Infinite"));
        }

        //Increment timer value by -100 (button).
        if (Widgets.ButtonText(new Rect(rightRect.xMin, rightRect.y, rightRect.height, rightRect.height), "-", true, false, true)) {
          if (QuarrySettings.quarryMaxHealth >= 200) {
            QuarrySettings.quarryMaxHealth -= 100;
          }
        }

        QuarrySettings.quarryMaxHealth = RoundToAsInt(100, Widgets.HorizontalSlider(
          new Rect(rightRect.xMin + rightRect.height + 10f, rightRect.y, rightRect.width - ((rightRect.height * 2) + 20f),rightRect.height),
          QuarrySettings.quarryMaxHealth, 100f, 10100f, true));

        //Increment timer value by +100 (button).
        if (Widgets.ButtonText(new Rect(rightRect.xMax - rightRect.height, rightRect.y, rightRect.height, rightRect.height), "+", true, false, true)) {
          if (QuarrySettings.quarryMaxHealth < 10100) {
            QuarrySettings.quarryMaxHealth += 100;
          }
        }

        list.Gap(25);

        {
          Rect letterRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();

          Widgets.CheckboxLabeled(letterRect, Static.LetterSent, ref QuarrySettings.letterSent);
          if (Mouse.IsOver(letterRect)) {
            Widgets.DrawHighlight(letterRect);
          }
          TooltipHandler.TipRegion(letterRect, Static.ToolTipLetter);
        }

        list.Gap(25);

        {
          Rect junkRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();
          Rect junkSliderOffset = junkRect.RightHalf().Rounded().RightPartPixels(200);

          Widgets.Label(junkRect, "QRY_SettingsJunkChance".Translate(QuarrySettings.junkChance));
          QuarrySettings.junkChance = RoundToAsInt(5, Widgets.HorizontalSlider(
          junkSliderOffset,
          QuarrySettings.junkChance, 10f, 90f, true));
          if (Mouse.IsOver(junkRect)) {
            Widgets.DrawHighlight(junkRect);
          }
          TooltipHandler.TipRegion(junkRect, Static.ToolTipJunkChance);

        }

        list.Gap(25);

        {
          Rect chunkRect = list.GetRect(Text.LineHeight).LeftHalf().Rounded();
          Rect chunkSliderOffset = chunkRect.RightHalf().Rounded().RightPartPixels(200);

          Widgets.Label(chunkRect, "QRY_SettingsChunkChance".Translate(QuarrySettings.chunkChance));
          QuarrySettings.chunkChance = RoundToAsInt(5, Widgets.HorizontalSlider(
          chunkSliderOffset,
          QuarrySettings.chunkChance, 10f, 90f, true));
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
