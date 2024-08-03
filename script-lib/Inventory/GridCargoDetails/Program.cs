using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;

namespace Indoors.SpaceEngineers.Inventory.GridCargoDetails
{
    public class Program : MyGridProgram
    {
        // -------------- SCRIPT START --------------------
        readonly UpdateFrequency UPDATE_FREQ = UpdateFrequency.Update100;
        readonly string[] MATERIALS = { "Stone", "Silicon","Nickel", "Iron", "Cobalt", "Silver", "Gold", "uranium" };
        readonly string[] COMPONENTS = { "SteelPlate", "InteriorPlate", "Construction", "Computer", "Motor", "MetalGrid"};
        private readonly string MATERIAL_STATS_LCD_NAME = "Cargo Material Stats";
        private readonly string COMPONENT_STATS_LCD_NAME = "Cargo Component Stats";
        
        public Program()
        {
            Runtime.UpdateFrequency = UPDATE_FREQ;
        }

        void Main()
        {
            var materialsLCD = GridTerminalSystem.GetBlockWithName(MATERIAL_STATS_LCD_NAME) as IMyTextPanel;
            var componentsLCD = GridTerminalSystem.GetBlockWithName(COMPONENT_STATS_LCD_NAME) as IMyTextPanel;
            if (materialsLCD == null || componentsLCD == null)
            {
                Echo($"Error: LCD panels named {MATERIAL_STATS_LCD_NAME} or {COMPONENT_STATS_LCD_NAME} not found.");
                return;
            }
            
            var materialAmounts = new Dictionary<string, double>();
            foreach (string material in MATERIALS)
            {
                materialAmounts[material] = 0;
            }
            
            var componentAmounts = new Dictionary<string, double>();
            foreach (string component in COMPONENTS)
            {
                componentAmounts[component] = 0;
            }
            
            var allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            foreach (var block in allBlocks)
            {
                if (block is IMyCargoContainer || block is IMyAssembler || block is IMyRefinery)
                {
                    for (int i = 0; i < block.InventoryCount; i++)
                    {
                        var inventory = block.GetInventory(i);
                        var items = new List<MyInventoryItem>();
                        inventory.GetItems(items);

                        foreach (var item in items)
                        {
                            string itemName = item.Type.SubtypeId;
                            foreach (string material in MATERIALS)
                            {
                                if (itemName.Equals(material, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    double amount;
                                    materialAmounts.TryGetValue(material, out amount);
                                    materialAmounts[material] = amount + (double)item.Amount;
                                    break;
                                }
                            }
                            foreach (string component in COMPONENTS)
                            {
                                if (itemName.Equals(component, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    double amount;
                                    componentAmounts.TryGetValue(component, out amount);
                                    componentAmounts[component] = amount + (double)item.Amount;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            var summary = new StringBuilder();
            summary.AppendLine("Materials Summary:");
            foreach (var kvp in materialAmounts)
            {
                summary.AppendLine(kvp.Key + ": " + kvp.Value.ToString("N2"));
            }
            
            materialsLCD.ContentType = ContentType.TEXT_AND_IMAGE;
            materialsLCD.WriteText(summary.ToString());
            Echo(summary.ToString());
            
            summary = new StringBuilder();
            summary.AppendLine("Components Summary:");
            foreach (var kvp in componentAmounts)
            {
                summary.AppendLine(kvp.Key + ": " + kvp.Value.ToString("N2"));
            }
            
            componentsLCD.ContentType = ContentType.TEXT_AND_IMAGE;
            componentsLCD.WriteText(summary.ToString());
            
            Echo(summary.ToString());
        }

        // -------------- SCRIPT END --------------------
    }
}