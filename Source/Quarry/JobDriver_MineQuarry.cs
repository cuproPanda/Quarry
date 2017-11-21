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
          quarryBuilding = job.GetTarget(TargetIndex.A).Cell.GetThingList(Map).Find(q => q is Building_Quarry) as Building_Quarry;
        }
        return quarryBuilding;
      }
    }

    protected Thing Haulable {
      get {
        if (TargetB.Thing != null) {
          return TargetB.Thing;
        }
        Log.Warning("Quarry:: Trying to assign a null haulable to a pawn.");
        EndJobWith(JobCondition.Errored);
        return null;
      }
    }


    protected override IEnumerable<Toil> MakeNewToils() {

      // Set up fail conditions
      this.FailOn(delegate {
        return Quarry == null || Quarry.IsForbidden(pawn) || Quarry.Depleted;
      });

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


    private Toil Mine() {
      Toil toil = new Toil();
      toil.tickAction = delegate {
        pawn.rotationTracker.Face(Quarry.Position.ToVector3Shifted());

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

        MoteType mote = MoteType.None;
        bool singleSpawn = true;
        bool eventTriggered = false;
        int stackCount = 1;

        // Get the resource from the quarry
        ThingDef def = Quarry.GiveResources(req, out mote, out singleSpawn, out eventTriggered);

        // If something went wrong, bail out
        if (def == null || def.thingClass == null) {
          Log.Warning("Quarry:: Tried to quarry mineable ore, but the ore given was null.");
          mote = MoteType.None;
          singleSpawn = true;
          // This shouldn't happen at all, but if it does let's add a little reward instead of just giving rubble
          def = ThingDefOf.ChunkSlagSteel;
        }

        Thing haulableResult = ThingMaker.MakeThing(def);
        if (!singleSpawn && def != ThingDefOf.Component) {
          int sub = (int)(def.BaseMarketValue / 3f);
          if (sub >= 10) {
            sub = 9;
          }
          
          stackCount += Mathf.Min(Rand.RangeInclusive(15 - sub, 40 - (sub * 2)), def.stackLimit - 1 );
        }

        if (def == ThingDefOf.Component) {
          stackCount += Random.Range(0, 1);
        }

        haulableResult.stackCount = stackCount;

        if (stackCount >= 30) {
          mote = MoteType.LargeVein;
        }

        // Place the resource near the pawn
        GenPlace.TryPlaceThing(haulableResult, pawn.Position, Map, ThingPlaceMode.Near);

        // If the resource had a mote, throw it
        if (mote == MoteType.LargeVein) {
          MoteMaker.ThrowText(haulableResult.DrawPos, Map, Static.TextMote_LargeVein, Color.green, 3f);
        }
        else if (mote == MoteType.Failure) {
          MoteMaker.ThrowText(haulableResult.DrawPos, Map, Static.TextMote_MiningFailed, Color.red, 3f);
        }

        // If the sinkhole event was triggered, damage the pawn and end this job
        // Even if the sinkhole doesn't incapacitate the pawn, they will probably want to seek medical attention
        if (eventTriggered) {
          Messages.Message("QRY_MessageSinkhole".Translate(pawn.NameStringShort), pawn, MessageTypeDefOf.ThreatSmall);
          DamageInfo dInfo = new DamageInfo(DamageDefOf.Crush, 9, -1f, category: DamageInfo.SourceCategory.Collapse);
          dInfo.SetBodyRegion(BodyPartHeight.Bottom, BodyPartDepth.Inside);
          pawn.TakeDamage(dInfo);
          pawn.TakeDamage(dInfo);

          EndJobWith(JobCondition.Succeeded);
        }

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
        // If there are no platforms with free space, try to haul it to a storage area
        if (Quarry.autoHaul) {
					if (Quarry.HasConnectedPlatform && Quarry.TryFindBestStoreCellFor(haulableResult, pawn, Map, pawn.Faction, out c)) {
						job.SetTarget(TargetIndex.B, haulableResult);
						job.count = haulableResult.stackCount;
						job.SetTarget(TargetIndex.C, c);
					}
					else {
						StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(haulableResult.Position, haulableResult);
						if (StoreUtility.TryFindBestBetterStoreCellFor(haulableResult, pawn, Map, currentPriority, pawn.Faction, out c)) {
							job.SetTarget(TargetIndex.B, haulableResult);
							job.count = haulableResult.stackCount;
							job.SetTarget(TargetIndex.C, c);
						}
					}
        }
        // If there is no spot to store the resource, end this job
        else {
          EndJobWith(JobCondition.Succeeded);
        }
      };
      toil.defaultCompleteMode = ToilCompleteMode.Instant;

      return toil;
    }

        public override bool TryMakePreToilReservations()
        {
            return true; //Nothing needs to be reserved... ?
        }
    }
}

