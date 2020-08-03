using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_BarberPod
{
    public class CarouselOption<T>
    {
        public string Label;
        public int Index;
        public T Value;
    }

    public class CarouselBoxes<T>
    {
        public bool Enabled;
        public Rect DrawRect;
        public string Label;
        public float LabelHeight;
        public float SideButtonPerc;
        public List<CarouselOption<T>> Options;
        public int CurrentIndex;
        public bool DropDownChange;
        public float Padding;
        public bool IsLocked;
        public string LockedMessage;
        
        public T CurrentOptionValue {
            get
            {
                var option = GetOption(CurrentIndex);
                if (option != null)
                    return option.Value;
                else
                    return default(T);
            }
        }

        public CarouselBoxes(Rect DrawRect, string Label, float LabelHeight = 20f, float SideButtonPercent = 0.2f, float Padding = 5f)
        {
            this.DrawRect = DrawRect;
            this.Options = new List<CarouselOption<T>>();
            this.SideButtonPerc = SideButtonPercent;
            this.Label = Label;
            this.LabelHeight = LabelHeight;
            this.Enabled = true;
            this.Padding = Padding;
        }

        public Action DoChange;

        public void AddOption(string Label, T Value)
        {
            if (Options == null)
                Options = new List<CarouselOption<T>>();

            if (Options.Where(x => x.Label == Label).Any())
            {
                Log.Warning("PS_CarouselBoxes: Attemped to add duplicate label: " + Label);
                return;
            }

            var newOpt = new CarouselOption<T>
            {
                Label = Label,
                Index = Options.Count(),
                Value = Value
            };

            Options.Add(newOpt);
        }

        public CarouselOption<T> GetOption(int i)
        {
            if ((this.Options?.Count ?? 0) == 0)
            {
                Log.Error("PS_CarouselBoxes: Attempted to get option from null or empty Carousel");
                return null;
            }
            else if(this.Options.Count <= i)
            {
                Log.Error(string.Format("PS_CarouselBoxes: Attempted to get option from Carousel outside of range. Count: {0}, Requested: {1}", this.Options.Count(), i));
                return null;
            }
            return this.Options[i];
        }

        public void ClearOptions()
        {
            Options.Clear();
        }

        public bool Draw()
        {
            var trueDrawRect = new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (Padding * 2f), this.DrawRect.height - (Padding * 2f));
            
            var sideButWidth = trueDrawRect.width * this.SideButtonPerc;
            var hadChange = false;

            Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y, trueDrawRect.width, this.LabelHeight), Label);

            if (!this.IsLocked)
            {
                var backButRect = new Rect(trueDrawRect.x, trueDrawRect.y + this.LabelHeight, sideButWidth, trueDrawRect.height - this.LabelHeight);
                if (Widgets.ButtonText(backButRect, "<") && this.Enabled)
                {
                    this.CurrentIndex--;
                    if (this.CurrentIndex < 0)
                        this.CurrentIndex = Options.Count - 1;
                    hadChange = true;
                }


                var nextButRect = new Rect(trueDrawRect.x + trueDrawRect.width - sideButWidth, trueDrawRect.y + this.LabelHeight, sideButWidth, trueDrawRect.height - this.LabelHeight);
                if (Widgets.ButtonText(nextButRect, ">") && this.Enabled)
                {
                    this.CurrentIndex++;
                    if (this.CurrentIndex >= Options.Count)
                        this.CurrentIndex = 0;
                    hadChange = true;
                }

                var centerButRect = new Rect(trueDrawRect.x + sideButWidth, trueDrawRect.y + this.LabelHeight, trueDrawRect.width - (sideButWidth * 2f), trueDrawRect.height - this.LabelHeight);
                GUI.backgroundColor = Color.gray;
                if (Widgets.ButtonText(centerButRect, this.Enabled ? GetOption(CurrentIndex)?.Label ?? "Failed To Load" : "None") && this.Enabled)
                {
                    var dropDownActions = new List<Action>();
                    var dropDownList = new List<FloatMenuOption>();
                    foreach (CarouselOption<T> opt in Options)
                    {
                        var label = opt.Label;
                        dropDownActions.Add(delegate { this.SelectOption(opt); });
                        dropDownList.Add(new FloatMenuOption(label, dropDownActions.Last(), MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(dropDownList, null, false));
                    hadChange = true;
                }

                if (this.DropDownChange)
                {
                    hadChange = true;
                    this.DropDownChange = false;
                }

                if (hadChange && this.Enabled)
                {
                    if (this.DoChange != null)
                        DoChange();
                }
            }
            else
            {
                var lightGrey = new Color(0.6f, 0.6f, 0.6f);
                var darkGrey = new Color(0.3f, 0.3f, 0.3f);
                var textRed = new Color(0.9f, 0f, 0f);

                var lockedRect = new Rect(trueDrawRect.x, trueDrawRect.y + this.LabelHeight, trueDrawRect.width, trueDrawRect.height - this.LabelHeight);
                Widgets.DrawBoxSolid(lockedRect, darkGrey);
                GUI.color = lightGrey;
                Widgets.DrawBox(lockedRect);
                
                var style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 16;
                style.normal.textColor = Color.white;

                GUI.Label(lockedRect, this.LockedMessage, style);

                GUI.color = Color.white;
                GUI.contentColor = Color.white;
            }

            return !this.IsLocked && this.Enabled && hadChange;
        }

        public void SelectOption(CarouselOption<T> option)
        {
            this.CurrentIndex = option.Index;
            this.DropDownChange = true;
        }
    }
}
