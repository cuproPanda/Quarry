using System;

using UnityEngine;
using Verse;

namespace Quarry {
  [StaticConstructorOnStartup]
  public static class Static {

    public static Texture2D DesignationQuarryResources = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/QuarryResources", false);
    public static Texture2D DesignationQuarryBlocks = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/QuarryBlocks", false);
    public static Texture2D DesignationReclaimSoil = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/ReclaimSoil", false);
		public static Texture2D DesignationHaul = ContentFinder<Texture2D>.Get("UI/Designators/Haul");

		public static Graphic Platform_Bricks = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_Bricks", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);
		public static Graphic Platform_GraniticStone = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_GraniticStone", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);
		public static Graphic Platform_Planks = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_Planks", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);
		public static Graphic Platform_RockyStone = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_RockyStone", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);
		public static Graphic Platform_Smooth = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_Smooth", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);
		public static Graphic Platform_SmoothStone = GraphicDatabase.Get<Graphic_Single>("Cupro/Object/Platform/Platform_SmoothStone", ShaderDatabase.DefaultShader, new Vector2(3,3), Color.white);

		public static IntVec3 LadderOffset_Big1 = new IntVec3(-3,0,5);
    public static IntVec3 LadderOffset_Big2 = new IntVec3(-3,0,6);
    public static IntVec3 LadderOffset_Big3 = new IntVec3(4,0,5);
    public static IntVec3 LadderOffset_Big4 = new IntVec3(4,0,6);
    public static IntVec3 LadderOffset_Small = new IntVec3(0,0,3);

		public static string Quarry = "QRY_Quarry".Translate();

    public static string LabelHaulMode = "QRY_LabelHaulMode".Translate();
    public static string LabelHaul = "QRY_Haul".Translate();
    public static string LabelNotHaul = "QRY_NotHaul".Translate();
		public static string LabelDictionary = "QRY_DictionaryLabel".Translate();
		public static string LabelMineResources = "QRY_LabelMineResources".Translate();
		public static string LabelAddThing = "QRY_LabelAddThing".Translate();
		public static string LabelRemoveThing = "QRY_LabelRemoveThing".Translate();
		public static string LabelResetList = "QRY_LabelResetList".Translate();
    public static string LabelMineBlocks = "QRY_LabelMineBlocks".Translate();
		public static string LabelReclaimSoil = "QRY_LabelReclaimSoil".Translate();

		public static string DescriptionMineResources = "QRY_DescriptionMineResources".Translate();
    public static string DescriptionMineBlocks = "QRY_DescriptionMineBlocks".Translate();
		public static string DescriptionReclaimSoil = "QRY_DescriptionReclaimSoil".Translate();

    public static string LetterLabel = "QRY_LetterLabel".Translate();
    public static string LetterText = "QRY_LetterText".Translate();
    public static string LetterSent = "QRY_LetterSentQuery".Translate();

    public static string ToolTipLetter = "QRY_TooltipLetter".Translate();
    public static string ToolTipJunkChance = "QRY_TooltipJunkChance".Translate();
    public static string ToolTipChunkChance = "QRY_TooltipChunkChance".Translate();

    public static string TextMote_LargeVein = "QRY_TextMote_LargeVein".Translate();
    public static string TextMote_MiningFailed = "QRY_TextMote_MiningFailed".Translate();

		public static string StringGraniticStone = "GraniticStone";
		public static string StringRockyStone = "RockyStone";
		public static string StringSmoothStone = "SmoothStone";

    public static string ReportNotEnoughStone = "QRY_ReportNotEnoughStone".Translate();
    public static string InspectQuarryPercent = "QRY_InspectQuarryPercent".Translate();
    public static string SettingsDepletionPercent = "QRY_SettingsDepletionPercent".Translate();
		public static string MessageMustDesignateQuarriedGround = "QRY_MessageMustDesignateQuarriedGround".Translate();
		public static string CommandBedSetOwnerLabel = "CommandBedSetOwnerLabel".Translate();
		public static string CommandSetOwnerDesc = "QRY_CommandSetOwnerDesc".Translate();
		public static string ReportGizmoLackingResearch = "QRY_ReportGizmoLackingResearch".Translate();
	}
}
