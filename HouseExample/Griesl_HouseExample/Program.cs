// See https://aka.ms/new-console-template for more information
GrieslsHaus house1 = new GrieslsHaus();
GrieslsHaus house2 = new GrieslsHaus();
house1.name = "GrieslHaus1";
house1.hausnummer = 1;
house2.name = "GrieslHaus2";
house2.hausnummer = 2;
object objh = house1;

house1.HauptstromAn = true;
house1.Wasserverbrauch = 1.0;
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
