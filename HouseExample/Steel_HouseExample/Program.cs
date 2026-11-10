var haus = new Wohnhaus();

haus.AddRaum(new Raum(true, 21m));
haus.AddRaum(new RaumMitJalousie(true, 19m, false));
haus.AddRaum(new RaumMitWasser(true, 22m, 120m));
haus.AddRaum(new RaumMitJalousie(false, 18m, false));

var pool = new Pool(1.75m);
pool.VerbraucheWasser(500m);
haus.AddPool(pool);
Console.WriteLine(
    $"Pool: {pool.FlächeQuadratmeter} m² Fläche, {pool.TiefeMeter} m Tiefe, " +
    $"Fassungsvermögen {pool.FassungsvermögenKubikmeter} m³ ({pool.FassungsvermögenLiter} l).");

var garage = new Garage();
garage.VerbraucheWasser(30m);
haus.AddGarage(garage);
Console.WriteLine(
    $"Garage: {garage.Stellplätze} Stellplätze, Wasseranschluss: " +
    $"{(garage.HatWasseranschluss ? "Ja" : "Nein")}, Wasserverbrauch: {garage.Wasserverbrauch} l.");

Console.WriteLine($"Das Haus hat {haus.Räume.Count} Räume:");
foreach (var raum in haus.Räume)
{
    var lichtzustand = raum.LichtAn ? "An" : "Aus";
    Console.WriteLine($"Licht: {lichtzustand}, Temperatur: {raum.Temperatur} °C");

    if (raum is IRaumJalousie raumMitJalousie)
    {
        Console.WriteLine($"Jalousie geschlossen: {raumMitJalousie.JalousieGeschlossen}");
    }

    if (raum is IRaumNass raumMitWasser)
    {
        Console.WriteLine($"Wasserverbrauch: {raumMitWasser.Wasserverbrauch} l");
    }
}

haus.AusschaltenOderHerunterfahren();
Console.WriteLine("Nach dem Ausschalten/Herunterfahren:");
foreach (var raum in haus.Räume)
{
    var jalousieStatus = raum is IRaumJalousie raumMitJalousie
        ? $", Jalousie geschlossen: {raumMitJalousie.JalousieGeschlossen}"
        : string.Empty;

    Console.WriteLine($"Licht: {(raum.LichtAn ? "An" : "Aus")}{jalousieStatus}");
}

var garten = new Garten();
var apfelbaum = new Obstbaum("Apfelbaum");
var kirschbaum = new Obstbaum("Kirschbaum");
var zwetschgenbaum = new Obstbaum("Zwetschgenbaum");
var rose = new Zierpflanze("Rose");
var margerite = new Zierpflanze("Margerite");
var vergissmeinnicht = new Zierpflanze("Vergissmeinnicht");
garten.AddObstbaum(apfelbaum);
garten.AddObstbaum(kirschbaum);
garten.AddObstbaum(zwetschgenbaum);
garten.AddZierpflanze(rose);
garten.AddZierpflanze(margerite);
garten.AddZierpflanze(vergissmeinnicht);
garten.Kräutergarten.AddKraut(new Kraut("Basilikum"));
garten.Kräutergarten.AddKraut(new Kraut("Petersilie"));
garten.Kräutergarten.AddKraut(new Kraut("Lavendel"));
garten.Kräutergarten.AddKraut(new Kraut("Dill"));
garten.Kräutergarten.AddKraut(new Kraut("Melisse"));
garten.Kräutergarten.AddKraut(new Kraut("Oregano"));
garten.Kräutergarten.AddKraut(new Kraut("Schnittlauch"));
garten.Kräutergarten.AddKraut(new Kraut("Thymian"));
garten.BewässereAlle(10m);
haus.AddGarten(garten);

Console.WriteLine(
    $"Garten: {garten.Obstbäume.Count} Obstbäume, {garten.Zierpflanzen.Count} Zierpflanzen " +
    $"und {garten.Kräutergarten.Kräuter.Count} Kräuter.");
Console.WriteLine(
    $"Bewässerung: {apfelbaum.Name} {apfelbaum.Wasserverbrauch} l, " +
    $"{kirschbaum.Name} {kirschbaum.Wasserverbrauch} l, {rose.Name} {rose.Wasserverbrauch} l, " +
    $"{margerite.Name} {margerite.Wasserverbrauch} l, " +
    $"{vergissmeinnicht.Name} {vergissmeinnicht.Wasserverbrauch} l.");
Console.WriteLine(
    $"Kräutergarten: {string.Join(", ", garten.Kräutergarten.Kräuter.Select(kraut => kraut.Name))}, " +
    $"Wasserverbrauch: {garten.Kräutergarten.Wasserverbrauch} l.");
Console.WriteLine($"Wasserverbrauch Garten: {garten.Wasserverbrauch} l");

var scheune = new Scheune();
scheune.TränkeAlle(5m);
scheune.FüttereAlle(2m);
haus.AddScheune(scheune);

Console.WriteLine(
    $"Scheune: {scheune.Schwarznasenschafe.Count} Schwarznasenschafe, " +
    $"{scheune.Ponys.Count} Ponys und {scheune.Hochlandrinder.Count} Hochlandrinder.");
Console.WriteLine($"Alle {scheune.Koppel.Tiere.Count} Tiere sind gemeinsam auf der Koppel.");
Console.WriteLine(
    $"Verbrauch Scheune: {scheune.Wasserverbrauch} l Wasser und " +
    $"{scheune.Futterverbrauch} kg Futter.");
Console.WriteLine($"Wasserverbrauch Haus gesamt: {haus.WasserverbrauchGesamt} l");
Console.WriteLine($"Futterverbrauch Haus gesamt: {haus.FutterverbrauchGesamt} kg");
