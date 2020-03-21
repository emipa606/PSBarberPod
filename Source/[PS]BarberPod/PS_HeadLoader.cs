using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Reflection;
using System.IO;

namespace PS_BarberPod
{
    public static class PS_HeadLoader
    {
        public class HeadType
        {
            public string FullName;
            public string FaceName;
            public string TexturePath;
            public Gender? Gender;
            public CrownType CrownType;
        }

        public static List<HeadType> _AllHeads;
        public static List<HeadType> AllHeads
        {
            get
            {
                if (_AllHeads != null && _AllHeads.Count > 0)
                    return _AllHeads;
                else
                {
                    _AllHeads = LoadHeads();
                    return _AllHeads;
                }
            }
        }

        public static List<CrownType> AllCorwnTypes { get { return AllHeads.Select(x => x.CrownType).Distinct().ToList(); } }

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
            typeof(GraphicDatabaseHeadRecords).GetMethod("BuildDatabaseIfNecessary", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            string[] arr = new string[]
            {
                "Things/Pawn/Humanlike/Heads/Male",
                "Things/Pawn/Humanlike/Heads/Female",
                "Things/Pawn/Humanlike/Heads/Female"
            };
            var heads = new List<HeadType>();
            foreach (string text in arr)
            {
                GraphicDatabaseUtility.GraphicNamesInFolder(text);
                foreach (string str in GraphicDatabaseUtility.GraphicNamesInFolder(text))
                {
                    string text2 = text + "/" + str;
                    HeadType newHead = CreateHeadFromFilePath(text2);
                    heads.Add(newHead);
                }
            }
            return heads;
        }
        
        private static HeadType CreateHeadFromFilePath(string path)
        {
            HeadType headType = new HeadType();
            headType.TexturePath = path;
            string[] arr = Path.GetFileNameWithoutExtension(path).Split(new char[] { '_' });
            try
            {
                headType.Gender = new Gender?((Gender)ParseHelper.FromString(arr[arr.Length - 3], typeof(Gender)));
                headType.FullName = arr[arr.Length - 1] + " " + arr[arr.Length - 2];
                headType.FaceName = arr[arr.Length - 1] + "-" + headType.Gender.ToString();
                headType.CrownType = (CrownType)ParseHelper.FromString(arr[arr.Length - 2], typeof(CrownType));
            }
            catch (Exception ex)
            {
                Log.Warning(string.Format("PS_HeadLoader Failed to parse head at {0}: {1}", path, ex.Message), false);
                headType.FullName = arr[arr.Length - 1] + " " + arr[arr.Length - 2];
                headType.FaceName = arr[arr.Length - 1];
                headType.CrownType = CrownType.Undefined;
                headType.Gender = new Gender?(Gender.None);
            }
            return headType;
        }
    }
}
