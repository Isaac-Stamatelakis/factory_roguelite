# CaveTech
CaveTech is a 2D factory game currently being developed in Unity. It's a fusion of Terraria and Modded Minecraft that is heavily inspired by the minecraft modpack GregTech New Horizons with aspects of GregTech, EnderIO, AE2, BetterQuesting, LootGames, and more. All coding, art, etc are custom made for this project.

### View Feature Demos
[Youtube](https://www.youtube.com/@CaveTechDev/videos)

## Game Loop
In CaveTech you play as a mining Robot named HAPPY that is developed by the company 'CaveTech'. The player spawns in a confined area called 'The HUB' which is their primary base, and contains a powerful teleportation device that allows them to teleport to other dimensions. These dimensions contain materials use to upgrade their production lines and eventually unlock other dimensions, with the goal of eventually completeing their assigned mission.

## Features in CaveTech:
* 6 Different Types of Tiles:
  * Blocks: Can be collided with by the player.
  * Backgrounds: Behind all other tile types and is about 1.5x larger.
  * Objects: Cannot be collided with by the player, and can be any size.
  * Platforms: Only collide with the player from above, and be can be fallen through.
  * Fluids: Simulated with cellular automata rules. Can be moved through, optionally can slow the player and/or damage the player.
  * Conduits: Five different conduit types which can interact with objects placed in the world. More on them below.
* Cave generation with a wide variety of generation types and fully customizable distrubtors for:
  * Ore, tile, fluid, entity, and structures.
* Many Types of Items:
  * Tile Items, Conduit Items, and Fluid Items for placing tiles in the world.
  * Transmutable Items which automatically are automatically generated from a Transmutable Material that defines a list of states [Ingot, Plate, Wire, Ore, etc], color, and optional shaders to be applied to all its child Items.
  * GameStages which prevent the player from seeing certain items until they have progressed far enough.
  * Displayable in both the User Interface and World with optional visual options such as overlays, colors, shaders, chemical formulas, and more.
  * Many Item Tags for storing data in items (Fluids, DimensionData, etc).
* Tile Entities for adding behavior to Tiles placed in the world such as:
  * Light Sources, Puzzles, Item/Fluid/Energy Storage, Ladders, Doors, Teleporters, Portals, Auto-Miners, Logic, Workbenches, and more.
  * Four Different Types of Machines:
    * Passive Machines which run constantly.
    * Burner Machines which run off of burnable fuel.
    * Processing Machines which run off of energy.
    * Generator Machines which generate energy.
  * Compact Machines which "shrink space". Players can teleport inside of them and build encapsulated factories within them. Conduits can connect to inside them. They can be copy and pasted by blueprinting. They support a max recursive depth of 5.
* Five different types of Conduits for interacting between Tile Entities:
  * Item Conduits that transfer Items.
  * Fluid Conduits that transfer Fluids.
  * Energy Conduits that transfer Energy.
  * Signal Conduits that transfer On/Off Signals.
  * Matrix Conduits which connect Tile Entities to a larger item system.
* Five different types of Recipes:
  * Energy, generator, passive, burner which are used by their respective Machines.
  * Transmutation Recipes which modify the state of a Transmutable Item [Ingot -> Plate].
* Two Types of Entities:
  *  Item Entities which can be picked up by the player.
  *  Mob Entities can be attacked by the player and may attack the player.
* Questbook that guides players through the game and rewards them for completing quests such as item retrieval, visiting a dimension, and more.
* In game text chat and many commands the player can use if their world has cheat mode enabled.
* Item Catalogue
* Four types of Robot Tools each with many upgrades:
  * Laser Drill that breaks blocks, backgrounds, and can suck up fluids. Multi-Hit, Speed, VeinMine, and Magnet Upgrades.
  * Laser Gun that shoots lasers. Fire Rate, Multi-Shot and Explosion Upgrades.
  * Laser Cutters that breaks conduits. VeinMine and Magnet Upgrades.
  * Buildinator which can Rotate, Hammer [Tile -> Slab -> Stair -> etc], and Chisel Tiles. Multi-Hit Upgrades.
* Player Robot Upgrades:
  * Speed, Jump Height, Reach, Health, Bonus Jumps, and Energy.
  * Rocket Boots which push the player upwards.
  * Teleportation which teleports the player to their cursor.
  * Nano-Bots which give the player passive healing.
* Many Unity Editor Tools and In-Game Developer Tools:
  * Structure Editor for generating and editing randomly occuring structures.
  * QuestBook Editor for editing and creating QuestBooks.
  * Upgrade Editor for editing and creating Robot/Tool Upgrades.
  * Item Generators for all Item Types and Sub-Types. 
* (In-Progress) Matrix Item System supporting a massive centralized Item storage, Item Input/Output and Auto Crafting. 


## Early Production Images
### Image of the Fiery Heavens
![image](https://github.com/user-attachments/assets/fef3f620-45eb-482a-ab8e-f69cc05e5cf1)
### Image of the Icy Caverns
![image](https://github.com/user-attachments/assets/799311ae-3807-4c88-ac0a-48dafbd82dbf)
### Image of Transmutable Item Ingots
![image](https://github.com/user-attachments/assets/143f9a15-e85d-4ec9-8654-bb7f4a42b389)
### Image of Transmutable Item States
![image](https://github.com/user-attachments/assets/0112d317-2c8f-44f4-9c52-6e15c73e1e9d)
![image](https://github.com/user-attachments/assets/6249f37a-0f27-433a-94c1-e699858f208c)



![jan3-5](https://github.com/user-attachments/assets/dfef118f-a376-413f-ad4b-24ede81b4d1b)




