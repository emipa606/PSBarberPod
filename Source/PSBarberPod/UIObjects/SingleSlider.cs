using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    public class SingleSlider
    {
        public Rect DrawRect;
        public float CurrentValue;
        public string Label;
        public float LabelHeight;
        public Action DoChange;
        public float Padding;

        public SingleSlider(Rect DrawRect, float StartValue, string Label, float LabelHeight = 20f, float Padding = 5f)
        {
            this.DrawRect = DrawRect;
            this.CurrentValue = StartValue;
            this.Label = Label;
            this.LabelHeight = LabelHeight;
            this.Padding = Padding;
        }

        private bool HasChanged(float a, float b)
        {
            return !((a > b - 0.0001f) && (a < b + 0.0001f));
        }

        public bool Draw()
        {
            var trueDrawRect = new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (Padding * 2f), this.DrawRect.height - (Padding * 2f));

            Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y + trueDrawRect.height - this.LabelHeight - 16f, trueDrawRect.width, this.LabelHeight), Label);

            var sliderBox = new Rect(trueDrawRect.x, trueDrawRect.y + trueDrawRect.height - 16f, trueDrawRect.width, trueDrawRect.height - this.LabelHeight);
            
            GUI.color = Color.white;
            float newValue = GUI.HorizontalSlider(sliderBox, this.CurrentValue, 0f, 1f);
            
            if (HasChanged(newValue, this.CurrentValue))
            {
                this.CurrentValue = newValue;
                if (this.DoChange != null)
                    DoChange();
                return true;
            }

            return false;
        }

    }
}
