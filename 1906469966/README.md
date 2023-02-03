# Coastlines
![](https://img.shields.io/badge/Mod_Version-1.3-blue.svg)
![](https://img.shields.io/badge/Built_for_RimWorld-1.2-blue.svg)
![](https://img.shields.io/badge/Powered_by_Harmony-2.x-blue.svg)

![Steam Subscribers](https://img.shields.io/badge/dynamic/xml.svg?label=Steam+Subscribers&query=//table/tr[2]/td[1]&colorB=blue&url=https://steamcommunity.com/sharedfiles/filedetails/%3Fid=1906469966&suffix=+total)

[Link to Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1906469966)

---

Currently adds two new coastline types:

**Peninsula**\
Has only one land border for easy defense

**Island**\
Has no land borders. No traders, raids or new wildlife can appear.


These only appear on tiles that border ocean tiles (or single tiles in oceans in the case of islands).
Not every coast is a peninsula and the chance of one appearing can be changed in the settings.

This mod is in beta and needs further testing, especially in the case of islands which have no land borders on the maps edge.
Islands may cause all sorts of issues but from preliminary testing this has not caused any errors.

---

##### INSTALLATION
- **[Go to the Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1906469966) and subscribe to the mod.**

---

The following base methods are patched with Harmony:
```C#
Prefix* : RimWorld.BeachMaker
Prefix* : RimWorld.Planet.WITab_Terrain.FillTab
```
*A prefix marked by a \* denotes that in some circumstances the original method will be bypassed**
