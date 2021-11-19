# Cross-Level Enemies

Jump to:
* [TR2](#tr2)
* [TR3](#tr3)

# TR2

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

There is currently an inconsistency with the small spider being excluded from HSH. This was initially done as it was thought to be too awkward to chase these around the level. But the Rat could be considered equally as awkward, but this is not currently excluded.

~~Barkhang Monastery also has exclusions in place, but these are mainly attempts at workarounds for the freezing problem some players have experienced (#136, #158). We think this is caused by the number of active enemies, so unkillable enemies are excluded and testing found the level was most stable with all MonkWithLongStick and Mercenary1 enemies left in place. So the other type of monk and Mercenary2 are also excluded.~~
The exclusions in Opera House and Barkhang Monastery are related to freezing issues experienced by some players (#136, #158, #192). The cause is believed to be water creatures when they float after death, so each of these enemy types is banished from both levels.

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

# TR3

TR3 has bigger limits in terms of tile space (32 tiles) and `TRObjectTexture` entries (4096), which means we are less restricted when it comes to cross-level enemies compared with TR2. There are other restrictions however, mainly caused by some hard-coded features in the game.

## Boss Limits
* TonyFirehands (73) automatically triggers the flipmap on activation so he can only appear in River Ganges, Antarctica and All Hallows (these have no flipmaps). He will always appear at the end of Caves of Kaliya as standard.
* Puna (36) will currently be unsupported because he summons LizardMan (35) at the fixed coordinates of Temple of Puna. So while he does work in other levels, when he summons the first lizard, it spawns out of bounds and so Puna sits waiting indefinitely for Lara to kill it. Puna can be killed quickly so he never spawns a lizard to begin with, but for now he will only appear at the end of Temple of Puna.
* SophiaLee (57) will only appear in City because the fusebox behaviour is tied to the level sequence. She can potentially be randomized to something else in this level, but she will never appear elsewhere.
* Willard (49) has quite a few caveats:
  * He is not very intelligent outside of Meteorite Cavern as he relies on AIPath entities so that he knows where to go. He can appear in 9 other levels where paths have been defined for him (some levels are unsuitable because of the overall entity limit, which is still 256 in TR3).
  * He cannot be killed outside of Meteorite Cavern because he will only die when Lara has all four artefacts in her inventory. It is not feasible to make the artefacts available to pickup in any level other than sequence 19 (default Meteorite Cavern) because the game is hard-coded to end the level (i.e. like the end of Caves of Kaliya).
  * This also means that if Meteorite Cavern is not in its usual sequence, Willard is replaced because the artefacts would end the level (these are changed to different pickup types as their triggers are still needed to complete the level).
  * Willard doesn't shoot at Lara outside of Meteorite Cavern (lack of AICheck entities) but he will hurt her if she gets too close. He is therefore treated as docile (the UI has an option to enable this or not - if switched off, he won't appear cross-level).
  * So, the only time Willard will appear/behave normally is if Meteorite Cavern is in its usual sequence, and Willard ends up in the randomization pool here.

## General Enemy Points
There are further nuances with other enemy types in TR3.

* Monkeys (71) are hostile unless the Tiger model (28) is present.
* Monkeys always try to pick-up nearby small medis, unless they are hostile.
* Monkeys only pick-up Key4 (227) items if an AIModify entity is present.
* The Crash Site Mercenary (37) will attack any other enemy except other mercenaries.
* RXTechFlameLad (50) attacks any other enemy except others of the same type and RXGunLad (40). Only Crawler (42) will actually catch fire.
* RXTechFlameLad is hostile to Lara in any level that falls on Meteorite Cavern's sequence.
* Crash site Mercenary and RXTechFlameLad will fight each other. The Mercenary always wins.
* Raptors (288) will attack any other enemy, other than other raptors, unless Lara is the closest target.

## Spawn Points

* Crash Site Raptor spawns will be replaced with actual enemies, unless raptors are chosen in the enemy randomization pool. 
* Tinnos Wasps will always remain in Tinnos.

For now, spawn points will not be added to other levels. Raptors and Wasps can appear individually in other levels.

## Shoals

Note that the game crashes if a level contains piranhas or friendly shoals if _any_ level is out of normal sequence. The reason is currently unknown, so if level sequencing is randomized, all fish are disabled by default.

## Enemy Type Count

Because of the increased limits in TR3, most levels have had their number of enemy types increased as follows. This is a maximum rather than a guaranteed number of types (e.g. for Jungle, 6 models will always be imported, but no checks are performed to ensure at least one of each is assigned to entities). Levels in _italics_ remain unchanged.

| Level  | Num Enemy Entities | Old Num Types | New Num Types |
| ------------- | ------------- | ------------- | ------------- |
| JUNGLE.TR2 | 19 | 2 | 6 |
| TEMPLE.TR2 | 30 | 3 | 7 |
| QUADCHAS.TR2 | 37 | 3 | 5 |
| TONYBOSS.TR2 | 15 | 2 | 5 |
| SHORE.TR2 | 26 | 3 | 6 |
| CRASH.TR2 | 35 | 4 | 5 |
| RAPIDS.TR2 | 16 | 2 | 6 |
| _TRIBOSS.TR2_ | _9_ | _3_ | _3_ |
| ROOFS.TR2 | 26 | 4 | 5 |
| SEWER.TR2 | 29 | 3 | 6 |
| TOWER.TR2 | 20 | 4 | 5 |
| _OFFICE.TR2_ | _1_ | _1_ | _1_ |
| NEVADA.TR2 | 29 | 3 | 6 |
| COMPOUND.TR2 | 29 | 4 | 5 |
| AREA51.TR2 | 19 | 4 | 5 |
| ANTARC.TR2 | 31 | 3 | 6 |
| MINES.TR2 | 25 | 3 | 5 |
| CITY.TR2 | 13 | 2 | 6 |
| CHAMBER.TR2 | 7 | 3 | 4 |
| _STPAUL.TR2_ | _2_ | _2_ | _2_ |
