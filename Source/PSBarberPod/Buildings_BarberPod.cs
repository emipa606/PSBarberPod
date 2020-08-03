using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PS_BarberPod
{
    // Token: 0x020006C8 RID: 1736
    public class Buildings_BarberPod : Building_Casket
    {
        private CompProperties_Power powerComp;

        // Token: 0x06002513 RID: 9491 RVA: 0x00116F67 File Offset: 0x00115367
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                return true;
            }
            return false;
        }

        // Token: 0x06002514 RID: 9492 RVA: 0x00116FA0 File Offset: 0x001153A0
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }
            if (!this.TryGetComp<CompPowerTrader>().PowerOn)
            {
                yield return new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (this.innerContainer.Count == 0)
            {
                if (myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    JobDef jobDef = BarberPodDefsOf.UseBarberPod;
                    string jobStr = "Enter barber pod";
                    Action jobAction = delegate ()
                    {
                        Job job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                }
            }
            yield break;
        }
        
    }
}
