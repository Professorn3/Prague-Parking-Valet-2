
```
## Personlig Reflektion (VG-del)

Här är min personliga reflektion över projektet och kursen.

### 4.1 Sammanfattning

Det här projektet handlade om att gå från enklare C#-scripting till att bygga en riktig, objektorienterad applikation. Målet var att strukturera om (refaktorera) ett gammalt system så att det blev lättare att underhålla och bygga vidare på. Jag använde klasser, arv, interface och delade upp koden i flera projekt (klassbibliotek) för att lyckas.

### 4.2 Hur jag löste uppgiften

Jag delade upp lösningen i fyra projekt för att hålla koden organiserad (enligt principen "Separation of Concerns"):

* **`PragueParking.Core`:** Här finns all kärnlogik, själva "hjärnan" i programmet.
    * **Interface:** `IVehicle` och `IParkingSpot` bestämmer *vad* ett fordon och en P-plats måste kunna göra.
    * **Klasser:**
        * `Vehicle`: En basklass (som implementerar `IVehicle`) som `Car`, `Motorcycle`, `Bus`, och `Bicycle` ärver ifrån.
        * `ParkingSpot`: Sköter logiken för en enskild P-plats, som att hålla koll på vilka fordon som står där och hur mycket plats som är kvar.
        * `ParkingGarage`: Hanterar själva garaget, som en lista av P-platser (`List<IParkingSpot>`), och sköter logik för att parkera, hämta och flytta fordon.

* **`PragueParking.App`:** Detta är användargränssnittet (det användaren ser). Det innehåller ingen "tänkande" logik, utan anropar bara metoder i `Core`-projektet. Här hanteras menyn och den visuella kartan av P-huset med hjälp av `Spectre.Console`.

* **`PragueParking.DataAccess`:** Sköter allt som har med filhantering att göra (spara/ladda).
    * `DataHandler.cs` har metoder som `LoadConfig`, `SaveGarage`, och `LoadGarage`. Fördelen är att `Core`-projektet inte behöver bry sig om *hur* datan sparas (om det är JSON, XML eller en databas).

* **`PragueParking.Test`:** Enhetstester för att kolla att viktiga delar av logiken (som kostnadsberäkning eller parkeringsregler) fungerar som de ska.

### 4.3 Utmaningar i uppgiften och hur de löstes

Jag stötte på två stora utmaningar.

**Utmaning 1: Hantering av JSON och Arv (Polymorfism)**

Ett klurigt problem var att spara och ladda garaget till en JSON-fil. Garaget har en lista av P-platser, som i sin tur har en lista av fordon (`List<IVehicle>`). När man laddar filen, hur vet programmet om ett `IVehicle` ska återskapas som en `Car`, `Bus` eller `Motorcycle`?

**Mitt första (misslyckade) försök:** Jag försökte bygga en egen "översättare" (`JsonConverter`). Det blev väldigt komplicerat och skapade mest buggar, vilket mina anteckningar (bild 2) också visar.

**Lösningen:** Jag insåg att `Newtonsoft.Json` redan har en inbyggd lösning. Genom att ta bort min egna `JsonConverter` och istället lägga till inställningen `TypeNameHandling = TypeNameHandling.Auto` (en rad kod) så löstes allt. Biblioteket skriver då själv in vilken typ objektet har (t.ex. `"$type": "PragueParking.Core.Car"`) i JSON-filen. När filen läses in igen vet programmet exakt vilken klass den ska skapa. En viktig läxa: använd ramverkets inbyggda funktioner innan du bygger egna.

**Utmaning 2: Den logiska konflikten med Buss-parkering (VG-delen)**

Den mest frustrerande utmaningen var VG-kravet för bussar.

**Problemet:** I uppgiften stod det att en buss har storlek 16, men en P-ruta har storlek 4. Detta är en logisk omöjlighet; bussen kan inte parkeras.

**Felsökningen:** Jag letade fel i min egen parkeringslogik (`ParkingGarage.cs`) i timmar. Jag var säker på att min kod för att kolla ledig plats var fel.

**Lösningen:** Till slut insåg jag (vilket mina anteckningar, bild 2, bekräftar) att felet inte låg i min kod, utan i själva uppgiften. Lösningen var att använda `config.json`-filen som vi hade byggt. Genom att ändra `SpotSize` i config-filen från 4 till 16 (eller ett annat passande värde) så fungerade systemet. En buss (16) kunde nu parkera på en P-plats (16).

Detta krävde också en liten uppdatering i `Program.cs` (`GetSpotMarkup`-metoden) för att rita ut kartan snyggt, oavsett om en plats höll en stor buss eller flera motorcyklar. Jag lade också till en extra regel för bussar (`BusSpotLimit` nämnd i mina anteckningar) för att se till att de bara parkerade på avsedda platser.

### 4.4 Metoder och modeller som använts

* **OOP (Objektorienterad programmering):** Hela grunden för projektet.
* **Arv:** En `Vehicle`-basklass/interface som `Car` och `Bus` ärvde ifrån.
* **Polymorfism:** Möjligheten att lagra alla fordonstyper i en och samma lista (`List<IVehicle>`).
* **Interface:** Användningen av `IVehicle` och `IParkingSpot` gjorde koden flexibel och lättare att testa.
* **Uppdelad arkitektur (N-lager):** Tydlig uppdelning mellan Gränssnitt (`.App`), Logik (`.Core`) och Datalagring (`.DataAccess`).
* **Enhetstestning (TDD):** Använde `MSTest` för att skriva tester, speciellt för att se till att kostnadsberäkningen blev rätt.
* **JSON-serialisering:** Använde `Newtonsoft.Json` för att spara data till fil.

### 4.5 Hur jag skulle lösa uppgiften nästa gång

* **JSON-hantering:** Jag skulle direkt använt `TypeNameHandling` istället för att slösa tid på att bygga en egen `JsonConverter`.
* **Bättre validering:** Jag skulle lagt mer tid på att validera `config.json` när programmet startar. Om en användare skriver "Bil" istället för "Car", eller ett negativt pris, ska programmet ge ett tydligt felmeddelande, inte krascha.
* **Fler enhetstester:** Jag skulle skrivit fler tester för "kantfall" (edge cases), som att parkera en buss eller försöka flytta ett fordon till en redan full plats.

### 4.6 Slutsats hemuppgift

Detta var en väldigt lärorik uppgift. Jag fick en djupare förståelse för *varför* OOP är användbart. Att dela upp koden i olika projekt kändes först onödigt, men jag insåg snabbt hur mycket enklare det gjorde det att testa och underhålla koden. Problemen med JSON och den logiska buggen i uppgiften var frustrerande, men också en bra lärdom i felsökning och att våga ifrågasätta kraven.

### 4.7 Slutsats kurs

Kursen var ett stort steg upp från grunderna. Att gå från att skriva all kod i en enda fil (`Program.cs`) till att designa en hel lösning med klassbibliotek, interface och filhantering var både utmanande och givande. Jag känner mig nu mycket mer bekväm med att strukturera kod på ett professionellt och hållbart sätt.
```
