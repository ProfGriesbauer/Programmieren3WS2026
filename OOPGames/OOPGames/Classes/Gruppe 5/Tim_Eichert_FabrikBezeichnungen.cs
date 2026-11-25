using System;

namespace OOPGames
{
    internal static class G5_FabrikBezeichnungen
    {
        internal static string Gebaeude(G5_FabrikGebaeudeTyp typ)
        {
            switch (typ)
            {
                case G5_FabrikGebaeudeTyp.Hub: return "Lager";
                case G5_FabrikGebaeudeTyp.Foerderer: return "Förderer";
                case G5_FabrikGebaeudeTyp.Schmelze: return "Schmelze";
                case G5_FabrikGebaeudeTyp.Konstruktor: return "Konstruktor";
                case G5_FabrikGebaeudeTyp.Assembler: return "Montagewerk";
                case G5_FabrikGebaeudeTyp.Verbrennungsgenerator: return "Kohlekraftwerk";
                case G5_FabrikGebaeudeTyp.Strommast: return "Strommast";
                case G5_FabrikGebaeudeTyp.Lager: return "Lager";
                case G5_FabrikGebaeudeTyp.Laufband: return "Förderband";
                case G5_FabrikGebaeudeTyp.Splitter: return "Verteiler";
                case G5_FabrikGebaeudeTyp.Merger: return "Zusammenführer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(typ), typ, "Unbekannter Gebäudetyp.");
            }
        }

        internal static string Werkzeug(G5_Bauwerkzeug werkzeug)
        {
            switch (werkzeug)
            {
                case G5_Bauwerkzeug.Keines: return "Kein Werkzeug";
                case G5_Bauwerkzeug.Foerderer: return "Förderer";
                case G5_Bauwerkzeug.Schmelze: return "Schmelze";
                case G5_Bauwerkzeug.Foerderband: return "Förderband";
                case G5_Bauwerkzeug.GebaeudeVerschieben: return "Gebäude verschieben";
                case G5_Bauwerkzeug.Konstruktor: return "Konstruktor";
                case G5_Bauwerkzeug.Assembler: return "Montagewerk";
                case G5_Bauwerkzeug.Verbrennungsgenerator: return "Kohlekraftwerk";
                case G5_Bauwerkzeug.Strommast: return "Strommast";
                case G5_Bauwerkzeug.Lager: return "Lager";
                default:
                    throw new ArgumentOutOfRangeException(nameof(werkzeug), werkzeug, "Unbekanntes Bauwerkzeug.");
            }
        }

        internal static string Gegenstand(G5_Item gegenstand)
        {
            switch (gegenstand)
            {
                case G5_Item.Holz: return "Holz";
                case G5_Item.Eisenerz: return "Eisenerz";
                case G5_Item.Kupfererz: return "Kupfererz";
                case G5_Item.Kohle: return "Kohle";
                case G5_Item.Kalkstein: return "Kalkstein";
                case G5_Item.Quarz: return "Quarz";
                case G5_Item.Schwefel: return "Schwefel";
                case G5_Item.Uran: return "Uran";
                case G5_Item.Wasser: return "Wasser";
                case G5_Item.Gas: return "Gas";
                case G5_Item.Oel: return "Öl";
                case G5_Item.Eisenbarren: return "Eisenbarren";
                case G5_Item.Kupferbarren: return "Kupferbarren";
                case G5_Item.Eisenplatte: return "Eisenplatte";
                case G5_Item.Eisenstange: return "Eisenstange";
                case G5_Item.Beton: return "Beton";
                case G5_Item.Kupferdraht: return "Kupferdraht";
                case G5_Item.Schraube: return "Schraube";
                case G5_Item.Rotor: return "Rotor";
                default:
                    throw new ArgumentOutOfRangeException(nameof(gegenstand), gegenstand, "Unbekannter Gegenstand.");
            }
        }
    }
}
