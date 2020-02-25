using BetterTaxes.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace BetterTaxes
{
    public class InvalidConfigException : Exception
    {
        public static readonly string messageFormat = "Malformed config: {0}. See https://github.com/atenfyr/bettertaxes/wiki/Config-File-Format for more information.";
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

    /*
        SpecialInt only exists because I wanted to implement a slider which displays the correct units within ModConfig's DictionaryElement, but there's no way to do that without making a custom slider class, and you can't apply the attribute for the custom slider class onto int because int is a primitive
        The only way to circumvent this was to make a class which replicated the behavior of int so that I could apply attributes to it, which is why SpecialInt exists
    */
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

        public override int GetHashCode()
        {
            return Value.GetHashCode();
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

        public static readonly Dictionary<string, SpecialInt> VanillaTaxRatesDefaults = new Dictionary<string, SpecialInt>
        {
            {"Base.always", 50}
        };

        public static bool Equals(IDictionary<string, SpecialInt> x, IDictionary<string, SpecialInt> y)
        {
            if (x == null || y == null) return x == y;
            if (x.Count != y.Count) return false;

            foreach (string k in x.Keys)
            {
                if (!y.ContainsKey(k)) return false;
            }
            foreach (string k in x.Keys)
            {
                if (!x[k].Value.Equals(y[k].Value)) return false;
            }

            return true;
        }
    }

    public class BetterTaxesConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Presets")]
        [JsonIgnore]
        [Label("Classic")]
        [Tooltip("$Mods.BetterTaxes.Config.ClassicD")]
        public bool ClassicPreset
        {
            get
            {
                return StaticConstants.Equals(TaxRates, StaticConstants.TaxRatesDefaults) && IsFlexible && AddCustomDialog && EnableAutoCollect && (TimeBetweenPaychecks == 60) && (MoneyCap == Item.buyPrice(50, 0, 0, 0)) && (ExpertModeBoost == 1.5f);
            }
            set
            {
                if (value)
                {
                    IsFlexible = true;
                    AddCustomDialog = true;
                    EnableAutoCollect = true;
                    TimeBetweenPaychecks = 60;
                    MoneyCap = Item.buyPrice(50, 0, 0, 0);
                    ExpertModeBoost = 1.5f;
                    TaxRates = StaticConstants.TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
                }
            }
        }

        [JsonIgnore]
        [Label("Vanilla")]
        [Tooltip("$Mods.BetterTaxes.Config.VanillaD")]
        public bool VanillaPreset
        {
            get
            {
                return StaticConstants.Equals(TaxRates, StaticConstants.VanillaTaxRatesDefaults) && !IsFlexible && !AddCustomDialog && !EnableAutoCollect && (TimeBetweenPaychecks == 60) && (MoneyCap == Item.buyPrice(0, 10, 0, 0)) && (ExpertModeBoost == 1f);
            }
            set
            {
                if (value)
                {
                    IsFlexible = false;
                    AddCustomDialog = false;
                    EnableAutoCollect = false;
                    TimeBetweenPaychecks = 60;
                    MoneyCap = Item.buyPrice(0, 10, 0, 0);
                    ExpertModeBoost = 1f;
                    TaxRates = StaticConstants.VanillaTaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
                }
            }
        }

        [DefaultDictionaryKeyValue("")]
        [Header("Configuration")]
        [Tooltip("$Mods.BetterTaxes.Config.TaxRatesD")]
        public Dictionary<string, SpecialInt> TaxRates
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.IsFlexibleD")]
        [DefaultValue(true)]
        public bool IsFlexible
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.AddCustomDialogD")]
        [DefaultValue(true)]
        public bool AddCustomDialog
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.EnableAutoCollectD")]
        [DefaultValue(true)]
        public bool EnableAutoCollect
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.TimeBetweenPaychecksD")]
        [DefaultValue(60)]
        [SliderColor(183, 88, 25)]
        [Range(1, 300)]
        [Increment(5)]
        [Units(Unit.Time)]
        public SpecialInt TimeBetweenPaychecks
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.MoneyCapD")]
        [Range(0, 100000000)]
        [Increment(1000000)]
        [DefaultValue(50000000)]
        public SpecialInt MoneyCap
        {
            get;
            set;
        }

        [Tooltip("$Mods.BetterTaxes.Config.ExpertModeBoostD")]
        [DefaultValue(1.5f)]
        [SliderColor(135, 206, 250)]
        [CustomModConfigItem(typeof(BoostRangeElement))]
        public float ExpertModeBoost
        {
            get;
            set;
        }

        public override bool Autoload(ref string name)
        {
            name = "Config";
            return true;
        }

        public override ModConfig Clone()
        {
            var clone = (BetterTaxesConfig)base.Clone();
            clone.TaxRates = TaxRates?.ToDictionary(i => i.Key, i => i.Value);
            return clone;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (TaxRates == null) TaxRates = StaticConstants.TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
            if (IsFlexible && TaxRates.Count == 0) TaxRates = StaticConstants.TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
            if (MoneyCap > 2000000000) MoneyCap = 2000000000;
        }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            if (BetterTaxes.Instance.herosMod != null && BetterTaxes.Instance.herosMod.Version >= new Version(0, 2, 2))
            {
                bool theBool = false;
                object data = BetterTaxes.Instance.herosMod.Call("HasPermission", whoAmI, "ModifyBTConfig");
                if (data is bool) theBool = (bool)data;

                if (!theBool) message = Language.GetTextValue("Mods.BetterTaxes.Config.NoPerms").Replace(@"%1", "Modify Better Taxes Config");
                return theBool;
            }
            return true;
        }

        public override void OnLoaded()
        {
            TaxWorld.serverConfig = this;
        }
    }
}