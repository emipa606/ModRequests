using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

//Fluffies Magic
namespace Clutter_Structure
{
    public static class CompGlower_Extensions
    {
        // get a FieldInfo object for the NonPublic, Instance field named "glowOnInt" in class CompGlower.
        // a FieldInfo object is a bunch of metadata about the field, it's properties, location, etc. etc.
        // essentially, it's the blueprint of the field. The same exists for other stuff; MethodInfo, TypeInfo, etc.
        private static FieldInfo _litFI = typeof(CompGlower).GetField("glowOnInt", BindingFlags.Instance | BindingFlags.NonPublic);

        // create a public Extension method for objects of type CompGlower.
        // extension methods are always static methods in a static class, with the 'this' keyword on the first argument.
        // They basically allow you to add methods to external classes.
        public static void SetLit(this CompGlower glower, bool lit = true)
        {            
            // just making sure that we actually found the field info
            if (_litFI == null)
                throw new Exception("Field glowOnInt not found in CompGlower");

            // the FieldInfo object has Get/SetValue methods that do what you expect them to do.
            // Since the FieldInfo is tied to the Type, not any instance of the Type, we need to tell it what instance to get the 
            // value of, or what instance to set it to. 
            _litFI.SetValue(glower, lit);
        }
    }
}