using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace dragonagemetals
{
    [DefOf]
    public static class ThingDefOf
	{
		// Token: 0x0600809E RID: 32926 RVA: 0x002E05D3 File Offset: 0x002DE7D3
		static ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
		}

		// Token: 0x040046F7 RID: 18167
		public static ThingDef Silver;

		public static ThingDef Lyrium;

		public static ThingDef SilveriteOre;

		public static ThingDef MineableVeridium;

		public static ThingDef MineableSilverite;

		public static ThingDef MineableVolcanicAurum;

		public static ThingDef MineableLyrium;

		// Token: 0x040046F8 RID: 18168
		public static ThingDef Gold;

		// Token: 0x040046F9 RID: 18169
		public static ThingDef Steel;

		// Token: 0x040046FA RID: 18170
		public static ThingDef WoodLog;

		// Token: 0x040046FB RID: 18171
		public static ThingDef MedicineHerbal;

		// Token: 0x040046FC RID: 18172
		public static ThingDef MedicineIndustrial;

		// Token: 0x040046FD RID: 18173
		public static ThingDef MedicineUltratech;

		// Token: 0x040046FE RID: 18174
		public static ThingDef BlocksGranite;

		// Token: 0x040046FF RID: 18175
		public static ThingDef Plasteel;

		// Token: 0x04004700 RID: 18176
		public static ThingDef Beer;

		// Token: 0x04004701 RID: 18177
		public static ThingDef SmokeleafJoint;

		// Token: 0x04004702 RID: 18178
		public static ThingDef Chocolate;

		// Token: 0x04004703 RID: 18179
		public static ThingDef ComponentIndustrial;

		// Token: 0x04004704 RID: 18180
		public static ThingDef ComponentSpacer;

		// Token: 0x04004705 RID: 18181
		public static ThingDef InsectJelly;

		// Token: 0x04004706 RID: 18182
		public static ThingDef Cloth;

		// Token: 0x04004707 RID: 18183
		public static ThingDef Leather_Plain;

		// Token: 0x04004708 RID: 18184
		public static ThingDef Hyperweave;

		// Token: 0x04004709 RID: 18185
		public static ThingDef RawPotatoes;

		// Token: 0x0400470A RID: 18186
		public static ThingDef RawBerries;

		// Token: 0x0400470B RID: 18187
		public static ThingDef Granite;

		// Token: 0x0400470C RID: 18188
		public static ThingDef Wort;

		// Token: 0x0400470D RID: 18189
		public static ThingDef AIPersonaCore;

		// Token: 0x0400470E RID: 18190
		public static ThingDef TechprofSubpersonaCore;

		// Token: 0x0400470F RID: 18191
		public static ThingDef OrbitalTargeterBombardment;

		// Token: 0x04004710 RID: 18192
		public static ThingDef OrbitalTargeterPowerBeam;

		// Token: 0x04004711 RID: 18193
		public static ThingDef Chemfuel;

		// Token: 0x04004712 RID: 18194
		public static ThingDef Uranium;

		// Token: 0x04004713 RID: 18195
		public static ThingDef Jade;

		// Token: 0x04004714 RID: 18196
		public static ThingDef Shell_HighExplosive;

		// Token: 0x04004715 RID: 18197
		public static ThingDef Shell_AntigrainWarhead;

		// Token: 0x04004716 RID: 18198
		public static ThingDef ReinforcedBarrel;

		// Token: 0x04004717 RID: 18199
		public static ThingDef MealSurvivalPack;

		// Token: 0x04004718 RID: 18200
		public static ThingDef MealNutrientPaste;

		// Token: 0x04004719 RID: 18201
		public static ThingDef MealSimple;

		// Token: 0x0400471A RID: 18202
		public static ThingDef MealFine;

		// Token: 0x0400471B RID: 18203
		public static ThingDef Pemmican;

		// Token: 0x0400471C RID: 18204
		public static ThingDef Kibble;

		// Token: 0x0400471D RID: 18205
		public static ThingDef Hay;

		// Token: 0x0400471E RID: 18206
		public static ThingDef Meat_Human;

		// Token: 0x0400471F RID: 18207
		public static ThingDef Luciferium;

		// Token: 0x04004720 RID: 18208
		public static ThingDef Penoxycyline;

		// Token: 0x04004721 RID: 18209
		public static ThingDef DropPodIncoming;

		// Token: 0x04004722 RID: 18210
		public static ThingDef DropPodLeaving;

		// Token: 0x04004723 RID: 18211
		public static ThingDef ShipChunkIncoming;

		// Token: 0x04004724 RID: 18212
		public static ThingDef CrashedShipPartIncoming;

		// Token: 0x04004725 RID: 18213
		public static ThingDef MeteoriteIncoming;

		// Token: 0x04004726 RID: 18214
		[MayRequireRoyalty]
		public static ThingDef ShuttleIncoming;

		// Token: 0x04004727 RID: 18215
		[MayRequireRoyalty]
		public static ThingDef ShuttleLeaving;

		// Token: 0x04004728 RID: 18216
		[MayRequireRoyalty]
		public static ThingDef ShuttleCrashing;

		// Token: 0x04004729 RID: 18217
		[MayRequireIdeology]
		public static ThingDef SpacedroneIncoming;

		// Token: 0x0400472A RID: 18218
		[MayRequireRoyalty]
		public static ThingDef PawnJumper;

		// Token: 0x0400472B RID: 18219
		public static ThingDef ActiveDropPod;

		// Token: 0x0400472C RID: 18220
		public static ThingDef Fire;

		// Token: 0x0400472D RID: 18221
		public static ThingDef Heart;

		// Token: 0x0400472E RID: 18222
		public static ThingDef ChunkSlagSteel;

		// Token: 0x0400472F RID: 18223
		public static ThingDef SteamGeyser;

		// Token: 0x04004730 RID: 18224
		public static ThingDef Hive;

		// Token: 0x04004731 RID: 18225
		public static ThingDef ShipChunk;

		// Token: 0x04004732 RID: 18226
		public static ThingDef ElephantTusk;

		// Token: 0x04004733 RID: 18227
		public static ThingDef GlowPod;

		// Token: 0x04004734 RID: 18228
		public static ThingDef MinifiedThing;

		// Token: 0x04004735 RID: 18229
		public static ThingDef MinifiedTree;

		// Token: 0x04004736 RID: 18230
		[MayRequireRoyalty]
		public static ThingDef MonumentMarker;

		// Token: 0x04004737 RID: 18231
		[MayRequireRoyalty]
		public static ThingDef PsychicAmplifier;

		// Token: 0x04004738 RID: 18232
		[MayRequireIdeology]
		public static ThingDef Skull;

		// Token: 0x04004739 RID: 18233
		[MayRequireIdeology]
		public static ThingDef Skullspike;

		// Token: 0x0400473A RID: 18234
		public static ThingDef Filth_Blood;

		// Token: 0x0400473B RID: 18235
		public static ThingDef Filth_AmnioticFluid;

		// Token: 0x0400473C RID: 18236
		public static ThingDef Filth_Dirt;

		// Token: 0x0400473D RID: 18237
		public static ThingDef Filth_Vomit;

		// Token: 0x0400473E RID: 18238
		public static ThingDef Filth_AnimalFilth;

		// Token: 0x0400473F RID: 18239
		public static ThingDef Filth_Trash;

		// Token: 0x04004740 RID: 18240
		public static ThingDef Filth_Slime;

		// Token: 0x04004741 RID: 18241
		public static ThingDef Filth_FireFoam;

		// Token: 0x04004742 RID: 18242
		public static ThingDef Filth_Fuel;

		// Token: 0x04004743 RID: 18243
		public static ThingDef Filth_RubbleRock;

		// Token: 0x04004744 RID: 18244
		public static ThingDef Filth_RubbleBuilding;

		// Token: 0x04004745 RID: 18245
		public static ThingDef Filth_CorpseBile;

		// Token: 0x04004746 RID: 18246
		public static ThingDef Filth_Ash;

		// Token: 0x04004747 RID: 18247
		public static ThingDef Filth_MachineBits;

		// Token: 0x04004748 RID: 18248
		public static ThingDef Filth_Water;

		// Token: 0x04004749 RID: 18249
		public static ThingDef Filth_Hair;

		// Token: 0x0400474A RID: 18250
		[MayRequireIdeology]
		public static ThingDef Filth_DriedBlood;

		// Token: 0x0400474B RID: 18251
		[MayRequireIdeology]
		public static ThingDef Filth_ScatteredDocuments;

		// Token: 0x0400474C RID: 18252
		[MayRequireIdeology]
		public static ThingDef Filth_MoldyUniform;

		// Token: 0x0400474D RID: 18253
		[MayRequireIdeology]
		public static ThingDef Filth_PodSlime;

		// Token: 0x0400474E RID: 18254
		[MayRequireIdeology]
		public static ThingDef Filth_OilSmear;

		// Token: 0x0400474F RID: 18255
		public static ThingDef RectTrigger;

		// Token: 0x04004750 RID: 18256
		public static ThingDef TriggerUnfogged;

		// Token: 0x04004751 RID: 18257
		public static ThingDef TriggerContainerEmptied;

		// Token: 0x04004752 RID: 18258
		public static ThingDef Explosion;

		// Token: 0x04004753 RID: 18259
		public static ThingDef Bombardment;

		// Token: 0x04004754 RID: 18260
		public static ThingDef PowerBeam;

		// Token: 0x04004755 RID: 18261
		public static ThingDef SignalAction_Letter;

		// Token: 0x04004756 RID: 18262
		public static ThingDef SignalAction_Ambush;

		// Token: 0x04004757 RID: 18263
		public static ThingDef SignalAction_OpenCasket;

		// Token: 0x04004758 RID: 18264
		public static ThingDef SignalAction_OpenDoor;

		// Token: 0x04004759 RID: 18265
		public static ThingDef SignalAction_Message;

		// Token: 0x0400475A RID: 18266
		public static ThingDef SignalAction_Infestation;

		// Token: 0x0400475B RID: 18267
		public static ThingDef SignalAction_SoundOneShot;

		// Token: 0x0400475C RID: 18268
		public static ThingDef SignalAction_DormancyWakeUp;

		// Token: 0x0400475D RID: 18269
		public static ThingDef SignalAction_Incident;

		// Token: 0x0400475E RID: 18270
		public static ThingDef SignalAction_StartWick;

		// Token: 0x0400475F RID: 18271
		public static ThingDef SignalAction_Delay;

		// Token: 0x04004760 RID: 18272
		public static ThingDef Blight;

		// Token: 0x04004761 RID: 18273
		public static ThingDef Tornado;

		// Token: 0x04004762 RID: 18274
		public static ThingDef TunnelHiveSpawner;

		// Token: 0x04004763 RID: 18275
		[MayRequireRoyalty]
		public static ThingDef Flashstorm;

		// Token: 0x04004764 RID: 18276
		public static ThingDef RadialTrigger;

		// Token: 0x04004765 RID: 18277
		[MayRequireIdeology]
		public static ThingDef TunnelJellySpawner;

		// Token: 0x04004766 RID: 18278
		public static ThingDef Sandstone;

		// Token: 0x04004767 RID: 18279
		public static ThingDef Ship_Beam;

		// Token: 0x04004768 RID: 18280
		public static ThingDef Ship_Reactor;

		// Token: 0x04004769 RID: 18281
		public static ThingDef Ship_CryptosleepCasket;

		// Token: 0x0400476A RID: 18282
		public static ThingDef Ship_ComputerCore;

		// Token: 0x0400476B RID: 18283
		public static ThingDef Ship_Engine;

		// Token: 0x0400476C RID: 18284
		public static ThingDef Ship_SensorCluster;

		// Token: 0x0400476D RID: 18285
		public static ThingDef MineableSteel;

		// Token: 0x0400476E RID: 18286
		public static ThingDef MineableComponentsIndustrial;

		// Token: 0x0400476F RID: 18287
		public static ThingDef MineableGold;

		// Token: 0x04004770 RID: 18288
		public static ThingDef RaisedRocks;

		// Token: 0x04004771 RID: 18289
		public static ThingDef Door;

		// Token: 0x04004772 RID: 18290
		public static ThingDef Wall;

		// Token: 0x04004773 RID: 18291
		public static ThingDef Bed;

		// Token: 0x04004774 RID: 18292
		public static ThingDef Bedroll;

		// Token: 0x04004775 RID: 18293
		public static ThingDef SleepingSpot;

		// Token: 0x04004776 RID: 18294
		public static ThingDef OrbitalTradeBeacon;

		// Token: 0x04004777 RID: 18295
		public static ThingDef NutrientPasteDispenser;

		// Token: 0x04004778 RID: 18296
		public static ThingDef Grave;

		// Token: 0x04004779 RID: 18297
		public static ThingDef Sandbags;

		// Token: 0x0400477A RID: 18298
		public static ThingDef Barricade;

		// Token: 0x0400477B RID: 18299
		public static ThingDef AncientCryptosleepCasket;

		// Token: 0x0400477C RID: 18300
		public static ThingDef CryptosleepCasket;

		// Token: 0x0400477D RID: 18301
		public static ThingDef SolarGenerator;

		// Token: 0x0400477E RID: 18302
		public static ThingDef WoodFiredGenerator;

		// Token: 0x0400477F RID: 18303
		public static ThingDef PowerConduit;

		// Token: 0x04004780 RID: 18304
		public static ThingDef Battery;

		// Token: 0x04004781 RID: 18305
		public static ThingDef GeothermalGenerator;

		// Token: 0x04004782 RID: 18306
		public static ThingDef WatermillGenerator;

		// Token: 0x04004783 RID: 18307
		public static ThingDef Hopper;

		// Token: 0x04004784 RID: 18308
		public static ThingDef BilliardsTable;

		// Token: 0x04004785 RID: 18309
		public static ThingDef Telescope;

		// Token: 0x04004786 RID: 18310
		public static ThingDef MarriageSpot;

		// Token: 0x04004787 RID: 18311
		public static ThingDef PartySpot;

		// Token: 0x04004788 RID: 18312
		public static ThingDef TransportPod;

		// Token: 0x04004789 RID: 18313
		[MayRequireRoyalty]
		public static ThingDef MeditationSpot;

		// Token: 0x0400478A RID: 18314
		[MayRequireIdeology]
		public static ThingDef RitualSpot;

		// Token: 0x0400478B RID: 18315
		[MayRequireIdeology]
		public static ThingDef Ideogram;

		// Token: 0x0400478C RID: 18316
		public static ThingDef RoyalBed;

		// Token: 0x0400478D RID: 18317
		public static ThingDef TrapSpike;

		// Token: 0x0400478E RID: 18318
		public static ThingDef Cooler;

		// Token: 0x0400478F RID: 18319
		public static ThingDef Heater;

		// Token: 0x04004790 RID: 18320
		public static ThingDef Snowman;

		// Token: 0x04004791 RID: 18321
		public static ThingDef WindTurbine;

		// Token: 0x04004792 RID: 18322
		public static ThingDef FermentingBarrel;

		// Token: 0x04004793 RID: 18323
		public static ThingDef DeepDrill;

		// Token: 0x04004794 RID: 18324
		public static ThingDef LongRangeMineralScanner;

		// Token: 0x04004795 RID: 18325
		public static ThingDef GroundPenetratingScanner;

		// Token: 0x04004796 RID: 18326
		public static ThingDef CollapsedRocks;

		// Token: 0x04004797 RID: 18327
		public static ThingDef TorchLamp;

		// Token: 0x04004798 RID: 18328
		public static ThingDef StandingLamp;

		// Token: 0x04004799 RID: 18329
		public static ThingDef Campfire;

		// Token: 0x0400479A RID: 18330
		public static ThingDef FirefoamPopper;

		// Token: 0x0400479B RID: 18331
		public static ThingDef PassiveCooler;

		// Token: 0x0400479C RID: 18332
		public static ThingDef CaravanPackingSpot;

		// Token: 0x0400479D RID: 18333
		public static ThingDef PlantPot;

		// Token: 0x0400479E RID: 18334
		public static ThingDef Table1x2c;

		// Token: 0x0400479F RID: 18335
		public static ThingDef Table2x2c;

		// Token: 0x040047A0 RID: 18336
		public static ThingDef Table3x3c;

		// Token: 0x040047A1 RID: 18337
		public static ThingDef TableButcher;

		// Token: 0x040047A2 RID: 18338
		public static ThingDef DiningChair;

		// Token: 0x040047A3 RID: 18339
		public static ThingDef Stool;

		// Token: 0x040047A4 RID: 18340
		public static ThingDef PsychicEmanator;

		// Token: 0x040047A5 RID: 18341
		public static ThingDef VanometricPowerCell;

		// Token: 0x040047A6 RID: 18342
		public static ThingDef InfiniteChemreactor;

		// Token: 0x040047A7 RID: 18343
		public static ThingDef CommsConsole;

		// Token: 0x040047A8 RID: 18344
		public static ThingDef Sarcophagus;

		// Token: 0x040047A9 RID: 18345
		public static ThingDef Turret_Mortar;

		// Token: 0x040047AA RID: 18346
		public static ThingDef Turret_MiniTurret;

		// Token: 0x040047AB RID: 18347
		[MayRequireRoyalty]
		public static ThingDef Turret_AutoMiniTurret;

		// Token: 0x040047AC RID: 18348
		[MayRequireRoyalty]
		public static ThingDef MechCapsule;

		// Token: 0x040047AD RID: 18349
		public static ThingDef DefoliatorShipPart;

		// Token: 0x040047AE RID: 18350
		public static ThingDef PsychicDronerShipPart;

		// Token: 0x040047AF RID: 18351
		public static ThingDef Column;

		// Token: 0x040047B0 RID: 18352
		public static ThingDef Urn;

		// Token: 0x040047B1 RID: 18353
		public static ThingDef SteleLarge;

		// Token: 0x040047B2 RID: 18354
		public static ThingDef SteleGrand;

		// Token: 0x040047B3 RID: 18355
		public static ThingDef EggBox;

		// Token: 0x040047B4 RID: 18356
		public static ThingDef Fence;

		// Token: 0x040047B5 RID: 18357
		public static ThingDef SculptureLarge;

		// Token: 0x040047B6 RID: 18358
		[MayRequireRoyalty]
		public static ThingDef PsychicDroner;

		// Token: 0x040047B7 RID: 18359
		[MayRequireRoyalty]
		public static ThingDef Shuttle;

		// Token: 0x040047B8 RID: 18360
		[MayRequireRoyalty]
		public static ThingDef Drape;

		// Token: 0x040047B9 RID: 18361
		[MayRequireRoyalty]
		public static ThingDef ActivatorProximity;

		// Token: 0x040047BA RID: 18362
		[MayRequireRoyalty]
		public static ThingDef ShieldGeneratorBullets;

		// Token: 0x040047BB RID: 18363
		[MayRequireRoyalty]
		public static ThingDef ShieldGeneratorMortar;

		// Token: 0x040047BC RID: 18364
		[MayRequireRoyalty]
		public static ThingDef BroadshieldProjector;

		// Token: 0x040047BD RID: 18365
		[MayRequireRoyalty]
		public static ThingDef Brazier;

		// Token: 0x040047BE RID: 18366
		[MayRequireRoyalty]
		public static ThingDef ShuttleCrashed;

		// Token: 0x040047BF RID: 18367
		[MayRequireIdeology]
		public static ThingDef StylingStation;

		// Token: 0x040047C0 RID: 18368
		[MayRequireIdeology]
		public static ThingDef Spacedrone;

		// Token: 0x040047C1 RID: 18369
		[MayRequireIdeology]
		public static ThingDef Reliquary;

		// Token: 0x040047C2 RID: 18370
		[MayRequireIdeology]
		public static ThingDef Lectern;

		// Token: 0x040047C3 RID: 18371
		[MayRequireIdeology]
		public static ThingDef AncientTerminal;

		// Token: 0x040047C4 RID: 18372
		[MayRequireIdeology]
		public static ThingDef AncientTerminal_Worshipful;

		// Token: 0x040047C5 RID: 18373
		[MayRequireIdeology]
		public static ThingDef AncientSystemRack;

		// Token: 0x040047C6 RID: 18374
		[MayRequireIdeology]
		public static ThingDef AncientEquipmentBlocks;

		// Token: 0x040047C7 RID: 18375
		[MayRequireIdeology]
		public static ThingDef AncientMachine;

		// Token: 0x040047C8 RID: 18376
		[MayRequireIdeology]
		public static ThingDef AncientStorageCylinder;

		// Token: 0x040047C9 RID: 18377
		[MayRequireIdeology]
		public static ThingDef AncientCommsConsole;

		// Token: 0x040047CA RID: 18378
		[MayRequireIdeology]
		public static ThingDef AncientHermeticCrate;

		// Token: 0x040047CB RID: 18379
		[MayRequireIdeology]
		public static ThingDef LightBall;

		// Token: 0x040047CC RID: 18380
		[MayRequireIdeology]
		public static ThingDef Loudspeaker;

		// Token: 0x040047CD RID: 18381
		[MayRequireIdeology]
		public static ThingDef Drum;

		// Token: 0x040047CE RID: 18382
		[MayRequireIdeology]
		public static ThingDef AncientLamp;

		// Token: 0x040047CF RID: 18383
		[MayRequireIdeology]
		public static ThingDef SkyLantern;

		// Token: 0x040047D0 RID: 18384
		[MayRequireIdeology]
		public static ThingDef ArchonexusCore;

		// Token: 0x040047D1 RID: 18385
		[MayRequireIdeology]
		public static ThingDef MajorArchotechStructure;

		// Token: 0x040047D2 RID: 18386
		[MayRequireIdeology]
		public static ThingDef MajorArchotechStructureStudiable;

		// Token: 0x040047D3 RID: 18387
		[MayRequireIdeology]
		public static ThingDef ArchotechTower;

		// Token: 0x040047D4 RID: 18388
		[MayRequireIdeology]
		public static ThingDef GrandArchotechStructure;

		// Token: 0x040047D5 RID: 18389
		[MayRequireIdeology]
		public static ThingDef AncientBed;

		// Token: 0x040047D6 RID: 18390
		[MayRequireIdeology]
		public static ThingDef AncientLockerBank;

		// Token: 0x040047D7 RID: 18391
		[MayRequireIdeology]
		public static ThingDef AncientBarrel;

		// Token: 0x040047D8 RID: 18392
		[MayRequireIdeology]
		public static ThingDef AncientCrate;

		// Token: 0x040047D9 RID: 18393
		[MayRequireIdeology]
		public static ThingDef AncientFence;

		// Token: 0x040047DA RID: 18394
		[MayRequireIdeology]
		public static ThingDef AncientRazorWire;

		// Token: 0x040047DB RID: 18395
		[MayRequireIdeology]
		public static ThingDef AncientMegaCannonTripod;

		// Token: 0x040047DC RID: 18396
		[MayRequireIdeology]
		public static ThingDef AncientMegaCannonBarrel;

		// Token: 0x040047DF RID: 18399
		[MayRequireIdeology]
		public static ThingDef AncientMechDropBeacon;

		// Token: 0x040047E0 RID: 18400
		[MayRequireIdeology]
		public static ThingDef AncientTankTrap;

		// Token: 0x040047E1 RID: 18401
		[MayRequireIdeology]
		public static ThingDef AncientRustedCar;

		// Token: 0x040047E2 RID: 18402
		[MayRequireIdeology]
		public static ThingDef AncientRustedJeep;

		// Token: 0x040047E3 RID: 18403
		[MayRequireIdeology]
		public static ThingDef AncientGenerator;

		// Token: 0x040047E4 RID: 18404
		[MayRequireIdeology]
		public static ThingDef AncientOperatingTable;

		// Token: 0x040047E5 RID: 18405
		[MayRequireIdeology]
		public static ThingDef AncientDisplayBank;

		// Token: 0x040047E6 RID: 18406
		[MayRequireIdeology]
		public static ThingDef AncientSecurityTurret;

		// Token: 0x040047E7 RID: 18407
		[MayRequireIdeology]
		public static ThingDef AncientRustedCarFrame;

		// Token: 0x040047E8 RID: 18408
		[MayRequireIdeology]
		public static ThingDef AncientRustedEngineBlock;

		// Token: 0x040047E9 RID: 18409
		[MayRequireIdeology]
		public static ThingDef AncientLargeRustedEngineBlock;

		// Token: 0x040047EA RID: 18410
		[MayRequireIdeology]
		public static ThingDef BiosculpterPod;

		// Token: 0x040047EB RID: 18411
		[MayRequireIdeology]
		public static ThingDef NeuralSupercharger;

		// Token: 0x040047EC RID: 18412
		[MayRequireIdeology]
		public static ThingDef AncientWarwalkerTorso;

		// Token: 0x040047ED RID: 18413
		[MayRequireIdeology]
		public static ThingDef AncientWarwalkerClaw;

		// Token: 0x040047EE RID: 18414
		[MayRequireIdeology]
		public static ThingDef AncientWarwalkerLeg;

		// Token: 0x040047EF RID: 18415
		[MayRequireIdeology]
		public static ThingDef AncientMiniWarwalkerRemains;

		// Token: 0x040047F0 RID: 18416
		[MayRequireIdeology]
		public static ThingDef AncientWarspiderRemains;

		// Token: 0x040047F1 RID: 18417
		[MayRequireIdeology]
		public static ThingDef AncientShipBeacon;

		// Token: 0x040047F2 RID: 18418
		[MayRequireIdeology]
		public static ThingDef AncientSecurityCrate;

		// Token: 0x040047F3 RID: 18419
		[MayRequireIdeology]
		public static ThingDef AncientCryptosleepPod;

		// Token: 0x040047F4 RID: 18420
		[MayRequireIdeology]
		public static ThingDef AncientEnemyTerminal;

		// Token: 0x040047F5 RID: 18421
		[MayRequireIdeology]
		public static ThingDef AncientFuelNode;

		// Token: 0x040047F6 RID: 18422
		[MayRequireIdeology]
		public static ThingDef AncientPodCar;

		// Token: 0x040047F7 RID: 18423
		[MayRequireIdeology]
		public static ThingDef AncientHydrant;

		// Token: 0x040047F8 RID: 18424
		[MayRequireIdeology]
		public static ThingDef AncientTank;

		// Token: 0x040047F9 RID: 18425
		[MayRequireIdeology]
		public static ThingDef AncientWarwalkerFoot;

		// Token: 0x040047FA RID: 18426
		public static ThingDef Plant_Potato;

		// Token: 0x040047FB RID: 18427
		public static ThingDef Plant_TreeOak;

		// Token: 0x040047FC RID: 18428
		public static ThingDef Plant_Grass;

		// Token: 0x040047FD RID: 18429
		public static ThingDef Plant_Ambrosia;

		// Token: 0x040047FE RID: 18430
		public static ThingDef Plant_Dandelion;

		// Token: 0x040047FF RID: 18431
		public static ThingDef BurnedTree;

		// Token: 0x04004800 RID: 18432
		[MayRequireRoyalty]
		public static ThingDef Plant_TreeAnima;

		// Token: 0x04004801 RID: 18433
		[MayRequireRoyalty]
		public static ThingDef AnimusStone;

		// Token: 0x04004802 RID: 18434
		[MayRequireRoyalty]
		public static ThingDef NatureShrine_Small;

		// Token: 0x04004803 RID: 18435
		[MayRequireRoyalty]
		public static ThingDef NatureShrine_Large;

		// Token: 0x04004804 RID: 18436
		[MayRequireRoyalty]
		public static ThingDef Plant_GrassAnima;

		// Token: 0x04004805 RID: 18437
		[MayRequireIdeology]
		public static ThingDef Plant_Nutrifungus;

		// Token: 0x04004806 RID: 18438
		[MayRequireIdeology]
		public static ThingDef Plant_PodGauranlen;

		// Token: 0x04004807 RID: 18439
		[MayRequireIdeology]
		public static ThingDef Plant_TreeGauranlen;

		// Token: 0x04004808 RID: 18440
		[MayRequireIdeology]
		public static ThingDef DryadCocoon;

		// Token: 0x04004809 RID: 18441
		[MayRequireIdeology]
		public static ThingDef DryadHealingPod;

		// Token: 0x0400480A RID: 18442
		[MayRequireIdeology]
		public static ThingDef GaumakerCocoon;

		// Token: 0x0400480B RID: 18443
		public static ThingDef Human;

		// Token: 0x0400480C RID: 18444
		public static ThingDef Muffalo;

		// Token: 0x0400480D RID: 18445
		public static ThingDef Dromedary;

		// Token: 0x0400480E RID: 18446
		public static ThingDef Cow;

		// Token: 0x0400480F RID: 18447
		public static ThingDef Goat;

		// Token: 0x04004810 RID: 18448
		public static ThingDef Chicken;

		// Token: 0x04004811 RID: 18449
		public static ThingDef Thrumbo;

		// Token: 0x04004812 RID: 18450
		public static ThingDef Spark;

		// Token: 0x04004813 RID: 18451
		public static ThingDef Apparel_ShieldBelt;

		// Token: 0x04004814 RID: 18452
		public static ThingDef Apparel_SmokepopBelt;

		// Token: 0x04004815 RID: 18453
		public static ThingDef Apparel_Parka;

		// Token: 0x04004816 RID: 18454
		public static ThingDef Apparel_Tuque;

		// Token: 0x04004817 RID: 18455
		[MayRequireRoyalty]
		public static ThingDef Apparel_RobeRoyal;

		// Token: 0x04004818 RID: 18456
		[MayRequireIdeology]
		public static ThingDef Apparel_BodyStrap;

		// Token: 0x04004819 RID: 18457
		[MayRequireIdeology]
		public static ThingDef Apparel_Collar;

		// Token: 0x0400481A RID: 18458
		[MayRequireIdeology]
		public static ThingDef Dye;

		// Token: 0x0400481B RID: 18459
		[MayRequireIdeology]
		public static ThingDef Apparel_Blindfold;

		// Token: 0x0400481C RID: 18460
		public static ThingDef Mote_Text;

		// Token: 0x0400481D RID: 18461
		public static ThingDef Mote_TempRoof;

		// Token: 0x0400481E RID: 18462
		public static ThingDef Mote_ColonistFleeing;

		// Token: 0x0400481F RID: 18463
		public static ThingDef Mote_ColonistAttacking;

		// Token: 0x04004820 RID: 18464
		public static ThingDef Mote_Danger;

		// Token: 0x04004821 RID: 18465
		public static ThingDef Mote_Speech;

		// Token: 0x04004822 RID: 18466
		public static ThingDef Mote_ThoughtBad;

		// Token: 0x04004823 RID: 18467
		public static ThingDef Mote_ThoughtGood;

		// Token: 0x04004824 RID: 18468
		public static ThingDef Mote_Stun;

		// Token: 0x04004825 RID: 18469
		public static ThingDef Mote_Bombardment;

		// Token: 0x04004826 RID: 18470
		public static ThingDef Mote_PowerBeam;

		// Token: 0x04004827 RID: 18471
		public static ThingDef Mote_Leaf;

		// Token: 0x04004828 RID: 18472
		public static ThingDef Mote_ForceJob;

		// Token: 0x04004829 RID: 18473
		public static ThingDef Mote_ForceJobMaintained;

		// Token: 0x0400482A RID: 18474
		[MayRequireRoyalty]
		public static ThingDef Mote_CastPsycast;

		// Token: 0x0400482B RID: 18475
		[MayRequireRoyalty]
		public static ThingDef Mote_PsychicLinkLine;

		// Token: 0x0400482C RID: 18476
		[MayRequireRoyalty]
		public static ThingDef Mote_PsychicLinkPulse;

		// Token: 0x0400482D RID: 18477
		[MayRequireRoyalty]
		public static ThingDef Mote_PsyfocusPulse;

		// Token: 0x0400482E RID: 18478
		[MayRequireRoyalty]
		public static ThingDef Mote_Bestow;

		// Token: 0x0400482F RID: 18479
		[MayRequireIdeology]
		public static ThingDef Mote_LightBall;

		// Token: 0x04004830 RID: 18480
		[MayRequireIdeology]
		public static ThingDef Mote_LightBallLights;

		// Token: 0x04004831 RID: 18481
		[MayRequireIdeology]
		public static ThingDef Mote_LoudspeakerLights;

		// Token: 0x04004832 RID: 18482
		[MayRequireIdeology]
		public static ThingDef Mote_RolePositionHighlight;

		// Token: 0x04004833 RID: 18483
		[MayRequireIdeology]
		public static ThingDef Mote_RolePawnHighlight;

		// Token: 0x04004834 RID: 18484
		[MayRequireIdeology]
		public static ThingDef Mote_GauranlenCasteChanged;

		// Token: 0x04004836 RID: 18486
		[MayRequireRoyalty]
		public static ThingDef Throne;

		// Token: 0x04004837 RID: 18487
		[MayRequireRoyalty]
		public static ThingDef Harp;

		// Token: 0x04004838 RID: 18488
		[MayRequireRoyalty]
		public static ThingDef Harpsichord;

		// Token: 0x04004839 RID: 18489
		[MayRequireRoyalty]
		public static ThingDef Piano;

		// Token: 0x0400483A RID: 18490
		[MayRequireRoyalty]
		public static ThingDef MeleeWeapon_PsyfocusStaff;

		// Token: 0x0400483B RID: 18491
		[MayRequireRoyalty]
		public static ThingDef ShipLandingBeacon;

		// Token: 0x0400483C RID: 18492
		[MayRequireRoyalty]
		public static ThingDef ActivatorCountdown;
	}
}
