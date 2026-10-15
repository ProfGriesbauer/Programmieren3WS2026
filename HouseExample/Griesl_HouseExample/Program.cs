// See https://aka.ms/new-console-template for more information
GrieslsHaus house1 = new GrieslsHaus();
GrieslsHaus house2 = new GrieslsHaus();
house1.name = "GrieslHaus1";
house1.hausnummer = 1;
house2.name = "GrieslHaus2";
house2.hausnummer = 2;
object objh = house1;

house1.RaumUmbenennen(0, "Wohnzimmer");
house1.RaumUmbenennen(1, "Schlafzimmer");
house1.RaumUmbenennen(2, "Küche");
house1.RaumUmbenennen(3, "Badezimmer");

Raum arbeitszimmer = new Raum();
arbeitszimmer.Name = "Arbeitszimmer";
house1.RaumHinzufuegen(arbeitszimmer);

Bad gaestebad = new Bad();
gaestebad.Name = "Gästebad";
gaestebad.WasserAn = true;
house1.RaumHinzufuegen(gaestebad);
Console.WriteLine($"Wasserverbrauch der Räume: {house1.WasserverbrauchRaeume()} m³/h");



house1.HauptstromAn = true;

if (house1.Stromverbrauch > 0)
{
    Console.WriteLine("Stromverbrauch ist größer als 0");
}
else
{
    Console.WriteLine("Stromverbrauch ist kleiner oder gleich 0");
}

Console.WriteLine("Hello, World!");
Console.WriteLine(house1.ToString());
Console.WriteLine($"Haus2: {house2.name}, Hausnummer: {house2.hausnummer}");
