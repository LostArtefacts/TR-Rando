# TR I-III Remastered Options

Note that for TR I-III Remastered, the options that are available to randomize are much more limited than the classics. The following table shows what can be randomized; the remaining options will be greyed out in the UI.

| Aspect | TR1R | TR2R | TR3R | Notes |
|-|-|-|-|-|
| Level sequencing | Yes | Yes | No | Some levels cannot be moved because of hard-coded logic in-game. For TR3, there are too many issues to work around to make this feature feasible. |
| Unarmed/ammoless levels | No | No | No | This is hard-coded in the games and cannot be controlled. |
| Night mode/VFX/sunsets | No | No | No | Currently, remastered rooms cannot be manipulated. |
| Secrets | Yes | Yes | Yes | - |
| Items | Yes | Yes | Yes | - |
| Enemies | Yes | Yes | Yes | - |
| Textures | Yes | Yes | Yes | This currently differs from classic in that textures are moved around between levels rather than hues/colours being changed or swapped. |
| Secret rewards | Yes | No | Yes | TR1 and TR3 will default to stacked rewards because reward rooms are not possible currently. TR2 rewards are hard-coded. |
| Audio | Yes | Yes | Yes | TR1 SFX randomization is slightly more limited than in classic (mainly weapon sounds). |
| Text | Yes | Yes | Yes | Only English is currently supported. |
| Environment | No | No | No | This requires further understanding of the remastered room structures, so is disabled entirely for the time being. |

 ## Level Sequencing

 In the classics, the gameflow is entirely configurable, which means we can control everything related to level sequencing changes. The gameflow is hard-coded in TRR, so we have to work around this by renaming data files. This can produce some side effects when levels are off-sequence, which are pointed out below. These are issues that we cannot control and/or fix.

 - Key item names will generally default to "Key Item"
 - Lara will always lose her guns/ammo on level slots where this originally happens (i.e. Natla's Mines, Offshore Rig and Home Sweet Home)
 - Cutscenes and FMVs are hard-coded, so will always follow the original level sequencing e.g. Larson vs. Lara will always show after the fourth level in TR1R
 - You may notice some minor visual glitches in remastered graphics, such as the skybox flickering occasionally in Colosseum
 - Ember colours in TR2 will change depending on the sequence e.g. in the Diving Area slot they are green, otherwise they are orange
 - In Barkhang Monastery, the Seraph will not be in Lara's inventory. A workaround is provided at the end of this level, such that the Seraph is not required

 Another point to stress is that if you need to look up items in trview, you will need to open the level file whose name matches the sequence you are currently on. For example, if you are playing Great Wall but the sequence is what would normally be Living Quarters, open `LIVING.TR2` rather than `WALL.TR2`.
