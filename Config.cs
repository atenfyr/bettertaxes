using BetterTaxes.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria.ModLoader.Config;

namespace BetterTaxes
{
    public class InvalidConfigException : Exception
    {
        public static readonly string messageFormat = "Malformed config: {0}. See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md for more information.";
        public InvalidConfigException()
        {
        }

        public InvalidConfigException(string message) : base(string.Format(messageFormat, message))
        {
        }

        public InvalidConfigException(string message, Exception inner) : base(string.Format(messageFormat, message), inner)
        {
        }
    }

    public struct StaticConstants
    {
        public static readonly Dictionary<string, SpecialInt> TaxRatesDefaults = new Dictionary<string, SpecialInt> {
            {"Base.always", 50},
            {"Base.mechAny", 100},
            {"Base.plantera", 200},
            {"Base.golem", 500},
            {"Base.moonlord", 1000},
            {"Calamity.providence", 1250},
            {"Thorium.ragnarok", 1500},
            {"Calamity.dog", 1500},
            {"Calamity.yharon", 2500},
            {"Calamity.scal", 5000},
        };
    }

    public class SpecialIntConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is SpecialInt val)) val = new SpecialInt(100);
            writer.WriteValue(val.Value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SpecialInt val = (SpecialInt)(long)reader.Value;
            if (val == null) val = new SpecialInt(100);
            return val;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(SpecialInt).IsAssignableFrom(objectType);
        }
    }

    [SliderColor(204, 181, 72)]
    [JsonConverter(typeof(SpecialIntConverter))]
    [CustomModConfigItem(typeof(SpecialIntRangeElement))]
    public class SpecialInt : IComparable<SpecialInt>
    {
        [JsonIgnore]
        public int Value { get; set; }

        public SpecialInt(int value)
        {
            Value = value;
            if (Value < 0) Value = 0;
        }

        public SpecialInt()
        {
            Value = 0;
        }

        public static implicit operator SpecialInt(int value)
        {
            return new SpecialInt(value);
        }

        public static implicit operator int(SpecialInt value)
        {
            return value.Value;
        }

        public static explicit operator SpecialInt(long value)
        {
            return new SpecialInt((int)value);
        }

        public static explicit operator long(SpecialInt value)
        {
            return value.Value;
        }

        public int CompareTo(SpecialInt that)
        {
            return Value.CompareTo(that.Value);
        }

        public bool Equals(SpecialInt other)
        {
            return Value == other.Value;
        }
    }

    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Tooltip("Maps \"statements\" representing game progression to rent per NPC. See the GitHub page.")]
        public Dictionary<string, SpecialInt> TaxRates;

        [Tooltip("Should this config file be subject to automatic changes made by this mod and other mods?")]
        [DefaultValue(true)]
        public bool IsFlexible;

        [Tooltip("Should the new lines of dialog be added to the Tax Collector's dialog pool?")]
        [DefaultValue(true)]
        public bool AddCustomDialog;

        [Tooltip("Should the Tax Collector place his stored money into any personal storage tile in his room at midnight?")]
        [DefaultValue(true)]
        public bool EnableAutoCollect;

        [Tooltip("The amount of time between updates of the Tax Collector's money storage.")]
        [DefaultValue(60)]
        [SliderColor(183, 88, 25)]
        [Range(1, 300)]
        [Increment(5)]
        [Units(Unit.Time)]
        public SpecialInt TimeBetweenPaychecks;

        [Tooltip("The amount of money that the Tax Collector can hold at once.")]
        [Range(0, 100000000)]
        [Increment(1000000)]
        [DefaultValue(50000000)]
        public SpecialInt MoneyCap;

        [Tooltip("The amount to multiply tax rates by in an expert mode world. Set to 1 to disable.")]
        [DefaultValue(1.5f)]
        [SliderColor(135, 206, 250)]
        [CustomModConfigItem(typeof(BoostRangeElement))]
        public float ExpertModeBoost;

        public override ModConfig Clone()
        {
            var clone = (Config)base.Clone();
            clone.TaxRates = TaxRates?.ToDictionary(i => i.Key, i => i.Value);
            return clone;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (IsFlexible)
            {
                TaxRates = TaxRates?.Count > 0 ? TaxRates : StaticConstants.TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
                foreach (KeyValuePair<string, SpecialInt> entry in StaticConstants.TaxRatesDefaults)
                {
                    if (!TaxRates.ContainsKey(entry.Key))
                    {
                        TaxRates.Add(entry.Key, entry.Value);
                    }
                }
            }
            TimeBetweenPaychecks = TimeBetweenPaychecks > 0 ? TimeBetweenPaychecks : (SpecialInt)1;
        }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            message = "No changes allowed";
            return false;
        }

        public override void OnLoaded()
        {
            TaxWorld.serverConfig = this;
        }
    }
}