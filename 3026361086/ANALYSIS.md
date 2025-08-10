# A Hemogenic Manifesto

## Sanguophage thought process

There are a bunch of moving pieces and variations with sanguophages, especially when mods are added on top of Biotech. This section documents the facts I am basing my thinking on, assumptions I am making (about either how things work conceptually or how I think they \[should\] work), and what conclusions I can draw from those. Together, these pieces lead me to what aspects of sanguophages I want to change in this mod, and how I want to change them.

The final section includes some "issues" I have identified during this process. Some might be difficult to account for, while others open avenues to new features that could be added.

### Facts (based on game mechanics)
* (Fact) Bloodfeeders take a constant amount of time to feed, regardless of biter/victim body sizes.
* (Fact?) Blood loss hediff is a measure of the percentage of blood that has been lost.
* (Fact?) Extracting a hemogen pack from a pawn takes a constant amount of time, regardless of the pawn's body size.
* (Fact) Spending hemogen (e.g., for abilities) does not consume a pawn's blood (it does not increase their blood loss hediff).
* (Fact) Hemogenic and non-hemogenic pawns gain the same amount of blood loss when receiving a blood transfusion.
* (Fact) Most/all hemogenic pawns have a net loss of hemogen per day at a constant rate, irrespective of any conscious actions taken.
    * (Fact) The rate hemogen is lost can vary between pawns and depends on their genes.
* (Fact) Pawns have a "hemogen gain factor", which modifies all hemogen gains, but not losses.
    * This appears to be only affected by genes, but there are no such genes in Biotech itself.

### Non-assumptions
* (False) Hemogen is not contained in hemogen packs nor is transferred from victim to biter when bloodfeeding, or at least not in a sufficient quantity to account for all of the hemogen pack consumer's or bloodfeeding biter's hemogen gain.
* (False) Hemogenic pawns can create hemogen from blood that enters their body as a one-time process, and this has a negligible impact on the volume of usable blood.

### Assumptions
* All pawns have hemogen and at the same concentration (relative to body size), but only hemogenic pawns can use it or suffer any effects when it is low.
    * Non-hemogenic pawns do not consume hemogen automatically each day, or reach a steady-state of creation and loss at their max capacity.
* Hemogen is contained in hemogen packs (along with blood), and is transferred from victim to biter during bloodfeeding.
    * As opposed to them only containing blood and bloodfeeding only transferring blood, and the hemogenic pawn creates the hemogen from the new blood.
* Blood has the same concentration of hemogen, regardless of body size (at max hemogen capacity).
* Bloodfeeders drain a constant volume of blood from a victim, regardless of biter/victim body sizes.
* The total volume of blood a pawn has is proportional to body size.
* A pawn's hemogen bar is a measure of the percentage of hemogen that a pawn has, relative to maximum capacity.
* Hemogen costs (e.g., for abilities) consume a percentage of a pawn's maximum hemogen.
* Hemogen packs contain a constant volume of blood, regardless of whom they were taken from.
* Hemogen and blood are two distinct substances.
* Hemogen effectively completely resides in a pawn's bloodstream.
* Hemogenic pawns generate hemogen in their bodies at the same rate as non-hemogenic pawns, but this is only sufficient to support normal human bodily functions, or is wasted.
    * A hemogenic pawn's hemogen drain gene corresponds to the additional hemogen requirement to support their superhuman genes/abilities, such as immortality.

### Conclusions
Note that "hemogen" and "hemogen resource" are interchangable here, as opposed to the non-normalized "total hemogen".

* Pawns with a larger body size have more total hemogen.
* The hemogen resource for a pawn is a normalized value, so a single unit of hemogen resource can represent a different amount of total hemogen for different pawns.
* Bloodfeeder victims lose a fraction of their blood and fraction of their hemogen per bite that is inversely proportional to their body size.
* Bloodfeeder biters gain an amount of hemogen that is inversely proportional to their own body size.
* The amount of hemogen a bloodfeeder biter gains does not depend on the victim's body size.
* The amount of blood and hemogen a blood transfusion restores a pawn is inversely proportional to their own body size.
* The amount of blood loss a pawn gains when a hemogen pack is extracted is inversely proportional to their own body size.
* The process to extract a hemogen pack from a pawn results in only one hemogen pack created, regardless of the pawn's body size.
* Considering our assumption that all pawns create some amount of hemogen per day, but hemogenic pawns do not create enough to sustain their gene requirements:
    * We could create drugs (or other things) that increase a pawn's hemogen production. This would have no effect on non-hemogenic pawns, but hemogenic pawns could partially or fully counteract their daily hemogen loss.
        * It could also give them a mood buff (some kind of high), and be addictive. Because all good things have to be addictive.
* A hemogenic pawn could be given a "hemogen concentration" stat, based on their current hemogen amount and current blood loss.
    * As bleeding currently has no effect on hemogen level, bleeding increases hemogen concentration, and recovering from blood loss lowers it.
    * Gaining or losing hemogen directly (and not changing blood loss) has a direct effect on hemogen concentration.

### Issues
* Variable hemogen concentration in the blood
    * If a pawn's current hemogen amount (% of total) does not equal the amount of blood they have (1 - blood loss severity), their blood will have a different concentration of hemogen.
        * Extracting a hemogen pack from the pawn should then drain a different percentage of their hemogen.
        * Consuming a hemogen pack extracted from this pawn should give the drinker a different percentage of their hemogen back.
        * Similarly, feeding on these pawns would drain and restore a different amount of hemogen.
    * It would not be difficult to account for this when bloodfeeding.
    * However, we would need to add a new Comp class or something to hemogen packs, indicating their hemogen concentration.
        * This is more difficult.
        * It would, however, give us other opportunities. E.g,. combining blood packs to increase their hemogen concentration.
    * (Conjecture) This would not be a problem if we assume hemogenic pawns create the hemogen when they gain new blood.
        * They could, also, be continuously generating some amount of hemogen. But not at a sufficient rate to counteract their daily loss.
* Should bleeding cause hemogen loss?
    * If hemogen is mixed in with the blood, then if a hemogenic pawn loses blood, they should also lose an amount of hemogen, proportional to the bleed rate and their hemogen concentration.
    * Bleed rate and hemogen loss rate from bleeding would then usually not be the same, but would converge as bleeding continues (not counting other influences to blood loss and hemogen level).
    * As of version 0.5.0, hemogenic pawns now *do* lose hemogen while bleeding.
* HemogenGainFactor stat is not being used.
    * It might be perfect to include body size as a factor of HemogenGainFactor. It is already incorporated into the HemogenOffset method, so a patch is not necessary.
        * Actually, it is only included for hemogen *gains*, not losses.
    * It would also not require creating yet another stat.
    * A corresponding "BloodGainFactor" could also be added, which performs the same modification, but for blood (not including bleed rate).

## Stat collection

These values have been updated for version 0.5.0 of Sanguophage Tweaks.

Blank entries in the table mean the related action is not possible with that column's mod combination, or the action has no effect on that stat.

The column headers correspond to which mods are active:
* B - Biotech
* VRES - Biotech, Vanilla Races Expanded - Sanguophage
* ST - Biotech, Sanguophage Tweaks
* VRES + ST - Biotech, Vanilla Races Expanded - Sanguophage, and Sanguophage Tweaks

### Bloodfeeding

Note that blood and hemogen losses are for the bloodfeeding victim, and blood and hemogen gains are for the bloodfeeding biter.

#### Human

Note that hemogen loss values are irrelevant, since non-hemogenic humans (the valid target for this ability) do not have a hemogen resource.

| Stat or Action                                          | B     | VRES  | ST    | VRES + ST |
|:------------------------------------------------------- | ----- | ----- | ----- | --------- |
| Blood loss base amount                                  | 45%   | 45%   | 45%   | 45%       |
| Blood loss multiplier - victim minimum size threshold   | 1     | 1     | 1     | 1         |
| Blood loss multiplier - at victim minimum size          | 1     | 1     | 1     | 1         |
| Blood loss amount - at victim minimum size              | 45%   | 45%   | 45%   | 45%       |
| Blood loss multiplier - victim maximum size threshold   | 1     | 1     | 1     | 1         |
| Blood loss multiplier - at victim maximum size          | 1     | 1     | 1     | 1         |
| Blood loss amount - at victim maximum size              | 45%   | 45%   | 45%   | 45%       |
| Hemogen loss base amount                                | 20%   | 20%   | 20%   | 20%       |
| Hemogen gain base amount                                | 20%   | 20%   | 20%   | 20%       |
| Hemogen loss affected by target body size               | Y (D) | Y (D) | Y (I) | Y (I)     |
| Hemogen gain affected by target body size               | Y (D) | Y (D) | N     | N         |
| Hemogen gain affected by biter body size                | N     | N     | Y (I) | Y (I)     |
| Hemogen loss multiplier - victim minimum size threshold | 0     | 0     | 0.2   | 0.2       |
| Hemogen loss multiplier - at victim minimum size        | 0     | 0     | 5     | 5         |
| Hemogen loss multiplier - victim maximum size threshold | inf   | inf   | 10    | 10        |
| Hemogen loss multiplier - at victim maximum size        | inf   | inf   | 0.1   | 0.1       |
| Hemogen gain multiplier - victim minimum size threshold | 0     | 0     | 1     | 1         |
| Hemogen gain multiplier - at victim minimum size        | 0     | 0     | 1     | 1         |
| Hemogen gain multiplier - victim maximum size threshold | inf   | inf   | 1     | 1         |
| Hemogen gain multiplier - at victim maximum size        | inf   | inf   | 1     | 1         |
| Hemogen gain multiplier - biter minimum size threshold  | 1     | 1     | 0.2   | 0.2       |
| Hemogen gain multiplier - at biter minimum size         | 1     | 1     | 5     | 5         |
| Hemogen gain multiplier - biter maximum size threshold  | 1     | 1     | 10    | 10        |
| Hemogen gain multiplier - at biter maximum size         | 1     | 1     | 0.1   | 0.1       |

#### Sanguophage

Note that the `CompAbilityEffect` for this is `CompAbilityEffect_SanguofeederBite`.

The victim must have at least 10% hemogen to be fed on.

| Stat or Action                                          | B   | VRES       | ST  | VRES + ST   |
|:------------------------------------------------------- | --- | ---------- | --- | ----------- |
| Blood loss base amount                                  |     | 45%        |     | 45%         |
| Blood loss multiplier - victim minimum size threshold   |     | 1          |     | 1           |
| Blood loss multiplier - at victim minimum size          |     | 1          |     | 1           |
| Blood loss amount - at victim minimum size              |     | 45%        |     | 45%         |
| Blood loss multiplier - victim maximum size threshold   |     | 1          |     | 1           |
| Blood loss multiplier - at victim maximum size          |     | 1          |     | 1           |
| Blood loss amount - at victim maximum size              |     | 45%        |     | 45%         |
| Hemogen loss base amount                                |     | 20%        |     | 20%         |
| Hemogen gain base amount                                |     | 20%        |     | 20%         |
| Hemogen loss affected by target body size               |     | Y (direct) |     | Y (inverse) |
| Hemogen gain affected by target body size               |     | Y (direct) |     | N           |
| Hemogen gain affected by biter body size                |     | N          |     | Y (inverse) |
| Hemogen loss multiplier - victim minimum size threshold |     | 0          |     | 0.2         |
| Hemogen loss multiplier - at victim minimum size        |     | 0          |     | 5           |
| Hemogen loss multiplier - victim maximum size threshold |     | inf        |     | 10          |
| Hemogen loss multiplier - at victim maximum size        |     | inf        |     | 0.1         |
| Hemogen gain multiplier - victim minimum size threshold |     | 0          |     | 1           |
| Hemogen gain multiplier - at victim minimum size        |     | 0          |     | 1           |
| Hemogen gain multiplier - victim maximum size threshold |     | inf        |     | 1           |
| Hemogen gain multiplier - at victim maximum size        |     | inf        |     | 1           |
| Hemogen gain multiplier - biter minimum size threshold  |     | 1          |     | 0.2         |
| Hemogen gain multiplier - at biter minimum size         |     | 1          |     | 5           |
| Hemogen gain multiplier - biter maximum size threshold  |     | 1          |     | 10          |
| Hemogen gain multiplier - at biter maximum size         |     | 1          |     | 0.1         |

#### Animal

| Stat or Action                                          | B   | VRES | ST  | VRES + ST |
|:------------------------------------------------------- | --- | ---- | --- | --------- |
| Blood loss base amount                                  |     | 60%  |     | 60%       |
| Blood loss multiplier - victim minimum size threshold   |     | 0.2  |     | 0.2       |
| Blood loss multiplier - at victim minimum size          |     | 1.67 |     | 5         |
| Blood loss amount - at victim minimum size              |     | 100% |     | 300%      |
| Blood loss multiplier - victim maximum size threshold   |     | 2    |     | 10        |
| Blood loss multiplier - at victim maximum size          |     | 1/6  |     | 0.1       |
| Blood loss amount - at victim maximum size              |     | 10%  |     | 6%        |
| Hemogen loss base amount                                |     | 20%  |     | 20%       |
| Hemogen gain base amount                                |     | 20%  |     | 20%       |
| Hemogen loss affected by target body size               |     | Y    |     | Y         |
| Hemogen gain affected by target body size               |     | Y    |     | N         |
| Hemogen gain affected by biter body size                |     | N    |     | Y         |
| Hemogen loss multiplier - victim minimum size threshold |     | 0    |     | 0         |
| Hemogen loss multiplier - at victim minimum size        |     | 0    |     | 0         |
| Hemogen loss multiplier - victim maximum size threshold |     | inf  |     | inf       |
| Hemogen loss multiplier - at victim maximum size        |     | inf  |     | inf       |
| Hemogen gain multiplier - victim minimum size threshold |     | 0    |     | 0         |
| Hemogen gain multiplier - at victim minimum size        |     | 0    |     | 0         |
| Hemogen gain multiplier - victim maximum size threshold |     | inf  |     | inf       |
| Hemogen gain multiplier - at victim maximum size        |     | inf  |     | inf       |
| Hemogen gain multiplier - biter minimum size threshold  |     | 1    |     | 0.2       |
| Hemogen gain multiplier - at biter minimum size         |     | 1    |     | 5         |
| Hemogen gain multiplier - biter maximum size threshold  |     | 1    |     | 10        |
| Hemogen gain multiplier - at biter maximum size         |     | 1    |     | 0.1       |

#### Corpse

The corpse is destroyed after feeding, so its loss values are irrelevant.

This ability also uses its own custom DoBite method. This method does not modify the victim's hemogen value nor its blood loss hediff. This method would require a separate transpiler patch to not include the target's body size in hemogen gain.

| Stat or Action                                          | B   | VRES  | ST  | VRES + ST |
|:------------------------------------------------------- | --- | ----- | --- | --------- |
| Blood loss base amount                                  |     | 0%    |     | 0%        |
| Blood loss base amount (according to comp props)        |     | 45%   |     | 45%       |
| Blood loss multiplier - victim minimum size threshold   |     | 1     |     | 1         |
| Blood loss multiplier - at victim minimum size          |     | 1     |     | 1         |
| Blood loss amount - at victim minimum size              |     | 45%   |     | 45%       |
| Blood loss multiplier - victim maximum size threshold   |     | 1     |     | 1         |
| Blood loss multiplier - at victim maximum size          |     | 1     |     | 1         |
| Blood loss amount - at victim maximum size              |     | 45%   |     | 45%       |
| Hemogen loss base amount                                |     | 20%   |     | 20%       |
| Hemogen gain base amount                                |     | 20%   |     | 20%       |
| Hemogen loss affected by target body size               |     | Y (D) |     | Y (I)     |
| Hemogen gain affected by target body size               |     | Y (D) |     | Y (D)     |
| Hemogen gain affected by biter body size                |     | N     |     | Y (I)     |
| Hemogen loss multiplier - victim minimum size threshold |     |       |     |           |
| Hemogen loss multiplier - at victim minimum size        |     |       |     |           |
| Hemogen loss multiplier - victim maximum size threshold |     |       |     |           |
| Hemogen loss multiplier - at victim maximum size        |     |       |     |           |
| Hemogen gain multiplier - victim minimum size threshold |     | 0     |     | 0         |
| Hemogen gain multiplier - at victim minimum size        |     | 0     |     | 0         |
| Hemogen gain multiplier - victim maximum size threshold |     | inf   |     | inf       |
| Hemogen gain multiplier - at victim maximum size        |     | inf   |     | inf       |
| Hemogen gain multiplier - biter minimum size threshold  |     | 1     |     | 0.2       |
| Hemogen gain multiplier - at biter minimum size         |     | 1     |     | 5         |
| Hemogen gain multiplier - biter maximum size threshold  |     | 1     |     | 10        |
| Hemogen gain multiplier - at biter maximum size         |     | 1     |     | 0.1       |

### Operations

Note that the blood and hemogen changes are for the pawn being operated on -- the one having their hemogen extracted, and the one getting the blood transfusion.

#### Human

| Stat or Action                                                        | B   | VRES | ST  | VRES + ST |
|:--------------------------------------------------------------------- | --- | ---- | --- | --------- |
| Extract hemogen pack - Blood loss base amount                         | 45% | 45%  | 45% | 45%       |
| Extract hemogen pack - Blood loss multiplier - minimum size threshold | 1   | 1    | 1   | 1         |
| Extract hemogen pack - Blood loss multiplier - at minimum size        | 1   | 1    | 1   | 1         |
| Extract hemogen pack - Blood loss amount - at minimum size            | 45% | 45%  | 45% | 45%       |
| Extract hemogen pack - Blood loss multiplier - maximum size threshold | 1   | 1    | 1   | 1         |
| Extract hemogen pack - Blood loss multiplier - at maximum size        | 1   | 1    | 1   | 1         |
| Extract hemogen pack - Blood loss amount - at maximum size            | 45% | 45%  | 45% | 45%       |
| Extract hemogen pack - Hemogen loss base amount                       | 0%  | 0%   | 0%  | 0%        |
| Blood transfusion - Blood gain base amount                            | 35% | 35%  | 35% | 35%       |
| Blood transfusion - blood gain multiplier - minimum size threshold    | 1   | 1    | 1   | 1         |
| Blood transfusion - blood gain multiplier - at minimum size           | 1   | 1    | 1   | 1         |
| Blood transfusion - blood gain multiplier - maximum size threshold    | 1   | 1    | 1   | 1         |
| Blood transfusion - blood gain multiplier - at maximum size           | 1   | 1    | 1   | 1         |
| Blood transfusion - hemogen gain base amount                          | 20% | 20%  | 20% | 20%       |
| Blood transfusion - hemogen gain multiplier - minimum size threshold  | 1   | 1    | 0.2 | 0.2       |
| Blood transfusion - hemogen gain multiplier - at minimum size         | 1   | 1    | 5   | 5         |
| Blood transfusion - hemogen gain multiplier - maximum size threshold  | 1   | 1    | 10  | 10        |
| Blood transfusion - hemogen gain multiplier - at maximum size         | 1   | 1    | 0.1 | 0.1       |

#### Sanguophage

No operations currently exist which extract hemogen packs (sanguophage) with any combination of mods. Hemogenic pawns can have the "human" operations performed on them, with the same results.

#### Animal

| Stat or Action                                                        | B   | VRES | ST   | VRES + ST |
|:--------------------------------------------------------------------- | --- | ---- | ---- | --------- |
| Extract hemogen pack - Blood loss base amount                         |     |      | 45%  | 45%       |
| Extract hemogen pack - Blood loss multiplier - minimum size threshold |     |      | 0.2  | 0.2       |
| Extract hemogen pack - Blood loss multiplier - at minimum size        |     |      | 5    | 5         |
| Extract hemogen pack - Blood loss amount - at minimum size            |     |      | 225% | 225%      |
| Extract hemogen pack - Blood loss multiplier - maximum size threshold |     |      | 10   | 10        |
| Extract hemogen pack - Blood loss multiplier - at maximum size        |     |      | 0.1  | 0.1       |
| Extract hemogen pack - Blood loss amount - at maximum size            |     |      | 4.5% | 4.5%      |
| Extract hemogen pack - Hemogen loss base amount                       |     |      | 0%   | 0%        |
| Blood transfusion - blood gain base amount                            |     |      | 35%  | 35%       |
| Blood transfusion - blood gain multiplier - minimum size threshold    |     |      | 1    | 1         |
| Blood transfusion - blood gain multiplier - at minimum size           |     |      | 1    | 1         |
| Blood transfusion - blood gain multiplier - maximum size threshold    |     |      | 1    | 1         |
| Blood transfusion - blood gain multiplier - at maximum size           |     |      | 1    | 1         |
| Blood transfusion - hemogen gain base amount                          |     |      | 20%  | 20%       |
| Blood transfusion - hemogen gain multiplier - minimum size threshold  |     |      | 0.2  | 0.2       |
| Blood transfusion - hemogen gain multiplier - at minimum size         |     |      | 5    | 5         |
| Blood transfusion - hemogen gain multiplier - maximum size threshold  |     |      | 10   | 10        |
| Blood transfusion - hemogen gain multiplier - at maximum size         |     |      | 0.1  | 0.1       |

#### Corpse

No operations currently exist which target corpses with any combination of mods.

### Bloodfeeder extract jobs

Bloodfeeders are able to create a hemogen pack of their respective type (from a legal victim) with a float menu job. The targets need to be downed. Along with creating a hemogen pack (animal or corpse), performing this action on an animal automatically kills it, and performing it on a corpse destroys the corpse. Performing this on a sanguophage will, along with creating a hemogen pack (sanguophage), give the sanguophage 99% blood loss. If the sanguophage already had substantial blood loss, they are put into deathrest or a regenerative coma (whatever is appropriate for their genes).

There does not appear to be a job to extract a normal hemogen pack from a non-hemogenic pawn.

## Observations

### Bloodfeeding blood loss amounts
This mod does not yet modify blood loss amounts for bloodfeeding. Human and sanguophage bloodfeeding always drain 45%, and animal bloodfeeding drains 60%, modified by body size. I think capping the maximum body size effect at 2 is way too low. It leads to bloodfeeding on animals to only drain 10% of their blood per bite for most species.

### Animal operations and balance
Having operations to extract hemogen packs from animals gives a pretty easy way to accumulate animal hemogen packs. I can add a requirement that these operations require medicine. Along with the additional cost, I can have better medicine lead to less blood loss (which is supported in the code, but the recipes do not require medicine, so it is unused).

### Non-destructive corpse feeding
I would like to make it so bloodfeeding on a corpse does not immediately destroy it. Instead, I think it should give the corpse a blood loss hediff, like for non-corpses, and once it has reached 100% blood loss, immediately become desiccated. Right now a corpse bloodfeeder can feed on any corpse, but I think this should be disabled for desiccated corpses. This should also probably consume a larger percent of the corpse's blood than when biting a human., at least if the body is rotten.

On the one hand, one could potentially get more hemogen from a single corpse, especially with larger body size corpses, but it also means a bloodfeeder would not be able to feed *at all* on corpses that have bled out or have become desiccated. There will be less hemogen available if the corpse feeder relies mostly on bodies left after battles, and the same or more if they rely on, say, executing otherwise healthy prisoners.

This also means the corpse's equipment and inventory won't go *poof* after being fed on.

### Separate job drivers
There are also JobDrivers for each of the types of bloodfeeding, which appear to all have their own version of the code to perform the bite. I believe these are used when the hemogenic pawn decides to obtain hemogen by bloodfeeding, based on its hemogen level. These appear to use `OffsetHemogen`, so they should incorporate my patch, but they likely also still incorporate the size of the victim's body size, which would need to be patched.

### Other mods

The [Vampires, Demons, and the Undead](https://steamcommunity.com/sharedfiles/filedetails/?id=2926556467) mod adds a bunch of other hemogenic xenotypes. Some of their related abilities use the base bloodfeed implementation, while others use custom ones. I'll want to check if/should I update those to use my value tweaks.
