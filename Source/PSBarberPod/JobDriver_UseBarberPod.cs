using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PS_BarberPod;

public class JobDriver_UseBarberPod : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
        var openMissionSelect = new Toil();
        openMissionSelect.initAction = delegate
        {
            var actor = openMissionSelect.actor;
            var pod = (Buildings_BarberPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            StartBarbering(pod, actor);
        };
        yield return openMissionSelect;
    }

    public void StartBarbering(Buildings_BarberPod pod, Pawn pawn)
    {
        pawn.DeSpawn();
        pod.TryAcceptThing(pawn);

        var window = new PS_BarberPodPanel();
        window.SetPawnAndPod(pawn, pod);
        Find.WindowStack.Add(window);
    }
}