# SE-InventoryOrganizer
Inventory Organizer script for Space Engineers on C#
[Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=1756626980)

Simply pulls all items from all inventories on grid to one main container.

Then distributes items from main to other containers that specifies it's needs with given format below, through "Custom Data" section on blocks.

# Format
`[Item name]:[Amount]`

All item names is given at workshop page.

# Example
```
SteelPlate:1000
ConstructionComponent:250
InteriorPlate:300
```
Container will always have 1000 steel plates, if main container has that amount.

# Usage
- Place a Cargo Container & Timer to grid, and PB for running the script.
- Set timer block's and main container's name at top of the script.
- Set interval of timer and action of timer as "Programmable Block -> Run"
- After running the script once on PB or Timer, program continuously runs itself through timer.
