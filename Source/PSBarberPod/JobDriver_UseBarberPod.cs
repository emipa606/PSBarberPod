using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PS_BarberPod
{
    public class JobDriver_UseBarberPod : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil openMissionSelect = new Toil();
            openMissionSelect.initAction = delegate ()
            {
                Pawn actor = openMissionSelect.actor;
                Buildings_BarberPod pod = (Buildings_BarberPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                this.StartBarbering(pod, actor);
            };
            yield return openMissionSelect;
            yield break;
        }
        
        public void StartBarbering(Buildings_BarberPod pod, Pawn pawn)
        {
            pawn.DeSpawn(DestroyMode.Vanish);
            pod.TryAcceptThing(pawn, true);

            var window = new PS_BarberPodPanel();
            window.SetPawnAndPod(pawn, pod);
            Find.WindowStack.Add(window);

        }
    }
}
