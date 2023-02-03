# BeatingsContinue

This is a mod for the game RimWorld by Ludeon Studios.

It adds widgets, jobs, and work givers, for the express purpose of beating prisoners conveniently.

# Table of Contents

* [Introduction and Explanation](#introduction-and-explanation)
* [How to Install](#how-to-install)
* [How to Update](#how-to-update)
* [Bugs, New Features, and Updates](#bugs-new-features-and-updates)

# Introduction and Explanation

Alright ... so ... this mod is probably a little out-there. Bear with me. Here's the deal.

Let's talk about some more violent methods of prisoner management.

You can beat them, y'know? Just hit 'em for a while. Until they can't walk. That'll prevent mental breaks and prison breaks.

But you don't wanna slice 'em in half with your plasma sword. You just wanna punch 'em. With your fists. You can do this in vanilla. But you gotta tell your pawn to drop their weapon on the ground first. And you gotta remember to tell your pawn to pick it back up when they're done. That's annoying. So we add our first feature, a new widget: "Unarmed attack". This is just like "Melee" attack, except they don't use their weapon, they do it as if they were social fighting, and also you can issue the order when they're not drafted, and they'll go back to something else when it is finished, instead of standing there drafted like idiots. Just like "Melee", they only stop if/when the target becomes downed. This is also very useful and convenient against your own Berserk colonists.

Also, uh, if you're beating prisoners, you wanna probably keep going sometimes (even when they're downed) as long as they can take it, but you also probably wanna stop sooner other times. Like, before you kill them (for prisoners you're not recruiting), or before you destroy any parts at all (for prisoners you're recruiting). You can do this in vanilla. But you gotta laboriously watch the "Health" tab of the target, watch for any parts that get damaged below 12 or so, and stop them. That's really annoying to do manually. So we add our second feature, another widget: "Beat". This one is like "Unarmed attack", except they will keep going when the target is downed, but instead stop before killing them (for prisoners you're not recruiting) or before they destroy any parts at all (for prisoners you're recruiting). Conveniently, your pawn doing the beating will also stop early to protect their own limbs, like if they take a lucky punch to a finger, so they don't lose it to another punch. (Fingers and toes aren't a problem once you're on artificial hands/limbs.)

Finally, once the beatings are done, your prisoners are going to lay there nicely downed and controlled for a while. But they'll eventually heal back up, and you wanna beat 'em again. It is annoying to remember to keep checking this. So we add our final new feature: A designator "Suppress", that you can put on prisoners, and a checkbox in the Work tab "Suppress". For any pawns with this Work checked, for any prisoners with the "Suppress" designated, the pawns will automatically come back and beat them again, as soon as they're able to (per the above limitations on when to stop).

I personally suggest, for your colonist doing the beating, to replace their jaw with a denture, so they don't bite anybody, and don't cause infections. But that's your choice.

The result is a truly deranged method of prisoner management. And you could technically do all of this in pure vanilla RimWorld. We don't technically change any behaviors. We just make it easier and more convenient to be the war criminal you've always wanted to be.

# How to Install

At the top of this page, on the right-hand-side, a little ways down, will be a green button, labeled "Clone or download". Click it, then click "Download ZIP". Your browser will download it.

Unzip it, and it will spew out a single folder, which is probably named something like `BeatingsContinue-master`.

Assuming you are working with default installation directories on a Windows system, you will want to move this entire folder into:

`C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods`

If you did it correctly, the result should be a directory structure that looks something like this:

`C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\BeatingsContinue-master\Assemblies`

Then restart RimWorld and enable it like any other mod.

# How to Update

First and foremost, please note that I never test updating mods on older saved games. You can try it, but please assume that a new game might always be necessary.

I also don't explicitly test whether the mod can be disabled on an existing game. Please also assume that a new game might always be necessary.

With that out of the way:

Updating is just deleting the previous version of the mod and then installing a new version.

So again, assuming default installation directories on a Windows system, you'll want to delete the same folder that you added during installation, which probably looks something like:

`C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\BeatingsContinue-master`

Then follow the previous instructions to download and install the new version, by repeating the same steps as installing the original version.

# Bugs, New Features, and Updates

You are currently looking at a GitHub repository for managing application code. I work out of this GitHub repository, and so to talk about bugs, new features, or updates, you need to know a little bit about navigating a GitHub repository like this one.

Beneath the aforementioned green button "Clone or download", it will say "Latest commit", followed by a couple random characters, followed by an amount of time. This stamp indicates how long ago the mod was last updated.

So if you think you found a problem, check this stamp. Perhaps the mod has already been updated since you downloaded it last, and you should download a new version and update. See the above instructions for how to update.

By default, you are probably looking at the "master" branch. You can see this at the top of the page, on the left-hand-side, a little ways down, it will say "Branch: something", probably "Branch: master", with a little down arrow.

The "master" branch contains the current version of the mod which I consider to be tested and stable. Mostly. I guess.

Most (but not all) of my mods have a "beta" branch for pre-release, which might offer new features or bug fixes that should probably work, theoretically, but I haven't really done much testing on, so I'm not quite sure.

So if you tried updating from the "master" branch, and you still think you found a bug or a problem, or if you just want to try the shiny new features before everybody else, consider downloading the "beta" branch and installing that instead.

To do this, just click the button where it says "Branch: master", and then click the option for "beta". Congratulations! You've changed branches! Follow the same steps to download and install, except instead of `BeatingsContinue-master` it will now be `BeatingsContinue-beta`. You can have both versions installed, but please don't try to have both versions enabled at once using the in-game Mod menu.

You will probably see other choices besides "master" and "beta", but I don't recommend clicking them. I am probably in the middle of working on them, and they are probably only halfway done, and broken, otherwise they'd already be part of "beta".

So if you tried updating the master branch, and you tried the beta branch, and you still thing you found a bug, or a problem, or want to suggest a new feature, wander over to the "Issues" tab. You can find this at the very top of the page, you are currently on the first tab "Code", you want to change to the second tab "Issues".

You can look here to see if your bug, issue, or suggestion is already present, and add comments if you wish.

If it's not there, look to the right-hand-side, click the green button "New issue", just type a Title, and Leave a comment, and then look below and click the green button "Submit new issue". I will get back to you. Maybe. Eventually. Meanwhile, other users might be able to chime in and help you too!
