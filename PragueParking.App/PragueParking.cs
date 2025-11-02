using PragueParking.Core;
using PragueParking.DataAccess;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PragueParking.App
{
    class Program
    {

        private static readonly string configFilePath = "config.json";
        private static DataHandler dataHandler = new DataHandler();
        private static Configuration? config;
        private static ParkingGarage? garage;

        static void Main()
        {
            Console.Title = "Prague Parking 2.1";

            try
            {
                config = dataHandler.LoadConfig(configFilePath);
                if (config == null)
                {
                    AnsiConsole.MarkupLine("[red]Kunde inte ladda eller skapa config.json. Avslutar.[/]");
                    Console.ReadKey();
                    return;
                }
                AnsiConsole.MarkupLine("[green]Konfiguration laddad.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte ladda konfigurationen. Kontrollera config.json.[/]");
                AnsiConsole.WriteException(ex);
                Console.ReadKey();
                return;
            }

            try
            {
                garage = dataHandler.LoadGarage(config.ParkingDataFile);
                if (garage == null)
                {
                    AnsiConsole.MarkupLine($"[yellow]Datafil ({config.ParkingDataFile}) hittades inte, skapar nytt P-hus.[/]");
                    garage = new ParkingGarage(config);
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Parkeringsdata uppladdad.[/]");
                }

                // Synkronisera garaget med config 
                SynchronizeGarageWithConfig(config, garage);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte ladda parkeringsdata. Kontrollera parking_data.json.[/]");
                AnsiConsole.WriteException(ex);
                Console.ReadKey();
                return;
            }

            AnsiConsole.MarkupLine("Tryck valfri tangent för att starta programmet...");
            Console.ReadKey(true);

            RunMainMenu();
        }

        private static void RunMainMenu()
        {
            bool keepRunning = true;
            while (keepRunning)
            {
                if (garage == null || config == null)
                {
                    AnsiConsole.MarkupLine("[red]Kritiskt fel: Garage eller Config är null. Avslutar.[/]");
                    return;
                }

                Console.Clear();
                ShowGarageMap(); // Översikten för P-huset

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]HUVUDMENY[/]")
                        .PageSize(10)
                        .AddChoices(new[] {
                            "1. Parkera fordon",
                            "2. Hämta ut fordon",
                            "3. Flytta fordon",
                            "4. Sök fordon",
                            "5. Ladda om konfiguration",
                            "0. Avsluta"
                        }));

                string menuChoice = choice.Split('.')[0];
                try
                {
                    switch (menuChoice)
                    {
                        case "1":
                            Menu_Park();
                            break;
                        case "2":
                            Menu_Remove();
                            break;
                        case "3":
                            Menu_Move();
                            break;
                        case "4":
                            Menu_Find();
                            break;
                        case "5":
                            Menu_ReloadConfig();
                            break;
                        case "0":
                            keepRunning = false;
                            AnsiConsole.MarkupLine("[yellow]Sparar och avslutar...[/]");
                            dataHandler.SaveGarage(garage, config.ParkingDataFile);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("\n[red]Ett fel inträffade:[/]");
                    AnsiConsole.WriteException(ex);
                }

                if (keepRunning)
                {
                    AnsiConsole.MarkupLine("\nTryck valfri tangent för att återgå till början");
                    Console.ReadKey(true);
                }
            }
        }

        #region Meny-metoder

        private static void Menu_Park()
        {
            if (garage == null || config == null) return;

            // Avbryt finns som ett val
            var choices = new List<string>(config.VehicleSizes.Keys);
            choices.Add("Avbryt");

            var vehicleType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Vilken typ av fordon vill du parkera?")
                    .AddChoices(choices));

            if (vehicleType == "Avbryt")
            {
                AnsiConsole.MarkupLine("[yellow]Parkering avbruten.[/]");
                return;
            }

            string regNum = AnsiConsole.Ask<string>("Ange [green]registreringsnummer[/] (lämna tomt för att avbryta): ").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(regNum))
            {
                AnsiConsole.MarkupLine("[yellow]Parkering avbruten.[/]");
                return;
            }
            if (regNum.Length > 10 || regNum.Contains("#") || regNum.Contains("|"))
            {
                AnsiConsole.MarkupLine("[red]Ogiltigt registreringsnummer (max 10 tecken, inga '#' eller '|').[/]");
                return;
            }

            if (garage.FindVehicle(regNum) != null)
            {
                AnsiConsole.MarkupLine("[red]Ett fordon med detta registreringsnummer är redan parkerat.[/]");
                return;
            }

            IVehicle? vehicle = CreateVehicle(vehicleType, regNum);
            if (vehicle == null) return;

            bool specificSpot = AnsiConsole.Confirm("Vill du välja en specifik plats?");

            bool success = false;
            int parkedAtSpot = -1;

            if (specificSpot)
            {
                parkedAtSpot = AnsiConsole.Ask<int>($"Ange platsnummer (1-{config.TotalSpots}): ");


                success = garage.ParkVehicle(vehicle, parkedAtSpot);
            }

            else

            {
                success = garage.ParkVehicle(vehicle, out parkedAtSpot);
            }

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Fordon {regNum} parkerat på plats {parkedAtSpot}.[/]");
                dataHandler.SaveGarage(garage, config.ParkingDataFile);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Kunde inte parkera fordonet. Platsen(erna) är fulla, ogiltiga, eller fordonstypen är inte tillåten där.[/]");
            }
        }

        private static void Menu_Remove()
        {
            if (garage == null || config == null) return;

            string regNum = AnsiConsole.Ask<string>("Ange [green]registreringsnummer[/] för fordonet som ska hämtas (lämna tomt för att avbryta): ").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(regNum))
            {
                AnsiConsole.MarkupLine("[yellow]Åtgärd avbruten.[/]");
                return;
            }

            IVehicle? vehicle = garage.UnparkVehicle(regNum, out int spotNumber);

            if (vehicle != null)
            {
                TimeSpan duration = DateTime.Now - vehicle.ArrivalTime;
                int cost = config.CalculateCost(vehicle.VehicleType, duration);

                AnsiConsole.MarkupLine($"[green]Fordon {vehicle.RegNumber} ({vehicle.VehicleType}) hämtat från plats {spotNumber}.[/]");
                AnsiConsole.MarkupLine($"Parkerad tid: [bold]{duration.TotalMinutes:F0} minuter[/].");
                AnsiConsole.MarkupLine($"[bold yellow]Att betala: {cost} CZK[/]");

                dataHandler.SaveGarage(garage, config.ParkingDataFile);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            }
        }

        private static void Menu_Move()
        {
            if (garage == null || config == null) return;

            string regNum = AnsiConsole.Ask<string>("Ange [green]registreringsnummer[/] för fordonet som ska flyttas (lämna tomt för att avbryta): ").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(regNum))
            {
                AnsiConsole.MarkupLine("[yellow]Åtgärd avbruten.[/]");
                return;
            }

            IVehicle? vehicle = garage.UnparkVehicle(regNum, out int oldSpotNumber);

            if (vehicle == null)
            {
                AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]Fordon {vehicle.RegNumber} hämtat från plats {oldSpotNumber}.[/]");
            int newSpotNumber = AnsiConsole.Ask<int>($"Ange ny plats (1-{config.TotalSpots}): ");

            // Försök parkera på den nya platsen
            if (garage.ParkVehicle(vehicle, newSpotNumber))
            {
                AnsiConsole.MarkupLine($"[green]Fordon {vehicle.RegNumber} flyttat till plats {newSpotNumber}.[/]");
                dataHandler.SaveGarage(garage, config.ParkingDataFile);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Kunde inte parkera på plats {newSpotNumber} (platsen full eller ogiltig).[/]");
                AnsiConsole.MarkupLine($"[yellow]Fordonet parkeras tillbaka på sin gamla plats {oldSpotNumber}.[/]");
                garage.ParkVehicle(vehicle, oldSpotNumber); // Parkera tillbaka
            }
        }

        private static void Menu_Find()
        {
            if (garage == null) return;
            string regNum = AnsiConsole.Ask<string>("Ange [green]registreringsnummer[/] att söka efter (lämna tomt för att avbryta): ").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(regNum))
            {
                AnsiConsole.MarkupLine("[yellow]Åtgärd avbruten.[/]");
                return;
            }

            IParkingSpot? spot = garage.FindVehicle(regNum);

            if (spot != null)
            {
                AnsiConsole.MarkupLine($"[green]Fordon {regNum} hittades på plats {spot.SpotNumber}.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            }
        }

        private static void Menu_ReloadConfig()
        {
            // Bekräfta först
            string prompt = "Detta laddar om config.json och skapar ett NYTT TOMT P-hus.\nALLA parkerade fordon i minnet kommer raderas. Vill du fortsätta?";
            if (!AnsiConsole.Confirm(prompt))
            {
                AnsiConsole.MarkupLine("[yellow]Omladdning avbruten.[/]");
                return;
            }

            AnsiConsole.MarkupLine("[yellow]Laddar om konfiguration och skapar nytt P-hus...[/]");
            try
            {
                // Steg 1: Ladda om config.json
                config = dataHandler.LoadConfig(configFilePath);
                if (config == null)
                {
                    AnsiConsole.MarkupLine("[red]Kunde inte ladda config.json. Avbryter.[/]");
                    config = dataHandler.LoadConfig(configFilePath); // Försök ladda om den gamla
                    return;
                }

                // Steg 2: Skapa ett NYTT, TOMT P-hus
                garage = new ParkingGarage(config);

                AnsiConsole.MarkupLine("[green]Omladdning klar! P-huset är nu tomt.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Ett fel inträffade vid omladdning:[/]");
                AnsiConsole.WriteException(ex);
            }
        }

        #endregion

        #region Hjälp-metoder

        private static void SynchronizeGarageWithConfig(Configuration config, ParkingGarage garage)
        {
            if (garage.Spots.Count > config.TotalSpots)
            {
                for (int i = garage.Spots.Count - 1; i >= config.TotalSpots; i--)
                {
                    if (garage.Spots[i].GetCurrentFill() == 0)
                    {
                        garage.Spots.RemoveAt(i);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Varning: Kunde inte ta bort plats {garage.Spots[i].SpotNumber} eftersom den är upptagen.[/]");
                    }
                }
            }
            else if (garage.Spots.Count < config.TotalSpots)
            {
                for (int i = garage.Spots.Count + 1; i <= config.TotalSpots; i++)
                {
                    garage.Spots.Add(new ParkingSpot(i, config.SpotSize));
                }
            }

            foreach (var spotInterface in garage.Spots)
            {
                if (spotInterface is ParkingSpot spot)
                {
                    spot.Capacity = config.SpotSize;
                }
            }
        }

        private static void ShowGarageMap()
        {
            if (garage == null) return;

            var table = new Table().SimpleBorder().BorderColor(Color.Grey);
            table.Title("[bold]ÖVERSIKT P-HUS[/]");

            int spotsPerRow = 10;
            for (int i = 0; i < spotsPerRow; i++)
            {
                table.AddColumn(new TableColumn($"").Centered());
            }

            int spotIndex = 0;
            while (spotIndex < garage.Spots.Count)
            {
                var rowCells = new List<string>();
                for (int i = 0; i < spotsPerRow && spotIndex < garage.Spots.Count; i++)
                {
                    rowCells.Add(GetSpotMarkup(garage.Spots[spotIndex]));
                    spotIndex++;
                }
                table.AddRow(rowCells.ToArray());
            }
            AnsiConsole.Write(table);
        }

        // Kartlogiken för att visa varje parkeringsplats
        private static string GetSpotMarkup(IParkingSpot spot)
        {
            if (config == null) return "[red]ERR[/]";

            string spotNum = $"[bold]P {spot.SpotNumber}[/]";
            string busMarker = (spot.SpotNumber <= config.BusSpotLimit) ? "B" : "";

            if (spot is ParkingSpot ps && ps.OccupiedByBusReg != null)
            {
                return $"[dim red]{spotNum}{busMarker}\n(Buss: {ps.OccupiedByBusReg})[/]";
            }

            int fill = spot.GetCurrentFill();
            int capacity = spot.Capacity;

            //  Kolla om platsen är tom
            if (fill == 0)
            {
                return $"[green]{spotNum}{busMarker}[/]";
            }

            // Bygg strängen med fordon (Buss, Bil, MC, etc.)
            string vehicles = string.Join("\n", spot.ParkedVehicles.Select(v =>
                $"{v.VehicleType.Substring(0, Math.Min(3, v.VehicleType.Length))}: {(!string.IsNullOrEmpty(v.RegNumber) ? v.RegNumber : "!!!")}"));

              //  Platsen är full (antingen 4/4 av MC, eller en Buss 16/16)
            if (fill >= capacity)
            {
                return $"[red]{spotNum}{busMarker} ({fill}/{capacity})\n{vehicles}[/]";
            }

            // 5. Platsen är delvis fylld (MC/Cyklar)
            return $"[yellow]{spotNum}{busMarker} ({fill}/{capacity})\n{vehicles}[/]";
        }


        private static IVehicle? CreateVehicle(string vehicleType, string regNum)
        {
            switch (vehicleType)
            {
                case "Car":
                    return new Car(regNum);
                case "Motorcycle":
                    return new Motorcycle(regNum);
                case "Bicycle":
                    return new Bicycle(regNum);
                case "Bus":
                    return new Bus(regNum);
                default:
                    AnsiConsole.MarkupLine($"[red]Okänd fordonstyp: {vehicleType}[/]");
                    return null;
            }
        }

        #endregion
    }
}

