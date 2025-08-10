## v0.5.0 - 2023.09.15

### New features

* Introduced new stats related to hemogen:
  * Hemogen concentration
    * A measure of how much hemogen is in a pawn's blood, based on their current hemogen value and how much blood they have lost.
  * Hemogen bleed rate
    * A measure of how quickly a pawn is losing hemogen due to bleeding, based on bleed rate and hemogen concentration.
* A bleeding hemogenic pawn now also loses hemogen.
  * This can be disabled in the mod settings.

## v0.4.0 - 2023.09.11

### New features

* Deathless pawns no longer necessarily die from surgery failures.
  * The process that runs when a surgery's "death on surgery failure chance" triggers has been modified so it adds an immediately-lethal hediff ("fatal surgery mishap").
    * This will kill non-deathless pawns all the same, but deathless pawns will have a chance to enter deathrest or a regenerative coma.
    * If the pawn does not immediately die, the affected body part (if any) will also be immediately destroyed.
      * This *will* kill a deathless pawn if the destroyed body part in question is the neck/head/brain/etc, because deathless pawns still kinda need those.
    * This hediff will disappear after a couple of hours, allowing deathless pawns to deathrest/regenerate normally.

## v0.3.0 - 2023.08.31

### New features

* Use this mod's blood loss formula for computing the victim's blood loss from VRES's animal bloodfeeder ability.
  * Can be disabled in the settings (requires restart).

### Bug fixes

* Move all patches and code (including a compiled assembly) that requires VRES to a subdirectory that is only loaded when VRES is also present.
  * Uses the LoadFolders.xml feature for this conditional loading.

## v0.2.0 - 2023.08.27

* Documentation cleanup.
* Add analysis of mod changes (and what is not changed [yet]) and thought process that led to this mod.

## v0.1.0 - 2023.08.26

* Initial version.
