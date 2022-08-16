using System;
using UnityEngine;
using Verse;

namespace PS_BarberPod;

public class ColorSlider
{
    public float CurrentBlue;
    public float CurrentGreen;
    public float CurrentRed;
    public Action DoChange;
    public Rect DrawRect;
    public string Label;
    public float LabelHeight;
    public float Padding;

    public ColorSlider(Rect DrawRect, Color StartColor, string Label, float LabelHeight = 20f, float Padding = 5f)
    {
        this.DrawRect = DrawRect;
        CurrentRed = StartColor.r;
        CurrentGreen = StartColor.g;
        CurrentBlue = StartColor.b;
        this.Label = Label;
        this.LabelHeight = LabelHeight;
        this.Padding = Padding;
    }

    public Color CurrentColor => new Color(CurrentRed, CurrentGreen, CurrentBlue);

    public void SetCurrentColor(Color c)
    {
        CurrentRed = c.r;
        CurrentBlue = c.b;
        CurrentGreen = c.g;
    }

    private bool HasChanged(float a, float b)
    {
        return !(a > b - 0.0001f && a < b + 0.0001f);
    }

    public void Draw()
    {
        // Hair Color Sliders
        //var hairColor = this.CurrentHairColor;
        //var colorBoxRect = new Rect(hairBox.x, hairBox.yMax + 5, hairBox.width * 0.25f, hairBox.width * 0.25f);

        var trueDrawRect = new Rect(DrawRect.x + Padding, DrawRect.y + Padding, DrawRect.width - (Padding * 2f),
            DrawRect.height - (Padding * 2f));

        Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y, trueDrawRect.width, LabelHeight), Label);

        var displayBox = new Rect(trueDrawRect.x, trueDrawRect.y + LabelHeight, trueDrawRect.height - LabelHeight,
            trueDrawRect.height - LabelHeight);
        Widgets.DrawBoxSolid(displayBox, CurrentColor);

        var sliderHeight = displayBox.height / 3f;
        var sliderLength = DrawRect.width - displayBox.width - 20;

        GUI.color = Color.red;
        var red = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBox.y, sliderLength, 16f), CurrentRed, 0f,
            1f);

        GUI.color = Color.green;
        var green = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBox.y + sliderHeight, sliderLength, 16f),
            CurrentGreen, 0f, 1f);

        GUI.color = Color.blue;
        var blue = GUI.HorizontalSlider(
            new Rect(displayBox.xMax + 10, displayBox.y + (sliderHeight * 2f), sliderLength, 36f), CurrentBlue, 0f, 1f);

        GUI.color = Color.white;
        //float blue = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBoxCenterY, (hairBox.width * 0.75f) - 20f, 16f), hairColor.b, 0f, 1f);


        if (!HasChanged(red, CurrentRed) && !HasChanged(blue, CurrentBlue) && !HasChanged(green, CurrentGreen))
        {
            return;
        }

        CurrentRed = red;
        CurrentGreen = green;
        CurrentBlue = blue;
        DoChange?.Invoke();
    }
}