using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod {

    public static Dictionary<ThingDef, int> oreDictionary;
		public static char slash = System.IO.Path.DirectorySeparatorChar;


    public QuarryMod(ModContentPack mcp) : base(mcp) {
      GetSettings<QuarrySettings>();
      LongEventHandler.ExecuteWhenFinished(PushDatabase);
      LongEventHandler.ExecuteWhenFinished(BuildDictionary);
    }


    public override void WriteSettings() {
      base.WriteSettings();
    }


    private void PushDatabase() {
      QuarrySettings.database = DefDatabase<ThingDef>.AllDefsListForReading;
    }


    private void BuildDictionary() {
      oreDictionary = new Dictionary<ThingDef, int>();

      IEnumerable<ThingDef> ores = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.deepCommonality != 0);

      foreach (ThingDef ore in ores) {
        // Chemfuel shouldn't show up here, since it would be heavily exploited
        // Players can still choose to add it manually, though
        if (ore != QuarryDefOf.Chemfuel) {
          float commonality = ore.deepCommonality;
          if (commonality < 1f) {
            commonality += 1f;
          }
          oreDictionary.Add(ore, (int)((commonality * commonality) * 10f));
        }
      }

      foreach (KeyValuePair<ThingDef, int> pair in DefDatabase<QuarryResourcesDef>.GetNamedSilentFail("AdditionalResources").additionalResources) {
        if (oreDictionary.ContainsKey(pair.Key)) {
          oreDictionary[pair.Key] += pair.Value;
        }
        else {
          oreDictionary.Add(pair.Key, pair.Value);
        }
      }
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
          QuarrySettings.junkChance, 0f, 100f, true));
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
          QuarrySettings.chunkChance, 0f, 100f, true));
          if (Mouse.IsOver(chunkRect)) {
            Widgets.DrawHighlight(chunkRect);
          }
          TooltipHandler.TipRegion(chunkRect, Static.ToolTipChunkChance);
        }

        list.Gap(15);
        {
          Vector2 rbCenter = list.GetRect(Text.LineHeight).center;
          Rect rbRect = new Rect(rbCenter.x - 100, rbCenter.x + 40, 200, 30);
					if (Widgets.ButtonText(rbRect, "Open Resource Defs Folder")) {
						Application.OpenURL($"{ModsConfig.ActiveModsInLoadOrder.Single(m => m.Name == "Quarry").RootDir.ToString()}{slash}Defs{slash}AdditionalResources");
          }

        }

        list.Gap(15);
        {
          Vector2 lbCenter = list.GetRect(Text.LineHeight).center;
          Rect lbRect = new Rect(lbCenter.x - 100, lbCenter.x + 70, 200, 30);
          // Print the resources to the debug log
          if (Widgets.ButtonText(lbRect, "Show Resources in Log")) {
            var enumerator = oreDictionary.GetEnumerator();
            Log.Message("Quarry Resources: \n====================");
            for (int i = 0; i < oreDictionary.Count; i++) {
              enumerator.MoveNext();
              Log.Message(enumerator.Current.Key.LabelCap + " -- Weight: " + enumerator.Current.Value);
            }
            Log.TryOpenLogWindow();
          }
        }

        list.Gap(30);

        {
          Vector2 cbCenter = list.GetRect(Text.LineHeight).center;
          Rect cbRect = new Rect(cbCenter.x - 100, cbCenter.x + 100, 200, 30);
          // Copy the list of resources to the clipboard
          if (Widgets.ButtonText(cbRect, "Copy Resources to Clipboard")) {
            StringBuilder clipboard = new StringBuilder();
            var enumerator = oreDictionary.GetEnumerator();
            for (int i = 0; i < oreDictionary.Count; i++) {
              if (clipboard.Length == 0) {
                clipboard.AppendLine("Quarry Resources: \n====================");
              }
              enumerator.MoveNext();
              clipboard.AppendLine(enumerator.Current.Key.LabelCap + " -- Weight: " + enumerator.Current.Value);
              if (clipboard[clipboard.Length - 1] != '\n') {
                clipboard.AppendLine();
              }
            }
            GUIUtility.systemCopyBuffer = clipboard.ToString();
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
