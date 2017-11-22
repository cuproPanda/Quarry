using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace Quarry {

	public static class QuarryPlacementUtility {

		public static Predicate<TerrainDef> validator = ((TerrainDef t) => t == TerrainDefOf.Gravel || t.defName.EndsWith("_Rough") || t.defName.EndsWith("_RoughHewn") || t.defName.EndsWith("_Smooth"));


		public static IEnumerable<IntVec3> CalculateAcceptableCells(Map map) {
			foreach (IntVec3 c in map.AllCells) {
				if (validator(map.terrainGrid.TerrainAt(c))) {
					yield return c;
				}
			}
		}
	}
}
