> ℹ️ Godot v4.2.1.stable.mono.official [b09f793f5] is required to run this.

> ℹ️ This tool only works on Fabric modpacks. Support for Forge modpacks will come if it gets requested enough.

![Untitled](https://github.com/Valks-Minecraft-Plugins/FiestaMC/assets/6277739/5ffa1587-9612-4892-ade5-64462cf42dc5)

## What is this?
Lets say you just put together a modpack with over 400 mods but one of the mods is causing problems and you don't know which one it is.

The brute force solution is to remove half your mods and check to see if the problem still exists and keep doing this until the problem goes away. You will eventually find the mod that was causing so much trouble.

This works but when you have over 400 mods and you quickly remove 200 mods without giving it a second thought, you're also removing a ton of dependency mods that your new reduced mod list depends on. You could manually add them back but this is very time consuming.

This tool will auto add back all the required dependencies you removed and add them back to the mods folder for you.

## Finding Culprit Mods
1. Click the "Remove Half of Mods" button (this will move half of your mods from `mods` to `mods/temp`)
2. Manually create the new folder `mods/not culprit` (it is very important it be named exactly like this and inside the mods folder)
3. Run Minecraft. Depending on if the issue exists or not follow the appropriate steps below: *(Make sure to close Minecraft before doing these steps)*
   - (A) Issue **exists**: Cut all mods from `mods/temp` to `mods/not culprit` and repeat step 1
   - (B) Issue does **NOT** exist: Cut all mods from `mods` to `mods/not culprit`. Cut all mods from `mods/temp` to `mods` and repeat step 1
4. Repeat step 3 until you find the culprit mod. You may find that when there are a select few mods left, manually moving the mods will be faster

## Commands
- `export`: Copies the modlist (excluding API mods) to the clipboard in a format intended to be shared with whoever asked for the modlist in your modpack
- `clear`: Clears the console

## Roadmap
- [ ] Implement info command, this will show the dependencies and incompatible ('breaks') mods for this mod (e.g. `info <mod_name>`)
- [ ] Implement whitelist command, this will prevent `move` command from being able to move this mod in the mods folder (e.g. `whitelist <mod_name>`)
- [ ] Add a button that will start Minecraft
- [ ] Add a new button "Find Culprit Mod" which will constantly remove half of mods and start minecraft as many times needed until the culprit mod is found. This should only be used if the culprit mod is crashing the Minecraft instance. If the Minecraft instance runs for more than x seconds then the culprit mod will have to be narrowed down from the mods left that were not marked as 'not culprit'.
- [ ] Support Forge Modpacks
