using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;

namespace Indoors.SpaceEngineers.Inventory.GallagherShipStats
{
    public class Program : MyGridProgram
    {
        // -------------- SCRIPT START --------------------
        
        private readonly string SHIP_STATS_LCD_NAME = "Gallagher Stat Screen";
        
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var lcd = GridTerminalSystem.GetBlockWithName(SHIP_STATS_LCD_NAME) as IMyTextPanel;
            if (lcd == null)
            {
                Echo("Error: LCD not found");
                return;
            }

            var cargoContainers = new List<IMyTerminalBlock>();
            var assemblers = new List<IMyAssembler>();
            var refineries = new List<IMyRefinery>();
            var hydrogenTanks = new List<IMyGasTank>();
            var oxygenTanks = new List<IMyGasTank>();
            var batteries = new List<IMyBatteryBlock>();

            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(cargoContainers, block => block.HasInventory);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries);
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(hydrogenTanks,
                tank => tank.BlockDefinition.SubtypeName.Contains("Hydrogen"));
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(oxygenTanks,
                tank => tank.BlockDefinition.SubtypeName.Contains("Oxygen"));
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);

            double totalVolume = 0.0;
            double currentVolume = 0.0;

            // Calculate cargo storage, assemblers, and refineries fullness
            foreach (var block in cargoContainers)
            {
                var inv = block.GetInventory(0);
                totalVolume += (double)inv.MaxVolume;
                currentVolume += (double)inv.CurrentVolume;
            }

            foreach (var assembler in assemblers)
            {
                var inv = assembler.GetInventory(0);
                totalVolume += (double)inv.MaxVolume;
                currentVolume += (double)inv.CurrentVolume;
            }

            foreach (var refinery in refineries)
            {
                var inv = refinery.GetInventory(0);
                totalVolume += (double)inv.MaxVolume;
                currentVolume += (double)inv.CurrentVolume;
            }

            double cargoPercentFull = (currentVolume / totalVolume) * 100.0;

            // Calculate hydrogen tanks fullness
            double hydrogenTotal = 0.0;
            double hydrogenCurrent = 0.0;
            foreach (var tank in hydrogenTanks)
            {
                hydrogenTotal += tank.Capacity;
                hydrogenCurrent += tank.FilledRatio * tank.Capacity;
            }

            double hydrogenPercentFull = (hydrogenCurrent / hydrogenTotal) * 100.0;

            // Calculate oxygen tanks fullness
            double oxygenTotal = 0.0;
            double oxygenCurrent = 0.0;
            foreach (var tank in oxygenTanks)
            {
                oxygenTotal += tank.Capacity;
                oxygenCurrent += tank.FilledRatio * tank.Capacity;
            }

            double oxygenPercentFull = (oxygenCurrent / oxygenTotal) * 100.0;

            // Calculate battery charge percentage
            double totalStoredPower = 0.0;
            double maxStoredPower = 0.0;
            foreach (var battery in batteries)
            {
                totalStoredPower += battery.CurrentStoredPower;
                maxStoredPower += battery.MaxStoredPower;
            }

            double batteryPercentFull = (totalStoredPower / maxStoredPower) * 100.0;

            // Calculate total of specific ingots in cargo storage, assemblers, and refineries
            var resources = new Dictionary<string, MyFixedPoint>
            {
                { "Iron", (MyFixedPoint)0 },
                { "Nickel", (MyFixedPoint)0 },
                { "Silicon", (MyFixedPoint)0 },
                { "Cobalt", (MyFixedPoint)0 },
                { "Gold", (MyFixedPoint)0 },
                { "Silver", (MyFixedPoint)0 }
            };

            Action<IMyInventory> addInventoryContents = inv =>
            {
                var items = new List<MyInventoryItem>();
                inv.GetItems(items);
                foreach (var item in items)
                {
                    var itemType = item.Type.TypeId.ToString();
                    var itemSubtype = item.Type.SubtypeId.ToString();

                    if (itemType == "MyObjectBuilder_Ingot" && resources.ContainsKey(itemSubtype))
                    {
                        resources[itemSubtype] += item.Amount;
                    }
                }
            };

            foreach (var block in cargoContainers)
            {
                addInventoryContents(block.GetInventory(0));
            }

            foreach (var assembler in assemblers)
            {
                addInventoryContents(assembler.GetInventory(0));
            }

            foreach (var refinery in refineries)
            {
                addInventoryContents(refinery.GetInventory(0));
            }

            // Build the LCD output
            string output = "Gallagher Station Stats\n";
            output += "----------------------------\n";
            output += string.Format("Cargo: {0:F1}% full\n", cargoPercentFull);
            output += string.Format("Hydrogen: {0:F1}% full\n", hydrogenPercentFull);
            output += string.Format("Oxygen: {0:F1}% full\n", oxygenPercentFull);
            output += string.Format("Battery: {0:F1}% charged\n", batteryPercentFull);
            output += "----------------------------\n";
            foreach (var resource in resources)
            {
                output += string.Format("{0} Ingots: {1}\n", resource.Key, resource.Value);
            }

            // Display on the LCD
            lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(output);
            lcd.FontSize = 1f;
            lcd.Alignment = TextAlignment.CENTER;
        }


        // -------------- SCRIPT END --------------------
    }
}