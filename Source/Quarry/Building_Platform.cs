using UnityEngine;
using RimWorld;
using Verse;

namespace Quarry {

	public class Building_Platform : Building_Storage {

		private Graphic cachedGraphic = null;

		// Graphic_Appearances needs an atlased texture to function properly,
		// and rewriting it to use single textures didn't work as expected. Thus:
		public override Graphic Graphic {
			get {
				if (cachedGraphic == null) {
					Color colorOne = def.graphicData.color;
					Color colorTwo = def.graphicData.colorTwo;
					Graphic graphic = Static.Platform_Smooth;

					if (Stuff != null && Stuff.stuffProps != null) {
						colorOne = Stuff.stuffProps.color;

						if (Stuff.stuffProps.appearance != null) {
							if (Stuff.stuffProps.appearance == QuarryDefOf.Bricks) {
								graphic = Static.Platform_Bricks;
								goto Set;
							}
							else if (Stuff.stuffProps.appearance == QuarryDefOf.Planks) {
								graphic = Static.Platform_Planks;
								goto Set;
							}
							else if (Stuff.stuffProps.appearance.defName == Static.StringGraniticStone) {
								graphic = Static.Platform_GraniticStone;
								goto Set;
							}
							else if (Stuff.stuffProps.appearance.defName == Static.StringRockyStone) {
								graphic = Static.Platform_RockyStone;
								goto Set;
							}
							else if (Stuff.stuffProps.appearance.defName == Static.StringSmoothStone) {
								graphic = Static.Platform_SmoothStone;
								goto Set;
							}
						} 
					}

					Set:
					cachedGraphic = graphic.GetColoredVersion(ShaderDatabase.DefaultShader, colorOne, colorTwo); 
				}
				return cachedGraphic;
			}
		}
	}
}
