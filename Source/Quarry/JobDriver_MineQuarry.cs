using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace Quarry {

  public class JobDriver_MineQuarry : JobDriver {

    private const int BaseTicksBetweenPickHits = 120;
    private const TargetIndex CellInd = TargetIndex.A;
    private const TargetIndex HaulableInd = TargetIndex.B;
    private const TargetIndex StorageCellInd = TargetIndex.C;
    private int ticksToPickHit = -1000;
    private Effecter effecter;
    private Building_Quarry quarryBuilding = null;

    protected Building_Quarry Quarry {
      get {
        if (quarryBuilding == null) {
          quarryBuilding = CurJob.GetTarget(TargetIndex.A).Cell.GetThingList(Map).Find(q => q is Building_Quarry) as Building_Quarry;
        }
        return quarryBuilding;
      }
    }

    protected Thing Haulable {
      get {
        if (TargetB.Thing != null) {
          return TargetB.Thing;
        }
        Log.Warning("Quarry:: Trying to assign a haulable to a pawn, but TargetB hasn't been changed.");
        EndJobWith(JobCondition.Errored);
        return null;
      }
    }


    protected override IEnumerable<Toil> MakeNewToils() {

      // Set up fail conditions
      this.FailOn(delegate {
        return Quarry == null || Quarry.IsForbidden(pawn) || Quarry.Depleted;
      });

      // Notify the quarry the worker intends to work here
      yield return BeginWork();

      // Reserve your spot in the quarry
      yield return Toils_Reserve.Reserve(CellInd);

      // Go to the quarry
      yield return Toils_Goto.Goto(CellInd, PathEndMode.OnCell);

      // Mine at the quarry. This is only for the delay
      yield return Mine();

      // Collect resources from the quarry
      yield return Collect();

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


    private Toil BeginWork() {
      Toil toil = new Toil();
      toil.initAction = delegate {
        // Notify the quarry that there is another worker working here
        Quarry.Notify_WorkerStarting();
      };
      toil.defaultCompleteMode = ToilCompleteMode.Instant;
      return toil;
    }


    private Toil Mine() {
      Toil toil = new Toil();
      toil.tickAction = delegate {
        pawn.Drawer.rotator.Face(Quarry.Position.ToVector3Shifted());

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
          effecter.Trigger(pawn, Quarry);

          ResetTicksToPickHit();
        }
      };
      toil.defaultCompleteMode = ToilCompleteMode.Delay;
      toil.handlingFacing = true;
      toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
      float skillFactor = pawn.skills.GetSkill(SkillDefOf.Mining).Level / 20f;
      toil.defaultDuration = (int)(3000 * Mathf.Lerp(1.5f, 0.5f, skillFactor));
      toil.AddFinishAction(delegate {
        // Notify the quarry that the worker is done working.
        Quarry.Notify_WorkerReleased();
      });
      return toil;
    }

    private Toil Collect() {
      Toil toil = new Toil();
      toil.initAction = delegate {
        // Increment the record for how many cells this pawn has mined since this counts as mining
        pawn.records.Increment(RecordDefOf.CellsMined);

        // Start with None to act as a fallback. Rubble will be returned with this parameter
        ResourceRequest req = ResourceRequest.None;

        // Use the mineModeToggle to determine the request
        req = (Quarry.mineModeToggle ? ResourceRequest.Resources : ResourceRequest.Blocks);

        // Get the resource from the quarry
        Thing haulableResult = Quarry.GiveResources(req);
        // Place the resource near the pawn
        GenPlace.TryPlaceThing(haulableResult, pawn.Position, Map, ThingPlaceMode.Near);

        // Prevent the colonists from trying to haul rubble, which just makes them visit the platform
        if (haulableResult.def == ThingDefOf.RockRubble) {
          EndJobWith(JobCondition.Succeeded);
        }

        // If this is a chunk or slag, mark it as haulable if allowed to
        if (haulableResult.def.designateHaulable && Quarry.autoHaul) {
          Map.designationManager.AddDesignation(new Designation(haulableResult, DesignationDefOf.Haul));
        }
        
        // Setup IntVec for assigning
        IntVec3 c;

        // Try to find a suitable storage spot for the resource, removing it from the quarry
        // If there are no platforms, hauling will be done by haulers
        if (Quarry.autoHaul && Quarry.TryFindBestStoreCellFor(haulableResult, pawn, Map, pawn.Faction, out c)) {
          CurJob.SetTarget(TargetIndex.B, haulableResult);
          CurJob.count = haulableResult.stackCount;
          CurJob.SetTarget(TargetIndex.C, c);
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Succeeded);
        }
      };
      toil.defaultCompleteMode = ToilCompleteMode.Instant;

      return toil;
    }
  }
}

