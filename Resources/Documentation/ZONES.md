# Zones
Zones are used to mark areas within a level in which items can appear. Each game behaves in the same way, and the zoning is applied in three separate cases.

## Regular items
Guns, ammo, medipacks etc are placed at completely random points in the level, but will always be within reach. Items are placed centre-tile and if placed on a slope, it will always be a slope Lara can stand on. Locations are filtered based on several factors, such as death tiles, tiles that contain large static meshes or other objects that could interfere with Lara picking up the item.

## Key items
Keys, puzzle items and quest items are placed in well-defined zones, so that the items will always appear before their respective locks/slots. The randomizer provides a `range` option to generally allow controlling the area in which a key item can appear. This is not a uniform setting for every level, because some levels are less linear than others. In addition, the randomizer allows key items to be placed based on whether or not there are [return paths](RETURNPATHS.md) available. This can produce lots of variety as a result - consider the following scenarios as an example, using *The Talion* from Ice Palace in TR2.

| Range | Return Paths | Zone |
|-|-|-|
| Small | Disabled | The ice structure around the gong at the end of the level. |
| Small | Enabled | The ice structure around the gong at the end of the level, plus the return path from here leading to the yeti pit (and no further). |
| Medium | Disabled | The entire area at the end of the level after dropping to the gong. |
| Medium | Enabled | Anywhere from room 29 onwards (where the gong hammer is normally located). |
| Large | Disabled | _Same as Medium/Disabled_ |
| Large | Enabled | Anywhere from level start to level end. |

## Secrets
Zone lines for secrets are not as clear-cut as they are for key items. Rather, secrets are placed based on how close they are to each other. So the first secret is placed by the randomizer, then when trying to place the next, if it's deemed too close to the first, another location will be picked. This process continues until all locations are populated, and the required distance is reduced if the randomizer is unable to find suitable locations.

For TR2, once all locations are selected, they are allocated as Stone, Jade and Gold based on the dev-intended completion route of a level.
For TR1 and TR3, models are imported artefacts so their order is irrelevant.

## Zone management
The above zoning is achieved using trview JSON files that define level routes, and along each route key waypoints are marked, for example to delimit key item zones. Zones can be managed and visualised using the `key_items` plugin located at https://github.com/LostArtefacts/TR-Rando/tree/master/Resources/trview/plugins. Refer to the [trview documentation](https://github.com/chreden/trview/blob/master/doc/lua/plugins.md) for guidance on setting this up.
