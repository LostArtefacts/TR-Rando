# Cross-Level Enemies

The main issue with cross-level enemies is working within the limits of the original game. The first problem is tile space, with each level only able to contain at most 16 256x256 texture tiles. All enemy models are exported and available in the Models folder for importing, and included there are the texture segments needed to make up the enemy's meshes. If there is not enough tile space in a level for an enemy, it can't be imported.
To work around this, redundant textures are removed before enemy randomization, such as removing duplicates and removing the textures of old enemies.

The game is also limited to 2048 texture objects (`TRObjectTexture[]` in `TR2Level`). We again repurpose unused texture objects from removed enemies and duplicate entries, but this can still at times not be enough. The randomizer deals with import failures as described below.

### Handling Import Failures
Because it's not feasible to test every combination of enemies, `EnemyRandomizer` allocates 5 candidate enemy groups per level and proceeds to attempt to import the enemies until a candidate group succeeds. The 5th candidate group will have one less enemy than the others as a final attempt for cross-level enemies, but if all 5 attempts fail, native enemy randomization will take place. 

### Unsupported Enemies
There are some enemies that will never fit in some levels. This is primarily the dragon due to the explosion texture when it spawns (128x128), but other than that Home Sweet Home has the most restrictions because it is the most texture-heavy level. The following cannot fit into HSH:

* BlackMorayEel
* Eagle
* MarcoBartoli (i.e. the dragon and all of its dependencies)
* MercSnowmobDriver
* MonkWithLongStick
* MonkWithKnifeStick
* Shark
* TRex

The other exclusions in HSH are unkillable enemies. We also exclude the defaults (Doberman, MaskedGoon1, StickWieldingGoon1) because they are needed for the kill count to work. The required number of these enemy types are rendered outside the gate so the game has a kill target. We could in theory replace the final ShotgunGoon with a different enemy type but this remains untested. MaskedGoon2 and MaskedGoon3 are excluded as well because they depend heavily on MaskedGoon1, and StickWieldingGoon2 depends on StickWieldingGoon1.

Barkhang Monastery also has exclusions in place, but these are mainly attempts at workarounds for the freezing problem some players have experienced (#136, #158). We think this is caused by the number of active enemies, so unkillable enemies are excluded and testing found the level was most stable with all MonkWithLongStick and Mercenary1 enemies left in place. So the other type of monk and Mercenary2 are also excluded.

All unsupported enemies are defined in `_unsupportedEnemies` in `EnemyUtilities`.

### Adjusting Enemy Type Count
The `_enemyAdjustmentCount` dictionary in `EnemyUtilities` is used to adjust the default number of enemy types for specific levels. The signed integers here are added to the original number of enemy types when `EnemyRandomizer` is populating the list of candidates. The numbers here are based on the number of used tiles and `TRObjectTexture[]` length after deduplication has taken place.

### Required Enemies
The `_requiredEnemies` dictionary in `EnemyUtilities` lists enemies that must remain in the corresponding levels. These are mainly ones with specific triggers or hard-coded behaviour which means that replacing them would potentially cause softlocks.

## Restrictions
If an enemy is not defined in any of the dictionaries for the following three rules, it can spawn anywhere (respecting the other enemy rules of course).

### Enemies Restricted by Room
The `_restrictedEnemyZones` dictionary in `EnemyUtilities` maps enemies to room numbers for specific levels. This is aimed currently at MarcoBartoli to ensure the room has enough space for dealing with him, and the MercSnowmobDriver to prevent difficult or potentially impossible situations with spawn locations. The map is populated via `Resources\enemy_restrictions.json` and consists of `level name -> entity ID -> room number[]`.

### Enemies Restricted by Count (per level)
The `_restrictedEnemyLevelCounts` dictionary in `EnemyUtilities` defines the maximum number of times that a type of enemy can appear in any single level. Multiple dragons crash the game, and Winston is restricted to prevent too many active entities (although he is technically killable with the grenade launcher). MercSnowmobDriver is here as well purely from a difficulty perspective.

### Enemies Restricted by Count (entire game)
The `_restrictedEnemyGameCounts` dictionary in `EnemyUtilities` defines enemies that will appear a maximum number of times throughout the game. More specifically, the number here is the maximum number of levels they will appear in, not necessarily the total count throughout the game. For example, the Chicken is restricted to appear in 3 levels, but there is no limit for its appearance within those levels. The reason for restricting the Chicken like this was to prevent levels ending too soon, too often. But this should ideally be an option that players can change.

Winston will only appear in at most two levels as he is more of an Easter Egg than anything else.

## Guising
Currently only the bird monster is targeted for guising. Either type of monk or MaskedGoon1 can be disguised as the bird monster. This is achieved by importing the bird monster model so that the correct textures, meshes, animations etc are in place, but then changing the model ID of the `TRModel` object for the bird monster to be one of the guisers. This tells the game to use that model's behaviour, strength etc, but the in-level definition points to the bird monster appearance. All other enemies were tested as guisers but only these had animations that matched.

## Misc Rules
`EnemyRandomizer` has a few other checks in place as follows.
* The enemies at the end of Diving Area must be killable.
* Checks are done if docile bird monsters are in place in terms of eligible guisers (for example, we can't have both types of monk, MaskedGoon1 and docile bird monsters together).
* If an enemy spawns on a tile with an item, it must be killable. Outwith `EnemyRandomizer`, we ensure that item randomization takes place first for this reason.
* Enemies that spawn in water must be water enemies. Room draining is currently disabled.

## Difficulty
Enemy difficulty is calculated approximately in order to decide how much extra ammo to give for unarmed levels. The `_enemyDifficulties` dictionary in `EnemyUtilities` holds the categorisation of each enemy type. This is loosely based on enemy strength but also on how awkward the enemy is to deal with, for example the flamethrower.
