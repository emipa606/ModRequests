﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Blueprints
{
    public class Designator_Blueprint : Designator
    {
        #region Fields

        private Blueprint _blueprint;

        private float _lineOffset = -4f;
        private float _middleMouseDownTime;
        private float _panelHeight = 999f;
        private Vector2 _scrollPosition = Vector2.zero;

        #endregion Fields

        #region Constructors

        public Designator_Blueprint( Blueprint blueprint )
        {
            _blueprint = blueprint;
            icon = Resources.Icon_Blueprint;
            soundSucceeded = SoundDefOf.Designate_PlaceBuilding;
        }

        #endregion Constructors

        #region Properties

        public Blueprint Blueprint => _blueprint;
        public override int DraggableDimensions => 0;
        public override string Label => _blueprint.name;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    // rename
                    new FloatMenuOption("Nuka.Blueprints.Rename".Translate(), delegate
                    { Find.WindowStack.Add(new Dialog_NameBlueprint(Blueprint)); }),

                    // delete blueprint
                    new FloatMenuOption("Nuka.Blueprints.Remove".Translate(), delegate
                    { Controller.Remove(this, false); })
                };

                // delete blueprint and remove from disk
                if (this.Blueprint.exported)
                {
                    options.Add(new FloatMenuOption("Nuka.Blueprints.RemoveAndDeleteXML".Translate(), delegate
                    { Controller.Remove(this, true); }));
                }

                // store to xml
                else
                {
                    options.Add(new FloatMenuOption("Nuka.Blueprints.SaveToXML".Translate(), delegate
                    { Controller.SaveToXML(this.Blueprint); }));
                }

                return options;
            }
        }

        #endregion Properties

        #region Methods
        public override AcceptanceReport CanDesignateCell( IntVec3 loc )
        {
            // always return true - we're looking at the larger blueprint in SelectedUpdate() and DesignateSingleCell() instead.
            return true;
        }

        // note that even though we're designating a blueprint, as far as the game is concerned we're only designating the _origin_ cell
        public override void DesignateSingleCell( IntVec3 origin )
        {
            bool somethingSucceeded = false;
            bool planningMode = Event.current.shift;

            // looping through cells, place where needed.
            foreach ( var item in Blueprint.AvailableContents )
            {
                PlacementReport placementReport = item.CanPlace( origin );
                if ( planningMode && placementReport != PlacementReport.Alreadyplaced )
                {
                    item.Plan( origin );
                    somethingSucceeded = true;
                }
                if ( !planningMode && placementReport == PlacementReport.CanPlace )
                {
                    item.Designate( origin );
                    somethingSucceeded = true;
                }
            }

            // TODO: Add succeed/failure sounds, failure reasons
            Finalize( somethingSucceeded );
        }

        // copy-pasta from RimWorld.Designator_Place, with minor changes.
        public override void DoExtraGuiControls( float leftX, float bottomY )
        {
            float height = 90f;
            float width = 200f;
            float button = 64f;
            float numButtons = 2f;
            float margin = ( width - button * numButtons) / ( numButtons + 1 );
            float topmargin = 15f;

            Rect winRect = new Rect( leftX, bottomY - height, width, height);
            HandleRotationShortcuts();

            Find.WindowStack.ImmediateWindow( 73095, winRect, WindowLayer.GameUI, delegate
            {
                RotationDirection rotationDirection = RotationDirection.None;
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;

                Rect rotLeftRect = new Rect( margin, topmargin, button, button );
                if ( Widgets.ButtonImage( rotLeftRect, Resources.RotLeftTex ) )
                {
                    SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    rotationDirection = RotationDirection.Counterclockwise;
                    Event.current.Use();
                }
                Widgets.Label( rotLeftRect, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel );
                
                // todo; figure out blueprint flipping
                //Rect flipRect = new Rect( 2 * margin + button, topmargin, button, button );
                //if ( Widgets.ButtonImage( flipRect, Resources.FlipTex ) )
                //{
                //    SoundDefOf.FlickSwitch.PlayOneShotOnCamera();
                //    Blueprint.Flip();
                //    Event.current.Use();
                //}

                Rect rotRightRect = new Rect( 2 * margin + button, topmargin, button, button );
                if ( Widgets.ButtonImage( rotRightRect, Resources.RotRightTex ) )
                {
                    SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    rotationDirection = RotationDirection.Clockwise;
                    Event.current.Use();
                }
                Widgets.Label( rotRightRect, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel );
                if ( rotationDirection != RotationDirection.None )
                {
                    Blueprint.Rotate( rotationDirection );
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            });
        }

        public override void DrawPanelReadout( ref float curY, float width )
        {
            // we need the window's size to be able to do a scrollbar, but are not directly given that size.
            Rect outRect = ArchitectCategoryTab.InfoRect.AtZero();

            // our window actually starts from curY
            outRect.yMin = curY;

            // our contents height is given by final curY - which we conveniently saved
            Rect viewRect = new Rect( 0f, 0f, width, _panelHeight );

            // if contents are larger than available canvas, leave some room for scrollbar
            if ( viewRect.height > outRect.height )
            {
                outRect.width -= 16f;
                width -= 16f;
            }

            // since we're going to work in new GUI group (through the scrollview), we'll need to keep track of
            // our own relative Y position.
            float oldY = curY;
            curY = 0f;

            // start scrollrect
            Widgets.BeginScrollView( outRect, ref _scrollPosition, viewRect );

            // list of objects to be build
            Text.Font = GameFont.Tiny;
            foreach ( var buildables in Blueprint.GroupedBuildables )
            {
                // count
                float curX = 5f;
                Widgets.Label( new Rect( 5f, curY, width * .2f, 100f ), buildables.Value.Count + "x" );
                curX += width * .2f;

                // stuff selector
                float height = 0f;
                if ( buildables.Value.First().Stuff != null )
                {
                    string label = buildables.Value.First().Stuff.LabelAsStuff.CapitalizeFirst() + " " + buildables.Key.label;

                    Rect iconRect = new Rect( curX, curY, 12f, 12f );
                    curX += 16f;

                    height = Text.CalcHeight( label, width - curX ) + _lineOffset;
                    Rect labelRect = new Rect( curX, curY, width - curX, height );
                    Rect buttonRect = new Rect( curX - 16f, curY, width - curX + 16f, height + _lineOffset );

                    if ( Mouse.IsOver( buttonRect ) )
                    {
                        GUI.DrawTexture( buttonRect, TexUI.HighlightTex );
                        GUI.color = GenUI.MouseoverColor;
                    }
                    GUI.DrawTexture( iconRect, Resources.Icon_Edit );
                    GUI.color = Color.white;
                    Widgets.Label( labelRect, label );
                    if ( Widgets.ButtonInvisible( buttonRect ) )
                        Blueprint.DrawStuffMenu( buildables.Key );
                }
                else
                {
                    // label
                    float labelWidth = width - curX;
                    string label = buildables.Key.LabelCap;
                    height = Text.CalcHeight( label, labelWidth ) + _lineOffset;
                    Widgets.Label( new Rect( curX, curY, labelWidth, height ), label );
                }

                // next line
                curY += height + _lineOffset;
            }

            // complete cost list
            curY += 12f;
            Text.Font = GameFont.Small;
            Widgets.Label( new Rect( 0f, curY, width, 24f ), "Nuka.Blueprint.Cost".Translate() );
            curY += 24f;

            Text.Font = GameFont.Tiny;
            List<ThingDefCount> costlist = Blueprint.CostListAdjusted;
            for ( int i = 0; i < costlist.Count; i++ )
            {
                ThingDefCount thingCount = costlist[i];
                Texture2D image;
                if ( thingCount.ThingDef == null )
                {
                    image = BaseContent.BadTex;
                }
                else
                {
                    image = thingCount.ThingDef.uiIcon;
                }
                GUI.DrawTexture( new Rect( 0f, curY, 20f, 20f ), image );
                if ( thingCount.ThingDef != null && thingCount.ThingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && Find.CurrentMap.resourceCounter.GetCount( thingCount.ThingDef ) < thingCount.Count )
                {
                    GUI.color = Color.red;
                }
                Widgets.Label( new Rect( 26f, curY + 2f, 50f, 100f ), thingCount.Count.ToString() );
                GUI.color = Color.white;
                string text;
                if ( thingCount.ThingDef == null )
                {
                    text = "(" + "UnchosenStuff".Translate() + ")";
                }
                else
                {
                    text = thingCount.ThingDef.LabelCap;
                }
                float height = Text.CalcHeight( text, width - 60f ) - 2f;
                Widgets.Label( new Rect( 60f, curY + 2f, width - 60f, height ), text );
                curY += height + _lineOffset;
            }

            Widgets.EndScrollView();

            _panelHeight = curY;

            // need to give some extra offset to properly align description.
            // also, add back in our internal offset
            curY += 28f + oldY;
        }

        public override bool GroupsWith( Gizmo other )
        {
            return this.Blueprint == ( other as Designator_Blueprint ).Blueprint;
        }

        public override void Selected()
        {
            base.Selected();
            Blueprint.RecacheBuildables();
            if ( !Blueprint.AvailableContents.Any() )
                Messages.Message( "Nuka.Blueprints.NothingAvailableInBlueprint".Translate( Blueprint.name ),
                                  MessageTypeDefOf.RejectInput );
            else
            {
                var unavailable = Blueprint.contents.Except( Blueprint.AvailableContents ).Select( bi => bi.BuildableDef.label ).Distinct();
                if ( unavailable.Any() )
                    Messages.Message( "Nuka.Blueprints.XNotAvailableInBlueprint".Translate( Blueprint.name, string.Join( ", ", unavailable.ToArray() ) ), MessageTypeDefOf.CautionInput );
            }
        }

        public override void SelectedUpdate()
        {
            GenDraw.DrawNoBuildEdgeLines();
            if ( !ArchitectCategoryTab.InfoRect.Contains( UI.MousePositionOnUI ) )
            {
                IntVec3 origin = UI.MouseCell();
                _blueprint.DrawGhost( origin );
            }
        }

        // Copy-pasta from RimWorld.HandleRotationShortcuts()
        private void HandleRotationShortcuts()
        {
            RotationDirection rotationDirection = RotationDirection.None;
            if ( Event.current.button == 2 )
            {
                if ( Event.current.type == EventType.MouseDown )
                {
                    Event.current.Use();
                    _middleMouseDownTime = Time.realtimeSinceStartup;
                }
                if ( Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - _middleMouseDownTime < 0.15f )
                {
                    rotationDirection = RotationDirection.Clockwise;
                }
            }
            if ( KeyBindingDefOf.Designator_RotateRight.KeyDownEvent )
            {
                rotationDirection = RotationDirection.Clockwise;
            }
            if ( KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent )
            {
                rotationDirection = RotationDirection.Counterclockwise;
            }
            if ( rotationDirection == RotationDirection.Clockwise )
            {
                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                Blueprint.Rotate( RotationDirection.Clockwise );
            }
            if ( rotationDirection == RotationDirection.Counterclockwise )
            {
                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                Blueprint.Rotate( RotationDirection.Counterclockwise );
            }
        }

        #endregion Methods
    }
}
