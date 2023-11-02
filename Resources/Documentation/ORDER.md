# Randomization Order
This provides an explanation of the order of processing during randomization, detailing dependencies between randomizers and processors.

## 01 GameStrings
Takes place first to ensure gamestrings can be saved to the script before TRGE writes to the gameflow file.

## 02 Scripting
All remaining gameflow features such as level order are processed next by TRGE.

## 03 Texture Deduplication
Must take place before the first use of TRModelTransporter (ItemRandomizer for vehicle import) and before any texture manipulation as all targeting is done based on deduplicated tiles.

## 04 Secrets
Performed before enemy randomization otherwise a secret could potentially end up in the same position as an enemy who can't drop it. In addition, performed before environment randomization as this contains logic to (manually) reposition secrets after flooding/draining.

## 05 Items (standard pickups)
Must take place after texture deduplication for vehicle import and before enemy randomization so enemies dropping items can be checked in EnemyRandomizer. If cross-level enemies are in use, unarmed location logic will be handled post enemy randomization.

## 06 Model Adjustment
Must take place after item randomization as entity types may change. For example, Puzzle2 entities are switched to Puzzle3 because Puzzle2 is required for the dragon to work properly in non-native levels. If this took place before item randomization, zoning for those items would break.

## 07 Enemies
Must take place after secret and item randomization, and texture deduplication. If cross-level enemies are used, model adjustment must be performed first - see note above re dragon.
In addition, this must take place before texture randomization to ensure cross-level enemy textures can be targeted.

## 08 Items (unarmed)
Following enemy randomization, item randomizer runs again to calculate additional ammo to give in unarmed levels. This is based on enemy difficulty.

## 09 Start Position
Must take place after secret randomization - if a secret is in Lara's starting room, she will not be repositioned. Must also take place before environment randomization as level mirroring will relocate entities.

## 10 Environment
Takes place after enemy randomization as enemy types are taken into consideration for flooding/draining.
Takes place before audio randomization as new triggers may be added that should take precedence over secret music triggers added in AudioRandomizer. Also allows any new music triggers to be later randomized.
This must also take place before night randomization as new room vertices may have been added so these will need their lighting values set.

## 11 Items (key item pickups)
Key items are placed after environment randomization to account for those that have pickups triggers. This avoids clashing potentially with such things as added trap triggers.

## 12 Environment (finalization)
Conditional environment checks are now performed as well as any level mirroring.

## 13 Audio
Performed after secret and environment randomization to (potentially) allocate music triggers to the correct floor sectors.

## 14 Outfits
Takes place after enemy randomization as priority is given to that for texture import (in terms of the tile limits, we aim to get more enemies in place rather than a different outfit for Lara). This must take place before texture randomization so the new outfit can be targeted.

## 15 Night Mode
Must take place before texture randomization to determine if specific night-time textures are required.

## 16 Textures
Must take place after deduplication, enemy randomization and outfit randomization.  
Must take place before sprite randomization when it's activated

## 17 Sprites
No forward dependencies.  
Must take place after texture randomization to avoid overflow of random texture.
