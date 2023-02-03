using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Copyprint
{
    public class Dialog_NameCopyprint : Dialog_Rename
    {
        #region Fields

        private Copyprint _copyprint;

        #endregion Fields

        #region Constructors

        public Dialog_NameCopyprint( Copyprint copyprint ) : base()
        {
            _copyprint = copyprint;
            curName = copyprint.name;
        }

        #endregion Constructors

        #region Properties

        protected override int MaxNameLength => 24;

        #endregion Properties

        #region Methods

        protected override AcceptanceReport NameIsValid( string newName )
        {
            // always ok if we didn't change anything
            if ( newName == _copyprint.name )
                return true;

            // otherwise check for used symbols and uniqueness
            AcceptanceReport validName = Copyprint.IsValidCopyprintName( newName );
            if ( !validName.Accepted )
                return validName;

            // finally, if we're renaming an already exported copyprint, check if the new name doesn't already exist
            if ( _copyprint.exported && !Controller.TryRenameFile( _copyprint, newName ) )
                return new AcceptanceReport( "Nuka.Copyprints.ExportedCopyprintWithThatNameAlreadyExists".Translate( newName ) );

            // if all checks are passed, return true.
            return true;
        }

        protected override void SetName( string name )
        {
            _copyprint.name = name;
        }

        #endregion Methods
    }
}