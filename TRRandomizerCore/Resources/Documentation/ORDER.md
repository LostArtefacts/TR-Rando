# Randomization Order
This provides an explanation of the order of processing in TR2LevelRandomizer, detailing dependencies between randomizers and processors.

## 01 GameStrings
Takes place first to ensure gamestrings can be saved to the script before TRGE writes to the dat file.

## 02 Scripting
All scripting features are processed next by TRGE.
- Level order/count
- Unarmed/Ammoless levels
- Sunset flags
- Secret rewards
- Level ambient tracks, global secret sound

## 03 Texture Deduplication
Must take place before the first use of TRModelTransporter (ItemRandomizer for vehicle import) and before any texture manipulation as all targeting is done based on deduplicated tiles.

## 03 Secrets
Performed before enemy randomization otherwise a secret could potentially end up in the same position as an enemy who can't drop it. In addition, performed before environment randomization as this contains logic to (manually) reposition secrets after flooding/draining.

## 04 Items (non-unarmed)
Must take place after texture deduplication for vehicle import and before enemy randomization so enemies dropping items can be checked in EnemyRandomizer. If cross-level enemies are in use, unarmed location logic will be handled post enemy randomization.

## 05 Model Adjustment
Must take place after item randomization as entity types may change. For example, Puzzle2 entities are switched to Puzzle3 because Puzzle2 is required for the dragon to work properly in non-native levels. If this took place before item randomization, zoning for those items would break.

## 06 Enemies
Must take place after secret and item randomization, and texture deduplication. If cross-level enemies are used, model adjustment must be performed first - see note above re dragon.
In addition, this must take place before texture randomization to ensure cross-level enemy textures can be targeted.

## 07 Items (unarmed)
Following enemy randomization, item randomizer runs again to calculate additional ammo to give in unarmed levels. This is based on enemy difficulty.

## 08 Start Position
Must take place after secret randomization - if a secret is in Lara's starting room, she will not be repositioned. Must also take place before environment randomization as level mirroring will relocate entities.

## 09 Environment
Takes place after enemy randomization as enemy types are taken into consideration for flooding/draining.
Takes place before audio randomization as new triggers may be added that should take precedence over secret music triggers added in AudioRandomizer. Also allows any new music triggers to be later randomized.
This must also take place before night randomization as new room vertices may have been added so these will need their lighting values set.

## 10 Audio
Performed after secret and environment randomization to (potentially) allocate music triggers to the correct floor sectors.

## 11 Outfits
Takes place after enemy randomization as priority is given to that for texture import (in terms of the tile limits, we aim to get more enemies in place rather than a different outfit for Lara). This must take place before texture randomization so the new outfit can be targeted.

## 13 Night Mode
Must take place before texture randomization to determine if specific night-time textures are required.

## 14 Textures
Must take place after deduplication, enemy randomization and outfit randomization.  
Must take place before sprite randomization when it's activated

## 15 Sprites ( if activated )
No forward dependencies.  
Must take place after texture randomization to avoid overflow of random texture.
