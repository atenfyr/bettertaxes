using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace BetterTaxes.UI
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class UnitsAttribute : Attribute
    {
        public Unit units;
        public UnitsAttribute(Unit unit)
        {
            units = unit;
        }
    }

    public enum Unit : byte
    {
        Coins,
        Time
    }

    public abstract class ChangedTextRangeElement<T> : RangeElement where T : IComparable<T>
    {
        public T min;
        public T max;
        public T increment;
        public IList<T> tList;

        public override void OnBind()
        {
            base.OnBind();
            tList = (IList<T>)List;
            TextDisplayFunction = () => TransformValue(GetValue(), MemberInfo.Name);

            if (tList != null) TextDisplayFunction = () => TransformValue(tList[Index], (Index + 1).ToString());
            if (LabelAttribute != null) TextDisplayFunction = () => TransformValue(GetValue(), LabelAttribute.Label);
            if (RangeAttribute != null && RangeAttribute.Min is T && RangeAttribute.Max is T)
            {
                min = (T)RangeAttribute.Min;
                max = (T)RangeAttribute.Max;
            }
            if (IncrementAttribute != null && IncrementAttribute.Increment is T) increment = (T)IncrementAttribute.Increment;
        }

        public virtual string TransformValue(T val, string label)
        {
            return label + ": " + val.ToString();
        }

        protected virtual T GetValue() => (T)GetObject();

        protected virtual void SetValue(object value)
        {
            if (value is T t) SetObject(value);
        }
    }

    public class SpecialIntRangeElement : RangeElement
    {
        public Unit units;
        public int min;
        public int max;
        public int increment;
        public IList<int> tList;
        public override int NumberTicks => ((max - min) / increment) + 1;
        public override float TickIncrement => (float)increment / (max - min);

        protected override float Proportion
        {
            get => (GetValue() - min) / (float)(max - min);
            set => SetValue((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
        }

        public override void OnBind()
        {
            base.OnBind();
            units = ConfigManager.GetCustomAttribute<UnitsAttribute>(MemberInfo, Item, List)?.units ?? Unit.Coins;
            tList = (IList<int>)List;
            TextDisplayFunction = () => TransformValue(GetValue(), MemberInfo.Name);

            if (tList != null) TextDisplayFunction = () => TransformValue(tList[Index], (Index + 1).ToString());
            if (LabelAttribute != null) TextDisplayFunction = () => TransformValue(GetValue(), LabelAttribute.Label);
            if (RangeAttribute != null && RangeAttribute.Min is int && RangeAttribute.Max is int)
            {
                min = (int)RangeAttribute.Min;
                max = (int)RangeAttribute.Max;
            }
            if (IncrementAttribute != null && IncrementAttribute.Increment is int) increment = (int)IncrementAttribute.Increment;
        }

        public string TransformValue(int val, string label)
        {
            string newLabel = label == "value" ? Language.GetTextValue("Mods.BetterTaxes.Config.Rent") : label;
            if (units == Unit.Time) return newLabel + ": " + UsefulThings.SecondsToHMS(val, "1 " + Language.GetTextValue("Mods.BetterTaxes.Config.Tick"));
            return newLabel + ": " + UsefulThings.ValueToCoins(val, (label == "value") ? ("0 " + Language.GetTextValue("LegacyInterface.18")) : Language.GetTextValue("Mods.BetterTaxes.Config.Unlimited"));
        }

        protected SpecialInt GetValue()
        {
            return (SpecialInt)GetObject();
        }

        protected void SetValue(object value)
        {
            if (value is int t) SetObject(new SpecialInt((int)value));
        }

        public SpecialIntRangeElement()
        {
            min = 1;
            max = 5000;
            increment = 50;
        }
    }

    public class BoostRangeElement : ChangedTextRangeElement<float>
    {
        public override int NumberTicks => (int)((max - min) / increment) + 1;
        public override float TickIncrement => (increment) / (max - min);

        protected override float Proportion
        {
            get => (GetValue() - min) / (max - min);
            set => SetValue((float)Math.Round((value * (max - min) + min) * (1 / increment)) * increment);
        }

        public BoostRangeElement()
        {
            min = 1f;
            max = 4f;
            increment = 0.1f;
        }

        public override string TransformValue(float val, string label)
        {
            if (val == 1) return label + ": " + Language.GetTextValue("Mods.BetterTaxes.Config.Disabled");
            return label + ": " + string.Format("{0:F1}", val) + "×";
        }
    }

    public class HappinessRangeElement : ChangedTextRangeElement<float>
    {
        public override int NumberTicks => (int)((max - min) / increment) + 1;
        public override float TickIncrement => (increment) / (max - min);

        protected override float Proportion
        {
            get => (GetValue() - min) / (max - min);
            set => SetValue((float)Math.Round((value * (max - min) + min) * (1 / increment)) * increment);
        }

        public HappinessRangeElement()
        {
            min = 0f;
            max = 1f;
            increment = 0.1f;
        }

        public override string TransformValue(float val, string label)
        {
            if (val == 0) return label + ": " + Language.GetTextValue("Mods.BetterTaxes.Config.Disabled");
            return label + ": " + string.Format("{0:F1}", val) + "×";
        }
    }
}
