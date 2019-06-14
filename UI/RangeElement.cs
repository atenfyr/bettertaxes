using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.Config.UI;

namespace BetterTaxes.UI
{
    public abstract class ChangedTextRangeElement<T> : RangeElement where T : IComparable<T>
    {
        public T min;
        public T max;
        public T increment;
        public IList<T> tList;

        public override void OnBind()
        {
            base.OnBind();
            tList = (IList<T>)list;
            TextDisplayFunction = () => TransformValue(GetValue(), memberInfo.Name);

            if (tList != null) TextDisplayFunction = () => TransformValue(tList[index], (index + 1).ToString());
            if (labelAttribute != null) TextDisplayFunction = () => TransformValue(GetValue(), labelAttribute.Label);
            if (rangeAttribute != null && rangeAttribute.min is T && rangeAttribute.max is T)
            {
                min = (T)rangeAttribute.min;
                max = (T)rangeAttribute.max;
            }
            if (incrementAttribute != null && incrementAttribute.increment is T) increment = (T)incrementAttribute.increment;
        }

        public virtual string TransformValue(T val, string label)
        {
            return label + ": " + val.ToString();
        }

        protected virtual T GetValue() => (T)GetObject();

        protected virtual void SetValue(object value)
        {
            if (value is T t) SetObject(Utils.Clamp(t, min, max));
        }
    }

    public class CoinRangeElement : ChangedTextRangeElement<int>
    {
        public override int NumberTicks => ((max - min) / increment) + 1;
        public override float TickIncrement => (float)increment / (max - min);

        protected override float Proportion
        {
            get => (GetValue() - min) / (float)(max - min);
            set => SetValue((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
        }

        public override string TransformValue(int val, string label)
        {
            string newLabel = label == "value" ? "rent" : label;
            return newLabel + ": " + UsefulThings.ValueToCoins(val, "Unlimited");
        }

        public CoinRangeElement()
        {
            min = 0;
            max = 10000000;
            increment = 100000;
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
