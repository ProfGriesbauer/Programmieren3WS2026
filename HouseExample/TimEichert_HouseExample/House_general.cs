using System;

namespace TimEichert_HouseExample;

public class House_general
{
	public string farbe = string.Empty;
	public string strasse = string.Empty;
	public int hausnummer;
    public string name = string.Empty;
    public Stockwerke stockwerke = new Stockwerke();

	public House_general(string name, string farbe, string strasse, int hausnummer)
	{
		this.name = name;
		this.farbe = farbe;
		this.strasse = strasse;
		this.hausnummer = hausnummer;
	}

	public Stockwerk StockwerkHinzufuegen(int nummer)
	{
		return stockwerke.StockwerkHinzufuegen(nummer);
	}

	public void HausAnzeigen()
	{
		Console.WriteLine($"Haus: {name}");
		Console.WriteLine($"Farbe: {farbe}");
		Console.WriteLine($"Adresse: {strasse} {hausnummer}");
		stockwerke.Anzeigen();
		Console.WriteLine($"Lichtleistung im Haus: {stockwerke.LichtleistungBerechnen():0.##} W");
	}
}
