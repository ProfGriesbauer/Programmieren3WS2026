using Maxi_HouseExample;

var haus = new Haus(
    strom: 12,
    wasser: 0.15,
    heizleistung: 8);

Console.WriteLine($"Stromverbrauch: {haus.StromverbrauchAmpere} A");
Console.WriteLine($"Wasserverbrauch: {haus.WasserverbrauchKubikmeterProStunde} m³/h");
Console.WriteLine($"Heizungsleistung: {haus.HeizungsleistungKilowatt} kW");
Console.WriteLine($"Wassermenge in 3 Stunden: {haus.BerechneWasserverbrauch(TimeSpan.FromHours(3))} m³");

haus.Bad.An = true;
haus.Wohnzimmer.An = true;
haus.Sauna.An = true;
haus.WohnzimmerFenster.Oeffnen();
haus.Haustuer.Oeffnen();
Console.WriteLine($"Bad: {(haus.Bad.An ? "An" : "Aus")}");
Console.WriteLine($"Wohnzimmer: {(haus.Wohnzimmer.An ? "An" : "Aus")}");
Console.WriteLine($"Sauna: {(haus.Sauna.An ? "An" : "Aus")}");
Console.WriteLine($"Wohnzimmerfenster: {(haus.WohnzimmerFenster.Offen ? "Offen" : "Geschlossen")}");
Console.WriteLine($"Haustür: {(haus.Haustuer.Offen ? "Offen" : "Geschlossen")}");

haus.WasserAn = false;
Console.WriteLine($"Bad nach Abschalten des Wassers: {(haus.Bad.An ? "An" : "Aus")}");
haus.WasserAn = true;

haus.HauptstromschalterAn = false;
Console.WriteLine($"Stromverbrauch nach Ausschalten: {haus.StromverbrauchAmpere} A");
Console.WriteLine($"Heizung nach Ausschalten: {(haus.HeizungAn ? "An" : "Aus")}");
Console.WriteLine($"Wohnzimmer nach Ausschalten der Heizung: {(haus.Wohnzimmer.An ? "An" : "Aus")}");
Console.WriteLine($"Sauna nach Ausschalten der Heizung: {(haus.Sauna.An ? "An" : "Aus")}");
