using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    public class PS_ReseachLoader
    {
        public static bool ResearchExist(string defName)
        {
            return DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.defName == defName).Any();
        }


        public static bool ResearchIsDone(string defName)
        {
            var research = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.defName == defName).FirstOrDefault();

            if (research == null)
            {
                Log.Error(string.Format("PS_ResearchLoader: tried to get IsDone of {0} but could not find def", defName));
                return false;
            }

            return research.IsFinished;
        }

        public static bool ResearchExistAndIsDone(string defName)
        {
            return ResearchExist(defName) && ResearchIsDone(defName);
        }

        public static bool ResearchNotExistOrIsDone(string defName)
        {
            return !ResearchExist(defName) || ResearchIsDone(defName);
        }
    }
}
