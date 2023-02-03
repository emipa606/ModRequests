# AlienFaces
How to add feet:

claw hand + digitigrade feet = put PawDigitigrade between in the handType section in AlienPatch.xml under your race header.
feather wing + raptor feet = put FeatherWing between in the handType section in AlienPatch.xml under your race header.

How to add eyes:
Only adds eyes in the same position as human pawns, not recommended for Races whose eyes are in a vastly different location from the Human pawn

Change hasEyes to true under your race header. (this is the only required step, but it might look bad.)
Extra steps to make it look better:
1)go to the steam workshop page for the race mod and check the url of the mod. The long series of numbers at the end of the url is the Mod ID.
2)go to steam/steamapps/workshop/294100/(the series of numbers) and that is the Mod folder for the race.
3)find the head texture-- this is usually somewhere under themodfolder/Textures/Things.
4)rip the head texture, go into an editing program that supports transparent backgrounds, and colour over the eyes with the skin tone of the texture.
5)Make a folder tree in the Alien Faces Expanded Textures folder that EXACTLY mimics the folder tree you found the head texture in.
6)Put the head texture in the folder you just created.
7)make sure Alien Faces Expanded is below the race mods and below Facial Stuff
8)turn hasEyes to true in the .xml for your race. 
9)What you just did is make a retexture that has no eyes for the Race of your choosing. It will overwrite the old head and there will be no eyes under the Facial Stuff eyes anymore. This can make it look better, especially when they blink. 