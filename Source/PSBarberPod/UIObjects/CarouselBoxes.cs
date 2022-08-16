using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PS_BarberPod;

public class CarouselOption<T>
{
    public int Index;
    public string Label;
    public T Value;
}

public class CarouselBoxes<T>
{
    public int CurrentIndex;

    public Action DoChange;
    public Rect DrawRect;
    public bool DropDownChange;
    public bool Enabled;
    public bool IsLocked;
    public string Label;
    public float LabelHeight;
    public string LockedMessage;
    public List<CarouselOption<T>> Options;
    public float Padding;
    public float SideButtonPerc;

    public CarouselBoxes(Rect DrawRect, string Label, float LabelHeight = 20f, float SideButtonPercent = 0.2f,
        float Padding = 5f)
    {
        this.DrawRect = DrawRect;
        Options = new List<CarouselOption<T>>();
        SideButtonPerc = SideButtonPercent;
        this.Label = Label;
        this.LabelHeight = LabelHeight;
        Enabled = true;
        this.Padding = Padding;
    }

    public T CurrentOptionValue
    {
        get
        {
            var option = GetOption(CurrentIndex);
            if (option != null)
            {
                return option.Value;
            }

            return default;
        }
    }

    public void AddOption(string Label, T Value)
    {
        if (Options == null)
        {
            Options = new List<CarouselOption<T>>();
        }

        if (Options.Any(x => x.Label == Label))
        {
            //Log.Warning("PS_CarouselBoxes: Attemped to add duplicate label: " + Label);
            return;
        }

        var newOpt = new CarouselOption<T>
        {
            Label = Label,
            Index = Options.Count,
            Value = Value
        };

        Options.Add(newOpt);
    }

    public CarouselOption<T> GetOption(int i)
    {
        if ((Options?.Count ?? 0) == 0)
        {
            Log.Error("PS_CarouselBoxes: Attempted to get option from null or empty Carousel");
            return null;
        }

        if (Options.Count > i)
        {
            return Options[i];
        }

        Log.Error(
            $"PS_CarouselBoxes: Attempted to get option from Carousel outside of range. Count: {Options.Count}, Requested: {i}");
        return null;
    }

    public void ClearOptions()
    {
        Options.Clear();
    }

    public void Draw()
    {
        var trueDrawRect = new Rect(DrawRect.x + Padding, DrawRect.y + Padding, DrawRect.width - (Padding * 2f),
            DrawRect.height - (Padding * 2f));

        var sideButWidth = trueDrawRect.width * SideButtonPerc;
        var hadChange = false;

        Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y, trueDrawRect.width, LabelHeight), Label);

        if (!IsLocked)
        {
            var backButRect = new Rect(trueDrawRect.x, trueDrawRect.y + LabelHeight, sideButWidth,
                trueDrawRect.height - LabelHeight);
            if (Widgets.ButtonText(backButRect, "<") && Enabled)
            {
                CurrentIndex--;
                if (CurrentIndex < 0)
                {
                    CurrentIndex = Options.Count - 1;
                }

                hadChange = true;
            }


            var nextButRect = new Rect(trueDrawRect.x + trueDrawRect.width - sideButWidth, trueDrawRect.y + LabelHeight,
                sideButWidth, trueDrawRect.height - LabelHeight);
            if (Widgets.ButtonText(nextButRect, ">") && Enabled)
            {
                CurrentIndex++;
                if (CurrentIndex >= Options.Count)
                {
                    CurrentIndex = 0;
                }

                hadChange = true;
            }

            var centerButRect = new Rect(trueDrawRect.x + sideButWidth, trueDrawRect.y + LabelHeight,
                trueDrawRect.width - (sideButWidth * 2f), trueDrawRect.height - LabelHeight);
            GUI.backgroundColor = Color.gray;
            if (Widgets.ButtonText(centerButRect,
                    Enabled ? GetOption(CurrentIndex)?.Label ?? "Failed To Load" : "None") && Enabled)
            {
                var dropDownActions = new List<Action>();
                var dropDownList = new List<FloatMenuOption>();
                foreach (var opt in Options)
                {
                    var label = opt.Label;
                    dropDownActions.Add(delegate { SelectOption(opt); });
                    dropDownList.Add(new FloatMenuOption(label, dropDownActions.Last()));
                }

                Find.WindowStack.Add(new FloatMenu(dropDownList, null));
                hadChange = true;
            }

            if (DropDownChange)
            {
                hadChange = true;
                DropDownChange = false;
            }

            if (!hadChange || !Enabled)
            {
                return;
            }

            DoChange?.Invoke();
            return;
        }

        var lightGrey = new Color(0.6f, 0.6f, 0.6f);
        var darkGrey = new Color(0.3f, 0.3f, 0.3f);

        var lockedRect = new Rect(trueDrawRect.x, trueDrawRect.y + LabelHeight, trueDrawRect.width,
            trueDrawRect.height - LabelHeight);
        Widgets.DrawBoxSolid(lockedRect, darkGrey);
        GUI.color = lightGrey;
        Widgets.DrawBox(lockedRect);

        var style = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            normal =
            {
                textColor = Color.white
            }
        };

        GUI.Label(lockedRect, LockedMessage, style);

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
    }

    public void SelectOption(CarouselOption<T> option)
    {
        CurrentIndex = option.Index;
        DropDownChange = true;
    }
}