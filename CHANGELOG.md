## [Unreleased](https://github.com/LostArtefacts/TR-Rando/compare/V1.10.2...master) - xxxx-xx-xx
- added support for TR1X 4.14 (now the minimum version supported) (#803)
- added support TR2X (#821)
- added Spanish translations for TR1 (#800)
- added pickup items to Lara's Home for item randomization in TR2X (#832)
- fixed a crash at the end of Diving Area in TR2R (#814)
- fixed a potential key softlock in City of Khamoon if "large" range is selected and either return paths are disabled in classic, or playing remastered (#820)
- fixed the raptor spawns in Crash Site when enemies are randomized to guarantee that some appear when using the turret (#818)
- fixed enemy 211 in Area 51 potentially being untriggerable, and hence a potential softlock if carrying a key item (#816)
- fixed an unreachable secret in TR3R Madubu Gorge (#819)
- fixed uncontrolled SFX in TR2 and TR3 causing an error message during randomization (#827)

## [V1.10.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.10.1...V1.10.2) - 2024-12-06
- added support for TR1X 4.6 (#796)

## [V1.10.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.10.0...V1.10.1) - 2024-12-01
- fixed too many pickups on the same tile in TR2R patch 4 causing a crash (#793)

## [V1.10.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.9.3...V1.10.0) - 2024-11-05
- added (experimental) support for Linux (#143)
- added support for TRR patch 4 (#786)
- changed the number of secrets in TR3R Coastal Village to four to match the statistics (#775)
- fixed being unable to pick-up some items in TR3R High Security Compound (#783)
- fixed unreachable item locations in Coastal Village (#774)
- fixed the cobra not rendering properly in TR3R if used cross-level (#782)

## [V1.9.3](https://github.com/LostArtefacts/TR-Rando/compare/V1.9.2...V1.9.3) - 2024-09-29
- added an option to disable underwater corner secrets (#763)
- fixed dark pickup sprites in TR2R OG graphics (#760)
- fixed gun pickup sprites not showing properly in TR2R Floating Islands and Dragon's Lair OG graphics (#760)
- fixed all placement issues with underwater corner secrets in TR1-3 (#763)
- fixed monkey item drops causing crashes in TR3R (#768)
- removed support for 32-bit (#759)

## [V1.9.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.9.1...V1.9.2) - 2024-08-20
- added support for level sequence randomization in TR1R and TR2R (#756)
- added options to use textures from specific game areas only in TRR texture randomization (#726)
- changed vehicle randomization in TR2 so that it is now optional within item randomization (#750)
- fixed key item softlocks in remastered New Game+ when using shuffled item mode (#732, #734)
- fixed wireframe mode potentially exceeding texture limits and preventing levels from loading (#722)
- fixed docile bird monsters causing multiple Laras to spawn in remastered levels (#723)
- fixed the incomplete skidoo model in TR2R when it appears anywhere other than Tibetan Foothills (#721)
- fixed secrets on triangle portals not triggering in TR3 (#727)
- fixed secrets in 40 Fathoms all generally appearing too close to the start of the level (#729)
- fixed the Jade secret appearing before the Stone in TR2R Floating Islands (#729)
- fixed being unable to collect secret artefacts in TR3R High Security Compound (#737)
- fixed (the lack of) prisoners in Area51 crashing the game when loading a save (#739)
- fixed some enemies in TR3 causing triggers for other objects to break e.g. Crash Site room 72 (#742)
- fixed secret models in TR3R Aldwych appearing offset from their actual location (#744)
- fixed a crash in Palace Midas when randomizing enemies natively (#746)
- fixed being unable to shoot the scion in Atlantis if using the skip, without backtracking for its trigger when the T-rex or Adam is present (#746)
- fixed an awkwardly positioned egg in Sanctuary of the Scion that could prevent being able to reach a switch (#748)
- fixed the missing UI option to control adding extra pickups in TR1R (#754)
- improved data integrity checks when opening a folder and prior to randomization (#719)
- removed birds from the list of enemies that can drop items in TR2 and TR3 (#752)

## [V1.9.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.9.0...V1.9.1) - 2024-06-23
- fixed a missing reference related to Willard, which would cause enemy randomization to fail if he was selected (#712)
- fixed the "show error folder" link in popup message windows not working (#713)
- fixed being unable to randomize enemies natively in TR1R (#716)
- fixed the key in Jungle appearing mid-air or inside walls (#717)
- fixed a pickup issue in Natla's Mines that could cause a crash (#718)
- restored the option to replace required enemies in TR1R (#714)

## [V1.9.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.8.4...V1.9.0) - 2024-06-22
- added support for TR1X V4 (#626)
- added support for TR I-III Remastered (#614)
- added an option to shuffle items rather than randomize their types and locations in each level (#625)
- added an option to control weapon allocation in item randomization (#690)
- added an option to move enemies such as eels, whose placement can lead to forced damage or difficulty in passing (#311)
- added an option to stack rewards with secrets in TR1 and TR3, rather than using reward rooms (#687)
- added Lara's assault course outfit in TR2 for outfit randomization (#672)
- added gun holsters to Lara's robe outfit in TR2 (#672)
- added separate secret audio for TR1 and TR3 when not using reward rooms (#687)
- added Finnish, Portuguese, and Swedish translations to TR1 and added all supported language translations to TRUB (#701)
- fixed several potential key item softlocks in TR2 (#691)
- fixed a key item softlock in Crash Site (#662)
- fixed incorrect item and mesh positions in Home Sweet Home when mirrored (#676)
- fixed uncontrolled SFX in gym/assault course levels not being linked to the correct setting (#684)
- fixed the scion being difficult to shoot if Lara only has the shotgun (#696)
- fixed character encoding in TR3 gamestrings, which was causing data loss in some cases (#698)
- improved the layout of some options in the UI (#694)

## [V1.8.4](https://github.com/LostArtefacts/TR-Rando/compare/V1.8.3...V1.8.4) - 2024-02-12
- fixed item locking logic so that secrets that rely on specific enemies will always be obtainable (#570)
- fixed a crash at the end of Diving Area if a skidoo driver is replaced by a pickup (#604)
- fixed the submarine in the Diving Area cutscene using Lara's meshes (#605)
- fixed some awkwardly placed enemies in TR2, which could either block puzzle slots/keyholes or otherwise prove difficult to kill (#606)
- fixed pickup item lighting in TR2 (#607)
- fixed a secret in Temple of Xian that requires a glitch but was marked as glitchless (#608)
- fixed an item location in Wreck of the Maria Doria that required forced flame damage (#609)

## [V1.8.3](https://github.com/LostArtefacts/TR-Rando/compare/V1.8.2...V1.8.3) - 2024-01-21
- fixed incorrect items sometimes being allocated as secret rewards in Thames Wharf (#597)
- fixed an inaccessible secret in Offshore Rig when the main area is drained (#597)
- fixed a key item softlock in Floating Islands (#599)

## [V1.8.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.8.1...V1.8.2) - 2024-01-14
- fixed some pickups appearing in unreachable locations in TR2 (#591)
- fixed inaccurate pickup statistics in TR1 when playing one-item mode (#591)
- fixed additional secret rewards being placed in non-existent rooms when not randomizing secrets in TR1 (#591)
- fixed a secret becoming invisible after blowing up the final area in Bartoli's Hideout (#591)
- fixed Torso-sized eggs not being randomized in TRUB (#593)

## [V1.8.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.8.0...V1.8.1) - 2023-12-15
- fixed floor data issues in mirrored levels in TRUB (#583)
- fixed a softlock in Temple Ruins if using the return paths in a certain way (#584)
- fixed "Restore to Default" missing gold level files and the assault course (#585)

## [V1.8.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.7.3...V1.8.0) - 2023-12-10
- added support for Unfinished Business and playing in combined game mode for TR1 (#580)
- added a TR2 secret pack (Eycore) (#559)
- added a TR2 secret pack (Towandaa) (#558)
- added more return paths to TR2 and TR3 (#563)
- added an option to control whether or not key items can be allocated to enemies (#474)
- added an option to control the range in which key items can appear (#474)
- added an option to control whether or not key items can be placed in locations that rely on return paths (#474)
- added an option to control key item continuity e.g. The Seraph being placed as an item in Barkhang Monastery if The Deck hasn't been visited (#474)
- added item randomization to Home Sweet Home, provided the level starts with weapons and ammo already (#474)
- added support for TR1X 3.0, including randomized enemy item drops (#572)
- added support for Lara's gym outfit in all levels in TR1 (#580)
- fixed spelling mistakes in TR1 French gamestring localization (#560)
- fixed a key item softlock in Diving Area (#564)
- fixed Pierre not spawning if he is positioned underwater (he will now always spawn on land) (#580)
- fixed certain enemy combinations causing import failures in TR2 (#577)
- improved secret reward allocation in TR1 to be fairer (#580)
- improved changelog, readme and contributing documentation
- improved regular item, key item, and secret item location generation and selection in TR1-3 (#474)
- removed TombATI support (#572)
- replaced purist mode with explicit options for adding return paths, fixing OG bugs, and fixing shortcuts (#563)

## [V1.7.3](https://github.com/LostArtefacts/TR-Rando/compare/V1.7.2...V1.7.3) - 2023-09-30
- added a secret pack mode, to allow picking a selection of secrets from specific authors (#510)
- added several new secret locations to TR1 (#510)
- added an option to clone enemies when using TR1X (#536)
- added an option for extra weapons/ammo/medipacks in unarmed levels (previously enabled by default) (#535)
- added support for TR1X 2.16 (#551)
- fixed a softlock in Ice Palace when ladder randomization was not enabled and unconditional bird monsters were enabled (#497)
- fixed the original final secret in Great Pyramid not triggering (only applicable when not randomizing secrets) (#495)
- fixed certain secrets becoming embedded in the floor in Aldwych (#528)
- fixed an issue with fish and piranhas causing crashes in TR3 or making areas impossible to complete (#530)
- fixed crystals being converted into pickups in TR3 if they are secret reward items (#541)
- fixed issues with some secrets that require vehicles in TR2, where the vehicle could become unreachable (#550)
- improved medipack allocation in TR3 levels with cold water (#511)

## [V1.7.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.7.1...V1.7.2) - 2023-06-15
- added weather randomization (requires tomb3) (#461)
- added new secret locations to TR1 (#458)
- added a return path to The Cistern and an additional return path to Tomb of Tihocan to reach the start (#458)
- added return paths to Temple Ruins and Lost City of Tinnos to allow repeat exploration (#465)
- added support for TR1X 2.15 (#469)
- added support for randomizing Torso in Great Pyramid (#490)
- fixed additional OG room texture/face issues (#460)
- fixed issues reading the gameflow when using tomb3 (#461)
- fixed friendly monkeys causing crashes when Lara mounted a vehicle (#466)
- fixed key item placement issues in Diving Area and Temple of Xian (#465)
- fixed key item placement issues in Temple Ruins and Lost City of Tinnos (#465)
- fixed texture corruption on Lara in Great Pyramid when using the Steam/GOG version (#476)
- fixed a potential softlock in Nevada when it's cold (#485)
- fixed an error during randomization when only Tony is selected as the available enemy in TR3 (#488)
- fixed Pierre not being killable in City of Khamoon when he becomes the restored PS1 enemy in room 25 (#490)
- improved specific enemy triggers in Sanctuary of the Scion (#460)
- improved the colour variants for TR1 texture randomization (#459)
- improved SkateKid and Natla restrictions in line with OG bug fixes resolved in TR1X (#490)
- restored Bacon Lara in Atlantis when the level is mirrored (TR1X only) (#490)

## [V1.7.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.7.0...V1.7.1) - 2023-04-17
- fixed pushblocks that drop from the air causing floor height issues after saving and loading (#445)
- fixed an error during randomization when no SFX categories were selected (#446)
- fixed a key item in Natla's Mines not having a name in the inventory (#449)
- fixed a softlock in Atlantis when using the secret return path (#450)
- fixed some enemy exclusions and difficulty options not being properly adhered to (#451)

## [V1.7.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.6.0...V1.7.0) - 2023-04-07
- added environment randomization to TR1 (#360)
- added texture randomization to TR1 (#356)
- added gamestring randomization (and translations) to TR1 (#359)
- added a partial gym outfit to TR1 (#429)
- added Lara's braid to cutscenes if present in the parent level (#429)
- added an option to toggle randomization of Lara's outfit textures (#441)
- added an option to toggle randomization of enemy textures (#441)
- added an option to randomize the pitch of all sound effects (#397)
- added 169 new secret locations (#353)
- added a guaranteed mode for glitched and/or hard secrets (#411)
- added an option to wireframe mode to highlight certain triggers and death tiles (#404)
- added an option to wireframe mode to keep certain objects solid, such as levers and puzzle slots (#404)
- added new enemy textures and variants (#417)
- added data integrity checks when a folder is first opened (#392)
- added return paths to TR1 levels where possible (#360)
- added a UI warning when randomizing all options (#389)
- added an option in the UI to reset all settings to default (#419)
- fixed the credits track in TR1 not being randomized (#397)
- fixed camera issues (box generation) in copied rooms (#360)
- fixed the door after the lava flow room in Great Pyramid not closing when mirrored (#360)
- fixed sprite texture randomization causing mesh texture corruption in some levels (#395)
- fixed landmark import failures causing room texture issues (#399)
- fixed unreachable items in St. Francis' Folly, Natla's Mines and Atlantis (#400)
- fixed OG room texture/face issues (#360)
- fixed secrets not being added to levels when using TombATI (#353)
- fixed a proximity testing issue when placing secrets (#353)
- fixed unreachable secrets in Tomb of Qualopec and Tomb of Tihocan (#353)
- fixed an unreachable secret in Ice Palace (#412)
- fixed unreachable secrets in High Security Compound, Lud's Gate, RX-Tech Mines and Lost City of Tinnos (#412)
- fixed secret items in Tomb of Qualopec having no name in the inventory (#353)
- fixed WalkToItems (TR1X setting) affecting the ability to collect secrets on slopes (#353)
- fixed door texture hints in Midas being lost in wireframe mode (#353)
- fixed Crash Site swamp safe tile hints being lost in wireframe mode (#404)
- fixed the Midas door lever order not matching the texture hints in mirrored mode (#360)
- fixed low resolution wireframe textures appearing in some rooms where textures are shared with meshes (#356)
- fixed one of the fuse items in Natla's Mines not being included in item randomization (#405)
- fixed the positioning of spikes in Coastal Village and Madubu Gorge (#406)
- fixed a potential softlock in The Cistern with key item randomization enabled (#360)
- fixed Atlantean eggs not being included in unarmed level difficulty ratings, potentially leading to softlocks (#409)
- fixed a softlock in Obelisk of Khamoon where two key items with triggers could end up on the same tile (#414)
- fixed the delayed/offset dart sound effect when used as a weapon sound (#427)
- fixed outfit inconsistencies between levels and cutscenes (#429)
- fixed multiple pickups under monkeys, potentially leading to softlocks (#420)
- improved documentation and screenshots on GitHub (#413)
- improved the positioning of the Lost Valley and Obelisk of Khamoon secret reward rooms (#353)
- improved the colour of Lara's braid to match the rest of her hair exactly (#356)
- improved the handling of edge secrets to ensure triggers are copied into neighbouring tiles (#259)
- removed untriggered enemies from Opera House and Barkhang Monastery (#345)
- restored the OG death animation SFX for Pierre, Larson, SkateboardKid and Natla (#397)
- restored the ability for the OG explosion SFX to remain, otherwise these are never used (#397)
- restored the intended OG ceiling textures in Colosseum (#360)
- restored the UI option for performing enemy mesh swaps (#314)

## [V1.6.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.5...V1.6.0) - 2022-10-21
- added support for TR1 (#365)
- added vehicle randomization for TR2 (#369)
- added an option to retain original main level textures (#329)
- added new secrets to TR2 (#371)
- added an option to minimize dragon appearances (#381)
- added Seraph randomization to TR2 (#217)
- added an option to randomize pickup item sprites (#78)
- fixed issues with wheel doors when the dragon is present (#330)
- fixed the allocation of required enemies to drop items when not always needed (#331)
- fixed further limit issues caused by too many skidoo drivers (#272)
- fixed wireframe ladders appearing sideways at times (#337)
- fixed an unreachable secret in Tibetan Foothills (#362)
- fixed texture bleeding on some flame sprites in TR2 (#363)
- fixed potentially missing the mines trigger in Venice (OG bug) (#364)
- fixed spelling mistakes in TR2 French gamestring localization (#367)
- fixed secret texture mismatches in TR2 (#377)
- improved environment modification JSON specification (#334, #335)

## [V1.5.5](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.4...V1.5.5) - 2022-04-29
- added an option to highlight ladders and monkey bars in wireframe mode (#301)
- added an option to randomize enemy meshes (#314)
- added support in every level for the dragon, along with an option to maximize dragon spawns (best effort attempt) (#310)
- added an option to select which enemy types to include in enemy randomization (#272)
- added an option for unconditional bird monsters (these are aggressive and do not end the level) (#326)
- added an option for uncontrolled sound effect randomization (#326)
- fixed missing weapon animation models in TR3 Lara's Home (#312)
- fixed water texture corruption in Floating Islands in wireframe mode (#305)
- fixed some unreachable secrets in 40 Fathoms and Tibetan Foothills after room draining/flooding (#307)
- fixed the lighting on some secrets (#308)
- fixed some spelling mistakes in TR2 French gamestring localization (#309)
- fixed some unreachable item locations in Wreck of the Maria Doria (#302)
- fixed multiple dragons spawning at the end of Ice Palace (#310)
- fixed the detonator animation being overwritten in Bartoli's Hideout (#316)
- fixed the missile room in Area 51 being impossible to complete when mirrored (#317)
- fixed a rare texture corruption issue related to Lara's extra animations (#321)
- fixed a softlock in Offshore Rig caused by environment randomization (#323)
- fixed an error during landmark import under rare conditions (#324)
- fixed a potential lost enemy in Floating Islands (#325)
- improved wireframe mode by adding full support for transparency (#301)
- improved and added landmarks for community involvement

## [V1.5.4](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.3...V1.5.4) - 2022-01-04
- added mirrored level randomization to TR3 (#241)
- added a wireframe mode option (#301)
- added further texture randomization to TR3 (#246)
- fixed caustic and wave effects being lost on water and swamp rooms in TR3 (#296)
- improved secret JSON definitions to allow setting a mirrored level state (#295)
- improved texture allocation in Home Sweet Home to allow for additional enemy model import (#299)

## [V1.5.3](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.2...V1.5.3) - 2021-12-18
- added start position randomization to TR3 (#245)
- added an option to have night-mode override the sunset effect in TR2 (#283)
- fixed softlocks in High Security Compound when prisoners are not present (#271)
- fixed texture corruption on Lara's Nevada outfit (#273)
- fixed key items sometimes causing floor data changes which made the items inaccessible (#243)
- fixed a crash in TR2 levels where flame sprites have been deduplicated (#275)
- fixed the entity limit being breached in Aldwych in certain scenarios (#276)
- fixed missing secret artefact models in the inventory (#277)
- fixed flamethrowers appearing in a specific location in Floating Islands to avoid an overly difficult scenario (in default mode) (#278)
- fixed texture corruption on Lara's braid in Caves of Kaliya (#246)
- fixed the reward room being inaccessible in a rare case in River Ganges (#280)
- fixed monkeys causing crashes when positioned beside AIAmbush items (#286)
- fixed a crash when Shiva statues are in a level without monkeys (#289)
- fixed invisible cobras (#291)
- fixed an issue with night-mode and visual effect randomization conflicting with each other in TR3
- fixed sound effect randomization on the skidoo (#294)
- improved performance during enemy randomization (#290)
- improved the handling of shifting key item triggers (#288)
- improved environment condition JSON (#292)
- improved unused entity reuse in Meteorite Cavern
- restored MPWithMP5 to the cross-level enemy pool

## [V1.5.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.1...V1.5.2) - 2021-12-04
- added enemy randomization to TR3 (#240)
- added item randomization to TR3 (#243)
- added texture randomization to TR3 (#246)
- added an option to control the TR3 globe behaviour (#239)
- added an option to disable reward room cameras (#239)
- added sound effect randomization to TR3 (#247)
- added the ability to define "damaging" secrets, which will come with medipacks (#264)
- fixed secrets on trapdoors not being triggered (#250)
- fixed texture colour differences after importing models or texture replacements (#255)
- fixed an issue during randomization if randomizing night-mode but not randomizing textures
- fixed an issue during randomization if a particular variant of Lara's TR2 bomber jacket was selected
- fixed texture bleeding in some font variants in TR2
- fixed several texture corruption issues in TR3 (#257)
- fixed incorrect reward items being allocated in Thames Wharf when using the Japanese version (#266)
- fixed a potential softlock in Antarctica when the level is not cold (#267)
- fixed an overly difficult enemy scenario in RX-Tech Mines (#269)
- improved secret trigger allocation (#248, #259)
- improved modifications needed in TR3 based on level sequencing (#219)

## [V1.5.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.5.0...V1.5.1) - 2021-11-10
- fixed several issues with TR3 secret reward rooms (#230)
- fixed a soflock in City when the level is off-sequence (#231)
- fixed spikes in South Pacific not being set to the correct height when the levels are off-sequence (#232)
- fixed a locking issue that could occur with Lara when randomizing outfits (#234)
- fixed issues with UPV locations in cold levels (#235)
- fixed an unreachable secret in Area 51 (#233)

## [V1.5.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.4.2...V1.5.0) - 2021-11-08
- added initial support for TR3 (#220, #224, #225, #226, #229)
- added an initial set of secrets for TR1 (#223)
- fixed a crash that could occur in one-item limit mode (#222)
- improved location JSON definitions (#221)

## [V1.4.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.4.1...V1.4.2) - 2021-10-09
- added a one-item limit mode to item randomization (#215)
- added randomization of sound effects (#218)
- added a purist mode option to avoid implementing some default environment changes (#218)
- improved unarmed level ammo allocation by adding directly to the inventory to avoid savegame issues (#218)

## [V1.4.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.4.0...V1.4.1) - 2021-10-02
- fixed pathfinding issues introduced by environment randomization (#214)
- fixed pathfinding issues in Catacombs of the Talion (OG bug) (#214)
- fixed several issues in environment randomization, such as inaccessible secrets (#214)

## [V1.4.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.3.3...V1.4.0) - 2021-08-01
- added difficulty options for enemy randomization (#196)
- added environment randomization (#200)
- added translations for gamestring randomization
- added further texture randomization
- added the ability to choose the level of darkness in night-mode
- fixed start position randomization sometimes causing secrets to become unreachable
- improved dragon interaction and behaviour in some cases
- migrated projects to SDK format (#201)

## [V1.3.3](https://github.com/LostArtefacts/TR-Rando/compare/V1.3.2...V1.3.3) - 2021-08-01
- added an option to hide the dagger on Lara's nightgown if she has not yet killed a dragon (#152)
- added an option to make Lara invisible (#152)
- added support to randomize cutscene levels (in line with their parent level) (#183)
- fixed key item softlocks in Diving Area (#168)

## [V1.3.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.3.1...V1.3.2) - 2021-07-25
- added Winston to the cross-level enemy pool (#161)
- added outfit randomization (#176)
- added gamestring randomization (#176)
- added further texture randomization (#176)
- added a night-mode option (#181)
- added triggered music randomization (#181)
- added start position randomization (#181)
- fixed crashes related to savegames when too many Skidoo drivers are present (#154)
- fixed flame texture issues (#157)
- fixed the freezing issue in Barkhang Monastry - water creatures will no longer appear here (#161)
- fixed skybox texture corruption when using TR2Main (#161)
- improved the UI layout and management of all randomization options (#172)

## [V1.3.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.3.0...V1.3.1) - 2021-06-06
- added an option to prevent items being allocated to monks (#131)
- added an option to use glitched secrets (#128)
- added skybox randomization to Offshore Rig and Diving Area (#147)
- added cross-level enemies to Home Sweet Home (#148)
- added an option to retain original key item textures (#145)
- disabled room draining to avoid softlocks (#121)
- fixed a softlock when Lara is killed by the T-Rex (#130)
- fixed the hidden gong hammer in Catacombs of the Talion not being randomized (#133)
- fixed issues with several secret and item locations
- fixed a freeze in Barkhang Monastry (#136)
- fixed texture corruption in Offshore Rig and Barkhang Monastry (#137, #139)
- fixed a savegame buffer overflow issue in some unarmed levels (#149)
- fixed dragons spawning more than once and causing a crash (#146)

## [V1.3.0](https://github.com/LostArtefacts/TR-Rando/compare/V1.2.2...V1.3.0) - 2021-05-29
- added cross-level enemy randomization (#97)
- added randomization of key items
- added an option to disable demos to prevent spoilers
- added more potential secret pickup sounds
- fixed texture corruption in Floating Islands
- fixed imported weapon sprites not showing correctly in Floating Islands and Dragon's Lair
- fixed air bubbles showing as blood sprites in Home Sweet Home
- fixed upside-down paintings in Home Sweet Home (OG bug)
- fixed potential softlocks in Ice Palace and Dragon's Lair when the levels are unarmed (#107)
- fixed the recent folder list not updating correctly in the UI (#125)
- improved texture randomization to allow for more than one element to be randomized per level (#58)
- improved the UI progress display during randomization
- increased the number of weapon locations for unarmed levels (#124)

## [V1.2.2](https://github.com/LostArtefacts/TR-Rando/compare/V1.2.1...V1.2.2) - 2021-02-28
- improved the allocation of secret reward items to be fairer
- improved the logic when carrying out data restores
- removed (individual) flares from being allocated as secret rewards

## [V1.2.1](https://github.com/LostArtefacts/TR-Rando/compare/V1.1b2...V1.2.1) - 2021-02-27
- added randomization of secret rewards (#48)
- added randomization of level order (#39)
- added randomization of unarmed levels (#98)
- added randomization of ammoless levels (#99)
- added ambient and secret pickup music randomization (#77)
- added randomization of the sunset effect from Bartoli's Hideout (#100)
- added backup and restore functionality for level and script data (#102)
- added an automatic update checker to the UI (#76)
- added functionality to import and export settings (#93)
- added the ability to have different weapons in Home Sweet Home (#47)
- added further texture randomization to Home Sweet Home
- improved the data folder selection in the UI and added history (#103)

## [V1.1b2](https://github.com/LostArtefacts/TR-Rando/compare/V1.0...V1.1b2) - 2020-08-01
- added the ability to specify the data folder via the UI (#61)
- added extra ammo for the randomized weapon in Offshore Rig (#68)
- fixed Stone and Jade secrets in Floating Islands and Dragon's Lair appearing in the wrong order (#67)
- fixed items being allocated to Xian soldiers and hence not being dropped (#65)
- fixed a softlock in Home Sweet Home with enemy randomization (#62)
- fixed a softlock in Ice Palace with enemy randomization (#60)
- fixed unreachable secret locations in Offshore Rig and Wreck of the Maria Doria (#66, #71)
- improved the UI progress bar appearance (#61)
- improved Lara's diving suit textures (#64)
- removed original textures from the randomization pool (#74)

## [V1.0](https://github.com/LostArtefacts/TR-Rando/compare/...V1.0) - 2020-07-29

Initial version.
