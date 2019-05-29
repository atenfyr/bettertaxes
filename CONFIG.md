# Better Taxes Config File
The config file is written in JSON and has 4 fields, which are elaborated on below.
Make sure you always backup your custom configs.
## TaxRates
- This field is a dictionary which maps statements composed of "keys" to the amount of money given per paycheck represented in copper coins.
- A statement can either be just a single key, or can be separated by [logic gates](#logic-gates), which work together to combine the results of each key to determine the value of the statement itself. Each key evaluates to "true" or "false" based on what said key is looking for; for example, `Base.crimson` would return true if the world generated with crimson and false if it didn't.
- The statement with the highest coin value that evaluates to true is the one whose corresponding coin value is put into effect.
- Each key consists of a list and a condition, separated by a period. For example, given the key `Base.crimson`, `Base` is the list, and `crimson` is the condition.
- There are three built-in lists: `Base`, `Calamity`, and `Thorium`. The `Calamity` and `Thorium` lists will always return false regardless of the condition if the corresponding mod is not enabled.
- Here is a list of every single key with the list `Base`:
    - `Base.always`: Always set to true. Use this for a "default value."
    - `Base.never`: Always set to false. Mostly used internally, but you can also use this for debugging.
    - `Base.downedMechBossAny`: Any mechanical boss has been defeated.
    - `Base.downedMechBossAll`: All mechanical bosses have been defeated.
    - `Base.downedPlantBoss:` Plantera has been defeated.
    - `Base.downedGolemBoss`: Golem has been defeated.
    - `Base.downedAncientCultist`: The Lunatic Cultist has been defeated.
    - `Base.downedMoonlord`: The Moon Lord has been defeated.
    - `Base.expertMode`: The world is in expert mode.
    - `Base.crimson`: The world generated with crimson.
- Here is a list of every single key with the list `Calamity`:
    - `Calamity.cryogen`: Cryogen has been defeated.
    - `Calamity.leviathan`: The Leviathan has been defeated.
    - `Calamity.astrumDeus`: Astrum Deus has been defeated.
    - `Calamity.plaguebringer`: The Plaguebringer Goliath has been defeated.
    - `Calamity.ravager`: The Ravager has been defeated.
    - `Calamity.bumblebirb`: Bumblebirb has been defeated.
    - `Calamity.downedProvidence`: Providence, the Profaned Goddess has been defeated.
    - `Calamity.downedDoG`: The Devourer of Gods has been defeated.
    - `Calamity.downedYharon`: Yharon has been defeated.
    - `Calamity.downedSCal`: Supreme Calamitas has been defeated.
- Here is a list of every single key with the list `Thorium`:
    - `Thorium.downedPatchwerk`: Patch Werk has been defeated.
    - `Thorium.downedBloom`: Corpse Bloom has been defeated.
    - `Thorium.downedStrider`: The Borean Strider has been defeated.
    - `Thorium.downedFallenBeholder`: Coznix, the Fallen Beholder has been defeated.
    - `Thorium.downedLich`: The Lich has been defeated.
    - `Thorium.downedDepthBoss`: Abyssion, The Forgotten One has been defeated.
    - `Thorium.downedRealityBreaker`: The Ragnarök has been defeated.
### Logic Gates
- Multiple keys can be separated by a logic gate inside the same statement with "and," "or", and "not," as follows: `Base.crimson and Base.expertMode` `Base.downedMechBossAny or Base.crimson` `not Base.expertMode`
- You can also combine logic gates, as follows: `Base.expertMode and not Base.crimson` `Base.expertMode and Base.downedMechBossAll and Base.crimson`
### Custom Mod Support (Exception to the List/Condition format)
- If you would like to use a flag from another mod, the following format is used instead of the standard list/condition format: `<mod name>.<mod world>.<field>`
- For example, `CalamityMod.CalamityWorld.downedDoG` would evaluate to true once the Devourer of Gods has been defeated, and this would be what you would use if the Calamity list did not exist.
- This alternative format is still compatible with logic gates.
- You may need to look inside the source code of the mod you wish to retrieve values from in order to determine the name of the mod's world and the name of the field you wish to check.
### Adding Support in Your Mod
- If you are a mod author, you may also employ the usage of Mod.Call in order to add support for your mod. See [MODAUTHORS.md](MODAUTHORS.md).
## TimeBetweenPaychecks
- This field corresponds to the amount of time in seconds that is separated between paychecks, or when money is contributed to the Tax Collector's "storage." In vanilla Terraria, this is 60 seconds. Note that when the Enchanted Sundial is being used, one second corresponds to what would otherwise be a minute.
## MoneyCap
- This field corresponds to the maximum amount of money in copper coins that the Tax Collector can hold at one period of time. In vanilla Terraria, this is 10 gold, or 100,000 copper. Any money earned that would put the user over the cap is discarded.
## AddCustomDialog
- This field is a boolean (true or false) which corresponds to whether or not the new lines of dialog should be added to the Tax Collector's dialog pool. This is the only field that is not synced between the server and the client.
## IsFlexible
- This field is a boolean (true or false) which corresponds to whether or not this config file is subject to changes made by the mod. If you plan on inserting your own values into this config file, this should be set to false. Keep in mind that setting this to false will not allow the mod to make changes to your config file, such as adding fields presented by other mods or adding fields introduced in later updates.