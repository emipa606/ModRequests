using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Verse;

namespace Copyprint
{
    public class Designator_CreateCopyprint : Designator
    {
        #region Public Constructors

        public Designator_CreateCopyprint()
        {
            icon = Resources.Icon_AddCopyprint;
            defaultLabel = "Nuka.Copyprints.Create".Translate();
            defaultDesc = "Nuka.Copyprints.CreateHelp".Translate();
            useMouseIcon = true;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundSucceeded = SoundDefOf.Designate_PlanAdd;
            tutorTag = "Copyprint";
        }

        #endregion Public Constructors

        #region Public Properties

        public override int DraggableDimensions => 2;

        #endregion Public Properties

        #region Public Methods

        public override AcceptanceReport CanDesignateCell( IntVec3 loc )
        {
            //var things = loc.GetThingList( Map );
            return true;//loc.InBounds( Map ) && ( !things.NullOrEmpty() && things.Any( thing => !thing.IsPawn() ) );
        }

        public override void RenderHighlight( List<IntVec3> dragCells )
        {
            DesignatorUtility.RenderHighlightOverSelectableCells( this, dragCells);
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions {
            get
            {
                var options = new List<FloatMenuOption>();

                foreach (var file in Controller.GetSavedFilesList())
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    if (Controller.FindCopyprint(name) == null)
                        options.Add(new FloatMenuOption("Nuka.Copyprints.LoadFromXML".Translate(name),
                            delegate { Controller.Add(Controller.LoadFromXML(file.Name)); }));
                }

                if (options.NullOrEmpty())
                    Messages.Message("Nuka.Copyprints.NoStoredTemplates".Translate(), MessageTypeDefOf.RejectInput);
                return options;
            }
        }

        public override void DesignateMultiCell( IEnumerable<IntVec3> cells )
        {
            // bail out if empty
            if ( cells == null || cells.Count() == 0 )
            {
                Messages.Message("Nuka.Copyprints.CannotCreateCopyprint_NothingSelected".Translate(),
                    MessageTypeDefOf.RejectInput );
                return;
            }

            // get list of buildings in the cells, note that this includes frames and copyprints, and so _may include floors!_
            var things = new List<Thing>( cells.SelectMany( cell => cell.GetThingList( Map ).Where( thing => !thing.IsPawn() && !thing.def.IsFilth ) ).Distinct() );

            // get list of creatable terrains
            var terrains = new List<Pair<TerrainDef, IntVec3>>();
            terrains.AddRange( cells.Select( cell => new Pair<TerrainDef, IntVec3>( cell.GetTerrain( Map ), cell ) ) );

            // get edges of copyprint area
            // (might be bigger than selected region, but never smaller).
            var allCells = cells.Concat( things.SelectMany( thing => thing.OccupiedRect().Cells ) );

            var left = allCells.Min( cell => cell.x );
            var top = allCells.Max( cell => cell.z );
            var right = allCells.Max( cell => cell.x );
            var bottom = allCells.Min( cell => cell.z );

            // total size ( +1 because x = 2 ... x = 4 => 4 - 2 + 1 cells )
            var size = new IntVec2( right - left + 1, top - bottom + 1 );

            // fetch origin for default (North) orientation
            var origin = Resources.CenterPosition( new IntVec3( left, 0, bottom ), size, Rot4.North );

            // create list of buildables
            var copyables = new List<CopyableInfo>();
            foreach ( var thing in things )
            {
                CopyableInfo copyableInfo;
                if (thing is Plant)
                {
                    copyableInfo = new CopyableInfo(thing, origin, ((Plant)thing).Growth);
                    
                }
                else if(thing.stackCount>1)
                {
                    copyableInfo = new CopyableInfo(thing, origin, thing.stackCount);
                }
                else
                {
                    copyableInfo= new CopyableInfo(thing, origin);
                }
                if (thing.TryGetQuality(out QualityCategory qc))
                {
                    copyableInfo._qualityCategory = qc;
                }
                copyables.Add(copyableInfo);
            }

            foreach ( var terrain in terrains )
            {
                copyables.Add( new CopyableInfo( terrain.First, terrain.Second, origin ) );
            }

            // try to get a decent default name: check if selection contains only a single room - if so, that's a decent name.
            var room = origin.GetRoom( Map );
            string defaultName = null;
            if ( room != null && room.Role != RoomRoleDefOf.None )
            {
                defaultName = room.Role.LabelCap;
            }
            // TODO: multiple (same) rooms, etc.

            // add to controller - controller handles adding to designations
            var copyprint = new Copyprint( copyables, size, defaultName );
            copyprint._cells.AddRange(cells);
            for (int index=0; index<copyprint._cells.Count;index++)
            {
                copyprint._cells[index] -= origin;
            }
            Controller.Add( copyprint );

#if DEBUG
            copyprint.Debug();
#endif
        }

        #endregion Public Methods
    }
}