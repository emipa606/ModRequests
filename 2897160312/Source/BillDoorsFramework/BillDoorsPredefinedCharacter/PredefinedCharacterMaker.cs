using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace BillDoorsPredefinedCharacter
{
    public class PredefinedCharacterMaker
    {
        const int maxTries = 100;

        public static Pawn GetPawn(PredefinedCharacterParmDef parm, Predicate<PredefinedCharacterParmDef, Pawn> existingPawnPredicate = null, Action<Pawn> postAction = null, Faction faction = null, bool rePostProcessExisting = true)
        {
            Pawn p = null;
            if (BDPDC_Mod.Tracker.ContainsKey(parm))
            {
                p = BDPDC_Mod.Tracker[parm];
                if (!p.Spawned && !p.Destroyed && (existingPawnPredicate == null || existingPawnPredicate(parm, p)))
                {
                    if (faction != null) p.SetFactionDirect(faction);
                    if (rePostProcessExisting)
                    {
                        PawnPostProcess(ref p, parm);
                    }
                }
                else
                {
                    p = null;
                }
            }

            if (p == null)
            {
                p = MakePawn(parm, faction);
            }
            postAction?.Invoke(p);
            p.Drawer.renderer.SetAllGraphicsDirty();
            p.health.overrideDeathOnDownedChance = 0;
            return p;
        }

        public static Pawn MakePawn(PredefinedCharacterParmDef parm, Faction faction = null)
        {
            var raceCache = parm.basePawnKindDef.race;
            parm.basePawnKindDef.race = parm.raceDef;
            Pawn p = PawnGenerator.GeneratePawn(generationRequest(parm, faction));
            if (p.def != parm.raceDef)
            {
                int i = 0;
                while (p.def != parm.raceDef)
                {
                    p = PawnGenerator.GeneratePawn(generationRequest(parm, faction));
                    Log.Message($"Generated {parm.defName} as {p.def.defName}");
                    i++;
                    if (i > maxTries)
                    {
                        Log.Error("Failed to generate with correct race. Returning with incorrect race. You have too many HAR race mods dammit.");
                        break;
                    }
                }
            }
            parm.basePawnKindDef.race = raceCache;

            PawnPostProcess(ref p, parm, true);
            BDPDC_Mod.PawnPDCPair.AddDistinct(p, parm);
            if (parm.tracked) GameComponent_CharacterTracker.TrackedPawns.SetOrAdd(parm, p);
            return p;
        }

        public static void PawnPostProcess(ref Pawn p, PredefinedCharacterParmDef parm)
        {
            PawnPostProcess(ref p, parm, false);
        }

        public static void PawnPostProcess(ref Pawn p, PredefinedCharacterParmDef parm, bool complete = false)
        {
            if (complete)
            {
                PawnBioPostProcess(p, parm);
                PawnIdeoPostProcess(p, parm);
            }
            PawnEquipmentPostProcess(p, parm);
            PawnStylePostProcess(p, parm);

            if (parm.alwaysRecruitable && p.guest != null)
            {
                p.guest.Recruitable = true;
            }
        }
        public static void PawnEquipmentPostProcess(Pawn p, PredefinedCharacterParmDef parm)
        {
            if (parm.weaponDef != null)
            {
                p.equipment.Primary?.Destroy();
                Thing wep = ThingMaker.MakeThing(parm.weaponDef, parm.weaponStuffDef);
                if (wep.TryGetComp<CompQuality>() is CompQuality q)
                {
                    q.SetQuality(QualityCategory.Normal, null);
                }

                p.equipment.AddEquipment((ThingWithComps)wep);
            }
            if (parm.forcedApparels != null)
            {
                p.apparel.DestroyAll();
                p.outfits?.forcedHandler?.Reset();
                foreach (var app in parm.forcedApparels)
                {
                    if (!Rand.Chance(app.DropChance)) continue;
                    Apparel apparel = (Apparel)MakeThingFromTDCC(app);
                    if (ApparelUtility.HasPartsToWear(p, apparel.def))
                    {
                        p.apparel.Wear(apparel, dropReplacedApparel: false);
                    }
                    if (app.color != null && apparel.TryGetComp<CompColorable>() is CompColorable cc)
                    {
                        if (app.color == Color.white) cc.SetColor(Color.black);
                        cc.SetColor(app.color.Value);
                    }
                }
            }
            if (parm.possessions.Any())
            {
                foreach (var thingCount in parm.possessions)
                {
                    if (!Rand.Chance(thingCount.DropChance)) continue;
                    int count = thingCount.count;
                    while (count > 0)
                    {
                        int usedCount = Math.Min(count, thingCount.thingDef.stackLimit);
                        count -= usedCount;
                        Thing t = MakeThingFromTDCC(thingCount);
                        t.stackCount = usedCount;
                        p.inventory.TryAddAndUnforbid(t);
                    }
                }
            }
        }
        public static void PawnStylePostProcess(Pawn p, PredefinedCharacterParmDef parm)
        {
            if (parm.headType != null) p.story.headType = parm.headType;
            if (parm.skinColor != null) p.story.SkinColorBase = parm.skinColor.Value;
            p.story.skinColorOverride = parm.skinColor;

            if (p.style != null)
            {
                p.style.FaceTattoo = parm.faceTattoo ?? TattooDefOf.NoTattoo_Face;
                p.style.BodyTattoo = parm.bodyTattoo ?? TattooDefOf.NoTattoo_Body;
            }

            if (parm.forcedHairDef != null) p.story.hairDef = parm.forcedHairDef;
            if (parm.hairColor != null) p.story.HairColor = parm.hairColor.Value;
        }
        public static void PawnIdeoPostProcess(Pawn p, PredefinedCharacterParmDef parm)
        {
            if (ModLister.IdeologyInstalled && ModsConfig.IdeologyActive)
            {
                if (parm.useFactionIdeo && p.Faction != null)
                {
                    p.ideo.SetIdeo(p.Faction.ideos?.PrimaryIdeo);
                }
                else
                {
                    p.ideo.SetIdeo(GetIdeo(parm));
                }
            }
        }

        static void PawnBioPostProcess(Pawn p, PredefinedCharacterParmDef parm)
        {
            p.kindDef = parm.basePawnKindDef;
            if (p.Name is NameTriple nameTriple)
            {
                var first = parm.firstName ?? nameTriple.First;
                var last = parm.lastname ?? nameTriple.Last;

                p.Name = new NameTriple(first, parm.nickname, last);
                p.story.birthLastName = last;
            }

            if (parm.abilities != null)
            {
                foreach (var ability in parm.abilities)
                {
                    p.abilities.GainAbility(ability);
                }
            }

            if (parm.fixedChildBackStories.Any()) p.story.Childhood = parm.fixedChildBackStories.RandomElement();
            if (parm.fixedAdultBackStories.Any()) p.story.Adulthood = parm.fixedAdultBackStories.RandomElement();
            if (parm.title != null) p.story.title = parm.title;

            foreach (BackstoryDef item in p.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                foreach (var skillGain in item.skillGains)
                {
                    p.skills.GetSkill(skillGain.skill).Level += skillGain.amount;
                }
            }

            foreach (var skillGain in parm.skillOverrides)
            {
                if (skillGain.amount != null) p.skills.GetSkill(skillGain.skill).Level = skillGain.amount.Value;
                if (skillGain.passion != null) p.skills.skills.Find(s => s.def == skillGain.skill).passion = skillGain.passion.Value;
            }

            if (parm.traits != null)
            {
                p.story.traits.allTraits.Clear();
                foreach (TraitRequirement forcedTrait in parm.traits)
                {
                    p.story.traits.GainTrait(new Trait(forcedTrait.def, forcedTrait.degree ?? 0, forced: true));
                }
            }

            foreach (var hediffInfo in parm.hediffs)
            {
                if (hediffInfo.bodyPart == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffInfo.hediff, p, null);
                    p.health.AddHediff(hediff);
                }
                else
                {
                    foreach (BodyPartRecord notMissingPart in p.health.hediffSet.GetNotMissingParts())
                    {
                        if ((hediffInfo.partCustomLabel != null && notMissingPart.untranslatedCustomLabel == hediffInfo.partCustomLabel)
                            || notMissingPart.def == hediffInfo.bodyPart)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(hediffInfo.hediff, p, notMissingPart);
                            p.health.AddHediff(hediff);
                            break;
                        }
                    }
                }
            }
            foreach (var scar in parm.scars)
            {
                foreach (BodyPartRecord notMissingPart in p.health.hediffSet.GetNotMissingParts())
                {
                    if (notMissingPart.def == scar.bodyPart && notMissingPart.def.canScarify)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(scar.damage.hediff, p, notMissingPart);
                        hediff.Severity = scar.damageAmount;
                        HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
                        hediffComp_GetsPermanent.IsPermanent = true;
                        hediffComp_GetsPermanent.SetPainCategory(PainCategory.Painless);
                        p.health.AddHediff(hediff);
                        break;
                    }
                }
            }

            if (ModLister.BiotechInstalled)
            {
                foreach (var generemove in parm.removeGenes)
                {
                    p.genes.RemoveGene(p.genes.GetGene(generemove));
                }

                if (parm.removeSkinColorGene || parm.removeHairColorGene)
                {
                    foreach (var g in p.genes.GenesListForReading)
                    {
                        if (parm.removeSkinColorGene && g.def.skinColorBase != null)
                        {
                            p.genes.RemoveGene(g);
                        }
                    }
                }
            }

            if (ModLister.RoyaltyInstalled)
            {
                foreach (var titles in parm.royalTitles)
                {
                    p.royalty.SetTitle(Find.FactionManager.FirstFactionOfDef(titles.faction), titles.titleDef, true, false);
                }
                foreach (var permits in parm.royalPermits)
                {
                    p.royalty.AddPermit(permits.permit, Find.FactionManager.FirstFactionOfDef(permits.faction));
                }
            }

            if (parm.eyeColor != null && ModLister.GetActiveModWithIdentifier("nals.facialanimation") != null)
            {
                try
                {
                    Assembly FA = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "FacialAnimation");
                    if (FA != null)
                    {
                        Type EyeCon = FA.GetType("FacialAnimation.EyeballControllerComp");
                        if (EyeCon != null)
                        {
                            var CompEyeControl = p.AllComps.Where(c => c.GetType() == EyeCon).FirstOrDefault();
                            if (CompEyeControl != null)
                            {
                                FieldInfo EyeColor = EyeCon.GetField("color", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (EyeColor != null)
                                {
                                    EyeColor.SetValue(CompEyeControl, parm.eyeColor);
                                    Log.Message("FA q");
                                }
                                FieldInfo EyeIIColor = EyeCon.GetField("secondColor", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (EyeIIColor != null)
                                {
                                    EyeIIColor.SetValue(CompEyeControl, parm.eyeColor);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        public static PawnGenerationRequest generationRequest(PredefinedCharacterParmDef parm, Faction faction = null)
        {
            PawnGenerationRequest req = new PawnGenerationRequest(parm.basePawnKindDef);
            //Not a cosplayer, must be a new pawn
            req.ForceGenerateNewPawn = true;
            req.RedressValidator = delegate { return false; };
            req.ForceBodyType = parm.forcedBodyTypeDef;
            req.CanGeneratePawnRelations = false;
            req.MustBeCapableOfViolence = true;
            req.ForceAddFreeWarmLayerIfNeeded = true;
            req.AllowGay = false;
            req.AllowAddictions = false;
            req.FixedBiologicalAge = parm.fixedAge;
            req.FixedChronologicalAge = parm.fixedChoroAge;
            req.FixedGender = parm.gender;
            req.FixedLastName = parm.lastname;
            req.ForcedXenotype = parm.baseXenotype ?? null;
            req.ForcedEndogenes = parm.endoGenes;
            req.ForcedXenogenes = parm.xenoGenes;
            req.Faction = faction ?? FactionUtility.DefaultFactionFrom(parm.fixedFaction) ?? null;
            req.FixedIdeo = GetIdeo(parm);
            return req;
        }

        public static Ideo GetIdeo(PredefinedCharacterParmDef parm)
        {
            if (parm.useFactionIdeo)
            {
                return null;
            }
            if (ModLister.IdeologyInstalled && ModsConfig.IdeologyActive)
            {
                IdeoGenerationParms parms = new IdeoGenerationParms(FactionDefOf.AncientsHostile, false, styles: parm.styles, disallowedPrecepts: parm.disallowedPreceptDefs, disallowedMemes: parm.disallowedMemes, forcedMemes: parm.forcedMemes, forceNoWeaponPreference: true, hidden: false, fixedIdeo: true);
                Faction f = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(FactionDefOf.Beggars, parms, true));
                return f.ideos.PrimaryIdeo;
            }
            return null;
        }

        [DebugAction("Predefined Character", "Spawn Character", false, false, false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static List<DebugActionNode> DebugSpawn()
        {
            return DebugSpawnInternal(Find.FactionManager.OfPlayer);
        }
        [DebugAction("Predefined Character", "Spawn Hostile Character", false, false, false, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static List<DebugActionNode> DebugSpawnHostile()
        {
            return DebugSpawnInternal(Find.FactionManager.OfPirates);
        }

        static List<DebugActionNode> DebugSpawnInternal(Faction faction)
        {
            List<DebugActionNode> list = new List<DebugActionNode>();
            foreach (var parm in DefDatabase<PredefinedCharacterParmDef>.AllDefsListForReading)
            {
                list.Add(new DebugActionNode(parm.defName, DebugActionType.ToolMap, delegate
                {
                    Pawn pawn = GetPawn(parm, faction: faction);
                    GenSpawn.Spawn(pawn, UI.MouseCell(), Find.CurrentMap);
                    PostDebugPawnSpawn(pawn);
                }));
            }
            return list;
        }

        [DebugAction("Predefined Character", "Output pawn as PDCdef", false, false, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void OutputPawnAsDef(Pawn p)
        {
            XmlDocument doc = new XmlDocument();

            var defE = doc.AppendChild(doc.CreateElement("BillDoorsPredefinedCharacter.PredefinedCharacterParmDef"));

            addElement(defE, "defName", p.Name.ToStringShort);
            addElementDef(defE, "forcedBodyTypeDef", p.story.bodyType);
            addElementDef(defE, "raceDef", p.def);
            addElementDef(defE, "basePawnKindDef", p.kindDef);
            addElement(defE, "gender", p.gender.ToString());

            var nt = p.Name as NameTriple;
            addElement(defE, "firstName", nt.First);
            addElement(defE, "nickname", nt.Nick);
            addElement(defE, "lastname", nt.Last);
            if (p.story.title != null) addElement(defE, "title", p.story.title);

            addElement(defE, "fixedAge", p.ageTracker.AgeBiologicalYearsFloat.ToString());
            addElement(defE, "fixedChoroAge", p.ageTracker.AgeChronologicalYearsFloat.ToString());

            addElementDef(defE, "forcedHairDef", p.story.hairDef);
            addElementDef(defE, "headType", p.story.headType);
            addElement(defE, "hairColor", p.story.HairColor.ToString());
            addElement(defE, "skinColor", p.story.SkinColor.ToString());
            addElementDef(defE, "faceTattoo", p.style.FaceTattoo);
            addElementDef(defE, "bodyTattoo", p.style.BodyTattoo);

            if (p.abilities?.abilities.Any() ?? false) addElementList(defE, "forcedBodyTypeDef", p.abilities.abilities.Select(b => b.def.defName).ToList());

            if (p.equipment.Primary != null)
            {
                addElementListSingle(defE, "weaponDefs", p.equipment.Primary.def.defName);
                if (p.equipment.Primary.Stuff != null) addElementDef(defE, "weaponStuffDef", p.equipment.Primary.Stuff);
            }

            if (p.apparel.WornApparel.Any())
            {
                var apps = addElement(defE, "forcedApparels", null);
                foreach (var t in p.apparel.WornApparel)
                {
                    reduceThingToTDCC(apps, t);
                }
            }

            addElementDefListSingle(defE, "fixedChildBackStories", p.story.Childhood);
            if (p.story.Adulthood != null) addElementDefListSingle(defE, "fixedAdultBackStories", p.story.Adulthood);

            var skps = addElement(defE, "skillOverrides", null);
            foreach (var s in p.skills.skills)
            {
                var skp = addElement(skps, "li", null);
                addElementDef(skp, "skill", s.def);
                addElement(skp, "amount", s.Level.ToString());
                addElement(skp, "passion", s.passion.ToString());
            }


            var trts = addElement(defE, "traits", null);
            foreach (var s in p.story.traits.allTraits)
            {
                var trt = addElement(trts, s.def.defName, s.Degree.ToString());
            }


            if (p.inventory.innerContainer.Any())
            {
                var pcss = addElement(defE, "possessions", null);
                foreach (var t in p.inventory.innerContainer)
                {
                    reduceThingToTDCC(pcss, t);
                }
            }

            if (ModLister.BiotechInstalled)
            {
                if (p.genes?.Endogenes.Any() ?? false) addElementList(defE, "endoGenes", p.genes.Endogenes.Select(g => g.def.defName).ToList());
                if (p.genes?.Xenogenes.Any() ?? false) addElementList(defE, "endoGenes", p.genes.Xenogenes.Select(g => g.def.defName).ToList());
                if (p.genes?.Xenotype != null) addElementDef(defE, "baseXenotype", p.genes.Xenotype);
            }

            if (ModLister.RoyaltyInstalled)
            {
                if (p.royalty.AllTitlesForReading.Any())
                {
                    var ttls = addElement(defE, "royalTitles", null);
                    foreach (var t in p.royalty.AllTitlesForReading)
                    {
                        addElementDef(ttls, t.faction.def.defName, t.def);
                    }
                }
                if (p.royalty.AllFactionPermits.Any())
                {
                    var pmts = addElement(defE, "royalTitles", null);
                    foreach (var t in p.royalty.AllFactionPermits)
                    {
                        addElementDef(pmts, t.Faction.def.defName, t.Permit);
                    }
                }
            }

            var scars = addElement(defE, "scars", null);
            var hediffs = addElement(defE, "hediffs", null);

            foreach (var h in p.health.hediffSet.hediffs)
            {
                if (h is Hediff_Injury ijy)
                {
                    if (ijy.IsPermanent())
                    {
                        var li = addElement(scars, "li", null);
                        addElementDef(li, "bodyPart", ijy.Part.def);
                        addElementDef(li, "damage", DefDatabase<DamageDef>.AllDefsListForReading.Where(d => d.hediff == ijy.def).FirstOrFallback(DamageDefOf.Cut));
                        addElement(li, "damageAmount", ijy.Severity.ToString());
                    }
                }
                else
                {
                    var li = addElement(hediffs, "li", null);
                    addElementDef(li, "hediff", h.def);
                    if (h.Part != null)
                    {
                        addElementDef(li, "bodyPart", h.Part.def);
                        addElement(li, "partCustomLabel", h.Part.untranslatedCustomLabel);
                    }
                }
            }

            Log.Message(doc.OuterXml);
            Log.TryOpenLogWindow();

            XmlNode addElement(XmlNode node, string name, string innerText)
            {
                var n = node.AppendChild(doc.CreateElement(name));
                n.InnerText = innerText;
                return n;
            }

            XmlNode addElementDef(XmlNode node, string name, Def def)
            {
                return addElement(node, name, def.defName);
            }

            XmlNode addElementList(XmlNode node, string name, List<string> list)
            {
                var n = addElement(node, name, null);
                foreach (var s in list)
                {
                    addElement(n, "li", s);
                }
                return n;
            }

            XmlNode addElementListSingle(XmlNode node, string name, string innerText)
            {
                return addElementList(node, name, new List<string>() { innerText });
            }

            XmlNode addElementDefListSingle(XmlNode node, string name, Def def)
            {
                return addElementListSingle(node, name, def.defName);
            }

            void reduceThingToTDCC(XmlNode node, Thing t)
            {
                var li = addElement(node, t.def.defName, null);
                bool flag = false;
                if (t.Stuff != null)
                {
                    flag = true;
                    addElementDef(li, "stuff", t.Stuff);
                }

                if (t.TryGetQuality(out var quality))
                {
                    flag = true;
                    addElement(li, "quality", quality.ToString());
                }

                if (flag)
                {
                    addElement(li, "count", t.stackCount.ToString());
                }
                else
                {
                    li.InnerText = t.stackCount.ToString();
                }
            }
        }

        public static Thing MakeThingFromTDCC(ThingDefCountClass tdc)
        {
            Thing t = ThingMaker.MakeThing(tdc.thingDef, tdc.stuff ?? tdc.thingDef.defaultStuff);
            t.stackCount = tdc.count;
            if (t.TryGetComp<CompQuality>() is CompQuality cq)
            {
                cq.SetQuality(tdc.quality, ArtGenerationContext.Outsider);
            }
            return t;
        }

        private static void PostDebugPawnSpawn(Pawn pawn)
        {
            if (pawn.Spawned && pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
            {
                Lord lord = null;
                if (pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) => p != pawn))
                {
                    lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f, (Thing p) => p != pawn && ((Pawn)p).GetLord() != null)).GetLord();
                }
                if (lord == null || !lord.CanAddPawn(pawn))
                {
                    lord = LordMaker.MakeNewLord(pawn.Faction, pawn.Faction.HostileTo(Faction.OfPlayer) ? (LordJob)new LordJob_AssaultColony() : new LordJob_DefendPoint(pawn.Position), Find.CurrentMap);
                }
                if (lord != null && lord.LordJob.CanAutoAddPawns && !lord.ownedPawns.Contains(pawn))
                {
                    lord.AddPawn(pawn);
                }
            }
            pawn.Rotation = Rot4.South;
        }
    }
}
