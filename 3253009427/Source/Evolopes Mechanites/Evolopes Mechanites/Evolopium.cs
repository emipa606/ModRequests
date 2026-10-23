using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Evolopes
{
    public class CompTargetable_EvolopeMechSerum : CompTargetable

    {
        // substring to check for in the defName
        private string targetSubstring = "EvoList";

        protected override bool PlayerChoosesTarget => true;

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetAnimals = true,
                canTargetBloodfeeders = true,
                canTargetBuildings = false,
                canTargetCorpses = false,
                canTargetHumans = false,
                canTargetItems = false,
                canTargetMechs = false,
                canTargetMutants = false,
                canTargetPawns = true,
                canTargetPlants = false,
                canTargetSelf = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                mustBeSelectable = true,
                validator = EvolopeValidator
            };
        }

        public bool EvolopeValidator(TargetInfo tInfo)
        {
            Thing target = tInfo.Thing;
            if (target == null || !(target is Pawn pawn))
            {
                return false;
            }

            // check if the defName of the target Pawn contains the targetSubstring
            if (pawn.def.defName.Contains("EvoList"))
            {
                return true;
            }

            return false;
        }
    }
}

namespace Evolopes
{
    class EvolopesList
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public EvolopesList(string name, string category)
        {
            Name = name;
            Category = category;
        }
        static void Main(string[] Evolopes)
        {
            List<EvolopesList> objectsList = new List<EvolopesList>();

            EvolopesList list1 = new EvolopesList("Boomalope", "EvoList");
            EvolopesList list2 = new EvolopesList("Chocolope", "EvoList");
            EvolopesList list3 = new EvolopesList("Devilope", "EvoList");
            EvolopesList list4 = new EvolopesList("Jellope", "Evolist");
            EvolopesList list5 = new EvolopesList("Milkalope", "Evolist");
            EvolopesList list6 = new EvolopesList("Neutroalope", "Evolist");

            // Llena objectsList con objetos MyObject
            objectsList.Add(list1);
            objectsList.Add(list2);
            objectsList.Add(list3);
            objectsList.Add(list4);
            objectsList.Add(list5);
            objectsList.Add(list6);

            // Supongamos que tienes una lista de objetos llamada objectsList
            foreach (var obj in objectsList)
            {
                // Verifica si el objeto pertenece a la categoría "Evolopes"
                if (obj.Category == "EvoList")
                {
                    // Hacer un substring del nombre del objeto
                    string EvoList = obj.Name.Substring(0, 4); // Suponiendo que deseas los primeros 4 caracteres
                    Console.WriteLine("Substring del nombre del objeto: " + EvoList);
                }
            }
        }
    }
}
