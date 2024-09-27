using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UniqueItems
{
	public class GameComponent_UniqueItems : GameComponent
	{

		public GameComponent_UniqueItems(Game game)
		{
			CompUniqueItem.instances.Clear();
			CompUniqueItem.spawnedInstances.Clear();
		}
		public override void GameComponentUpdate()
		{
			if (UniqueItemsDefOf.DM_ShowAllUniqueItems.JustPressed)
			{
				var window = Find.WindowStack.WindowOfType<UniqueItemsLocator>();
				if (window is null)
                {
					Find.WindowStack.Add(new UniqueItemsLocator());
                }
				else
                {
					Find.WindowStack.TryRemove(window);
                }
			}
		}
	}
}