using System.Linq;
using Verse;

namespace PS_BarberPod;

public class PS_ReseachLoader
{
    public static bool ResearchExist(string defName)
    {
        return DefDatabase<ResearchProjectDef>.AllDefs.Any(x => x.defName == defName);
    }


    public static bool ResearchIsDone(string defName)
    {
        var research = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(x => x.defName == defName);

        if (research != null)
        {
            return research.IsFinished;
        }

        Log.Error($"PS_ResearchLoader: tried to get IsDone of {defName} but could not find def");
        return false;
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