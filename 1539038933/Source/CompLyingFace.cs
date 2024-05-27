using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LyingFace {
    public class CompLyingFace : ThingComp {
        public Rot4 rotation = Rot4.Invalid;

        public override void Initialize(CompProperties props) {
            base.Initialize(props);

            if (rotation == Rot4.Invalid) {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null) {
                    rotation = DefaultRotation(pawn);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            Pawn pawn = this.parent as Pawn;
            if (pawn != null && pawn.GetPosture() == PawnPosture.LayingInBed) {
                yield return new Command_Action {
                    defaultLabel = "LyingFace.SetLyingFace".Translate(),
                    icon = getIcon(rotation),
                    action = delegate () {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        for (int i = 0; i < 4; i++) {
                            Rot4 rot = new Rot4(i);
                            string label = rot.ToStringHuman();
                            Action action = delegate {
                                rotation = rot;
                            };
                            options.Add(new FloatMenuOption(label, action));
                        }
                        FloatMenu menu = new FloatMenu(options);
                        Find.WindowStack.Add(menu);
                    }
                };
            }
            yield break;
        }

        public override void PostExposeData() {
            Rot4 defaultRotation = Rot4.Invalid;

            Scribe_Values.Look<Rot4>(ref this.rotation, "lyingFace", defaultRotation);

            if (Scribe.mode == LoadSaveMode.LoadingVars && this.rotation == Rot4.Invalid) {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null) {
                    this.rotation = DefaultRotation(pawn);
                }
            }
        }

        private static Rot4 DefaultRotation(Pawn pawn) {
            if (pawn.RaceProps.Humanlike) {
                switch (pawn.thingIDNumber % 4) {
                case 0:
                    return Rot4.South;
                case 1:
                    return Rot4.South;
                case 2:
                    return Rot4.East;
                case 3:
                    return Rot4.West;
                }
            } else {
                switch (pawn.thingIDNumber % 4) {
                case 0:
                    return Rot4.South;
                case 1:
                    return Rot4.East;
                case 2:
                    return Rot4.West;
                case 3:
                    return Rot4.West;
                }
            }
            return Rot4.Random;
        }

        public static Texture2D getIcon(Rot4 rot) {
            switch (rot.AsInt) {
            case 0:
                return MyTex.LyingFaceNorth;
            case 1:
                return MyTex.LyingFaceEast;
            case 2:
                return MyTex.LyingFaceSouth;
            case 3:
                return MyTex.LyingFaceWest;
            default:
                throw new Exception("rotInt's value cannot be >3 but it is:" + rot.AsInt);
            }
        }
    }
}
