# About

This project is a mod for Rimworld, the purpose is to convert some
messages into letters such as "pet is being hunted" or "marriage ceremony
is starting stuff" to their own letter.

# Architecture

## Implementation Overview

Original incident message (top left message) generation:

- Incident text is being translated via `string.Translate` extension method
- Translated text is passed to `Messages.AcceptsMessage` method
- Text is displayed to left

Patched code flow

- `string.Translate` extension method get `postfix` to keep `IncidentDef` key
- `Messages.AcceptsMessage` patched to rejected `IncidentDef` that will be converted into Letter.
- Incident is sent to `Find.LetterStack.ReceiveLetter` instead.

### `Patch_Message.cs`

This file is for patching `Message.AcceptMessage` method that is called
when as regular message is displayed. When called, this class checks
if the message should become it's own message, if so `__result` is set
to `false` so the message isn't shown, instead, a letter is created.

### `Patch_TranslatorFormattedStringExtensions.cs`

This file intercepts `string.Translation` extension method call, as it
is the most common place where message is pre-translated. We keep the
key so that `Patch_Message.cs` can know which original message the key is.

### `ModExtension_OnTranslationKey.cs`

This simply load the `IncidentDef` and check if they have `ModExtension`
with this class, if so load it into dictionary.

### `TranslationKeyObserver.cs`

A static class that is used as a medium for `Patch_Message` class and
`Patch_TranslatorFormattedStringExtensions.cs` to communicate when
a message is going to be displayed by `Message.AcceptsMessage`.

### `MainDelayed.cs`

Load all known incident at the time which this mod is loaded. Then
filter for ones which have `ModExtension_OnTranslationKey` to be kept
in `TranslationKeyObserver.translateToIncident` dictionary.

The dictionary is used to determine which incident (top left message) to
convert into a letter.