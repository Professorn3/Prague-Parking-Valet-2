using PragueParking.Core;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;

namespace PragueParking.DataAccess
{
    // Den här filen ska BARA innehålla DataHandler-klassen
    public class DataHandler
    {
        private static readonly JsonSerializerSettings _jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public Configuration LoadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = CreateDefaultConfig();
                SaveConfig(defaultConfig, configFilePath);
                return defaultConfig;
            }

            string json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Configuration>(json, _jsonSettings);
        }

        public void SaveConfig(Configuration config, string configFilePath)
        {
            string json = JsonConvert.SerializeObject(config, _jsonSettings);
            File.WriteAllText(configFilePath, json);
        }

        public ParkingGarage? LoadGarage(string garageDataPath)
        {
            if (!File.Exists(garageDataPath))
            {
                return null;
            }

            string json = File.ReadAllText(garageDataPath);
            return JsonConvert.DeserializeObject<ParkingGarage>(json, _jsonSettings);
        }

        public void SaveGarage(ParkingGarage garage, string garageDataPath)
        {
            string json = JsonConvert.SerializeObject(garage, _jsonSettings);
            // *** VIKTIG FIX: *** Sparade till fel fil förut
            File.WriteAllText(garageDataPath, json);
        }

        private Configuration CreateDefaultConfig()
        {
            return new Configuration
            {
                TotalSpots = 100,
                SpotSize = 4,
                FreeParkingMinutes = 10,
                ParkingDataFile = "parking_data.json",
                Prices = new Dictionary<string, int>
                {
                    { "Bicycle", 5 },
                    { "Motorcycle", 10 },
                    { "Car", 20 },
                    { "Bus", 80 }
                },
                VehicleSizes = new Dictionary<string, int>
                {
                    { "Bicycle", 1 },
                    { "Motorcycle", 2 },
                    { "Car", 4 },
                    { "Bus", 16 }
                }
            };
        }
    }
}

