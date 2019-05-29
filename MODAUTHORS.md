# Adding Support for Your Mod
If you are a mod author and would like to add support for your mod into Better Taxes, you can use Mod.Call to achieve just this. For this article to make sense, you should read [CONFIG.md](CONFIG.md) first.

Here's a code snippet to give you an idea:

```cs
Mod betterTaxes = ModLoader.GetMod("BetterTaxes");
if (betterTaxes != null)
{
    // Adding a brand new list with conditions
    betterTaxes.Call("AddList", "YourListName", (Func<string, bool>)delegate(string name)
    {
        switch (name)
        {
            case "YourCondition1":
                return YourModWorld.downedYourBoss1;
            case "YourCondition2":
                return YourModWorld.downedYourBoss2;
            default:
                return false;
        }
    });

    // Adding a statement to flexible config files
    betterTaxes.Call("AddStatement", "YourListName.YourCondition1", 9999);
    betterTaxes.Call("Save");
}
```

There are three "methods" supported by Better Taxes:
- `AddList` adds a new list that can be used by the config file if the mod in question is installed. The first parameter is the name of the list, while the second parameter is a delegate that takes the condition as a string and returns a boolean. If a list already exists with the name requested, the old list will be replaced with the new list; as such, you should make the name of your list unique to your mod to avoid another mod accidentally overwriting your list.
- `AddStatement` adds a new statement to the config file's `TaxRates` field. The first parameter is the statement to add to the config file, and the second is the rent measured in copper coins to map to the statement.
- `Save` takes no arguments and simply saves all changes made by `AddStatement` to the disk. If you don't call this method after calling `AddStatement`, your changes will be lost on reload.

Keep in mind that `AddStatement` and `Save` are both ignored if the config file's `IsFlexible` field has been set to false.