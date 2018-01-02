using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace Quarry {

	public static class QuarryUtility {

		public static Predicate<TerrainDef> IsValidQuarryTerrain = (
			(TerrainDef t) => t == TerrainDefOf.Gravel || 
			t.defName.EndsWith("_Rough") || 
			t.defName.EndsWith("_RoughHewn") || 
			t.defName.EndsWith("_Smooth")
		);


		public static bool IsValidQuarryRock(string str) {
			if (QuarrySettings.database.NullOrEmpty()) {
				Log.Error("Quarry:: Trying to validate rock types before the database has been built.");
				return false;
			}
			// If there isn't a known chunk or blocks for this, it probably isn't a rock type and wouldn't work for spawning anyways
			// This allows Cupro's Stones to work, and any other mod that uses standard naming conventions for stones
			if (QuarrySettings.database.Find(t => t.defName == "Chunk" + str) != null &&
					QuarrySettings.database.Find(t => t.defName == "Blocks" + str) != null) {
				return true;
			}
			return false;
		}


		public static bool IsValidQuarryChunk(string str, out ThingDef chunk) {
			chunk = null;
			if (QuarrySettings.database.NullOrEmpty()) {
				Log.Error("Quarry:: Trying to validate chunks before the database has been built.");
				return false;
			}
			chunk = QuarrySettings.database.Find(t => t.defName == "Chunk" + str);
			if (chunk != null) {
				return true;
			}
			return false;
		}


		public static bool IsValidQuarryBlocks(string str, out ThingDef blocks) {
			blocks = null;
			if (QuarrySettings.database.NullOrEmpty()) {
				Log.Error("Quarry:: Trying to validate blocks before the database has been built.");
				return false;
			}
			blocks = QuarrySettings.database.Find(t => t.defName == "Blocks" + str);
			if (blocks != null) {
				return true;
			}
			return false;
		}


		public static IEnumerable<ThingDef> PossibleThingDefs() {
			return from d in DefDatabase<ThingDef>.AllDefs
						 where (d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop && !d.MadeFromStuff && d.GetCompProperties<CompProperties_Rottable>() == null)
						 select d;
		}
	}
}
