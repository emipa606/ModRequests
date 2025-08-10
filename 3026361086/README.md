# Sanguophage Tweaks

Latest version: v0.5.0

## About

This mod makes a number of small[ish] tweaks to how Sanguophages behave, and
it also includes a few bits of new content.  Since it deals with Sanguophages,
the Biotech DLC is a hard requirement. Any other mods that this mod patches to
or adds content for are all optional; the features are only enabled when the
dependent mods are present.

With that said, many of the changes relate to [Vanilla Races Expanded -
Sanguophage](https://steamcommunity.com/sharedfiles/filedetails/?id=2963116383)
(VRES), so not also using that mod is almost a silly idea.

There were two main concerns I had that led to the creation of this mod.
First, with VRES, there are now four different types of bloodfeeders, and they
almost all work differently. I didn't see a reason they couldn't be made more
similar. Second, I enjoy playing with lots of different modded genes and
xenotypes (as well as HAR races), and from that there are ways to get very
large and very small colonists. I wanted to incorportate pawn body size (both
the biter and the victim) in as many situations as I could, and in a
consistent way.

For a more comprehensive dissection of the thought process that led me to
these changes, see the ANALYSIS.md file included with the downloaded files and
is in the GitHub repo.

## Features

The mod features are categorized by the mod they primarily modify or affect.

### Core

- Surgeries that have a chance to immediately kill the pawn on failure now
  instead apply a "fatal surgery mishap" hediff.
  - This hediff still immediately kills the pawn all the same. However,
    deathless pawns are able to enter deathrest or a regenerative coma, instead
    of bypassing those and dying outright.
  - If the pawn does not immediately die, the affected body part (if any) is
    destroyed, which can still cause a deathless pawn to die, anyway.
  - The hediff disappears after a couple in-game hours, so it itself will not
    prevent a pawn from finishing their deathrest/regenerative coma.

### Biotech

- Modify the amount of hemogen a pawn gains or loses by its body size (larger
  pawns gain less, smaller pawns gain more).
  - The effect of body size caps at body sizes of 0.2 and 10.
  - Examples hemogen change modifiers:
    - Body size 0.8: 1.25x   (25% more)
    - Body size 6.5: ~0.15x  (15% as much)
- The amount of hemogen a Sanguophage gains from feeding on a pawn is no
  longer modified by the body size of the victim. VRES, with its animal
  feeding, introduced the idea of the victim losing an amount of blood being
  modified by its body size, which implies the volume of blood extracted by
  the Sanguophage is constant. I like this interpretation better than
  vanilla's, which implies a Sanguophage always drinks a specific percentage
  of a pawn's blood.
  - Note that this change makes it harder for Sanguophages to fill up on
    hemogen, but in vanilla Sanguophages can not feed on animals nor are there
    many pawns with a larger body size (or a much larger body size), so its
    effect is minimal. With, for example, mods that add genes to modify a
    human pawn's body size in either direction, or when using VRE - S to feed
    on animals, this change will have much more of an effect.
- A pawn that is incapable of violence will not be able to feed on a pawn who
  would then have life threatening or lethal blood loss.
  - For now, this does *not* take into account body size of the target, as I
    want it to match as closely as possible to when a confirmation box would
    appear for causing life threatening or lethal blood loss (a vanilla
    feature).
  - This also only applies to "normal" bloodfeeding, not for feeding on
    animals or sanguophages (or corpses, but, well...).
- Add a concept of "hemogen concentration".
  - This is a measure of the amount of hemogen a pawn has, compared to the
    amount of blood they have. If a pawn has lost a lot of hemogen, but only a
    little (or no) blood, their hemogen concentration will be very low, and
    vice versa.
  - Since a hemogen pawn constantly loses hemogen from their genes, a pawn's
    hemogen concentration will continuously (albeit slowly) decrease.
- A hemogenic pawn that is bleeding will now also lose hemogen.
  - The rate at which hemogen is lost is based on the pawn's "hemogen bleed
    rate" stat, which is based on their bleed rate and hemogen concentration.
  - As a pawn bleeds (and assuming no other influences), their hemogen
    concentration will gradually move towards 1.
  - This negative feedback loop is designed so when a hemogenic pawn bleeds
    out, their hemgen value should reach 0 at the same time.
    - Note that a hemogenic pawn's hemogen value is set to zero when they start
      deathresting, anyway.
    - Also note that the exact value will be a little off, due to the pawn's
      natural hemogen drain, which is a constant rate.
  - This feature can be disabled in the mod settings.

### Vanilla Races Expanded - Sanguophage
- Add a recipe to perform a blood transfusion to animals using animal hemogen.
  - The amount of blood gained is modified by the body size of the animal.
  - The label for the operation will indicate how much blood will be restored
    per pack.
- Add a recipe to extract an animal hemogen pack from an animal.
  - The amount of blood lost is modified by the body size of the animal.
  - The label for the operation will indicate the maximum amount of blood that
    will be lost.
    - Note that the vanilla operation to extract a hemogen pack from a human
      has a computation based on what medicine is being used to determine how
      much blood is lost, but the operation does not actually use any
      medicine!  For consistency, the corresponding operation for animals
      behaves the same way, but I want to modify both to either not include
      that calculation or actually require medicine (requiring it would make
      the easier availability of animal hemogen more balanced).

## Notes and Future Ideas

This is my first published mod, and the first mod that includes non-trivial
Harmony patches. (Including transpilers!) I plan on taking things slowly to
learn and get things right. I've been making XML mods for a while now for
myself, and dabbling just a tiny bit in C# modding before this, so I am still
relying heavily on what scant documentation exists, and on lots of
[decompiled] code. Your patience is appreciated!

As for future ideas... well, while inspecting all the kinds of bloodfeeders we
have now (with VRES), we've got a bunch, yeah, but there are some obvious
gaps. Insect feeders?  Mechanoid feeders?

The analysis file also includes more details on features not implemented yet.

### Not enough hemogen packs for blood transfusions

One interesting tidbit: the operation to perform a blood transfusion only
"requires" one hemogen pack, but the doctor will try to get as many as are
necessary to completely remove the blood loss hediff (so part of one hemogen
pack will be "wasted"). This means sometimes you will get the game telling you
that "missing hemogen pack x1", but you've panned over to your stockpile and
you can point to it, it's right there! This is because some of the hemogen
packs are on the map, but not available (e.g., reserved by someone else).

Obviously, this is not a pleasant experience, and is not conveyed. For now, I
have the same behavior with the animal blood transfusion, *except* it does not
include forbidden hemogen packs (animal) to see how many are available. You
will still have the same issues with reserved packs, etc.

Ultimately, I want to make these operations clearer, and perhaps have versions
that use *at most* one pack.

I imagine this same confusion situation also happens for a bunch of other
operations/bills/etc.

## Known Issues

None so far.

## Adding and Removing the Mod

Adding this mod to an existing save is fine.

For removing it, I expect you could get some one-time errors, and I would
recommend not having any of this mod's operations queued up or in progress
when you do so. Otherwise, it should be fine.

## Links

* Steam [workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3026361086)
* GitHub repo
  * [main page](https://github.com/thailyn/RimWorld-SanguophageTweaks)
  * [download latest
    release](https://github.com/thailyn/RimWorld-SanguophageTweaks/releases/latest)

## Reporting Issues

Please follow the typical RimWorld modding conventions for error reporting.

* Determine the minimal set of mods that are necessary to recreate the issue.
* Provide the log file with the error, preferably using
  [HugsLib's](https://steamcommunity.com/workshop/filedetails/?id=818773962)
  Ctrl+F12 feature.
* Also include a description of what went wrong and what led up to the error
  (in as much detail as you can as the medium allows).

Please post these error reports to the Steam workshop comments (most
convenient for you but does not give much room for detail), in a bug reporting
discussion thread (if one exists), or as a GitHub issue (allows for the most
comprehensive disussion and documentation).

I reserve the right to ignore any error reports which do not follow these
steps, especially if no error is provided or the log has many, many mods in
use.

## "[KOM]"? What's that?

"KOM" is an abbreviation for Komishne, another name I often go by online.  It
is also the name I typically use as a personal identifier in code, such as in
namespaces (in lieu of a company name). "[KOM]", then, is used as a mod name
prefix to identify they are mods I have made.

## License and Permissions

This work is licensed under the Apache 2.0 license.  Refer to the LICENSE.txt
file included with this mod's files for details, or visit
http://www.apache.org/licenses/.  The NOTICE.txt file includes copyright
information and any disclaimers.

Note that this license allows -- among other uses -- commercial use, so you do
not need to ask me for permission to include it in your modlist or
YouTube/Twitch stream or video (which can have monetization via ads or
donations). However, plese ensure you do understand what the terms in the
license are, and remember that RimWorld has its own
[EULA](https://rimworldgame.com/eula/); make sure you understand its terms and
limitations for these situations, as well.

Portions of the materials used to create this content/mod are trademarks
and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon.
This content/mod is not official and is not endorsed by Ludeon.