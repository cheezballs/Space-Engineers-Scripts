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
        readonly string[] materials = { "Stone", "Silicon", "Nickel", "Iron", "Cobalt", "Silver", "Gold" };
        
        public Program()
        {
            Runtime.UpdateFrequency = UPDATE_FREQ;
        }

        void Main()
        {
            var materialAmounts = new Dictionary<string, double>();
            foreach (string material in materials)
            {
                materialAmounts[material] = 0;
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
                            foreach (string material in materials)
                            {
                                // Ensure exact match
                                if (itemName.Equals(material, StringComparison.OrdinalIgnoreCase))
                                {
                                    double amount;
                                    materialAmounts.TryGetValue(material, out amount);
                                    materialAmounts[material] = amount + (double)item.Amount;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            var lcd = GridTerminalSystem.GetBlockWithName("Cargo Stats") as IMyTextPanel;

            if (lcd == null)
            {
                Echo("Error: LCD panel named 'Cargo Stats' not found.");
                return;
            }

            var summary = new StringBuilder();
            summary.AppendLine("Material Summary:");
            foreach (var kvp in materialAmounts)
            {
                summary.AppendLine(kvp.Key + ": " + kvp.Value.ToString("N2"));
            }

            lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(summary.ToString());

            Echo(summary.ToString());
        }

        // -------------- SCRIPT END --------------------
    }
}