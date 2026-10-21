Haus meinHaus = new Haus(
    stromverbrauch: 16,
    wasserverbrauch: 0.2,
    heizungsleistung: 8);

meinHaus.HauptstromschalterAn = true;
meinHaus.HeizungAn = true;
meinHaus.Räume[0].LichtAn = true;
meinHaus.Räume[0].Temperatur = 21;

Console.WriteLine($"Stromverbrauch: {meinHaus.Stromverbrauch} A");
Console.WriteLine($"Wasserverbrauch: {meinHaus.Wasserverbrauch:F2} m³/h");
Console.WriteLine($"Heizungsleistung: {meinHaus.Heizungsleistung} kW");
Console.WriteLine(
    $"Wasserverbrauch in 3 Stunden: {meinHaus.BerechneWasserverbrauch(TimeSpan.FromHours(3)):F2} m³");
Console.WriteLine($"Anzahl der Räume: {meinHaus.Räume.Count}");
Console.WriteLine(
    $"Raum 1: Licht {(meinHaus.Räume[0].LichtAn ? "An" : "Aus")}, {meinHaus.Räume[0].Temperatur} °C");
