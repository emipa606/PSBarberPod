using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PS_BarberPod;

public class Buildings_BarberPod : Building_Casket
{
    private readonly CompPowerTrader powerComp;

    public Buildings_BarberPod()
    {
        powerComp = this.TryGetComp<CompPowerTrader>();
    }

    public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!base.TryAcceptThing(thing, allowSpecialEffects))
        {
            return false;
        }

        if (allowSpecialEffects)
        {
            SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(Position, Map));
        }

        return true;
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        var baseOptions = base.GetFloatMenuOptions(myPawn);
        if (baseOptions?.Any() == true)
        {
            foreach (var o in baseOptions)
            {
                yield return o;
            }
        }

        if (powerComp?.PowerOn == false)
        {
            yield return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            yield break;
        }

        if (innerContainer.Count != 0)
        {
            yield break;
        }

        if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
        {
            yield break;
        }

        var jobDef = BarberPodDefsOf.UseBarberPod;
        var jobStr = "Enter barber pod";

        void JobAction()
        {
            var job = new Job(jobDef, this);
            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, JobAction), myPawn,
            this);
    }
}