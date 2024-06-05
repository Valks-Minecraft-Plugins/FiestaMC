> ℹ️ Godot v4.2.1.stable.mono.official [b09f793f5] is required to run this.

> ℹ️ This tool only works on Fabric modpacks. Support for Forge modpacks will come if it gets requested enough.

![Untitled](https://github.com/Valks-Minecraft-Plugins/FiestaMC/assets/6277739/a149d558-257b-4930-ba2c-7ffc6dbeefbd)

## What is this?
A tool that simplifies the tedious process of adding required dependency mods back to the mods folder when debugging faulty Minecraft mods.

Lets say you just put together a modpack with over 400 mods but one of the mods is causing problems and you don't know which one it is.

The brute force solution is to remove half your mods and check to see if the problem still exists and keep doing this until the problem goes away. You will eventually find the mod that was causing so much trouble.

This works but when you have over 400 mods and you quickly remove 200 mods without giving it a second thought, you're also removing a ton of dependency mods that your new reduced mod list depends on. You could manually add them back but this is very time consuming.

This tool will auto add back all the required dependencies you removed and add them back to the mods folder for you.

## Roadmap
- [x] Code the logic for "Remove Half of Mods" button and make sure it 100% works
- [ ] Disable "Remove Half of Mods" button when it's doing its thing
- [ ] Print text in the actual UI instead of relying on GD.Print
- [ ] Code the logic for "Mark Removed Mods as Culprit" and "Restore All Mods"
- [ ] Auto start Minecraft exe when the "Remove Half of Mods" is pressed
- [ ] Add a new button "Find Culprit Mod" which will constantly remove half of mods and start minecraft as many times needed until the culprit mod is found. This should only be used if the culprit mod is crashing the Minecraft instance. If the Minecraft instance runs for more than x seconds then the culprit mod will have to be narrowed down from the mods left that were not marked as 'not culprit'.
- [ ] Add some kind of fancy UI so the user is able to find what dependencies x mod has. Perhaps the user wants to remove a mod but they don't know if the mod is required to run other mods in their modpack.
- [ ] Read the "breaks" field in json and warn the user if the mod breaks any other mods
