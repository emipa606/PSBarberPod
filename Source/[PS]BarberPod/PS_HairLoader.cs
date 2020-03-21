using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    public class PS_HairLoader
    {
        private static List<HairDef> _AllHairs;

        public static List<HairDef> AllHairs
        {
            get
            {
                if ((_AllHairs?.Count ?? 0) == 0)
                    _AllHairs = LoadAllHairs();
                return _AllHairs;
            }
        }

        public static List<HairDef> LimitedHair(HairGender gender = HairGender.Any, RaceProperties race = null)
        {
            //if(gender != HairGender.Any && race != null)
            //    return AllHairs.Where(x => x.hairGender == gender && race..)
            return AllHairs;
        }

        private static List<HairDef> LoadAllHairs()
        {
            var allHair = DefDatabase<HairDef>.AllDefs;
            var validHairs = new List<HairDef>();
            var failedHair = new List<string>();
            foreach (var hair in allHair)
            {
                try
                {
                    //var graphic = GraphicDatabase.Get<Graphic_Multi>(hair.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                    //if (graphic == null)
                    //    throw new Exception(string.Format("PS_BarberPod: Failed to load hair {0}", hair.defName));
                    if (TryLoadHairTex(hair.texPath))
                        validHairs.Add(hair);
                    else
                        failedHair.Add(hair.defName);

                }
                catch (Exception ex)
                {
                    Log.Message(string.Format("PS_BarberPod.LoadAllHairs: Error {0}", ex.Message));
                }
                
            }
            Log.Message(string.Format("PS_HairLoaded: Loaded {0} hairs", validHairs.Count()));
            Log.Message(string.Format("PS_HairLoader: Failed to load the following hairs: " + string.Join(", ", failedHair.ToArray())));
            return validHairs;
        }

        public static List<HairDef> HairByGender(Gender gender)
        {
            return AllHairs.Where(x => IsGenderAppropriate(gender, x.hairGender)).ToList();
        }

        private static bool IsGenderAppropriate(Gender gender, HairGender hairGender)
        {
            if (gender == Gender.Male)
            {
                return (hairGender == HairGender.Male || hairGender == HairGender.MaleUsually || hairGender == HairGender.Any);
            }
            else if (gender == Gender.Female)
            {
                return (hairGender == HairGender.Female || hairGender == HairGender.FemaleUsually || hairGender == HairGender.Any);
            }
            else
                return true;
        }

        private static bool TryLoadHairTex(string HairPath)
        {
            var north = ContentFinder<Texture2D>.Get(HairPath + "_north", false);
            var east = ContentFinder<Texture2D>.Get(HairPath + "_east", false);
            var south = ContentFinder<Texture2D>.Get(HairPath + "_south", false);
            var west = ContentFinder<Texture2D>.Get(HairPath + "_west", false);
            return (north != null) || (east != null) || (south != null) || (west != null);
        }
    }
}
