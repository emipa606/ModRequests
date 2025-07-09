# DocPawnOverhaul

The goal of this mod is to make pawns feel damage. I found inspiration in [VE-CO](https://ludeon.com/forums/index.php?topic=30799.0) to overhaul the Vanilla combat in a mod-friendly way. The way in which this mod handles that are through these features:
- Increase pain and bleeding rates to Human pawns from most damage types
- Pawns are less spongy and bleed out faster
- Pawns are less likely to lose small limbs
- Pawns are more likely to be shot in the torso
- Organs are critical wounds and more likely to be hit
- Medical aid will be more necessary in the field
- Animals are slightly more robust
- Bionics and Armor become far more important
- Completely save-friendly to add or remove as you wish

This mod only effects races using Vanilla bodies (Humans and Animals). Additional patches will be added in time for other races as needed. Feel free to leave a suggestion on races that may need patching.

### Compare Vanilla vs DPO
https://imgur.com/i259viP

### Suggested Mods To Use With
- [Ammunition](https://steamcommunity.com/sharedfiles/filedetails/?id=1892397131)
- [CustomDeathRandomness](https://steamcommunity.com/sharedfiles/filedetails/?id=1464742390)
- [Death Rattle](https://steamcommunity.com/sharedfiles/filedetails/?id=1552452572)
- [Field Medic](https://steamcommunity.com/sharedfiles/filedetails/?id=1541287769)
- [Smart Medicine](https://steamcommunity.com/sharedfiles/filedetails/?id=1309994319)
- [Yayo's Combat](https://steamcommunity.com/sharedfiles/filedetails/?id=1747929243)

### Bloodloss Pain
- When pawns lose a lot of blood, they get a bloodloss hediff. These values associate pain at varying degrees of bloodloss.
- This is a percentage, so 0.1=10% pain.

| **Blood Loss** | **Vanilla** | **DPO** |
| :-------------: | :-------------: | :-------------: |
| ------------- | ------------- | ------------- |
| Minor | - | 0.1 |
| Moderate | - | 0.25 |
| Severe | - | 0.35 |
| Extreme | - | 0.5 |

### Organ Bleedrate
- Organs will now bleed severely when they are damaged.
- This is a percentage, so 2=200% bleedrate.

| **Organ** | **Vanilla** | **DPO** |
| :-------------: | :-------------: | :-------------: |
| ------------- | ------------- | ------------- |
| Lung | - | 2 |
| Kidney | - | 2 |
| Liver | - | 2 |
| Stomach | - | 2 |

### Body Part Coverage
- These edits will cause pawns to be mostly shot in the torso, which means more organ damage, which means more bleeding and pain, which means they're more likely to be incapacitated or die from bleeding out.
- All body parts are grouped onto the Torso.
- Body parts in bold and italic are the parent parts of those without stylization beneath them. Likewise, the dashes demonstrate levels of children.


| **Body Part** | **Vanilla** | **DPO** |
| :------------- | :-------------: | :-------------: |
| ------------- | ------------- | ------------- |
| **Torso** | - | - |
| Ribcage | 0.036 | 0.08 |
| Sternum | 0.015 | 0.03 |
| Pelvis | 0.025 | 0.03 |
| Spine | 0.025 | - |
| Stomach | 0.025 | 0.03 |
| Heart | 0.020 | - |
| Lung | 0.025 | 0.04 |
| Kidney | 0.017 | 0.02 |
| Liver | 0.025 | 0.03 |
| ***Neck*** | 0.075 | 0.05 |
| -Head | 0.80 | - |
| --Skull | 0.18 | - |
| ---Brain | 0.8 | - |
| --Eye | 0.07 | 0.05 |
| --Ear | 0.07 | 0.05 |
| --Nose | 0.10 | 0.07 |
| --Jaw | 0.15 | 0.1 |
| ***Shoulder*** | 0.12 | 0.1 |
| -Clavicle | 0.09 | - |
| -Arm | 0.77 | 0.3 |
| --Humerus | 0.1 | - |
| --Radius | 0.1 | - |
| --Hand | 0.14 | 0.07 |
| ---Finger | 0.06 | 0.01 |
| ***Waist*** | - | - |
| ***Leg*** | 0.14 | - |
| -Femur | 0.1 | - |
| -Tibia | 0.1 | - |
| -Foot | 0.1 | 0.07 |
| --Toe | 0.06 | 0.01 |

### Local Injuries

| **Injury** | **Value** | **Vanilla** | **DPO** |
| :-------------: | :-------------: | :-------------: | :-------------: |
| ------------- | ------------- | ------------- | ------------- |
| MissingBodyPart | painPerSeverity | 0.0125 | 0.0300 |
|  | bleedRate | 0.12 | 0.36 |
| Misc | painPerSeverity | 0.0125 | 0.0200 |
|  | bleedRate | 0.06 | 0.12 |
| Burn | painPerSeverity | 0.01875 | 0.0250 |
|  | bleedRate | - | - |
| Crush | painPerSeverity | 0.0125 | 0.0200 |
|  | bleedRate | 0.01 | 0.02 |
| Crack | painPerSeverity | 0.01 | 0.0100 |
|  | bleedRate | - | - |
| Cut | painPerSeverity | 0.0125 | 0.0225 |
|  | bleedRate | 0.06 | 0.12 |
| SurgicalCut | painPerSeverity | 0.0125 | 0.0150 |
|  | bleedRate | 0.06 | 0.12 |
| Scratch | painPerSeverity | 0.0125 | 0.0150 |
|  | bleedRate | 0.06 | 0.12 |
| Bite | painPerSeverity | 0.0125 | 0.0200 |
|  | bleedRate | 0.06 | 0.12 |
| Stab | painPerSeverity | 0.0125 | 0.0225 |
|  | bleedRate | 0.06 | 0.15 |
| Gunshot | painPerSeverity | 0.0125 | 0.0225 |
|  | bleedRate | 0.06 | 0.15 |
| Shredded | painPerSeverity | 0.0125 | 0.0300 |
|  | bleedRate | 0.06 | 0.20 |
| Bruise | painPerSeverity | 0.0125 | 0.0150 |
|  | bleedRate | - | - |
| Frostbite | painPerSeverity | 0.0125 | 0.0200 |
|  | bleedRate | - | - |

### Body Part Hitpoints

| **Body Part** | **Vanilla** | **DPO** |
| :-------------: | :-------------: | :-------------: |
| ------------- | ------------- | ------------- |
| Torso | 40 | 50 |
| Shoulder | 30 | 40 |
| Arm | 30 | 40 |
| Hand | 20 | 25 |
| Finger | 8 | 10 |
| Waist | - | - |
| Toe | 8 | 10 |
| Clavicle | 25 | 35 |
| Sternum | 20 | 35 |
| Humerus | 25 | 35 |
| Radius | 20 | 35 |
| Pelvis | 25 | 35 |
| Femur | 25 | 35 |
| Tibia | 25 | 35 |
