# Secrets

Jump to:
* [Placement](#placement)
* [TR1](#tr1)
* [TR2](#tr2)
* [TR3](#tr3)
* [Generating Locations](#generating-locations)

## Placement
Refer to [zones](ZONES.MD) for details on how secret locations are selected.

## TR1
Original secret triggers are removed during randomization and artefacts are instead added to the level for Lara to collect. Once she collects all of the secrets in a level, a door will open to a new room where the rewards she would otherwise have collected can be found. When picking up a secret, a camera hint will show you the location of this room.

The imported artefacts become either a key type, puzzle type or quest type, whichever is available in the level to re-purpose.

If Secret Reward randomization is enabled, the types of the items in the reward room will be changed. Equally, if Secret randomization is not enabled, but Reward randomization is, the items will still be changed but will remain in their usual positions.

### Artefacts
The artefact types to collect will change per level. This is in place to ensure that the items stand out against the level's environment. There is only one artefact per level, and they are as follows.

| Level  | Artefact |
| ----   | -------- |
| Caves | Scion |
| Vilcabamba | Gold Bar |
| Lost Valley | Gold Idol |
| Qualopec | Gold Idol |
| Folly | Lead Bar |
| Colosseum | Lead Bar |
| Midas | Ankh (from Obelisk) |
| Cistern | Scion |
| Tihocan | Gold Idol |
| Khamoon | Lead Bar |
| Obelisk | Lead Bar |
| Sanctuary | Ankh (from Obelisk) |
| Mines | Gold Bar |
| Atlantis | Gold Idol |
| Pyramid | Gold Idol |

### Secret Count
If [TR1X](https://github.com/LostArtefacts/TR1X) is being used, the number of secrets to collect per level can be changed. When you start a level, check Lara's compass to find out how many secrets you need to collect.

The options are:

| Mode | Description |
|----|----|
| Default | Use the original secret count from the original level. |
| Shuffled | Use the secret count from another level - the total number of secrets in the game will remain the same. |
| Custom | Allocate a number of secrets of your choice (the maximum is currently 5 due to the current reward room implementations). |

----
# TR2
The standard Stone, Jade and Gold dragons will be placed in each level.

----
## TR3
Secret randomization logic in TR3 works in exactly the same way as TR1.

### Artefacts
Several artefacts are imported per level. This will depend on the secret count and the number of available allocation models that are available.

* Infada Stone
* Ora Dagger
* Element 115
* Eye of Isis
* Serpent Stone
* Hand of Rathmore

### Secret Count
Currently, the number of secrets per level is hard-coded to the level's original sequence.

----
## Generating Locations
Use trview to generate secret locations by making use of the available [randomizer settings](https://github.com/chreden/trview#randomizer-integration) feature. The file below should be copied locally to `%LOCALAPPDATA%/trview`.

https://github.com/LostArtefacts/TR-Rando/blob/master/TRRandomizerCore/Resources/Shared/randomizer.json

### Property guide
Not all properties are used in every circumstance, and their meanings can differ based on where they are used. Use the guide below for reference.

| Property | Usage | Description |
|-|-|-|
| Angle | Items, Enemies, Vehicles | Sets the Y rotation of an entity. |
| Difficulty | Secrets | Sets the difficulty level of a secret. |
| Entity Index | Enemies | For TR1 only, used to link normal enemies to Atlantean egg wall locations during randomization. |
| Entity Index | Secrets | Allows a secret to be linked to another pickup entity, such that the pickup location will be locked i.e. ignored in item randomization. |
| Invalidates Room | Items | Indicates that every sector in a room is invalid for general item positioning. |
| Key Item Group ID | Items | For TR1 and TR3, links locations to key items IDs to form zoning. See `TREntities` and `TR3Entities`. |
| Level State | Secrets | Indicates that a secret can only be used when a level is in a specific mirroring state e.g. for specific glitches. |
| Pack ID | Secrets | Used in [secret pack mode](#secret-packs) |
| Requires Damage | Secrets | Indicates that Lara will take damage collecting this secret; medipacks will be provided when this flag is detected. |
| Requires Glitch | Secrets | A glitch is needed to collect this secret - links to the UI option available to users. |
| Target Type | Vehicles | Links a location to a specific model ID, currently used for positioning randomized vehicles. |
| Validated | Items | If a room has `Invalidates Room` set to `true`, additional locations can be added with `Validated` set to `false` - this means that the entire room is excluded apart from those specific locations. |
| Validated | Secrets | For TR1 and TR3, if secrets are to be placed where there are already triggers, they will not be chosen during randomization unless `Validated` is selected. This allows for debugging to test that trigger behaviour remains as expected. |
| Vehicle Required | Secrets | Indicates that a vehicle is required to collect this secret, and so any planned randomization of existing vehicles in the level will not take place. Note that this will not automatically add a vehicle. |

## Secret Packs
The randomizer supports pre-defined secret packs, so allowing players to play with secrets that have been chosen by community members. To author your own secret pack, follow these steps.

1. Open your copy of `%LOCALAPPDATA%/trview/randomizer.json` and locate the `PackID` property. Add your pack title to the `options` array (this is what will be displayed both in trview and the randomizer itself).
2. Restart trview if you have it open.
3. Setup a local folder with each level for the game you wish to create a secret pack for. Ensure that the levels are vanilla, and that the level file names are all in uppercase.
4. In trview, open the first level in the folder you have created above.
5. Click on `Settings` and ensure that `Enable Randomizer Tools` is selected in the `General` tab.
6. Click on `Windows > Route`, then in the Route window, select `File > Open`. Select the relevant randomizer locations file for the game e.g. `Resources/TR1/locations.json` in your randomizer folder.
7. You will now see waypoints marking secret locations. You can add new waypoints and set the `PackID` property to your pack, or edit existing ones if applicable.
8. Clicking on the other level names in the Route window will open that level.
9. Once you have finished, click `File > Save` in the Route window.
10. You can now submit a pull request with the changed locations file and `randomizer.json` trview file if applicable.

Note that the `Requires Glitch` and `Difficulty` properties are ignored for secrets that are part of a pack - that is, they will be included regardless. However, you should ensure that these properties are correctly set anyway, as your secrets may be included in standard playthroughs, and so the user's choices in the randomizer should be honoured.

### Level State
Be sure to set the `Level State` property as well. If a secret within a pack only works when the level is mirrored, then this will enforce mirroring on that level. Equally, `Not Mirrored` means the secret only works when the level is in its normal state. `Any` is the default, and means the secret works in either state.

Be careful not to enforce a mirrored secret and non-mirrored in the same level. If this does happen, the randomizer will ignore your mirrored secret. You can create separate packs as an alternative.

Note that in normal mode, secrets whose states do not match the level are simply skipped and another is chosen.

Other environment changes should also be kept in mind when placing secrets; for example, rooms may be flooded/drained or some geometry may change. Refer to the specific level environment files in each case. Conditional changes can be made to either undo other environment modifications, or to add your own changes to fit your secrets. Conditional changes are performed following all other standard environment changes, aside from level mirroring, which is always performed last.

### Underwater Corner Secrets
When placing secrets in corners underwater, there is a minimum distance from each wall the secret will need to be positioned - this is 130 units. Any closer to the wall and Lara won't pick the secret up.
