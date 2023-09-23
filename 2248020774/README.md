# FriendshipMemoryFramework
A framework to let Colonists remember their friends and notice missing persons.

# Previous releases
Go to the Releases section of the GitHub repo.

# Technical description
A new `WorldComponent` (`FriendshipMemoryGlobalTracker`) is used to store the friendship memory of Colonists (in general the valid targets are those that can have mood), which may include how long ago the Colonist saw their friends/rivals, the last known alive/dead status of their friends/rivals, etc.

Under `FriendshipMemoryGlobalTracker`, there is a list of `FriendshipMemoryTracker`. Each `FriendshipMemoryTracker`, identified by the pawn it belongs to, then contains a list of `FriendshipMemory` describing the relationship from the subject pawn to each other pawn.

Navigate from the global level to individial level using various functions named like `Get...()`.
