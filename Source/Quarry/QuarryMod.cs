using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace Quarry {

  public sealed class QuarryMod : Mod {

		public static char slash = System.IO.Path.DirectorySeparatorChar;
		private Vector2 scrollPosition = Vector2.zero;
		private float scrollViewHeight = 0f;


		public QuarryMod(ModContentPack mcp) : base(mcp) {
      LongEventHandler.ExecuteWhenFinished(GetSettings);
      LongEventHandler.ExecuteWhenFinished(PushDatabase);
			LongEventHandler.ExecuteWhenFinished(BuildDictionary);
		}


    public void GetSettings() {
			GetSettings<QuarrySettings>();
		}


		public override void WriteSettings() {
			base.WriteSettings();
		}


		private void PushDatabase() {
      QuarrySettings.database = DefDatabase<ThingDef>.AllDefsListForReading;
    }


		private void BuildDictionary() {
			if (QuarrySettings.oreDictionary == null) {
				OreDictionary.Build();
			}
		}


    public override string SettingsCategory() {
      return Static.Quarry;
    }


    public override void DoSettingsWindowContents(Rect rect) {
			Listing_Standard list = new Listing_Standard() {
				ColumnWidth = rect.width
			};

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
					Rect labelRect = list.GetRect(32).LeftHalf().Rounded();
					Text.Font = GameFont.Medium;
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(labelRect, Static.LabelDictionary);
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.UpperLeft;
				}

				list.Gap(1);
				{
					Rect listRect = list.GetRect(200f).LeftHalf().Rounded();
					Rect cRect = listRect.ContractedBy(10f);
					Rect position = new Rect(cRect.x, cRect.y, cRect.width, cRect.height);
					Rect outRect = new Rect(0f, 0f, position.width, position.height);
					Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);

					float num = 0f;
					List<ThingCountExposable> dict = new List<ThingCountExposable>(QuarrySettings.oreDictionary);

					GUI.BeginGroup(position);
					Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

					foreach (var tc in dict.Select((value, index) => new { index, value })) {
						Rect entryRect = new Rect(0f, num, viewRect.width, 32);
						Rect iconRect = entryRect.LeftHalf().LeftHalf().LeftPartPixels(32).Rounded();
						Rect labelRect = entryRect.LeftHalf().RightPartPixels(150).Rounded();
						Rect pctRect = labelRect.RightPartPixels(50).Rounded();
						Rect sliderRect = entryRect.RightHalf().Rounded();

						Widgets.ThingIcon(iconRect, tc.value.thingDef);
						Widgets.Label(labelRect, tc.value.thingDef.LabelCap);
						Widgets.Label(pctRect, $"{OreDictionary.WeightAsPercentage(QuarrySettings.oreDictionary, tc.value.count)}%");
						int val = tc.value.count;
						val = RoundToAsInt(1, Widgets.HorizontalSlider(
							sliderRect,
							QuarrySettings.oreDictionary[tc.index].count, 0f, 500f, true
						));
						if (val != QuarrySettings.oreDictionary[tc.index].count) {
							QuarrySettings.oreDictionary[tc.index].count = val;
						}

						num += 32f;
						scrollViewHeight = num;
					}

					Widgets.EndScrollView();
					GUI.EndGroup();
				}

				list.Gap(15);
				{
					Vector2 abCenter = list.GetRect(Text.LineHeight).center;
					Rect abRect = new Rect(abCenter.x - 100, abCenter.x + 10, 200, 30);

					if (Widgets.ButtonText(abRect, Static.LabelAddThing)) {
						List<FloatMenuOption> thingList = new List<FloatMenuOption>();
						foreach (ThingDef current in from t in Static.PossibleThingDefs()
																				 orderby t.label
																				 select t) {

							bool skip = false;
							for (int i = 0; i < QuarrySettings.oreDictionary.Count; i++) {
								if (QuarrySettings.oreDictionary[i].thingDef == current) {
									skip = true;
									break;
								}
							};
							if (skip)	continue;

							thingList.Add(new FloatMenuOption(current.LabelCap, delegate {
								QuarrySettings.oreDictionary.Add(new ThingCountExposable(current, 1));
							}));
						}
						FloatMenu menu = new FloatMenu(thingList);
						Find.WindowStack.Add(menu);
					}
				}

				list.Gap(15);
				{
					Vector2 rbCenter = list.GetRect(Text.LineHeight).center;
					Rect rbRect = new Rect(rbCenter.x - 100, rbCenter.x + 40, 200, 30);

					if (Widgets.ButtonText(rbRect, Static.LabelRemoveThing) && QuarrySettings.oreDictionary.Count >= 2) {
						List<FloatMenuOption> thingList = new List<FloatMenuOption>();
						foreach (ThingCountExposable current in from t in QuarrySettings.oreDictionary
																				 orderby t.thingDef.label
																				 select t) {
							ThingDef localTd = current.thingDef;
							thingList.Add(new FloatMenuOption(localTd.LabelCap, delegate {
								for (int i = 0; i < QuarrySettings.oreDictionary.Count; i++) {
									if (QuarrySettings.oreDictionary[i].thingDef == localTd) {
										QuarrySettings.oreDictionary.Remove(QuarrySettings.oreDictionary[i]);
										break;
									}
								};
							}));
						}
						FloatMenu menu = new FloatMenu(thingList);
						Find.WindowStack.Add(menu);
					}
				}

				list.Gap(15);
        {
          Vector2 lbCenter = list.GetRect(Text.LineHeight).center;
          Rect lbRect = new Rect(lbCenter.x - 100, lbCenter.x + 70, 200, 30);
          // Reset the dictionary
          if (Widgets.ButtonText(lbRect, Static.LabelResetList)) {
						OreDictionary.Build();
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
