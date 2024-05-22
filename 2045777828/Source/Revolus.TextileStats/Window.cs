using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static Verse.Widgets;

namespace Revolus.TextileStats {
    public sealed class Window : MainTabWindow {
        public static List<(string name, Func<ITableGenerator> factory)> TableGenerators = new List<(string, Func<ITableGenerator>)> {
            ("materials", () => new StuffTableGeneratorClassic()),
            ("materials (full list)", () => new StuffTableGenerator()),
            ("buildings (unfinished)", () => new BuildingTableGenerator()),
        };

        private class Instance {
            public ITableGenerator tableGenerator;
            public Vector2 scrollPosition;

            public Instance(int index) {
                var tableGenerator = TableGenerators[index].factory();
                tableGenerator.ResetData();
                this.tableGenerator = tableGenerator;
            }
        }

        private static readonly Dictionary<int, Instance> Instances = new Dictionary<int, Instance>();

        public override Vector2 RequestedTabSize => new Vector2(1000, 800);

        private int tableGeneratorIndex = -1;
        private int newTableGeneratorIndex = -1;
        private int newCategoryIndex = -1;

        private void SelectTableGenerator(int index) {
            if (!Instances.ContainsKey(index)) {
                Instances.Add(index, new Instance(index));
            }
            this.tableGeneratorIndex = index;
        }

        public Window() {
            this.SelectTableGenerator(0);
        }

        public override void PreOpen() {
            Instances[this.tableGeneratorIndex].tableGenerator.ResetData();
            base.PreOpen();
        }

        private Action MakeSelectTableGeneratorAction(int tableGeneratorIndex) {
            void DoAction() {
                this.newTableGeneratorIndex = tableGeneratorIndex;
            }
            return DoAction;
        }

        private Action MakeSelectCategoryAction(int catIndex) {
            void DoAction() {
                this.newCategoryIndex = catIndex;
            }
            return DoAction;
        }

        public override void DoWindowContents(Rect inRect) {
            var oldFont = Text.Font;
            var oldAnchor = Text.Anchor;
            try {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;

                if (this.newTableGeneratorIndex >= 0) {
                    this.SelectTableGenerator(this.newTableGeneratorIndex);
                    this.newTableGeneratorIndex = -1;
                }
            
                var instance = Instances[this.tableGeneratorIndex];
                var tableGenerator = instance.tableGenerator;

                if (this.newCategoryIndex >= 0) {
                    instance.tableGenerator.CategoryIndex = this.newCategoryIndex;
                    instance.scrollPosition = new Vector2();
                    this.newCategoryIndex = -1;
                    instance.tableGenerator.ResetData();
                }

                // draw table generator selection button
                {
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    
                    var tableGeneratorOptions = new List<DropdownMenuElement<int>>();
                    for (int tableGeneratorIndex = 0, catLength = TableGenerators.Count; tableGeneratorIndex < catLength; ++tableGeneratorIndex) {
                        var menuElement = new DropdownMenuElement<int>();
                        menuElement.payload = tableGeneratorIndex;
                        menuElement.option = new FloatMenuOption(
                            TableGenerators[tableGeneratorIndex].name,
                            this.MakeSelectTableGeneratorAction(tableGeneratorIndex)
                        );
                        tableGeneratorOptions.Add(menuElement);
                    }
                    Dropdown(
                        rect: new Rect(inRect.position, new Vector2(170, 32)),
                        target: 0,
                        getPayload: v => v,
                        menuGenerator: v => tableGeneratorOptions,
                        buttonLabel: TableGenerators[this.tableGeneratorIndex].name
                    );
                }

                tableGenerator.DrawWindow(inRect, ref instance.scrollPosition, this.MakeSelectCategoryAction, new Vector2(180, 0));
            } finally {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
            }
        }
    }
}
