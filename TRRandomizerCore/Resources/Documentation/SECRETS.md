# Secrets

Jump to:
* [TR1](#tr1)
* [TR2](#tr2)
* [TR3](#tr3)
* [Generating Locations](#generating-locations)

# TR1
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

### Zoning
Secrets are positioned by proximity so that they are fairly equally spread out across the level.

### Secret Count
If [Tomb1Main](https://github.com/rr-/Tomb1Main) is being used, the number of secrets to collect per level can be changed. When you start a level, check Lara's compass to find out how many secrets you need to collect.

The options are:

| Mode | Description |
|----|----|
| Default | Use the original secret count from the original level. |
| Shuffled | Use the secret count from another level - the total number of secrets in the game will remain the same. |
| Custom | Allocate a number of secrets of your choice (the maximum is currently 5 due to the current reward room implementations). |


----
# TR2
Zoning details for TR2 can be found at https://github.com/DanzaG/TR2-Rando/wiki/Zones#secrets

----
# TR3
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
# Generating Locations
Use trview to generate secret locations by making use of the available [randomizer settings](https://github.com/chreden/trview#randomizer-integration) feature. The file below should be copied locally to `%LOCALAPPDATA%/trview`.

https://github.com/LostArtefacts/TR-Rando/blob/master/TRRandomizerCore/Resources/Shared/randomizer.json

## Authored Secrets
The randomizer supports the idea of branded secrets by specific authors, so allowing players to pick those specific secrets in a playthrough. To author your own secrets, follow these steps.

1. Open your copy of `%LOCALAPPDATA%/trview/randomizer.json` and locate the `Author` property. Add your name to the `options` array (this is how your name will be displayed both in trview and the randomizer itself).
2. Restart trview if you have it open.
3. Setup a local folder with each level for the game you wish to author secrets for. Ensure that the levels are vanilla, and that the level file names are all in uppercase.
4. In trview, open the first level in the folder you have created above.
5. Click on `Settings` and ensure that `Enable Randomizer Tools` is selected in the `General` tab.
6. Click on `Windows > Route`, then in the Route window, select `File > Open`. Select the relevant randomizer locations file for the game.
7. You will now see waypoints marking secret locations. You can add new waypoints and set the `Author` property to your name, or edit existing ones if applicable.
8. Clicking on the other level names in the Route window will open that level.
9. Once you have finished, click `File > Save` in the Route window.
10. You can now submit a pull request with the changed locations file and `randomizer.json` trview file if applicable.

Note that the `Requires Glitch` and `Difficulty` properties are ignored for authored secrets - that is, they will be included regardless. However, you should ensure that these properties are correctly set anyway, as your secrets may be included in standard playthroughs, and so the user's choices in the randomizer should be honoured.

### Level State
Be sure to set the `Level State` property as well. If an authored secret only works when the level is mirrored, then this will enforce mirroring on that level. Equally, `Not Mirrored` means the secret only works when the level is in its normal state. `Any` is the default, and means the secret works in either state.

Be careful not to enforce a mirrored secret and non-mirrored in the same authored set. If this does happen, the randomizer will ignore your mirrored secret. You can create different authored sets instead as an alternative.

Note that in normal mode, secrets whose states do not match the level are simply skipped and another is chosen.

### TR2 Zoning
As authored secrets may not necessarily fall into the default [secret zones](https://github.com/LostArtefacts/TR-Rando/wiki/Zones#secrets), you should position your waypoints manually in stone-jade-gold order instead. For TR1 and TR3, this is not important due to the different zoning technique and the arbitrary artefact types.

### Underwater Corner Secrets
When placing secrets in corners underwater, there is a minimum distance from each wall the secret will need to be positioned - this is 130 units. Any closer to the wall and Lara won't pick the secret up.
