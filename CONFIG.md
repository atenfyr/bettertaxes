# Better Taxes Config File
The config file is written in JSON and has 4 fields, which are elaborated on below:
- TaxRates
    - This key is an object which corresponds to the amount of money given per paycheck based on the progression of the game.
    - For each entry, you have the option of using the Base list of flags or looking at a flag from another mod.
    - You can separate the list from the flag with a period, as so: `Base.crimson`
    - Here is a list of every single Base flag:
        - `Base.always`: Always set to true. Use this for a "default value."
        - `Base.downedMechBossAny`: Any mechanical boss has been killed in the world.
        - `Base.downedMechBossAll`: All mechanical bosses have been killed in the world.
        - `Base.downedPlantBoss:` Plantera has been killed in the world.
        - `Base.downedGolemBoss`: Golem has been killed in the world.
        - `Base.downedAncientCultist`: The Lunatic Cultist has been killed in the world.
        - `Base.downedMoonlord`: The Moon Lord has been killed in the world.
        - `Base.expertMode`: The world is in expert mode.
        - `Base.crimson`: The world generated with crimson.
        - `Base.corruption`: The world generated with corruption.
    - If multiple flags are true, the one which provides the most money to the player is used.
    - If you would like to use a flag from another mod, the following format is used: `<mod name>.<mod world>.<value>`. For example, `CalamityMod.CalamityWorld.downedDoG` would evaluate to true once the Devourer of Gods has been defeated in that world. You may want to look inside the source code of the mod you wish to retrieve values from.
- TimeBetweenPaychecks
    - This key corresponds to the amount of time that is separated between paychecks, or when money is contributed to the Tax Collector's "storage." In vanilla Terraria, this is 60 seconds. Note that when the Enchanted Sundial is being used, one second corresponds to what would otherwise be a minute.
- MoneyCap
    - This key corresponds to the maximum amount of money, represented in copper coins, that the Tax Collector will hold at one period of time. In vanilla Terraria, this is 10 gold, or 100,000 copper.
- AddCustomDialog
    - This key is a boolean (true or false) which corresponds to whether or not the 3 new lines of dialog should be added to the Tax Collector's dialog pool.