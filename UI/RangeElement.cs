using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.Config.UI;

namespace BetterTaxes.UI
{
    public abstract class ChangedTextRangeElement<T> : RangeElement where T : IComparable<T>
    {
        public int min;
        public int max;
        public int increment;
        public IList<T> tList;

        public override void OnBind()
        {
            base.OnBind();
            tList = (IList<T>)list;
            TextDisplayFunction = () => TransformValue(GetValue(), memberInfo.Name);

            if (tList != null) TextDisplayFunction = () => TransformValue(tList[index], (index + 1).ToString());
            if (labelAttribute != null) TextDisplayFunction = () => TransformValue(GetValue(), labelAttribute.Label);
            if (rangeAttribute != null && rangeAttribute.min is int && rangeAttribute.max is int)
            {
                min = (int)rangeAttribute.min;
                max = (int)rangeAttribute.max;
            }
            if (incrementAttribute != null && incrementAttribute.increment is int) increment = (int)incrementAttribute.increment;
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

    public class CoinRangeElement : RangeElement
    {
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
            tList = (IList<int>)list;
            TextDisplayFunction = () => TransformValue(GetValue(), memberInfo.Name);

            if (tList != null) TextDisplayFunction = () => TransformValue(tList[index], (index + 1).ToString());
            if (labelAttribute != null) TextDisplayFunction = () => TransformValue(GetValue(), labelAttribute.Label);
            if (rangeAttribute != null && rangeAttribute.min is int && rangeAttribute.max is int)
            {
                min = (int)rangeAttribute.min;
                max = (int)rangeAttribute.max;
            }
            if (incrementAttribute != null && incrementAttribute.increment is int) increment = (int)incrementAttribute.increment;
        }

        public string TransformValue(int val, string label)
        {
            string newLabel = label == "value" ? "rent" : label;
            return newLabel + ": " + UsefulThings.ValueToCoins(val, (label == "value") ? "0 copper" : "Unlimited");
        }

        protected CoinValue GetValue()
        {
            return (CoinValue)GetObject();
        }

        protected void SetValue(object value)
        {
            if (value is int t) SetObject(new CoinValue((int)value));
        }

        public CoinRangeElement()
        {
            min = 0;
            max = 5000;
            increment = 50;
        }
    }

    public class TimeRangeElement : ChangedTextRangeElement<int>
    {
        public override int NumberTicks => ((max - min) / increment) + 1;
        public override float TickIncrement => (float)(increment) / (max - min);

        protected override float Proportion
        {
            get => (GetValue() - min) / (float)(max - min);
            set => SetValue((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
        }

        public override string TransformValue(int val, string label)
        {
            return label + ": " + UsefulThings.SecondsToHMS(val);
        }

        public TimeRangeElement()
        {
            min = 1;
            max = 300;
            increment = 5;
        }
    }
}
