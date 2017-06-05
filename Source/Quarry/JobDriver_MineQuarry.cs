using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace Quarry {

  public class JobDriver_MineQuarry : JobDriver {

    private const int BaseTicksBetweenPickHits = 120;
    private const TargetIndex QuarryInd = TargetIndex.A;
    private const TargetIndex CellInd = TargetIndex.B;
    private const TargetIndex StorageCellInd = TargetIndex.C;
    private int ticksToPickHit = -1000;
    private Effecter effecter;

    protected Building_Quarry Quarry {
      get {
        return (Building_Quarry)CurJob.GetTarget(TargetIndex.A).Thing;
      }
    }

    protected Thing Haulable {
      get {
        if (CurJob.GetTarget(TargetIndex.B).HasThing) {
          return CurJob.GetTarget(TargetIndex.B).Thing;
        }
        Log.Warning("Quarry:: Trying to assign a haulable to a pawn, but TargetIndex.B hasn't been changed.");
        EndJobWith(JobCondition.Errored);
        return null;
      }
    }


    protected override IEnumerable<Toil> MakeNewToils() {

      // Set up fail conditions
      this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
      this.FailOn(delegate {
        return Quarry == null || (!Quarry.quarryResources && !Quarry.quarryBlocks);
      });

      // Reserve your spot in the quarry
      yield return Toils_Reserve.Reserve(CellInd);

      // Go to the quarry
      //TODO: Add ladder climbing mechanic
      yield return Toils_Goto.Goto(CellInd, PathEndMode.OnCell);

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


        //ResourceRequest req = ResourceRequest.None;

        //// If both options are allowed, give one of the two
        //if (Quarry.quarryResources && Quarry.quarryBlocks) {
        //  req = ResourceRequest.Random;
        //}
        //// If only resources are allowed, try to get resources
        //else if (Quarry.quarryResources) {
        //  req = ResourceRequest.Resources;
        //}
        //// If only blocks are allowed, try to get blocks
        //else if (Quarry.quarryBlocks) {
        //  req = ResourceRequest.Blocks;
        //}
        //Log.Message(Quarry.GiveResources(req).def.label);



        if (ticksToPickHit <= 0) {
          if (effecter == null) {
            effecter = EffecterDefOf.Mine.Spawn();
          }
          effecter.Trigger(pawn, Quarry);

          ResetTicksToPickHit();
        }
      };
      quarryToil.defaultCompleteMode = ToilCompleteMode.Delay;
      quarryToil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
      float skillFactor = pawn.skills.GetSkill(SkillDefOf.Mining).Level / 20f;
      quarryToil.defaultDuration = (int)(3000 * Mathf.Lerp(1.5f, 0.5f, skillFactor));
      yield return quarryToil;
      

      // Collect resources from the quarry
      Toil collect = new Toil();
      collect.initAction = delegate {
        
        pawn.records.Increment(RecordDefOf.CellsMined);

        ResourceRequest req = ResourceRequest.None;
        
        // If both options are allowed, give one of the two
        if (Quarry.quarryResources && Quarry.quarryBlocks) {
          req = ResourceRequest.Random;
        }
        // If only resources are allowed, try to get resources
        else if (Quarry.quarryResources) {
          req = ResourceRequest.Resources;
        }
        // If only blocks are allowed, try to get blocks
        else if (Quarry.quarryBlocks) {
          req = ResourceRequest.Blocks;
        }

        Thing haulableResult = Quarry.GiveResources(req);
        
        GenPlace.TryPlaceThing(haulableResult, pawn.Position, Map, ThingPlaceMode.Direct);

        if (haulableResult.def.designateHaulable && Quarry.autoHaul) {
          Map.designationManager.AddDesignation(new Designation(haulableResult, DesignationDefOf.Haul));
        }

        StoragePriority storagePriority = HaulAIUtility.StoragePriorityAtFor(haulableResult.Position, haulableResult);
        IntVec3 c;

        // Try to find a suitable storage spot for the resource
        // TODO: Prioritize the quarry platforms
        if (Quarry.autoHaul && StoreUtility.TryFindBestBetterStoreCellFor(haulableResult, pawn, Map, storagePriority, pawn.Faction, out c)) {
          CurJob.SetTarget(TargetIndex.B, haulableResult);
          CurJob.count = haulableResult.stackCount;
          CurJob.SetTarget(TargetIndex.C, c);
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Succeeded);
        }
      };
      collect.defaultCompleteMode = ToilCompleteMode.Instant;

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

