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

https://github.com/DanzaG/TR2-Rando/blob/master/TRRandomizerCore/Resources/Shared/randomizer.json

### Underwater Corner Secrets
When placing secrets in corners underwater, there is a minimum distance from each wall the secret will need to be positioned - this is 130 units. Any closer to the wall and Lara won't pick the secret up.