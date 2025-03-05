using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;

namespace Krelinos_BiocodeIt
{
    class CompBiocodable_Custom : CompBiocodable
    {
        protected override void OnCodedFor( Pawn p )
        {
            base.OnCodedFor( p );
            Log.Message( "nyaa" );
            //Messages.Message( String.Format( "BiocodedToolApplied".Translate(), this.parent.LabelShort, this.codedPawn.LabelShort ), this.parent, MessageTypeDefOf.PositiveEvent, true );
            Messages.Message( String.Format( "BiocodedToolApplied".Translate(), "wepName", "pawnNameShort" ), this.parent, MessageTypeDefOf.PositiveEvent, true );
        }
    }
}
