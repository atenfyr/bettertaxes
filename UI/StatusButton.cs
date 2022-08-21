using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BetterTaxes.UI;

internal class StatusButton : ModSystem {
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("DialogueTweak", out Mod dialogueTweak))
        {
            dialogueTweak.Call("AddButton",
                NPCID.TaxCollector,
                () => Language.GetTextValue("Mods.BetterTaxes.Status.Status"),
                "DialogueTweak/Interfaces/Assets/Icon_Default",
                () => {
                    if (Main.mouseLeft) {
                        int rawTax = ModHandler.parser.CalculateRate();
                        int adjustedTax = rawTax * UsefulThings.CalculateNPCCount();
                        double rate = TaxWorld.serverConfig.TimeBetweenPaychecks / Main.dayRate;
                        Main.npcChatText = Language.GetTextValue("Mods.BetterTaxes.Status.StatusMessage").Replace(@"%1", UsefulThings.ValueToCoinsWithColor(rawTax)).Replace(@"%2", UsefulThings.SecondsToHMSCasual((int)rate)).Replace(@"%3", UsefulThings.ValueToCoinsWithColor(adjustedTax * (3600 / rate))).Replace(@"%4", UsefulThings.CalculateNPCCount().ToString()).Replace(@"%5", UsefulThings.CalculateNPCCount() > 1 ? "s" : "");
                    }
                }
            );
        }
    }
}