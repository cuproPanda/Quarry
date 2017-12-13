using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace Quarry {

  public static class OreDictionary {

    private static System.Random rand = new System.Random();
		private static SimpleCurve commonalityCurve = new SimpleCurve {
			{ new CurvePoint(0.0f, 10f) },
			{ new CurvePoint(0.02f, 9f) },
			{ new CurvePoint(0.04f, 8f) },
			{ new CurvePoint(0.06f, 6f) },
			{ new CurvePoint(0.08f, 3f) },
			{ new CurvePoint(float.MaxValue, 1f) }
		};
		private static Predicate<ThingDef> validOre = ((ThingDef def) => def.mineable && def != QuarryDefOf.MineableComponents && def.building != null && def.building.isResourceRock && def.building.mineableThing != null);


		public static void Build() {
			Dictionary<ThingDef, int> oreDictionary = new Dictionary<ThingDef, int>();
			// Get all ThingDefs that have mineable resources
			IEnumerable<ThingDef> ores = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => validOre(def));
			
			// Assign commonality values for ores
			foreach (ThingDef ore in ores) {				
				oreDictionary.Add(ore.building.mineableThing, ValueForMineableOre(ore));
			}
			// Include AdditionalResources.xml resources
			foreach (KeyValuePair<ThingDef, int> pair in DefDatabase<QuarryResourcesDef>.GetNamedSilentFail("AdditionalResources").additionalResources) {
				int value = pair.Value;

				if (oreDictionary.ContainsKey(pair.Key)) {
					oreDictionary[pair.Key] += value;
				}
				else {
					oreDictionary.Add(pair.Key, value);
				}
			}
			// Assign this dictionary for the mod to use
			QuarryMod.oreDictionary = PercentageDictionary(oreDictionary);
		}


		public static int ValueForMineableOre(ThingDef def) {
			if (!validOre(def)) {
				Log.Error($"{Static.Quarry}:: Unable to process def {def.LabelCap} as a mineable resource rock.");
				return 0;
			}
			return (int)(((((def.building.mineableThing.deepCommonality < 1.5f) ? def.building.mineableThing.deepCommonality : 1.5f) 
				* (def.building.mineableScatterCommonality * commonalityCurve.Evaluate(def.building.mineableScatterCommonality))) * 50) 
				/ ((def.building.mineableThing.BaseMarketValue < 2f) ? 2f : (def.building.mineableThing.BaseMarketValue / 5f)));
		}


		public static Dictionary<ThingDef, int> PercentageDictionary(Dictionary<ThingDef, int> dictionary) {
			Dictionary<ThingDef, int> dict = new Dictionary<ThingDef, int>();
			float sum = 0;

			foreach (KeyValuePair<ThingDef, int> pair  in dictionary) {
				sum += pair.Value;
			}
			foreach (KeyValuePair<ThingDef, int> pair in dictionary) {
				dict.Add(pair.Key, (int)((pair.Value / sum) * 100f));
			}
			return dict;
		}


    public static ThingDef TakeOne() {
			// Make sure there is a dictionary to work from
			if (QuarryMod.oreDictionary == null) {
				Build();
			}

			// Sorts the weight list
			List<KeyValuePair<ThingDef, int>> sortedWeights = Sort(QuarryMod.oreDictionary);

      // Sums all weights
      int sum = 0;
      foreach (KeyValuePair<ThingDef, int> ore in QuarryMod.oreDictionary) {
        sum += ore.Value;
      }

      // Randomizes a number from Zero to Sum
      int roll = rand.Next(0, sum);

      // Finds chosen item based on weight
      ThingDef selected = sortedWeights[sortedWeights.Count - 1].Key;
      foreach (KeyValuePair<ThingDef, int> ore in sortedWeights) {
        if (roll < ore.Value) {
          selected = ore.Key;
          break;
        }
        roll -= ore.Value;
      }

      // Returns the selected item
      return selected;
    }


    private static List<KeyValuePair<ThingDef, int>> Sort(Dictionary<ThingDef, int> weights) {
			List<KeyValuePair<ThingDef, int>> list = new List<KeyValuePair<ThingDef, int>>(weights);

      // Sorts the Weights List for randomization later
      list.Sort(
          delegate (KeyValuePair<ThingDef, int> firstPair,
										KeyValuePair<ThingDef, int> nextPair) {
                    return firstPair.Value.CompareTo(nextPair.Value);
                   }
       );

      return list;
    }
  }
}
