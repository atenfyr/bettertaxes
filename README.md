# Better Taxes
Overhauls the Tax Collector to be more useful with two small changes:
* Increases the Tax Collector's money cap to 10 platinum rather than 10 gold.
* Increases profit depending on how far in the game you are:
    * At the start, rent is 50 copper a minute per NPC, or 6.3 gold an hour with 21 NPCs. (This is the vanilla amount regardless of progression.)
    * After defeating any mechanical boss, rent raises to 1 silver a minute per NPC, or 12.6 gold an hour with 21 NPCs.
    * After defeating Plantera, rent raises to 2 silver a minute per NPC, or 25.2 gold an hour with 21 NPCs.
    * After defeating Golem, rent raises to 5 silver a minute per NPC, or 63 gold an hour with 21 NPCs.
    * After defeating the Moon Lord, rent raises to 10 silver a minute per NPC, or 1.2 platinum an hour with 21 NPCs.
* Adds a config file stored in the save directory to modify the above values as well as the paycheck rate and money cap. When joining a server, the config file on the machine's installation of Terraria will be used and enforced.
* Adds a basic system in the config to add custom rent values for mods, given programming knowledge. See [CONFIG.md](CONFIG.md).
