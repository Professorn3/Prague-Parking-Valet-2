using PragueParking.Core;
using PragueParking.DataAccess;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PragueParking.Appfv
{
    class Program
    {
        // Globala variabler som alla metoder kommer åt
        private static readonly string configFilePath = "config.json";
        private static DataHandler dataHandler = new DataHandler();
        private static Configuration? config;
        private static ParkingGarage? garage;

        static void Main()
        {
            Console.Title = "Prague Parking 2.1 (VG-version)";

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
                    AnsiConsole.MarkupLine("[green]Parkeringsdata laddad.[/]");
                }

                // Synkronisera garaget med config *efter* laddning (ifall config ändrats)
                SynchronizeGarageWithConfig(config, garage);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte ladda parkeringsdata. Kontrollera parking_data.json.[/]");
                AnsiConsole.WriteException(ex);
                Console.ReadKey();
                return;
            }

            AnsiConsole.MarkupLine("Tryck valfri tangent för att starta...");
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

                Console.Clear(); // Använd standard Console.Clear()
                ShowGarageMap(); // Rita kartan

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]HUVUDMENY[/]")
                        .PageSize(10)
                        .AddChoices(new[] {
                            "1. Parkera fordon",
                            "2. Hämta ut fordon",
                            "3. Flytta fordon",
                            "4. Sök fordon",
                            "5. Ladda om konfiguration (VG)",
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
                    AnsiConsole.MarkupLine("\n[red]Ett oväntat fel inträffade:[/]");
                    AnsiConsole.WriteException(ex);
                }

                if (keepRunning)
                {
                    AnsiConsole.MarkupLine("\nTryck valfri tangent för att återgå till menyn...");
                    Console.ReadKey(true);
                }
            }
        }

        #region Meny-metoder

        private static void Menu_Park()
        {
            if (garage == null || config == null) return;

            // Lägg till "Avbryt" som ett val
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

                if (vehicle is Bus && parkedAtSpot > config.BusSpotLimit)
                {
                    AnsiConsole.MarkupLine($"[red]Fel: Bussar får endast parkera på plats 1-{config.BusSpotLimit}.[/]");
                    success = false;
                }
                else
                {
                    success = garage.ParkVehicle(vehicle, parkedAtSpot);
                }
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
                AnsiConsole.MarkupLine("[red]Kunde inte parkera fordonet. Platsen är full, ogiltig, eller fordonstypen är inte tillåten där.[/]");
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

            if (vehicle is Bus && newSpotNumber > config.BusSpotLimit)
            {
                AnsiConsole.MarkupLine($"[red]Fel: Bussar får endast parkera på plats 1-{config.BusSpotLimit}. Fordonet flyttas tillbaka till plats {oldSpotNumber}.[/]");
                garage.ParkVehicle(vehicle, oldSpotNumber); // Parkera tillbaka
                return;
            }

            if (garage.ParkVehicle(vehicle, newSpotNumber))
            {
                AnsiConsole.MarkupLine($"[green]Fordon {vehicle.RegNumber} flyttat till plats {newSpotNumber}.[/]");
                dataHandler.SaveGarage(garage, config.ParkingDataFile);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Kunde inte parkera på plats {newSpotNumber} (platsen full eller ogiltig).[/]");
                AnsiConsole.MarkupLine($"[yellow]Fordonet parkeras tillbaka på sin gamla plats {oldSpotNumber}.[/]");
                garage.ParkVehicle(vehicle, oldSpotNumber);
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

        // ==========================================================
        // HÄR ÄR DEN REPARERADE "LADDA OM"-METODEN
        // ==========================================================
        private static void Menu_ReloadConfig()
        {
            // Bekräfta först
            if (!AnsiConsole.Confirm("Detta laddar om config.json och skapar ett NYTT TOMT P-hus.\nALLA parkerade fordon i minnet kommer raderas. Vill du fortsätta?"))
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
                    // Försök ladda om den gamla configen för att undvika krasch
                    config = dataHandler.LoadConfig(configFilePath);
                    return;
                }

                // Steg 2: Skapa ett NYTT, TOMT P-hus baserat på den nya configen
                // Detta raderar all gammal data i minnet.
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

        // Denna metod uppdaterar P-huset (som laddats från fil)
        // för att matcha nya inställningar i config.json (t.ex. ny SpotSize)
        private static void SynchronizeGarageWithConfig(Configuration config, ParkingGarage garage)
        {
            // Ta bort platser om config har färre platser än P-huset
            if (garage.Spots.Count > config.TotalSpots)
            {
                // Vi kollar baklänges om det är säkert att ta bort platserna
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
            // Lägg till platser om config har fler platser
            else if (garage.Spots.Count < config.TotalSpots)
            {
                for (int i = garage.Spots.Count + 1; i <= config.TotalSpots; i++)
                {
                    garage.Spots.Add(new ParkingSpot(i, config.SpotSize));
                }
            }

            // Uppdatera kapacitet på alla befintliga platser
            foreach (var spotInterface in garage.Spots)
            {
                if (spotInterface is ParkingSpot spot) // Typ-omvandla
                {
                    spot.Capacity = config.SpotSize; // Nu kan vi ändra kapaciteten
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

        private static string GetSpotMarkup(IParkingSpot spot)
        {
            if (config == null) return "[red]ERR[/]";

            int fill = spot.GetCurrentFill();
            int capacity = spot.Capacity;
            string spotNum = $"[bold]P {spot.SpotNumber}[/]";
            string busMarker = (spot.SpotNumber <= config.BusSpotLimit) ? "B" : "";

            if (fill == 0)
            {
                return $"[green]{spotNum}{busMarker}[/]";
            }

            // Bygg strängen med fordonsinformation
            string vehicles = string.Join("\n", spot.ParkedVehicles.Select(v =>
                $"{v.VehicleType.Substring(0, Math.Min(3, v.VehicleType.Length))}: {(!string.IsNullOrEmpty(v.RegNumber) ? v.RegNumber : "REG?")}"));

            if (fill >= capacity)
            {
                return $"[red]{spotNum}{busMarker} ({fill}/{capacity})\n{vehicles}[/]";
            }

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

