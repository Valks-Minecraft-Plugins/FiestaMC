> ℹ️ Godot v4.2.1.stable.mono.official [b09f793f5] is required to run this.

> ℹ️ This tool only works on Fabric modpacks. Support for Forge modpacks will come if it gets requested enough.

![Untitled](https://github.com/Valks-Minecraft-Plugins/FiestaMC/assets/6277739/5ffa1587-9612-4892-ade5-64462cf42dc5)

## What is this?
Lets say you just put together a modpack with over 400 mods but one of the mods is causing problems and you don't know which one it is.

The brute force solution is to remove half your mods and check to see if the problem still exists and keep doing this until the problem goes away. You will eventually find the mod that was causing so much trouble.

This works but when you have over 400 mods and you quickly remove 200 mods without giving it a second thought, you're also removing a ton of dependency mods that your new reduced mod list depends on. You could manually add them back but this is very time consuming.

This tool will auto add back all the required dependencies you removed and add them back to the mods folder for you.

## Technical
- `temp` is the folder where mods are moved to when the "Remove Half of Mods" folder is pressed  
- `not culprit` is the same as `temp` folder except this is where you're suppose to put mods that don't contribute to the problem  

## Roadmap
- [x] Code the logic for "Remove Half of Mods" button and make sure it 100% works
- [ ] Pull dependencies from the "not culprit" folder and not just the "temp" folder
- [ ] Implement move command (e.g. `move 100% mods to not_culprit` and `move 50% temp to mods`)
- [ ] Implement info command, this will show the dependencies and incompatible ('breaks') mods for this mod (e.g. `info <mod_name>`)
- [ ] Implement whitelist command, this will prevent `move` command from being able to move this mod in the mods folder (e.g. `whitelist <mod_name>`)
- [ ] Add a button that will start Minecraft
- [ ] Add a new button "Find Culprit Mod" which will constantly remove half of mods and start minecraft as many times needed until the culprit mod is found. This should only be used if the culprit mod is crashing the Minecraft instance. If the Minecraft instance runs for more than x seconds then the culprit mod will have to be narrowed down from the mods left that were not marked as 'not culprit'.
- [ ] Support Forge Modpacks
