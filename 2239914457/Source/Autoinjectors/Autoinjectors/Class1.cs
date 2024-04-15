using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace Autoinjectors
{ 
    [StaticConstructorOnStartup]

    public class IV_Drips_Ambrosia : HediffComp 
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Ambrosiadrip = DefDatabase<HediffDef>.GetNamed("AmbrosiaDrip"); //hediff def for ambrosia drip
        HediffDef Ambrosiahigh = DefDatabase<HediffDef>.GetNamed("AmbrosiaHigh"); //hediff def for ambrosia high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Ambrosiahigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Ambrosiadrip));
            }
        }
    }
    public class IV_Drips_Alcohol : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Alcoholdrip = DefDatabase<HediffDef>.GetNamed("AlcoholDrip"); //hediff def for alcohol drip
        HediffDef Alcoholhigh = DefDatabase<HediffDef>.GetNamed("AlcoholHigh"); //hediff def for alcohol high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Alcoholhigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Alcoholdrip));
            }
        }
    }
    public class IV_Drips_Morphine : HediffComp
    {
        bool Pain => Pawn.health.InPainShock;
        HediffDef Morphinedrip = DefDatabase<HediffDef>.GetNamed("MorphineDrip"); //hediff def for morphine drip
        HediffDef Morphinehigh = DefDatabase<HediffDef>.GetNamed("MorphineHigh"); //hediff def for morphine high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pain)
            {
                Pawn.health.AddHediff(Morphinehigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Morphinedrip));
            }
        }
    }
    public class IV_Drips_Smokeleaf : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Smokeleafdrip = DefDatabase<HediffDef>.GetNamed("SmokeleafDrip"); //hediff def for smokeleaf drip
        HediffDef Smokeleafhigh = DefDatabase<HediffDef>.GetNamed("SmokeleafHigh"); //hediff def for smokeleaf high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Smokeleafhigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Smokeleafdrip));
            }
        }
    }
    public class IV_Drips_WakeUp : HediffComp
    {
        float Sleep => Pawn.needs.rest.CurLevel; //pawn rest value
        public NeedDef Rest = NeedDefOf.Rest; // Rest need
        HediffDef wakeupdrip = DefDatabase<HediffDef>.GetNamed("WakeUpDrip"); //hediff def for wake up drip
        HediffDef wakeuphigh = DefDatabase<HediffDef>.GetNamed("WakeUpHigh"); //hediff def for wake up high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Sleep <= 0.001)
            {
                Need need = Pawn.needs.TryGetNeed(this.Rest);
                Pawn.health.AddHediff(wakeuphigh);
                need.CurLevel += 1;
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(wakeupdrip));
            }
        }
    }
    public class IV_Drips_Flake : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel; //pawn mood value
        HediffDef Flakedrip = DefDatabase<HediffDef>.GetNamed("FlakeDrip"); //hediff def for flake drip
        HediffDef Flakehigh = DefDatabase<HediffDef>.GetNamed("FlakeHigh"); //hediff def for flake high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.05)
            {
                Pawn.health.AddHediff(Flakehigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Flakedrip));
            }
        }
    }
    public class IV_Drips_Yayo : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel; //pawn mood value
        HediffDef yayodrip = DefDatabase<HediffDef>.GetNamed("YayoDrip"); //hediff def for yayo drip
        HediffDef yayohigh = DefDatabase<HediffDef>.GetNamed("YayoHigh"); //hediff def for yayo high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.05)
            {
                Pawn.health.AddHediff(yayohigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(yayodrip));
            }
        }
    }
    public class IV_Drips_GoJuice : HediffComp
    {
        bool Pain => Pawn.health.InPainShock;
        HediffDef gojuicedrip = DefDatabase<HediffDef>.GetNamed("GoJuiceDrip"); //hediff def for go juice drip
        HediffDef gojuicehigh = DefDatabase<HediffDef>.GetNamed("GoJuiceHigh"); //hediff def for go juice high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pain)
            {
                Pawn.health.AddHediff(gojuicehigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(gojuicedrip));
            }
        }
    }

    public class IV_Drips_GoJuiceMed : HediffComp
    {
        bool Pain => Pawn.health.InPainShock;
        HediffDef gojuicedrip = DefDatabase<HediffDef>.GetNamed("GoJuiceDripMed"); //hediff def for go juice drip
        HediffDef gojuicehigh = DefDatabase<HediffDef>.GetNamed("GoJuiceHighMed"); //hediff def for go juice high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pain)
            {
                Pawn.health.AddHediff(gojuicehigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(gojuicedrip));
            }
        }
    }

    public class IV_Drips_YayoMed : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel; //pawn mood value
        HediffDef yayodrip = DefDatabase<HediffDef>.GetNamed("YayoDripMed"); //hediff def for yayo drip
        HediffDef yayohigh = DefDatabase<HediffDef>.GetNamed("YayoHighMed"); //hediff def for yayo high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.05)
            {
                Pawn.health.AddHediff(yayohigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(yayodrip));
            }
        }
    }
    public class IV_Drips_AmbrosiaMed : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Ambrosiadrip = DefDatabase<HediffDef>.GetNamed("AmbrosiaDripMed"); //hediff def for ambrosia drip
        HediffDef Ambrosiahigh = DefDatabase<HediffDef>.GetNamed("AmbrosiaHighMed"); //hediff def for ambrosia high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Ambrosiahigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Ambrosiadrip));
            }
        }
    }
    public class IV_Drips_AlcoholMed : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Alcoholdrip = DefDatabase<HediffDef>.GetNamed("AlcoholDripMed"); //hediff def for alcohol drip
        HediffDef Alcoholhigh = DefDatabase<HediffDef>.GetNamed("AlcoholHighMed"); //hediff def for alcohol high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Alcoholhigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Alcoholdrip));
            }
        }
    }
    public class IV_Drips_SmokeleafMed : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel;
        HediffDef Smokeleafdrip = DefDatabase<HediffDef>.GetNamed("SmokeleafDripMed"); //hediff def for smokeleaf drip
        HediffDef Smokeleafhigh = DefDatabase<HediffDef>.GetNamed("SmokeleafHighMed"); //hediff def for smokeleaf high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.2)
            {
                Pawn.health.AddHediff(Smokeleafhigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Smokeleafdrip));
            }
        }
    }
    public class IV_Drips_WakeUpMed : HediffComp
    {
        float Sleep => Pawn.needs.rest.CurLevel; //pawn rest value
        public NeedDef Rest = NeedDefOf.Rest; // Rest need
        HediffDef wakeupdrip = DefDatabase<HediffDef>.GetNamed("WakeUpDripMed"); //hediff def for wake up drip
        HediffDef wakeuphigh = DefDatabase<HediffDef>.GetNamed("WakeUpHighMed"); //hediff def for wake up high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Sleep <= 0.001)
            {
                Need need = Pawn.needs.TryGetNeed(this.Rest);
                Pawn.health.AddHediff(wakeuphigh);
                need.CurLevel += 1;
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(wakeupdrip));
            }
        }
    }
    public class IV_Drips_FlakeMed : HediffComp
    {
        float Mood => Pawn.needs.mood.CurLevel; //pawn mood value
        HediffDef Flakedrip = DefDatabase<HediffDef>.GetNamed("FlakeDripMed"); //hediff def for flake drip
        HediffDef Flakehigh = DefDatabase<HediffDef>.GetNamed("FlakeHighMed"); //hediff def for flake high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Mood <= 0.05)
            {
                Pawn.health.AddHediff(Flakehigh);
                Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Flakedrip));
            }
        }
    }
    public class IV_Drips_Luci : HediffComp
    {
        HediffDef LuciDef => DefDatabase<HediffDef>.GetNamed("LuciferiumAddiction");
        float Luci => Pawn.health.hediffSet.GetFirstHediffOfDef(LuciDef).Severity;

        bool Lucif => Pawn.health.hediffSet.HasHediff(LuciDef); //pawn mood value
        HediffDef Lucidrip = DefDatabase<HediffDef>.GetNamed("LuciDrip"); //hediff def for flake drip
        HediffDef Lucihigh = DefDatabase<HediffDef>.GetNamed("LuciferiumHigh"); //hediff def for flake high

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Lucif)
            {
                if (Luci >0.005)
                {
                    Pawn.health.AddHediff(Lucihigh);
                    Pawn.health.hediffSet.hediffs.Remove(Pawn.health.hediffSet.GetFirstHediffOfDef(Lucidrip));
                }
            }
        }
    }
}