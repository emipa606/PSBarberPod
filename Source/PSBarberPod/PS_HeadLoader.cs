using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace PS_BarberPod;

public static class PS_HeadLoader
{
    public static List<HeadType> _AllHeads;

    public static List<HeadType> AllHeads
    {
        get
        {
            if (_AllHeads is { Count: > 0 })
            {
                return _AllHeads;
            }

            _AllHeads = LoadHeads();
            return _AllHeads;
        }
    }

    public static List<CrownType> AllCrownTypes
    {
        get { return AllHeads.Select(x => x.CrownType).Distinct().ToList(); }
    }

    public static List<HeadType> AllHeadsForCrown(CrownType crown)
    {
        return AllHeads.Where(x => x.CrownType == crown).ToList();
    }

    public static List<HeadType> AllHeadsForCrownAndGender(CrownType crown, Gender gender)
    {
        return AllHeads.Where(x => x.Gender == gender && x.CrownType == crown).ToList();
    }

    public static List<HeadType> LoadHeads()
    {
        typeof(GraphicDatabaseHeadRecords)
            .GetMethod("BuildDatabaseIfNecessary", BindingFlags.Static | BindingFlags.NonPublic)
            ?.Invoke(null, null);
        string[] arr =
        {
            "Things/Pawn/Humanlike/Heads/Male",
            "Things/Pawn/Humanlike/Heads/Female",
            "Things/Pawn/Humanlike/Heads/Female"
        };
        var heads = new List<HeadType>();
        foreach (var text in arr)
        {
            var unused = GraphicDatabaseUtility.GraphicNamesInFolder(text);
            foreach (var str in GraphicDatabaseUtility.GraphicNamesInFolder(text))
            {
                var text2 = text + "/" + str;
                var newHead = CreateHeadFromFilePath(text2);
                heads.Add(newHead);
            }
        }

        return heads;
    }

    private static HeadType CreateHeadFromFilePath(string path)
    {
        var headType = new HeadType
        {
            TexturePath = path
        };
        var arr = Path.GetFileNameWithoutExtension(path).Split('_');
        try
        {
            headType.Gender = (Gender)ParseHelper.FromString(arr[arr.Length - 3], typeof(Gender));
            headType.FullName = arr[arr.Length - 1] + " " + arr[arr.Length - 2];
            headType.FaceName = arr[arr.Length - 1] + "-" + headType.Gender;
            headType.CrownType = (CrownType)ParseHelper.FromString(arr[arr.Length - 2], typeof(CrownType));
        }
        catch (Exception ex)
        {
            Log.Warning($"PS_HeadLoader Failed to parse head at {path}: {ex.Message}");
            headType.FullName = arr[arr.Length - 1] + " " + arr[arr.Length - 2];
            headType.FaceName = arr[arr.Length - 1];
            headType.CrownType = CrownType.Undefined;
            headType.Gender = Gender.None;
        }

        return headType;
    }

    public class HeadType
    {
        public CrownType CrownType;
        public string FaceName;
        public string FullName;
        public Gender? Gender;
        public string TexturePath;
    }
}