using System;
using UnityEngine;
using Verse;

namespace KriilMod_CD
{
	public class TrainCombatDesignatorDef : Def
	{
		public string iconTexture;

		public string dragHighlightTexture;

		public Type designatorClass;

		public bool Injected
		{
			get;
			set;
		}

		public DesignationCategoryDef Category
		{
			get;
			set;
		}

		public Texture2D IconTexture
		{
			get;
			set;
		}

		public Texture2D DragHighlightTex
		{
			get;
			set;
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			Category = DefDatabase<DesignationCategoryDef>.GetNamed(defName);
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				IconTexture = ContentFinder<Texture2D>.Get(iconTexture);
				DragHighlightTex = ContentFinder<Texture2D>.Get(dragHighlightTexture);
			});
		}
	}
}
