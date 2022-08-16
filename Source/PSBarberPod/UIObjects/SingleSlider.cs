using System;
using UnityEngine;
using Verse;

namespace PS_BarberPod;

public class SingleSlider
{
    public float CurrentValue;
    public Action DoChange;
    public Rect DrawRect;
    public string Label;
    public float LabelHeight;
    public float Padding;

    public SingleSlider(Rect DrawRect, float StartValue, string Label, float LabelHeight = 20f, float Padding = 5f)
    {
        this.DrawRect = DrawRect;
        CurrentValue = StartValue;
        this.Label = Label;
        this.LabelHeight = LabelHeight;
        this.Padding = Padding;
    }

    private bool HasChanged(float a, float b)
    {
        return !(a > b - 0.0001f && a < b + 0.0001f);
    }

    public void Draw()
    {
        var trueDrawRect = new Rect(DrawRect.x + Padding, DrawRect.y + Padding, DrawRect.width - (Padding * 2f),
            DrawRect.height - (Padding * 2f));

        Widgets.Label(
            new Rect(trueDrawRect.x, trueDrawRect.y + trueDrawRect.height - LabelHeight - 16f, trueDrawRect.width,
                LabelHeight), Label);

        var sliderBox = new Rect(trueDrawRect.x, trueDrawRect.y + trueDrawRect.height - 16f, trueDrawRect.width,
            trueDrawRect.height - LabelHeight);

        GUI.color = Color.white;
        var newValue = GUI.HorizontalSlider(sliderBox, CurrentValue, 0f, 1f);

        if (!HasChanged(newValue, CurrentValue))
        {
            return;
        }

        CurrentValue = newValue;
        DoChange?.Invoke();
    }
}