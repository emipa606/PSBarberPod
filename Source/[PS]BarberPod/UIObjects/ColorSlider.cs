using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    public class ColorSlider
    {
        public Rect DrawRect;
        public float CurrentRed;
        public float CurrentGreen;
        public float CurrentBlue;
        public string Label;
        public float LabelHeight;
        public Action DoChange;
        public float Padding;

        public Color CurrentColor { get { return new Color(this.CurrentRed, this.CurrentGreen, this.CurrentBlue); } }

        public ColorSlider(Rect DrawRect, Color StartColor, string Label, float LabelHeight = 20f, float Padding = 5f)
        {
            this.DrawRect = DrawRect;
            this.CurrentRed = StartColor.r;
            this.CurrentGreen = StartColor.g;
            this.CurrentBlue = StartColor.b;
            this.Label = Label;
            this.LabelHeight = LabelHeight;
            this.Padding = Padding;
        }

        public void SetCurrentColor(Color c)
        {
            this.CurrentRed = c.r;
            this.CurrentBlue = c.b;
            this.CurrentGreen = c.g;
        }

        private bool HasChanged(float a, float b)
        {
            return !((a > b - 0.0001f) && (a < b + 0.0001f));
        }

        public bool Draw()
        {

            // Hair Color Sliders
            //var hairColor = this.CurrentHairColor;
            //var colorBoxRect = new Rect(hairBox.x, hairBox.yMax + 5, hairBox.width * 0.25f, hairBox.width * 0.25f);
            
            var trueDrawRect = new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (Padding * 2f), this.DrawRect.height - (Padding * 2f));

            Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y, trueDrawRect.width, this.LabelHeight), Label);

            var displayBox = new Rect(trueDrawRect.x, trueDrawRect.y + this.LabelHeight, trueDrawRect.height - this.LabelHeight, trueDrawRect.height - this.LabelHeight);
            Widgets.DrawBoxSolid(displayBox, this.CurrentColor);

            var sliderHeight = displayBox.height / 3f;
            var sliderLength = this.DrawRect.width - displayBox.width - 20;
            var displayBoxCenterY = displayBox.y  + (displayBox.height * 0.5f);

            GUI.color = Color.red;
            float red = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBox.y, sliderLength, 16f), this.CurrentRed, 0f, 1f);

            GUI.color = Color.green;
            float green = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBox.y + sliderHeight, sliderLength, 16f), this.CurrentGreen, 0f, 1f);

            GUI.color = Color.blue;
            float blue = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBox.y + sliderHeight * 2f, sliderLength, 36f), this.CurrentBlue, 0f, 1f);

            GUI.color = Color.white;
            //float blue = GUI.HorizontalSlider(new Rect(displayBox.xMax + 10, displayBoxCenterY, (hairBox.width * 0.75f) - 20f, 16f), hairColor.b, 0f, 1f);


            if (HasChanged(red, this.CurrentRed) || HasChanged(blue, this.CurrentBlue) || HasChanged(green, this.CurrentGreen))
            {
                CurrentRed = red;
                CurrentGreen = green;
                CurrentBlue = blue;
                if(this.DoChange != null)
                    DoChange();
                return true;
            }

            return false;
        }

    }
}
