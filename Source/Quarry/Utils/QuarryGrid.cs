using UnityEngine;
using Verse;

namespace Quarry {

	public sealed class QuarryGrid : MapComponent, ICellBoolGiver, IExposable {

		private BoolGrid boolGrid;
		private CellBoolDrawer drawer;

		public Color Color => Color.green;
		public bool GetCellBool(int index) => boolGrid[index];
		public bool GetCellBool(IntVec3 c) => boolGrid[c];
		public Color GetCellExtraColor(int index) => Color.white;

		private CellBoolDrawer Drawer {
			get {
				if (drawer == null) {
					drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
				}
				return drawer;
			}
		}


		public QuarryGrid(Map map) : base(map) {
			boolGrid = new BoolGrid(map);
		}


		public override void FinalizeInit() {
			base.FinalizeInit();
			// Create a new boolGrid - this also gets called for old saves where
			// there wasn't a QuarryGrid present
			if (boolGrid.TrueCount == 0) {
				ProcessBoolGrid();
			}
		}


		private void ProcessBoolGrid() {
			foreach (IntVec3 c in map.AllCells) {
				boolGrid.Set(c, QuarryUtility.IsValidQuarryTerrain(map.terrainGrid.TerrainAt(c)));
			}
			Drawer.SetDirty();
		}


		public void RemoveFromGrid(CellRect rect) {
			foreach (IntVec3 c in rect.Cells) {
				boolGrid.Set(c, false);
			}
			Drawer.SetDirty();
		}


		public void AddToGrid(CellRect rect) {
			foreach (IntVec3 c in rect.Cells) {
				boolGrid.Set(c, true);
			}
			Drawer.SetDirty();
		}


		public override void MapComponentUpdate() {
			base.MapComponentUpdate();
			Drawer.CellBoolDrawerUpdate();
		}


		public override void ExposeData() {
			Scribe_Deep.Look(ref boolGrid, "boolGrid", new object[0]);
		}


		public void MarkForDraw() {
			if (map == Find.VisibleMap) {
				Drawer.MarkForDraw();
			}
		}
	}
}
