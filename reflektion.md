## Personlig Reflektion (VG-del)

Här följer min personliga reflektion över projektet och kursen.

### 4.1 Sammanfattning

Detta projekt har varit en övergång från grundläggande skripting i C# till att bygga en sammanhållen, objektorienterad applikation med en tydlig arkitektur. Målet var att refaktorera ett befintligt system till att bli mer underhållbart, flexibelt och robust genom att använda klasser, arv, interface och en uppdelning i flera projekt (klassbibliotek).

### 4.2 Hur jag löste uppgiften

Jag strukturerade min lösning i fyra separata projekt för att följa principen om "Separation of Concerns":

* **`PragueParking.Core`:** Hjärtat i applikationen. Här finns all kärnlogik.
    * **Interface:** `IVehicle` och `IParkingSpot` definierar kontrakten för hur ett fordon och en P-plats ska bete sig.
    * **Klasser:**
        * `Vehicle`: En abstrakt basklass (eller en vanlig klass som implementerar `IVehicle`) som `Car`, `Motorcycle`, `Bus`, och `Bicycle` ärver ifrån.
        * `ParkingSpot`: Hanterar logiken för en enskild P-plats, inklusive att hålla reda på vilka fordon som står där och hur mycket plats som är kvar.
        * `ParkingGarage`: Innehåller en `List<IParkingSpot>` och hanterar all övergripande logik som att parkera, hämta och flytta fordon.

* **`PragueParking.App`:** Användargränssnittet. Detta projekt innehåller ingen affärslogik, utan anropar bara metoder i `ParkingGarage`.
    * `Program.cs` innehåller huvudmenyn och använder `Spectre.Console` för att rita ut menyn och den visuella kartan över P-huset.

* **`PragueParking.DataAccess`:** Ansvarar för all filhantering.
    * `DataHandler.cs` har metoder som `LoadConfig`, `SaveGarage`, och `LoadGarage`. Detta gör att Core-projektet inte behöver veta hur data sparas (om det är JSON, XML eller en databas).

* **`PragueParking.Test`:** Enhetstester för att verifiera att specifika delar av logiken (t.ex. kostnadsberäkning eller parkeringslogik) fungerar som de ska.

### 4.3 Utmaningar i uppgiften och hur de löstes

Jag stötte på två stora utmaningar under projektets gång.

**Utmaning 1: Hantering av JSON och Arv (Polymorfism)**

Ett av de första och mest kluriga problemen var att spara och ladda P-huset till JSON. P-huset har en `List<IParkingSpot>`, och varje `ParkingSpot` har en `List<IVehicle>`. Problemet är att JSON-deserialiseraren inte vet om ett `IVehicle` i filen ska återskapas som en `Car`, `Bus` eller `Motorcycle`.

**Min första (misslyckade) ansats:** Jag försökte bygga en egen, anpassad `JsonConverter` (en `VehicleConverter`). Detta visade sig vara extremt komplicerat och ledde till buggar och svårläst kod. Som mina anteckningar (bild 2) visar, "den behövdes inte och orsakade bara problem".

**Lösningen:** Jag upptäckte att `Newtonsoft.Json` har en inbyggd lösning för detta. Genom att ta bort hela min anpassade `VehicleConverter` och istället lägga till en inställning vid serialisering och deserialisering (`TypeNameHandling`), kunde problemet lösas på en enda rad kod.

Genom att ställa in `TypeNameHandling = TypeNameHandling.Auto` (eller `Objects`) i `JsonSerializerSettings`, skriver `Newtonsoft.Json` automatiskt ut en `$type`-egenskap i JSON-filen (t.ex. `"$type": "PragueParking.Core.Car, PragueParking.Core"`). När filen sedan läses in igen, vet deserialiseraren exakt vilken klass den ska skapa. Detta var en stor lärdom i att lita på och använda ramverkets inbyggda funktioner istället för att återuppfinna hjulet.

**Utmaning 2: Den logiska konflikten med Buss-parkering (VG-delen)**

Den absolut mest frustrerande utmaningen var att implementera VG-kravet för bussar.

**Problemet:** I uppgiftsbeskrivningen står det: "Buss 16" och "En P-ruta har då storleken 4". Detta är en logisk omöjlighet. En buss med storlek 16 kan omöjligen parkeras på en P-ruta med storlek 4.

**Felsökningen:** Jag felsökte min kod i `ParkingGarage.cs` i timmar. Jag trodde att min logik för att kontrollera tillgängligt utrymme var felaktig, eftersom en buss aldrig kunde parkeras.

**Lösningen:** Som mina anteckningar (bild 2) bekräftar, insåg jag till slut att detta inte var ett fel i min kod, utan en logisk motsägelse i själva uppgiftsbeskrivningen. Lösningen var att använda den flexibilitet vi själva hade byggt: `config.json`.

Genom att i min `config.json` ändra `SpotSize` från 4 till 16 (eller ett annat värde som var delbart med alla fordonsstorlekar), kunde systemet fungera. En buss (16) kunde nu parkeras på en P-plats (16).

Detta krävde också en uppdatering i `Program.cs` (`GetSpotMarkup`-metoden) för att visuellt kunna hantera fordon som tar upp hela platsen (som en buss eller bil) jämfört med fordon som kan dela plats (som MC och cyklar). Jag lade även till en specialregel för bussar (`BusSpotLimit` på rad 60 och 76 i `ParkingGarage.cs` nämndes i mina anteckningar) för att säkerställa att de bara kunde parkera på de platser som var avsedda för dem.

### 4.4 Metoder och modeller som använts

* **OOP (Objektorienterad programmering):** Kärnan i hela projektet.
* **Arv:** En `Vehicle`-basklass/interface som specifika fordon som `Car` och `Bus` ärver ifrån.
* **Polymorfism:** Möjligheten att lagra alla fordonstyper i en `List<IVehicle>` och behandla dem lika, trots att deras underliggande logik (t.ex. `Size`) skiljer sig.
* **Interface-baserad design:** Genom att koda mot `IVehicle` och `IParkingSpot` (som VG-kravet angav) blev koden mer flexibel och lättare att testa.
* **N-lagersarkitektur (förenklad):** Tydlig uppdelning mellan Presentation (`.App`), Affärslogik (`.Core`) och Dataåtkomst (`.DataAccess`).
* **TDD (Test-Driven Development):** Använde `MSTest` för att skriva enhetstester innan eller samtidigt som logiken skrevs, vilket hjälpte till att verifiera att mina metoder (särskilt kostnadsberäkningen) var korrekta.
* **JSON Serialisering:** Använde `Newtonsoft.Json` för att persistent lagra data.

### 4.5 Hur du skulle lösa uppgiften nästa gång

* **JSON-hantering:** Jag skulle från första början använt `TypeNameHandling` istället för att ens försöka skriva en egen `JsonConverter`.
* **Validering:** Jag skulle lägga mer tid på robust validering av `config.json` vid uppstart. Om en användare skriver in "Bil" istället för "Car" eller sätter priser till negativa tal, bör programmet ge tydliga felmeddelanden istället för att krascha.
* **Enhetstester:** Jag skulle skrivit fler och mer djupgående enhetstester, särskilt för kantfall som att parkera en buss, eller försöka flytta ett fordon till en full plats.

### 4.6 Slutsats hemuppgift

Detta var en extremt lärorik uppgift. Den tvingade mig att på djupet förstå varför OOP är användbart. Att separera logiken i olika projekt kändes först som överkurs, men jag insåg snabbt hur enkelt det gjorde det att underhålla och testa koden. Utmaningen med JSON-serialisering och den logiska buggen i uppgiftsbeskrivningen var frustrerande, men gav mig värdefulla insikter i felsökning och vikten av att ifrågasätta kraven.

### 4.7 Slutsats kurs

Kursen har varit ett stort steg upp från grundläggande programmering. Att gå från att skriva all kod i en enda `Program.cs` till att designa en helhetslösning med klassbibliotek, interface och extern datahantering har varit både utmanande och väldigt givande. Jag känner mig nu mycket mer bekväm med att strukturera kod på ett professionellt och skalbart sätt.
