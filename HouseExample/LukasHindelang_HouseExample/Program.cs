HindelangHaus house1 = new(16, 0.25, 8);
HindelangHaus house2 = new(12, 0.18, 6);

house1.Räume.Add(new Raum { LichtAn = true, Temperatur = 21 });
house1.Räume.Add(new Raum { LichtAn = false, Temperatur = 19 });
house1.Räume.Add(new Raum { LichtAn = true, Temperatur = 22 });
house1.Räume.Add(new Raum { LichtAn = false, Temperatur = 18 });

house1.name = "Hindelang Haus 1";
house1.nummer = 1;
house2.name = "Hindelang Haus 2";
house2.nummer = 2;

house1.HauptstromAn = true;
house1.HeizungAn = true;
house2.HauptstromAn = true;

Console.WriteLine($"{house1.name} {house1.nummer}");
Console.WriteLine($"Strom: {house1.Stromverbrauch} A, Wasser: {house1.Wasserverbrauch} m³/h, Heizung: {house1.Heizungsleistung} kW");
Console.WriteLine($"Wasser in 3 Stunden: {house1.BerechneWasserverbrauch(TimeSpan.FromHours(3))} m³");
Console.WriteLine($"{house2.name} {house2.nummer}");
Console.WriteLine($"Strom: {house2.Stromverbrauch} A, Wasser: {house2.Wasserverbrauch} m³/h, Heizung: {house2.Heizungsleistung} kW");

for (int i = 0; i < house1.Räume.Count; i++)
{
    IRaum raum = house1.Räume[i];
    Console.WriteLine($"Raum {i + 1}: Licht {(raum.LichtAn ? "An" : "Aus")}, Temperatur {raum.Temperatur} °C");
}