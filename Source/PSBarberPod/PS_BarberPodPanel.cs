using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod;

public class PS_BarberPodPanel : Window
{
    private CarouselBoxes<BodyTypeDef> BodyCarouselBoxes;
    private CarouselBoxes<CrownType> CrownCarouselBoxes;

    private Color CurrentHairColor;
    private PS_HeadLoader.HeadType CurrentHeadType;
    private CarouselBoxes<PS_HeadLoader.HeadType> FaceCarouselBoxes;
    private bool FoundHeadType;
    private ColorSlider HairColorSlider;

    private CarouselBoxes<HairDef> HairouselBoxes;

    private bool Initalized;
    private bool LimitOptionsByRace;
    private bool LimitOptionsBySex;
    private bool NeedOptionsUpdate;
    private Pawn Pawn;
    private List<Apparel> PawnStartApp;
    private BodyTypeDef PawnStartBodyType;
    private Color PawnStartHairColor;
    private HairDef PawnStartHairDef;
    private PS_HeadLoader.HeadType PawnStartHeadType;
    private Color PawnStartSkinColor;
    private float PawnStartTanValue;

    private Buildings_BarberPod Pod;
    private bool PortraitNeedUpdate;
    private SingleSlider SkinToneSlider;

    public PS_BarberPodPanel()
    {
        forcePause = true;
        absorbInputAroundWindow = true;
        Initalized = false;
        NeedOptionsUpdate = false;
    }

    private void DebugLog(string s)
    {
        //Log.Message(string.Format("[PS] Barber Pod Logging: {0}", s));
    }

    public void SetPawnAndPod(Pawn pawn, Buildings_BarberPod Pod)
    {
        this.Pod = Pod;
        Pawn = pawn;
        PawnStartHairDef = pawn.story.hairDef;
        CurrentHairColor = pawn.story.hairColor;
        PawnStartHairColor = new Color(pawn.story.hairColor.r, pawn.story.hairColor.g, pawn.story.hairColor.b);
        PawnStartSkinColor = new Color(pawn.story.SkinColor.r, pawn.story.SkinColor.g, pawn.story.SkinColor.b);
        PawnStartBodyType = pawn.story.bodyType;
        PawnStartTanValue = pawn.story.melanin;

        PawnStartApp = new List<Apparel>();
        while (Pawn.apparel.WornApparelCount > 0)
        {
            PawnStartApp.Add(Pawn.apparel.WornApparel[0]);
            Pawn.apparel.Remove(Pawn.apparel.WornApparel[0]);
        }

        PawnStartHeadType = PS_HeadLoader.AllHeads
            .FirstOrDefault(x => x.TexturePath == Pawn.story.HeadGraphicPath);
        FoundHeadType = PawnStartHeadType != null;
        if (!FoundHeadType)
        {
            Log.Warning("Unable to find pawn HeadType");
        }
        else
        {
            DebugLog($"Found pawn head type: {PawnStartHeadType?.FullName}");
        }

        CurrentHeadType = PawnStartHeadType;
        UpdatePortrate();
    }

    public override void DoWindowContents(Rect inRect)
    {
        GUI.BeginGroup(inRect);

        if (!Initalized)
        {
            Init(inRect);
        }
        //var portateRatio = 128f / 180f;

        // Portrait
        if (PortraitNeedUpdate)
        {
            UpdatePortrate();
        }

        var portBox =
            GetRecForGridLocation(0, 0, 1,
                3); // new Rect(inRect.x, inRect.y, inRect.width * 0.5f, inRect.height * 0.5f);
        //Widgets.DrawBoxSolid(portBox, Color.white);
        DrawPawnPortrait(portBox);

        // Cancel Button
        var cancelButtonRecGrid =
            GetRecForGridLocation(0,
                5); // new Rect(inRect.width * 0.5f, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
        var cancelButtonRectTrue = new Rect(cancelButtonRecGrid.x + 5,
            cancelButtonRecGrid.y + (cancelButtonRecGrid.height * 0.25f), cancelButtonRecGrid.width - 10f,
            cancelButtonRecGrid.height * 0.5f);
        var cancelButton = Widgets.ButtonText(cancelButtonRectTrue, "Cancel");
        if (cancelButton)
        {
            ResetPawn(Pawn);
            Pod.Open();
            Close();
        }

        // Submit Button
        var submitButtonRecGrid =
            GetRecForGridLocation(1,
                5); // new Rect(0, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
        var submitButtonRectTrue = new Rect(submitButtonRecGrid.x + 5,
            submitButtonRecGrid.y + (submitButtonRecGrid.height * 0.25f), submitButtonRecGrid.width - 10f,
            submitButtonRecGrid.height * 0.5f);
        var button = Widgets.ButtonText(submitButtonRectTrue, "Done");
        if (button)
        {
            RedressPawn(Pawn);
            var hairCutThought = DefDatabase<ThoughtDef>.GetNamed("PS_GotHairCut");
            Pawn.needs.mood.thoughts.memories.TryGainMemory(hairCutThought);
            Pod.Open();
            Close();
        }

        // CheckBoxes
        var tempGender = LimitOptionsBySex;
        var sexCheckBoxLabelRec = new Rect(22f, 0, inRect.width * 0.25f, 24f);
        Widgets.Label(sexCheckBoxLabelRec, "Limit By Sex");
        Widgets.Checkbox(new Vector2(0, 0), ref tempGender, 20f);
        if (tempGender != LimitOptionsBySex)
        {
            UpdateForGender(tempGender);
        }

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
        LimitOptionsBySex = tempGender;
    }

    public void Init(Rect rect)
    {
        DebugLog($"Window X:{rect.x}, Y:{rect.y}, W:{rect.width}, H:{rect.height}");
        Initalized = true;

        DebugLog("Initalizeing");
        // Hair Style
        var hairBox =
            GetRecForGridLocation(1, 0); // new Rect(rect.width * 0.5f, 0, rect.width * 0.5f, rect.height * 0.15f);
        HairouselBoxes = new CarouselBoxes<HairDef>(hairBox, "Hair Style");
        UpdateHairOptions(Pawn, HairouselBoxes, LimitOptionsBySex);
        HairouselBoxes.DoChange = delegate { ChangeHairStyle(Pawn, HairouselBoxes.CurrentOptionValue); };

        DebugLog("init hair styles");

        // Hair Color
        var hairColorRect =
            GetRecForGridLocation(1, 1); // new Rect(hairBox.x, hairBox.yMax + 10, hairBox.width, hairBox.height);
        HairColorSlider = new ColorSlider(hairColorRect, Pawn.story.hairColor, "Hair Color");
        HairColorSlider.DoChange = delegate { ChangeHairColor(Pawn, HairColorSlider.CurrentColor); };

        DebugLog("init hair colors");

        //Head  and face 
        var crownBox =
            GetRecForGridLocation(1,
                2); // new Rect(hairColorRect.x, hairColorRect.yMax + 10, hairColorRect.width, hairColorRect.height);
        CrownCarouselBoxes = new CarouselBoxes<CrownType>(crownBox, "Crown Type")
        {
            Enabled = FoundHeadType
        };


        if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_BasicResearch"))
        {
            CrownCarouselBoxes.IsLocked = true;
            CrownCarouselBoxes.LockedMessage = "Need Research:\nBasic Facial Reconstruction";
        }

        var faceBox =
            GetRecForGridLocation(1, 3); //  new Rect(crownBox.x, crownBox.yMax + 10, crownBox.width, crownBox.height);
        FaceCarouselBoxes = new CarouselBoxes<PS_HeadLoader.HeadType>(faceBox, "Face")
        {
            Enabled = FoundHeadType
        };

        if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_IntermediateResearch"))
        {
            FaceCarouselBoxes.LockedMessage = "Need Research: Intermediate\n Facial Reconstruction";
            FaceCarouselBoxes.IsLocked = true;
        }

        if (FoundHeadType)
        {
            var allCrowns = PS_HeadLoader.AllCrownTypes;
            foreach (var crown in allCrowns)
            {
                CrownCarouselBoxes.AddOption(crown.ToString(), crown);
            }

            CrownCarouselBoxes.CurrentIndex = CrownCarouselBoxes.Options
                .FirstOrDefault(x => x.Value == Pawn.story.crownType)!.Index;
            CrownCarouselBoxes.DoChange = delegate { ChangeCrown(Pawn, CrownCarouselBoxes.CurrentOptionValue); };

            UpdateFaceOptions(Pawn, CrownCarouselBoxes.CurrentOptionValue, FaceCarouselBoxes, LimitOptionsBySex);
            FaceCarouselBoxes.DoChange = delegate { ChangeFace(Pawn, FaceCarouselBoxes.CurrentOptionValue); };
        }

        DebugLog("init face and head");

        //Body Type
        var bodyBox =
            GetRecForGridLocation(0, 3); // new Rect(faceBox.x, faceBox.yMax + 10, faceBox.width, faceBox.height);
        BodyCarouselBoxes = new CarouselBoxes<BodyTypeDef>(bodyBox, "Body Type");
        UpdateBodyTypeOptions(Pawn, BodyCarouselBoxes, LimitOptionsBySex);

        BodyCarouselBoxes.CurrentIndex =
            BodyCarouselBoxes.Options.FirstOrDefault(x => x.Value == Pawn.story.bodyType)?.Index ?? 0;
        BodyCarouselBoxes.DoChange = delegate { ChangeBody(Pawn, BodyCarouselBoxes.CurrentOptionValue); };

        if (!PS_ReseachLoader.ResearchNotExistOrIsDone("PS_BodyModResearch"))
        {
            BodyCarouselBoxes.IsLocked = true;
            BodyCarouselBoxes.LockedMessage = "Need Research:\nBody Modification";
        }

        DebugLog("init body types");


        // Skin Tone
        var skinColorRect = GetRecForGridLocation(0, 2); // new Rect(rect.x, bodyBox.y, bodyBox.width, bodyBox.height);
        SkinToneSlider = new SingleSlider(skinColorRect, Pawn.story.melanin, "Skin Tone")
        {
            CurrentValue = PawnStartTanValue
        };
        SkinToneSlider.DoChange = delegate { ChangeSkinTone(Pawn, SkinToneSlider.CurrentValue); };
    }

    private void DrawPawnPortrait(Rect rect)
    {
        var portSize = new Vector2(128f, 180f);
        var portrait = PortraitsCache.Get(Pawn, portSize, Rot4.South, new Vector3(0f, 0f, 0f));
        var position = new Rect(rect.x + (rect.width * 0.5f) - (portSize.x * 0.5f),
            rect.y + (rect.height * 0.5f) - (portSize.y * 0.5f), portSize.x, portSize.y);
        GUI.DrawTexture(position, portrait);
        GUI.EndGroup();
        GUI.color = Color.white;
    }

    public override void OnAcceptKeyPressed()
    {
        RedressPawn(Pawn);
        UpdatePortrate();
        var hairCutThought = DefDatabase<ThoughtDef>.GetNamed("PS_GotHairCut");
        Pawn.needs.mood.thoughts.memories.TryGainMemory(hairCutThought);
        Pod.Open();
        base.OnAcceptKeyPressed();
    }

    public override void OnCancelKeyPressed()
    {
        ResetPawn(Pawn);
        UpdatePortrate();
        Pod.Open();
        base.OnCancelKeyPressed();
    }

    private void RedressPawn(Pawn pawn)
    {
        foreach (var app in PawnStartApp)
        {
            pawn.apparel.Wear(app);
        }
    }

    private void ResetPawn(Pawn pawn)
    {
        RedressPawn(pawn);
        ChangeHairStyle(pawn, PawnStartHairDef);
        ChangeHairColor(Pawn, PawnStartHairColor);
        if (FoundHeadType)
        {
            ChangeFace(pawn, PawnStartHeadType);
        }

        ChangeBody(pawn, PawnStartBodyType);
    }

    private void UpdatePortrate()
    {
        Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        PortraitsCache.Clear();
        PortraitsCache.PortraitsCacheUpdate();
        PortraitNeedUpdate = false;
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
        UpdateFaceOptions(pawn, crown, FaceCarouselBoxes, LimitOptionsBySex);
    }

    private void ChangeFace(Pawn pawn, PS_HeadLoader.HeadType head)
    {
        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(pawn.story, head.TexturePath);
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
        DebugLog($"Updateing for gender, limitByGender: {limitGender}");
        UpdateHairOptions(Pawn, HairouselBoxes, limitGender);
        if (FoundHeadType)
        {
            UpdateFaceOptions(Pawn, CrownCarouselBoxes.CurrentOptionValue, FaceCarouselBoxes, limitGender);
        }

        PortraitNeedUpdate = true;
    }

    private void UpdateHairOptions(Pawn pawn, CarouselBoxes<HairDef> carouselBoxes, bool limitByGender)
    {
        var hairs = limitByGender ? PS_HairLoader.HairByGender(pawn.gender) : PS_HairLoader.AllHairs;

        carouselBoxes.ClearOptions();

        foreach (var hair in hairs)
        {
            carouselBoxes.AddOption(hair.defName, hair);
        }

        var currentHairOption = carouselBoxes.Options.FirstOrDefault(x => x.Value == Pawn.story.hairDef);
        if (currentHairOption == null)
        {
            Log.Warning($"PS_BarberPod: Failed to find current hair: {Pawn.story.hairDef.defName}");
            carouselBoxes.CurrentIndex = 0;
        }
        else
        {
            carouselBoxes.CurrentIndex = currentHairOption.Index;
        }

        ChangeHairStyle(pawn, carouselBoxes.CurrentOptionValue);
    }

    private void UpdateFaceOptions(Pawn pawn, CrownType crownType, CarouselBoxes<PS_HeadLoader.HeadType> carouselBoxes,
        bool limitByGender)
    {
        carouselBoxes.ClearOptions();

        var faces = limitByGender
            ? PS_HeadLoader.AllHeadsForCrownAndGender(crownType, pawn.gender)
            : PS_HeadLoader.AllHeadsForCrown(crownType);

        if ((faces?.Count ?? 0) == 0)
        {
            Log.Warning($"PS_BarberPod: Found no faces for crown type: {CurrentHeadType.CrownType.ToString()}");
            carouselBoxes.CurrentIndex = 0;
        }
        else
        {
            foreach (var face in faces)
            {
                carouselBoxes.AddOption(face.FaceName, face);
            }

            var currentFace = carouselBoxes.Options
                .FirstOrDefault(x => x.Value.FaceName == CurrentHeadType.FaceName);
            if (currentFace == null)
            {
                Log.Warning(
                    $"PS_BarberPod: Could not find current face for selected crown type. CurrentFace: {CurrentHeadType.FaceName}, CrownType: {CurrentHeadType.CrownType.ToString()}");
                carouselBoxes.CurrentIndex = 0;
            }
            else
            {
                carouselBoxes.CurrentIndex = currentFace.Index;
            }
        }

        ChangeFace(pawn, carouselBoxes.CurrentOptionValue);
    }

    private void UpdateBodyTypeOptions(Pawn pawn, CarouselBoxes<BodyTypeDef> carouselBoxes, bool limitByGender)
    {
        if (carouselBoxes.Options?.Any() ?? false)
        {
            carouselBoxes.ClearOptions();
        }

        BodyCarouselBoxes.AddOption("Thin", BodyTypeDefOf.Thin);
        BodyCarouselBoxes.AddOption("Fat", BodyTypeDefOf.Fat);
        BodyCarouselBoxes.AddOption("Hulk", BodyTypeDefOf.Hulk);

        if (!limitByGender || pawn.gender == Gender.Male)
        {
            BodyCarouselBoxes.AddOption("Male", BodyTypeDefOf.Male);
        }

        if (!limitByGender || pawn.gender == Gender.Female)
        {
            BodyCarouselBoxes.AddOption("Female", BodyTypeDefOf.Female);
        }

        var currentBodyType = pawn.story.bodyType;
        if (carouselBoxes.Options != null)
        {
            var currentBodyTypeOption = carouselBoxes.Options.FirstOrDefault(x => x.Value == currentBodyType);
            if (currentBodyType == null)
            {
                Log.Warning($"PS_BarberPod: Failed to find body type: {pawn.story.bodyType}");
                carouselBoxes.CurrentIndex = 0;
            }
            else
            {
                if (currentBodyTypeOption != null)
                {
                    carouselBoxes.CurrentIndex = currentBodyTypeOption.Index;
                }
            }
        }

        ChangeBody(pawn, carouselBoxes.CurrentOptionValue);
    }

    //private void ChangeSkinTone(Pawn pawn, Color SkinTone)
    //{
    //    var melaninValue = pawn.story.melanin;
    //    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, head.TexturePath);
    //    CurrentHeadType = head;
    //    UpdatePortrate();

    //}

    private Rect GetRecForGridLocation(float x, float y, float width = 1f, float height = 1f, float MaxWidth = 2f,
        float MaxHeight = 6f)
    {
        var drawRect = new Rect(0, 0, windowRect.width - (Margin * 2f), windowRect.height - (Margin * 2f));

        var gridBoxWidth = drawRect.width / MaxWidth;
        var gridBoxHeight = drawRect.height / MaxHeight;

        return new Rect(gridBoxWidth * x, gridBoxHeight * y, gridBoxWidth * width, gridBoxHeight * height);
    }
}