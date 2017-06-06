using UnityEngine;
using Verse;

namespace Quarry {
  [StaticConstructorOnStartup]
  public static class Static {

    public static Texture2D DesignationQuarryResources = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/Quarry", false);
    public static Texture2D DesignationQuarryBlocks = ContentFinder<Texture2D>.Get("Cupro/UI/Designators/QuarryBlocks", false);
    public static Texture2D DesignationHaul = ContentFinder<Texture2D>.Get("UI/Designators/Haul");

    public static string LabelHaul = "QRY_Haul".Translate();
    public static string LabelNotHaul = "QRY_NotHaul".Translate();
    public static string LabelMineResources = "QRY_DesignatorMine".Translate();
    public static string DescriptionMineResources = "QRY_DesignatorMineDesc".Translate();
    public static string LabelMineBlocks = "QRY_DesignatorMineBlocks".Translate();
    public static string DescriptionMineBlocks = "QRY_DesignatorMineBlocksDesc".Translate();
    public static string LabelHaulMode = "QRY_Mode".Translate();
    public static string ReportNotEnoughStone = "QRY_NotEnoughStone".Translate();
  }
}
