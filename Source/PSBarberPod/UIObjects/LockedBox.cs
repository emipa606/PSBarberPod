using UnityEngine;
using Verse;

namespace PS_BarberPod.UIObjects;

public class LockedBox
{
    public Rect DrawRect;
    public string Label;
    public float LabelHeight;
    public string LockedMessage;
    public float Padding;

    public bool Draw()
    {
        var trueDrawRect = new Rect(DrawRect.x + Padding, DrawRect.y + Padding, DrawRect.width - (Padding * 2f),
            DrawRect.height - (Padding * 2f));

        Widgets.Label(new Rect(trueDrawRect.x, trueDrawRect.y, trueDrawRect.width, LabelHeight), Label);

        //var backButRect = new Rect(trueDrawRect.x, trueDrawRect.y + this.LabelHeight, sideButWidth, trueDrawRect.height - this.LabelHeight);
        //if (Widgets.ButtonText(backButRect, "<") && this.Enabled)
        //{
        //    this.CurrentIndex--;
        //    if (this.CurrentIndex < 0)
        //        this.CurrentIndex = Options.Count - 1;
        //    hadChange = true;
        //}

        Widgets.ButtonText(trueDrawRect, Label, true, false, new Color(1f, 0f, 0f), false);

        //var nextButRect = new Rect(trueDrawRect.x + trueDrawRect.width - sideButWidth, trueDrawRect.y + this.LabelHeight, sideButWidth, trueDrawRect.height - this.LabelHeight);
        //if (Widgets.ButtonText(nextButRect, ">") && this.Enabled)
        //{
        //    this.CurrentIndex++;
        //    if (this.CurrentIndex >= Options.Count)
        //        this.CurrentIndex = 0;
        //    hadChange = true;
        //}

        //var centerButRect = new Rect(trueDrawRect.x + sideButWidth, trueDrawRect.y + this.LabelHeight, trueDrawRect.width - (sideButWidth * 2f), trueDrawRect.height - this.LabelHeight);
        //if (Widgets.ButtonText(centerButRect, this.Enabled ? GetOption(CurrentIndex)?.Label ?? "Failed To Load" : "None") && this.Enabled)
        //{
        //    var dropDownActions = new List<Action>();
        //    var dropDownList = new List<FloatMenuOption>();
        //    foreach (CarouselOption<T> opt in Options)
        //    {
        //        var label = opt.Label;
        //        dropDownActions.Add(delegate { this.SelectOption(opt); });
        //        dropDownList.Add(new FloatMenuOption(label, dropDownActions.Last(), MenuOptionPriority.Default, null, null, 0f, null, null));
        //    }
        //    Find.WindowStack.Add(new FloatMenu(dropDownList, null, false));
        //    hadChange = true;
        //}

        //if (this.DropDownChange)
        //{
        //    hadChange = true;
        //    this.DropDownChange = false;
        //}

        //if (hadChange && this.Enabled)
        //{
        //    if (this.DoChange != null)
        //        DoChange();
        //}

        return false;
    }
}