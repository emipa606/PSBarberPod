using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Reflection;

namespace PS_BarberPod
{
    // Token: 0x02000095 RID: 149
    public class PS_BarberPodPanel : Window
    {

        private Buildings_BarberPod Pod;
        private Pawn Pawn;
        private HairDef PawnStartHairDef;
        private Color PawnStartHairColor;
        private Color PawnStartSkinColor;
        private List<Apparel> PawnStartApp;
        private PS_HeadLoader.HeadType PawnStartHeadType;
        private BodyTypeDef PawnStartBodyType;
        private float PawnStartTanValue;

        private Color CurrentHairColor;
        private PS_HeadLoader.HeadType CurrentHeadType;

        private CarouselBoxes<HairDef> HairouselBoxes;
        private ColorSlider HairColorSlider;
        private CarouselBoxes<CrownType> CrownCarouselBoxes;
        private CarouselBoxes<PS_HeadLoader.HeadType> FaceCarouselBoxes;
        private SingleSlider SkinToneSlider;
        private CarouselBoxes<BodyTypeDef> BodyCarouselBoxes;

        private bool Initalized;
        private bool NeedOptionsUpdate;
        private bool FoundHeadType;
        private bool LimitOptionsByRace;
        private bool LimitOptionsBySex;
        private bool PortraitNeedUpdate;

        public PS_BarberPodPanel()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.Initalized = false;
            this.NeedOptionsUpdate = false;
        }

        private void DebugLog(string s)
        {
            //Log.Message(string.Format("[PS] Barber Pod Logging: {0}", s));
        }

        public void SetPawnAndPod(Pawn pawn, Buildings_BarberPod Pod)
        {
            this.Pod = Pod;
            this.Pawn = pawn;
            this.PawnStartHairDef = pawn.story.hairDef;
            this.CurrentHairColor = pawn.story.hairColor;
            this.PawnStartHairColor = new Color(pawn.story.hairColor.r, pawn.story.hairColor.g, pawn.story.hairColor.b);
            this.PawnStartSkinColor = new Color(pawn.story.SkinColor.r, pawn.story.SkinColor.g, pawn.story.SkinColor.b);
            this.PawnStartBodyType = pawn.story.bodyType;
            this.PawnStartTanValue = pawn.story.melanin;

            this.PawnStartApp = new List<Apparel>();
            while(this.Pawn.apparel.WornApparelCount > 0)
            {
                this.PawnStartApp.Add(this.Pawn.apparel.WornApparel[0]);
                this.Pawn.apparel.Remove(this.Pawn.apparel.WornApparel[0]);
            }

            this.PawnStartHeadType = PS_HeadLoader.AllHeads.Where(x => x.TexturePath == this.Pawn.story.HeadGraphicPath).FirstOrDefault();
            FoundHeadType = (this.PawnStartHeadType != null);
            if (!FoundHeadType)
                Log.Warning("Unable to find pawn HeadType");
            else
                DebugLog(string.Format("Found pawn head type: {0}", this.PawnStartHeadType.FullName));

            this.CurrentHeadType = this.PawnStartHeadType;
            UpdatePortrate();
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            if(!this.Initalized)
            {
                this.Init(inRect);
            }
            //var portateRatio = 128f / 180f;

            // Portrait
            if (PortraitNeedUpdate)
                this.UpdatePortrate();
            Rect portBox = GetRecForGridLocation(0, 0, 1, 3);// new Rect(inRect.x, inRect.y, inRect.width * 0.5f, inRect.height * 0.5f);
            //Widgets.DrawBoxSolid(portBox, Color.white);
            DrawPawnPortrait(portBox);

            // Cancel Button
            var cancelButtonRecGrid = GetRecForGridLocation(0, 5);// new Rect(inRect.width * 0.5f, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var cancelButtonRectTrue = new Rect(cancelButtonRecGrid.x + 5, cancelButtonRecGrid.y + (cancelButtonRecGrid.height * 0.25f), cancelButtonRecGrid.width - 10f, cancelButtonRecGrid.height * 0.5f);
            var cancelButton = Widgets.ButtonText(cancelButtonRectTrue, "Cancel");
            if (cancelButton)
            {
                ResetPawn(this.Pawn);
                this.Pod.Open();
                this.Close(true);
            }

            // Submit Button
            var submitButtonRecGrid = GetRecForGridLocation(1, 5);// new Rect(0, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var submitButtonRectTrue = new Rect(submitButtonRecGrid.x + 5, submitButtonRecGrid.y + (submitButtonRecGrid.height * 0.25f), submitButtonRecGrid.width - 10f, submitButtonRecGrid.height * 0.5f); 
            var button = Widgets.ButtonText(submitButtonRectTrue, "Done");
            if (button)
            {
                RedressPawn(this.Pawn);
                var hairCutThought = DefDatabase<ThoughtDef>.GetNamed("PS_GotHairCut");
                this.Pawn.needs.mood.thoughts.memories.TryGainMemory(hairCutThought);
                this.Pod.Open();
                this.Close(true);
            }

            // CheckBoxes
            var tempGender = LimitOptionsBySex;
            var sexCheckBoxLabelRec = new Rect(22f, 0, inRect.width * 0.25f, 24f); 
            Widgets.Label(sexCheckBoxLabelRec, "Limit By Sex");
            Widgets.Checkbox(new Vector2(0, 0), ref tempGender, size: 20f);
            if (tempGender != LimitOptionsBySex)
                UpdateForGender(tempGender);
            LimitOptionsBySex = tempGender;
            
            //var raceCheckBoxLabelRec = new Rect(sexCheckBoxLabelRec.width + 22f, 0, inRect.width * 0.25f, 24f);
            //Widgets.Label(raceCheckBoxLabelRec, "Limit By Race");
            //Widgets.Checkbox(new Vector2(sexCheckBoxLabelRec.width, 0), ref LimitOptionsByRace, size: 20f);

            HairouselBoxes.Draw();
            HairColorSlider.Draw();
            CrownCarouselBoxes.Draw();
            FaceCarouselBoxes.Draw();
            BodyCarouselBoxes.Draw();
            SkinToneSlider.Draw();


            //}
            this.LimitOptionsBySex = tempGender;

        }

        public void Init(Rect rect)
        {
            this.DebugLog(string.Format("Window X:{0}, Y:{1}, W:{2}, H:{3}", rect.x, rect.y, rect.width, rect.height));
            this.Initalized = true;

            DebugLog("Initalizeing");
            // Hair Style
            var hairBox = GetRecForGridLocation(1, 0);// new Rect(rect.width * 0.5f, 0, rect.width * 0.5f, rect.height * 0.15f);
            this.HairouselBoxes = new CarouselBoxes<HairDef>(hairBox, "Hair Style");
            UpdateHairOptions(this.Pawn, this.HairouselBoxes, this.LimitOptionsBySex);
            HairouselBoxes.DoChange = delegate { this.ChangeHairStyle(this.Pawn, HairouselBoxes.CurrentOptionValue); };

            DebugLog("init hair styles");

            // Hair Color
            var hairColorRect = GetRecForGridLocation(1, 1);// new Rect(hairBox.x, hairBox.yMax + 10, hairBox.width, hairBox.height);
            this.HairColorSlider = new ColorSlider(hairColorRect, this.Pawn.story.hairColor, "Hair Color");
            this.HairColorSlider.DoChange = delegate { this.ChangeHairColor(this.Pawn, this.HairColorSlider.CurrentColor); };

            DebugLog("init hair colors");

            //Head  and face 
            var crownBox = GetRecForGridLocation(1, 2);// new Rect(hairColorRect.x, hairColorRect.yMax + 10, hairColorRect.width, hairColorRect.height);
            CrownCarouselBoxes = new CarouselBoxes<CrownType>(crownBox, "Crown Type");
            CrownCarouselBoxes.Enabled = FoundHeadType;


            if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_BasicResearch"))
            {
                CrownCarouselBoxes.IsLocked = true;
                CrownCarouselBoxes.LockedMessage = "Need Research:\nBasic Facial Reconstruction";
            }

            var faceBox = GetRecForGridLocation(1, 3);//  new Rect(crownBox.x, crownBox.yMax + 10, crownBox.width, crownBox.height);
            FaceCarouselBoxes = new CarouselBoxes<PS_HeadLoader.HeadType>(faceBox, "Face");
            FaceCarouselBoxes.Enabled = FoundHeadType;
            
            if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_IntermediateResearch"))
            {
                FaceCarouselBoxes.LockedMessage = "Need Research: Intermediate\n Facial Reconstruction";
                FaceCarouselBoxes.IsLocked = true;
            }

            if (FoundHeadType)
            {
                var allCrowns = PS_HeadLoader.AllCorwnTypes;
                foreach (var crown in allCrowns)
                {
                    CrownCarouselBoxes.AddOption(crown.ToString(), crown);
                }
                CrownCarouselBoxes.CurrentIndex = CrownCarouselBoxes.Options.Where(x => x.Value == Pawn.story.crownType).FirstOrDefault().Index;
                CrownCarouselBoxes.DoChange = delegate { ChangeCrown(this.Pawn, CrownCarouselBoxes.CurrentOptionValue); };

                this.UpdateFaceOptions(this.Pawn, CrownCarouselBoxes.CurrentOptionValue, FaceCarouselBoxes, this.LimitOptionsBySex);
                FaceCarouselBoxes.DoChange = delegate { this.ChangeFace(this.Pawn, this.FaceCarouselBoxes.CurrentOptionValue); };
            }
           
            DebugLog("init face and head");

            //Body Type
            var bodyBox = GetRecForGridLocation(0, 3);// new Rect(faceBox.x, faceBox.yMax + 10, faceBox.width, faceBox.height);
            BodyCarouselBoxes = new CarouselBoxes<BodyTypeDef>(bodyBox, "Body Type");
            UpdateBodyTypeOptions(this.Pawn, BodyCarouselBoxes, LimitOptionsBySex);

            BodyCarouselBoxes.CurrentIndex = BodyCarouselBoxes.Options.Where(x => x.Value == Pawn.story.bodyType).FirstOrDefault()?.Index ?? 0;
            BodyCarouselBoxes.DoChange = delegate { ChangeBody(this.Pawn, BodyCarouselBoxes.CurrentOptionValue); };

            if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_BodyModResearch"))
            {
                BodyCarouselBoxes.IsLocked = true;
                BodyCarouselBoxes.LockedMessage = "Need Research:\nBody Modification";
            }

            DebugLog("init body types");


            // Skin Tone
            var skinColorRect = GetRecForGridLocation(0, 2);// new Rect(rect.x, bodyBox.y, bodyBox.width, bodyBox.height);
            this.SkinToneSlider = new SingleSlider(skinColorRect, this.Pawn.story.melanin, "Skin Tone");
            this.SkinToneSlider.CurrentValue = this.PawnStartTanValue;
            this.SkinToneSlider.DoChange = delegate { this.ChangeSkinTone(this.Pawn, this.SkinToneSlider.CurrentValue); };
        }

        private void DrawPawnPortrait(Rect rect)
        {
            Vector2 portSize = new Vector2(128f, 180f);
            RenderTexture portrait = PortraitsCache.Get(this.Pawn, portSize, new Vector3(0f, 0f, 0f), 1f);
            Rect position = new Rect((rect.x + rect.width * 0.5f) - portSize.x * 0.5f, (rect.y + rect.height * 0.5f) - portSize.y * 0.5f, portSize.x, portSize.y);
            GUI.DrawTexture(position, portrait);
            GUI.EndGroup();
            GUI.color = Color.white;
        }

        public override void OnAcceptKeyPressed()
        {
            RedressPawn(this.Pawn);
            UpdatePortrate();
            var hairCutThought = DefDatabase<ThoughtDef>.GetNamed("PS_GotHairCut");
            this.Pawn.needs.mood.thoughts.memories.TryGainMemory(hairCutThought);
            this.Pod.Open();
            base.OnAcceptKeyPressed();
        }

        public override void OnCancelKeyPressed()
        {
            ResetPawn(this.Pawn);
            UpdatePortrate();
            this.Pod.Open();
            base.OnCancelKeyPressed();
        }
        
        private void RedressPawn(Pawn pawn)
        {
            foreach (var app in this.PawnStartApp)
                pawn.apparel.Wear(app);
        }

        private void ResetPawn(Pawn pawn)
        {
            RedressPawn(pawn);
            ChangeHairStyle(pawn, this.PawnStartHairDef);
            ChangeHairColor(this.Pawn, this.PawnStartHairColor);
            if(FoundHeadType)
                ChangeFace(pawn, this.PawnStartHeadType);
            ChangeBody(pawn, this.PawnStartBodyType);
        }

        private void UpdatePortrate()
        {
            this.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.Clear();
            PortraitsCache.PortraitsCacheUpdate();
            this.PortraitNeedUpdate = false;
        }

        private void ChangeHairStyle(Pawn pawn, HairDef hair)
        {
            pawn.story.hairDef = hair;
            PortraitNeedUpdate = true;
        }

        private void ChangeHairColor(Pawn pawn, Color hairColor)
        {
            pawn.story.hairColor = hairColor;
            PortraitNeedUpdate = true;
        }

        private void ChangeCrown(Pawn pawn, CrownType crown)
        {
            pawn.story.crownType = crown;
            UpdateFaceOptions(pawn, crown, this.FaceCarouselBoxes, this.LimitOptionsBySex);
        }

        private void ChangeFace(Pawn pawn, PS_HeadLoader.HeadType head)
        {
            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, head.TexturePath);
            CurrentHeadType = head;
            PortraitNeedUpdate = true;
        }
        
        private void ChangeBody(Pawn pawn, BodyTypeDef bodyType)
        {
            pawn.story.bodyType = bodyType;
            PortraitNeedUpdate = true;
        }

        private void ChangeSkinTone(Pawn pawn, float skinTone)
        {
            pawn.story.melanin = skinTone;
            PortraitNeedUpdate = true;
            //typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, head.TexturePath);
        }

        private void UpdateForGender(bool limitGender)
        {
            DebugLog(string.Format("Updateing for gender, limitByGender: {0}", limitGender));
            UpdateHairOptions(this.Pawn, this.HairouselBoxes, limitGender);
            if(this.FoundHeadType)
                UpdateFaceOptions(this.Pawn, this.CrownCarouselBoxes.CurrentOptionValue, this.FaceCarouselBoxes, limitGender);
            this.PortraitNeedUpdate = true;
        }

        private void UpdateHairOptions(Pawn pawn, CarouselBoxes<HairDef> carouselBoxes, bool limitByGender)
        {
            var hairs = new List<HairDef>();
            if (limitByGender)
                hairs = PS_HairLoader.HairByGender(pawn.gender);
            else
                hairs = PS_HairLoader.AllHairs;

            carouselBoxes.ClearOptions();

            foreach (var hair in hairs)
                carouselBoxes.AddOption(hair.defName, hair);

            var currentHairOption = carouselBoxes.Options.Where(x => x.Value == Pawn.story.hairDef).FirstOrDefault();
            if (currentHairOption == null)
            {
                Log.Warning(string.Format("PS_BarberPod: Failed to find current hair: {0}", this.Pawn.story.hairDef.defName));
                carouselBoxes.CurrentIndex = 0;
            }
            else
                carouselBoxes.CurrentIndex = currentHairOption.Index;

            ChangeHairStyle(pawn, carouselBoxes.CurrentOptionValue);
        }

        private void UpdateFaceOptions(Pawn pawn, CrownType crownType, CarouselBoxes<PS_HeadLoader.HeadType> carouselBoxes, bool limitByGender)
        {
            carouselBoxes.ClearOptions();

            var faces = new List<PS_HeadLoader.HeadType>();
            if (limitByGender)
                faces = PS_HeadLoader.AllHeadsForCrownAndGender(crownType, pawn.gender);
            else
                faces = PS_HeadLoader.AllHeadsForCrown(crownType);

            if ((faces?.Count ?? 0) == 0)
            {
                Log.Warning(string.Format("PS_BarberPod: Found no faces for crown type: {1}", this.CurrentHeadType.CrownType.ToString()));
                carouselBoxes.CurrentIndex = 0;
            }
            else
            {
                foreach (var face in faces)
                    carouselBoxes.AddOption(face.FaceName, face);
                var currentFace = carouselBoxes.Options.Where(x => x.Value.FaceName == this.CurrentHeadType.FaceName).FirstOrDefault();
                if (currentFace == null)
                {
                    Log.Warning(string.Format("PS_BarberPod: Could not find current face for selected crown type. CurrentFace: {0}, CrownType: {1}", this.CurrentHeadType.FaceName, this.CurrentHeadType.CrownType.ToString()));
                    carouselBoxes.CurrentIndex = 0;
                }
                else
                    carouselBoxes.CurrentIndex = currentFace.Index;
            }

            ChangeFace(pawn, carouselBoxes.CurrentOptionValue);
        }

        private void UpdateBodyTypeOptions(Pawn pawn, CarouselBoxes<BodyTypeDef> carouselBoxes, bool limitByGender)
        {
            if (carouselBoxes.Options?.Any() ?? false)
                carouselBoxes.ClearOptions();

            BodyCarouselBoxes.AddOption("Thin", BodyTypeDefOf.Thin);
            BodyCarouselBoxes.AddOption("Fat", BodyTypeDefOf.Fat);
            BodyCarouselBoxes.AddOption("Hulk", BodyTypeDefOf.Hulk);

            if(!limitByGender || pawn.gender == Gender.Male)
                BodyCarouselBoxes.AddOption("Male", BodyTypeDefOf.Male);

            if(!limitByGender || pawn.gender == Gender.Female)
                BodyCarouselBoxes.AddOption("Female", BodyTypeDefOf.Female);

            var currentBodyType = pawn.story.bodyType;
            var currentBodyTypeOption = carouselBoxes.Options.Where(x => x.Value == currentBodyType).FirstOrDefault();
            if (currentBodyType == null)
            {
                Log.Warning(string.Format("PS_BarberPod: Failed to find body type: {0}", pawn.story.bodyType.ToString()));
                carouselBoxes.CurrentIndex = 0;
            }
            else
                carouselBoxes.CurrentIndex = currentBodyTypeOption.Index;

            ChangeBody(pawn, carouselBoxes.CurrentOptionValue);



        }

        //private void ChangeSkinTone(Pawn pawn, Color SkinTone)
        //{
        //    var melaninValue = pawn.story.melanin;
        //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, head.TexturePath);
        //    CurrentHeadType = head;
        //    UpdatePortrate();

        //}

        private Rect GetRecForGridLocation(float x, float y, float width = 1f, float height = 1f, float MaxWidth = 2f, float MaxHeight = 6f)
        {
            var drawRect = new Rect(0, 0, this.windowRect.width - (this.Margin * 2f), this.windowRect.height - (this.Margin * 2f));

            float gridBoxWidth = drawRect.width / MaxWidth;
            float gridBoxHeight = drawRect.height / MaxHeight;

            return new Rect(gridBoxWidth * x, gridBoxHeight * y, gridBoxWidth * width, gridBoxHeight * height);
        }
    }
}
