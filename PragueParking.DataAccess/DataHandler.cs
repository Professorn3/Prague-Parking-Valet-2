using PragueParking.Core;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace PragueParking.DataAccess
{
    // hanterar laddning och sparning av konfiguration och parkeringsdata

    public class DataHandler
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public Configuration? LoadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = CreateDefaultConfig();
                SaveConfig(defaultConfig, configFilePath);
                return defaultConfig;
            }

            string json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Configuration?>(json, _jsonSettings);
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
            return JsonConvert.DeserializeObject<ParkingGarage?>(json, _jsonSettings);
        }

        //  Den här metoden sparar nu till rätt fil

        public void SaveGarage(ParkingGarage garage, string garageDataPath)
        {
            string json = JsonConvert.SerializeObject(garage, _jsonSettings);
            File.WriteAllText(garageDataPath, json);
        }

        private Configuration CreateDefaultConfig()
        {
            return new Configuration
            {
                TotalSpots = 100,
                SpotSize = 4,
                FreeParkingMinutes = 10,
                BusSpotLimit = 50,
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
                },
                ParkingDataFile = "parking_data.json"
            };
        }
    }
}

