# Description
This mod adds cages to hold wild (or your own) animals captive inside them. The animals can't escape it,
are generally peaceful, and will be looked after by your handlers. To capture an animal it has to be incapacitated
first. Then any handler can bring one into a cage it's assigned to; you can select a cage and assign any animal valid for the cage. By default cages allow any animal outside your faction, but if need be, you can toggle it to instead accept your own animals.

# Features
* Currently has cages of a 2×3, 3×3 and 5x5 sizes capable of holding a single animal/three/two animals respectively of most sizes (up to and including the size of a human). Should fit most vanilla animals.
* Handlers automatically capture downed marked animals. Colonists take care of animals inside.
* Captive animals roam inside the cage w/o getting out, although there are no physical barriers involved in construction of the cages as far as RimWorld is concerned; it's a single building.
* Access inside/outside cages is possible only via the entrance cell. But the cage doesn't prevent interaction through the bars up to one cell in either direction. In particular, hungry creatures can snatch food lying close enough outside.
* If captive animals somehow become free, they will attempt to flee the map.

# Knows issues
* Improper rendering of the cages with objects always drawn on top of them.
* If you unassign a creature from a cage w/o assigning it to another cage, it will be instantly considered free. Instead, if you wish to move it from one cage into another, just assign it to the destination cage directly. A colonist will transfer the creature as told. This might also happen by accident if you assign a cage more creatures than it can hold, the earliest assigned creature will be automatically unassigned, thus becoming free.
* Currently cages can become overpopulated (become stuffed w/ more animals than they may hold) under certain conditions. A better assignment and release system is underway.

# Plans
* Cages to hold prisoners: partial support for now, so no cages currently accept them.
* Cages that hold more than one creature according to their combined body size.
* Cages define a separate, specific area with distinct rules regarding their usage, akin to prison cells, for better integration into RimWorld.
* Option to forbid to interact with captive animals.
* Controls to release all held animals at once.
* Released wild animals are escorted outside the colony.
* Possibility of custom cage shapes instead of predefined ones.
* Hunters hunt animals marked for capture and then carry them into an assigned cage.
* Cages with captives provide entertainment to colonists as a joy source.
* Interactions like colonists teasing captive animals, events like animals trying to break free, etc. Raiders trying to save their hostages.
* Custom drawing of cages to animate doors/gates while opening and closing and proper z-order in relation to other objects.
