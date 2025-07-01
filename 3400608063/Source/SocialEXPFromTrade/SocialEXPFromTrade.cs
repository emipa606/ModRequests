using System;
using System.Collections.Generic;

using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace SocialEXPFromTrade
{
    class SocialEXPFromTrade : Mod
    {
        public SocialEXPFromTrade( ModContentPack pack ) : base( pack )
        {
            GetSettings<Settings>();

            var harmony = new Harmony("krelinos.SocialEXPFromTrade");
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
        }

        public override void DoSettingsWindowContents( Rect inRect )
        {
            Settings.DoSettingsWindowContents( inRect );
        }

        public override string SettingsCategory()
        {
            return "SocialEXPFromTrade.Title".Translate();
        }
    }

    [HarmonyPatch(typeof(TradeDeal))]
    [HarmonyPatch("TryExecute")]
    public class Patch1
    {
        static void Prefix( ref TradeDeal __instance, ref List<Tradeable> ___tradeables, ref object[] __state )
        {
            __state = new object[]
            {
                0f,     // Value of goods player is buying, in market value.
                0f,     // Value of goods player is selling, in actual selling value.
                false,  // Is pawn inspired for this trade?
                false   // Ignore this trade (because it was a Royal Tribute Collector trade)
            };

            if ( TradeSession.playerNegotiator.mindState.inspirationHandler.CurStateDef == InspirationDefOf.Inspired_Trade )
                __state[2] = true;

            if ( TradeSession.TradeCurrency == TradeCurrency.Favor )
            {
                //Log.Message( "Ignoring tribute trade." );
                __state[3] = true;
            }
            else if ( TradeSession.TradeCurrency == TradeCurrency.Silver )
                foreach ( Tradeable aTradeable in ___tradeables )
                {
                    // Since silver currency is technically being traded over, it is getting
                    // counted for the experience gain, resulting in double the expected output.
                    if ( aTradeable.ThingDef.defName == "Silver" ) // Silver not Sliver
                        continue;

                    float xpToAdd = 0f;
                    if (aTradeable.ActionToDo == TradeAction.PlayerBuys)
                    {
                        switch ( Settings.BuyingDeterminer )
                        {
                            case Settings.ValueDeterminer.MarketValue:
                                xpToAdd = aTradeable.AnyThing.MarketValue;
                                break;
                            case Settings.ValueDeterminer.RelativeValue:
                                xpToAdd = aTradeable.GetPriceFor( TradeAction.PlayerBuys );
                                break;
                            case Settings.ValueDeterminer.HigherValue:
                                xpToAdd = Math.Max( aTradeable.AnyThing.MarketValue, aTradeable.GetPriceFor( TradeAction.PlayerBuys ) );
                                break;
                            case Settings.ValueDeterminer.LowerValue:
                                xpToAdd = Math.Min( aTradeable.AnyThing.MarketValue, aTradeable.GetPriceFor( TradeAction.PlayerBuys ) );
                                break;
                        }

                        //Log.Message( String.Format( "Player bought {0} {1} ({3}) at {2} silver each." ) );
                            //Math.Abs( aTradeable.CountToTransfer ), aTradeable.Label, xpToAdd, aTradeable.ThingDef ) );

                        xpToAdd *= Math.Abs(aTradeable.CountToTransfer);
                        __state[0] = (float)__state[0] + xpToAdd;
                    }
                    else if (aTradeable.ActionToDo == TradeAction.PlayerSells)
                    {
                        switch (Settings.SellingDeterminer)
                        {
                            case Settings.ValueDeterminer.MarketValue:
                                xpToAdd = aTradeable.AnyThing.MarketValue;
                                break;
                            case Settings.ValueDeterminer.RelativeValue:
                                xpToAdd = aTradeable.GetPriceFor( TradeAction.PlayerSells );
                                break;
                            case Settings.ValueDeterminer.HigherValue:
                                xpToAdd = Math.Max( aTradeable.AnyThing.MarketValue, aTradeable.GetPriceFor( TradeAction.PlayerSells ) );
                                break;
                            case Settings.ValueDeterminer.LowerValue:
                                xpToAdd = Math.Min( aTradeable.AnyThing.MarketValue, aTradeable.GetPriceFor( TradeAction.PlayerSells ) );
                                break;
                        }

                        //Log.Message( String.Format( "Player sold {0} {1} ({3}) at {2} silver each..",
                        //    Math.Abs( aTradeable.CountToTransfer ), aTradeable.ThingDef, xpToAdd, aTradeable.ThingDef ) );

                        xpToAdd *= Math.Abs( aTradeable.CountToTransfer );
                        __state[1] = (float)__state[1] + xpToAdd;
                    }
                }
        }
        static void Postfix( object[] __state, ref bool __result )
        {
            if (__result)
            {
                if ( (bool)__state[3] == true )
                    ; // Nothing, since a Tribute Collector trade involves no negotiation.
                else
                {
                    Pawn playerNegotiator = TradeSession.playerNegotiator;
                    float playerXP = (float)__state[0] + (float)__state[1];
                    playerXP *= Settings.GlobalMultiplier;

                    if ( (bool)__state[2] == true )
                    {
                        playerXP *= Settings.InspiredMultiplier;
                    }

                    //int levelBeforeTrade = playerNegotiator.skills.GetSkill(SkillDefOf.Social).Level;

                    if ( PawnUtility.ShouldSendNotificationAbout( playerNegotiator ) && Settings.NotifyPlayer )
                    {
                        Messages.Message( String.Format( "{0} has received {1} social experience from this trade.", playerNegotiator.LabelShort, Math.Round( playerXP, 0 ) ), playerNegotiator, MessageTypeDefOf.PositiveEvent, true );
                    }

                    playerNegotiator.skills.Learn( SkillDefOf.Social, playerXP, false );

                    //int levelAfterTrade = playerNegotiator.skills.GetSkill(SkillDefOf.Social).Level;

                    //Log.Message(String.Format("{0} has received {1} social experience from this trade. Is inspired? {2}", playerNegotiator.Name, playerXP, __state[2]));
                }
            }
        }
    }

    // Thank you Smart Turret Covering by denev for being simple and small enough for me to
    // reunderstand RimWorld mod settings.
    // https://steamcommunity.com/sharedfiles/filedetails/?id=2636621800
    public class Settings : ModSettings
    {
        public enum ValueDeterminer
        {
            MarketValue,
            RelativeValue,
            HigherValue,
            LowerValue
        }

        public static ValueDeterminer BuyingDeterminer = ValueDeterminer.MarketValue;
        public static ValueDeterminer SellingDeterminer = ValueDeterminer.RelativeValue;

        public static float InspiredMultiplier = 1.2f;
        public static float GlobalMultiplier = 2f;

        public static bool NotifyPlayer = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look( ref BuyingDeterminer, "SocialEXPFromTrade_BuyingDeterminer", ValueDeterminer.MarketValue );
            Scribe_Values.Look( ref SellingDeterminer, "SocialEXPFromTrade_SellingDeterminer", ValueDeterminer.RelativeValue );
            Scribe_Values.Look( ref InspiredMultiplier, "SocialEXPFromTrade_InspiredMultiplier", 1.2f );
            Scribe_Values.Look( ref GlobalMultiplier, "SocialEXPFromTrade_GlobalMultiplier", 2f );
            Scribe_Values.Look( ref NotifyPlayer, "SocialEXPFromTrade_NotifyPlayer", true );
        }

        public static void DoSettingsWindowContents( Rect inRect )
        {
            var listing = new Listing_Standard();

            listing.Begin( inRect );

            ////// BUYING

            Text.Font = GameFont.Small;
            listing.Label( "SocialEXPFromTrade.BuyingDeterminer".Translate() );
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.MarketValue".Translate() + " " + "SocialEXPFromTrade.Default".Translate(), BuyingDeterminer == ValueDeterminer.MarketValue ))
                BuyingDeterminer = ValueDeterminer.MarketValue;

            Text.Font = GameFont.Tiny;
            listing.Label( "　　　<color=grey>" + "SocialEXPFromTrade.MarketValueDescription".Translate() + "</color>" );

            Text.Font = GameFont.Small;
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.RelativeValue".Translate(), BuyingDeterminer == ValueDeterminer.RelativeValue ))
                BuyingDeterminer = ValueDeterminer.RelativeValue;

            Text.Font = GameFont.Tiny;
            listing.Label( "　　　<color=grey>" + "SocialEXPFromTrade.RelativeValueDescription".Translate() + "</color>" );

            Text.Font = GameFont.Small;
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.HigherValue".Translate(), BuyingDeterminer == ValueDeterminer.HigherValue ))
                BuyingDeterminer = ValueDeterminer.HigherValue;
            listing.Label( "" );

            if (listing.RadioButton( "　" + "SocialEXPFromTrade.LowerValue".Translate(), BuyingDeterminer == ValueDeterminer.LowerValue ))
                BuyingDeterminer = ValueDeterminer.LowerValue;

            listing.Gap( 24 );

            ////// SELLING

            Text.Font = GameFont.Small;
            listing.Label( "SocialEXPFromTrade.SellingDeterminer".Translate() );
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.MarketValue".Translate(), SellingDeterminer == ValueDeterminer.MarketValue ))
                SellingDeterminer = ValueDeterminer.MarketValue;

            Text.Font = GameFont.Tiny;
            listing.Label( "　　　<color=grey>" + "SocialEXPFromTrade.MarketValueDescription".Translate() + "</color>" );

            Text.Font = GameFont.Small;
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.RelativeValue".Translate() + " " + "SocialEXPFromTrade.Default".Translate(), SellingDeterminer == ValueDeterminer.RelativeValue ))
                SellingDeterminer = ValueDeterminer.RelativeValue;

            Text.Font = GameFont.Tiny;
            listing.Label( "　　　<color=grey>" + "SocialEXPFromTrade.RelativeValueDescription".Translate() + "</color>" );

            Text.Font = GameFont.Small;
            if (listing.RadioButton( "　" + "SocialEXPFromTrade.HigherValue".Translate(), SellingDeterminer == ValueDeterminer.HigherValue ))
                SellingDeterminer = ValueDeterminer.HigherValue;
            listing.Label( "" );

            if (listing.RadioButton( "　" + "SocialEXPFromTrade.LowerValue".Translate(), SellingDeterminer == ValueDeterminer.LowerValue ))
                SellingDeterminer = ValueDeterminer.LowerValue;

            listing.Gap( 24 );

            ////// MULTIPLIERS

            Text.Font = GameFont.Small;

            listing.Label( String.Format( "SocialEXPFromTrade.InspirationMultiplier".Translate(), InspiredMultiplier ) );
            InspiredMultiplier = listing.Slider( (float)Math.Round( InspiredMultiplier, 1 ), 0f, 10f );

            listing.Label( String.Format( "SocialEXPFromTrade.GlobalMultiplier".Translate(), GlobalMultiplier ) );
            GlobalMultiplier = listing.Slider( (float)Math.Round( GlobalMultiplier, 1 ), 0f, 10f );

            listing.Gap( 24 );

            listing.CheckboxLabeled( "SocialEXPFromTrade.NotifyPlayer".Translate(), ref NotifyPlayer );
            listing.End();
        }
    }
}
