using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace Quarry {

  public class JobDriver_MineQuarry : JobDriver {

    public const int BaseTicksBetweenPickHits = 120;
    private int ticksToPickHit = -1000;
    private Effecter effecter;


//    bool gotOre = false;
//if (quarry.quarryResources) {
//    gotOre = Rand.Flip;
//}


  protected override IEnumerable<Toil> MakeNewToils() {

      Building_Quarry quarry = TargetThingA as Building_Quarry;

      // Set up fail conditions
      this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
      this.FailOn(delegate {
        return quarry == null || (!quarry.quarryResources && !quarry.quarryBlocks);
      });

      // Go to the quarry
      yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);

      // Mine at the quarry. This is only for the delay
      Toil quarryToil = new Toil();
      quarryToil.tickAction = delegate {

        if (ticksToPickHit < -100) {
          ResetTicksToPickHit();
        }
        if (pawn.skills != null) {
          pawn.skills.Learn(SkillDefOf.Mining, 0.11f, false);
        }
        ticksToPickHit--;
        if (ticksToPickHit <= 0) {
          if (effecter == null) {
            effecter = EffecterDefOf.Mine.Spawn();
          }
          effecter.Trigger(pawn, quarry);

          ResetTicksToPickHit();
        }
      };
      quarryToil.defaultCompleteMode = ToilCompleteMode.Delay;
      quarryToil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
      float skillFactor = pawn.skills.GetSkill(SkillDefOf.Mining).Level / 20f;
      quarryToil.defaultDuration = (int)(3000 * Mathf.Lerp(1.5f, 0.5f, skillFactor));
      yield return quarryToil;

      // Collect resources from the quarry
      Toil resourceToil = new Toil();
      resourceToil.initAction = delegate {

        pawn.records.Increment(RecordDefOf.CellsMined);
        QuarryResource product = quarry.GenProduct();

        Thing haulableResult = ThingMaker.MakeThing(product.thingDef);
        haulableResult.stackCount = product.stackCount;
        GenPlace.TryPlaceThing(haulableResult, pawn.Position, Map, ThingPlaceMode.Near);

        if (product.largeVein) {
          MoteMaker.ThrowText(haulableResult.DrawPos, Map, "QRY_TextMote_LargeVein".Translate(), 180);
        }

        StoragePriority storagePriority = HaulAIUtility.StoragePriorityAtFor(haulableResult.Position, haulableResult);
        IntVec3 c;

        // Try to find a suitable storage spot for the resource
        if (quarry.autoHaul && StoreUtility.TryFindBestBetterStoreCellFor(haulableResult, pawn, Map, storagePriority, pawn.Faction, out c)) {
          CurJob.SetTarget(TargetIndex.B, haulableResult);
          CurJob.count = haulableResult.stackCount;
          CurJob.SetTarget(TargetIndex.C, c);
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Succeeded);
        }
      };
      resourceToil.defaultCompleteMode = ToilCompleteMode.Instant;

      // Collect blocks from the quarry
      Toil blocksToil = new Toil();
      blocksToil.initAction = delegate {
        pawn.records.Increment(RecordDefOf.CellsMined);
        QuarryResource product = new QuarryResource();

        if (Rand.Chance(QuarryDefOf.Resources.JunkChance)) {
          if (Rand.Chance(QuarryDefOf.Resources.ChunkChance)) {
            string rockType = quarry.RockTypes.RandomElement().ToString().Replace("Chunk", "");
            if (DefDatabase<ThingDef>.GetNamed("Blocks" + rockType, false) == null) {
              
              product.thingDef = ThingDefOf.BlocksGranite;
              product.stackCount = Rand.RangeInclusive(10, 20);
            }
            else {
              product.thingDef = DefDatabase<ThingDef>.GetNamed("Blocks" + rockType, false);
              product.stackCount = Rand.RangeInclusive(10, 20);
            }
          }
          else {
            product.thingDef = ThingDefOf.RockRubble;
            product.stackCount = 1;
          }
        }

        Thing haulableResult = ThingMaker.MakeThing(product.thingDef);
        haulableResult.stackCount = product.stackCount;
        GenPlace.TryPlaceThing(haulableResult, pawn.Position, Map, ThingPlaceMode.Near);

        StoragePriority storagePriority = HaulAIUtility.StoragePriorityAtFor(haulableResult.Position, haulableResult);
        IntVec3 c;

        // Try to find a suitable storage spot for the resource
        if (quarry.autoHaul && StoreUtility.TryFindBestBetterStoreCellFor(haulableResult, pawn, Map, storagePriority, pawn.Faction, out c)) {
          CurJob.SetTarget(TargetIndex.B, haulableResult);
          CurJob.count = haulableResult.stackCount;
          CurJob.SetTarget(TargetIndex.C, c);
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Succeeded);
        }
      };
      blocksToil.defaultCompleteMode = ToilCompleteMode.Instant;
      yield return blocksToil;

      // If both options are allowed, choose a random one
      if (quarry.quarryResources && quarry.quarryBlocks) {
        yield return Rand.Bool ? resourceToil : blocksToil;
      }
      // If only resources are allowed, get ore
      else if (quarry.quarryResources) {
        yield return resourceToil;
      }
      // otherwise, get blocks
      else {
        yield return blocksToil;
      }

      // Reserve the resource
      yield return Toils_Reserve.Reserve(TargetIndex.B);

      // Reserve the storage cell
      yield return Toils_Reserve.Reserve(TargetIndex.C);

      // Go to the resource
      yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);

      // Pick up the resource
      yield return Toils_Haul.StartCarryThing(TargetIndex.B);

      // Carry the resource to the storage cell, then place it down
      Toil carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
      yield return carry;
      yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true);
    }


    private void ResetTicksToPickHit() {
      float statValue = pawn.GetStatValue(StatDefOf.MiningSpeed, true);
      ticksToPickHit = Mathf.RoundToInt((120f / statValue));
    }
  }
}

