# -*- coding: utf-8 -*-
"""
Renames all texture files requiring graphic_multi to the correct extension.

_back -> _north
_front -> _south
_side -> _east

Place this file in your mod folder for it to work.
@author: Spdskatr
"""
from __future__ import print_function
import os

for root, dirs, files in os.walk("Textures"):
    for file in files:
        if file.endswith(".png") and "_back" in file or "_front" in file or "_side" in file:
            newName = file.replace("_back", "_north").replace("_front", "_south").replace("_side", "_east")
            os.rename(os.path.join(root, file), 
                      os.path.join(root, file.replace("_back", "_north").replace("_front", "_south").replace("_side", "_east")))
            print(file, "->", newName)