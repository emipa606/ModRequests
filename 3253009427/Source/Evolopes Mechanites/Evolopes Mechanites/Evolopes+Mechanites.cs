using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolopes;

namespace Evolopes
{
    public class Hediff_InjectBoomium : HediffWithComps
    {
        IntVec3 position;
        Map map;
        Gender gender;
        Pawn_AgeTracker age;
        Faction ofPlayer;

        public override void Tick()
        {
            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.Boomalope)
            {
                return;
            }

            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoMilkalope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj1 = PawnGenerator.GeneratePawn(EvolopesDefOf.Boomalope, ofPlayer);
                obj1.gender = gender;
                obj1.ageTracker = age;
                GenSpawn.Spawn(obj1, position, map);
                obj1.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Fuel, new IntRange(2, 4).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();
            }
            else
                pawn.Kill(null);
        }
    }


    public class Hediff_InjectChocolatium : HediffWithComps
    {
        IntVec3 position;
        Map map;
        Gender gender;
        Pawn_AgeTracker age;
        Faction ofPlayer;

    public override void Tick()
        {
            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoChocolope)
            {
                return;
            }

            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoMilkalope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj2 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoChocolope, ofPlayer);
                obj2.gender = gender;
                obj2.ageTracker = age;
                GenSpawn.Spawn(obj2, position, map);
                obj2.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Slime, new IntRange(2, 4).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();                
            }

            else
                pawn.Kill(null);
        }
    }

    public class Hediff_InjectDevilium : HediffWithComps
    {
        IntVec3 position;
        Map map;
        Gender gender;
        Pawn_AgeTracker age;
        Faction ofPlayer;

    public override void Tick()
        {
            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoDevilope)
            {
                return;
            }
            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoMilkalope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj3 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoDevilope, ofPlayer);
                obj3.gender = gender;
                obj3.ageTracker = age;
                GenSpawn.Spawn(obj3, position, map);
                obj3.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Slime, new IntRange(3, 6).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();
            }
            else
                pawn.Kill(null);
        }
    }

    public class Hediff_InjectJellium : HediffWithComps
    {
        IntVec3 position;
        Map map;
        Gender gender;
        Pawn_AgeTracker age;
        Faction ofPlayer;

    public override void Tick()
        {
            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoJellope)
            {
                return;
            }

            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoMilkalope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj4 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoJellope, ofPlayer);
                obj4.gender = gender;
                obj4.ageTracker = age;
                GenSpawn.Spawn(obj4, position, map);
                obj4.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_BloodInsect, new IntRange(3, 6).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();
            }
            else
                pawn.Kill(null);
        }
    }

    public class Hediff_InjectMilkium : HediffWithComps
    {
        IntVec3 position;
        Map map;
        Gender gender;
        Pawn_AgeTracker age;
        Faction ofPlayer;

    public override void Tick()
        {
            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoMilkalope)
            {
                return;
            }
            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj5 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoMilkalope, ofPlayer);
                obj5.gender = gender;
                obj5.ageTracker = age;
                GenSpawn.Spawn(obj5, position, map);
                obj5.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Slime, new IntRange(3, 6).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();                
            }
            else
                pawn.Kill(null);
        }
    }

    public class Hediff_InjectNeutrium : HediffWithComps
    {
        public override void Tick()
        {
            IntVec3 position;
            Map map;
            Gender gender;
            Pawn_AgeTracker age;
            Faction ofPlayer;

            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                return;
            }
            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoMilkalope)
            {
                Pawn obj6 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoNeutroalope, ofPlayer);
                obj6.gender = gender;
                obj6.ageTracker = age;
                GenSpawn.Spawn(obj6, position, map);
                obj6.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Water, new IntRange(3, 6).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();                
            }
            else
                pawn.Kill(null);
        }
    }

    public class Hediff_InjectMegaLuciferium : HediffWithComps
    {
        public override void Tick()
        {
            IntVec3 position;
            Map map;
            Gender gender;
            Pawn_AgeTracker age;
            Faction ofPlayer;

            base.Tick();
            if (Severity >= 0.99)
            {
                return;
            }
            if (pawn.Map == null)
            {
                return;
            }
            if (pawn.kindDef == EvolopesDefOf.EvoMegaLuciferope)
            {
                return;
            }
            position = pawn.Position;
            map = pawn.Map;
            gender = pawn.gender;
            age = pawn.ageTracker;
            ofPlayer = Find.FactionManager.OfPlayer;

            if (pawn.kindDef == EvolopesDefOf.Boomalope || pawn.kindDef == EvolopesDefOf.EvoChocolope || pawn.kindDef == EvolopesDefOf.EvoDevilope || pawn.kindDef == EvolopesDefOf.EvoJellope || pawn.kindDef == EvolopesDefOf.EvoMilkalope || pawn.kindDef == EvolopesDefOf.EvoNeutroalope)
            {
                Pawn obj7 = PawnGenerator.GeneratePawn(EvolopesDefOf.EvoMegaLuciferope, ofPlayer);
                obj7.gender = gender;
                obj7.ageTracker = age;
                GenSpawn.Spawn(obj7, position, map);
                obj7.Drawer.renderer.renderTree.SetDirty();
                FilthMaker.TryMakeFilth(position, map, ThingDefOf.Filth_Water, new IntRange(3, 6).RandomInRange, FilthSourceFlags.None);
                pawn.Destroy();                
            }
            else
                pawn.Kill(null);
        }
    }
}