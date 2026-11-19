using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{
    internal sealed class G5_FabrikSpielfeld : IG5_FabrikSpielInfo, IG5_KartenansichtSteuerung
    {
        private readonly G5_WeltSpielfeld _welt;
        private readonly G5_FactorySimulation _simulation;
        private readonly G5_Storage _hub;
        private readonly List<IReadOnlyList<G5_GridPosition>> _foerderbandRouten =
            new List<IReadOnlyList<G5_GridPosition>>();
        private G5_Bauwerkzeug _aktuellesWerkzeug;
        private string _statusMeldung;
        private bool _foerderbaenderAktiv;
        private G5_Building _ausgewaehlterAusgang;
        private G5_Building _ausgewaehltesGebaeude;

        internal G5_FabrikSpielfeld(G5_WeltSpielfeld welt, int hubX, int hubY)
        {
            _welt = welt ?? throw new ArgumentNullException(nameof(welt));
            _simulation = new G5_FactorySimulation();
            _hub = new G5_Storage(new G5_GridPosition(hubX, hubY), 500);
            _simulation.AddBuilding(_hub);
            _aktuellesWerkzeug = G5_Bauwerkzeug.Keines;
            _statusMeldung = "F wählt Förderer, S wählt Schmelze. Danach auf die Karte klicken.";
        }

        internal G5_WeltSpielfeld Welt => _welt;
        internal G5_FactorySimulation Simulation => _simulation;
        internal G5_Storage Hub => _hub;
        internal IReadOnlyList<G5_Building> InterneGebaeude => _simulation.Buildings;

        int IG5_WeltSpielfeld.Breite => ((IG5_WeltSpielfeld)_welt).Breite;

        int IG5_WeltSpielfeld.Hoehe => ((IG5_WeltSpielfeld)_welt).Hoehe;

        bool IG5_WeltSpielfeld.TryGetKachelInfo(int x, int y, out IG5_KachelInfo info)
        {
            return ((IG5_WeltSpielfeld)_welt).TryGetKachelInfo(x, y, out info);
        }

        bool IG5_WeltSpielfeld.TryBildschirmZuRaster(int screenX, int screenY, out int x, out int y)
        {
            return ((IG5_WeltSpielfeld)_welt).TryBildschirmZuRaster(screenX, screenY, out x, out y);
        }

        bool IG5_WeltSpielfeld.TryRasterZuBildschirm(int x, int y, out System.Windows.Point position)
        {
            return ((IG5_WeltSpielfeld)_welt).TryRasterZuBildschirm(x, y, out position);
        }

        G5_Bauwerkzeug IG5_FabrikSpielInfo.AktuellesWerkzeug => _aktuellesWerkzeug;

        IReadOnlyList<IG5_GebaeudeInfo> IG5_FabrikSpielInfo.Gebaeude
        {
            get
            {
                IG5_GebaeudeInfo[] gebaeude = _simulation.Buildings
                    .Select(building => (IG5_GebaeudeInfo)new G5_GebaeudeInfo(
                        building,
                        ReferenceEquals(building, _ausgewaehlterAusgang),
                        ReferenceEquals(building, _ausgewaehltesGebaeude)))
                    .ToArray();
                return new ReadOnlyCollection<IG5_GebaeudeInfo>(gebaeude);
            }
        }

        bool IG5_FabrikSpielInfo.FoerderbaenderAktiv => _foerderbaenderAktiv;

        IReadOnlyList<IReadOnlyList<G5_GridPosition>> IG5_FabrikSpielInfo.FoerderbandRouten =>
            _foerderbandRouten.AsReadOnly();

        string IG5_FabrikSpielInfo.StatusMeldung => _statusMeldung;

        int IG5_FabrikSpielInfo.HubMenge(G5_Item item)
        {
            return _hub.CountOf(item);
        }

        bool IGameField.CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG5_FabrikPainter;
        }

        void IG5_KartenansichtSteuerung.Verschieben(double deltaX, double deltaY)
        {
            ((IG5_KartenansichtSteuerung)_welt).Verschieben(deltaX, deltaY);
        }

        void IG5_KartenansichtSteuerung.Zoomieren(double faktor, double fokusX, double fokusY)
        {
            ((IG5_KartenansichtSteuerung)_welt).Zoomieren(faktor, fokusX, fokusY);
        }

        void IG5_KartenansichtSteuerung.SetzeAnsicht(double zoom, double verschiebungX, double verschiebungY)
        {
            ((IG5_KartenansichtSteuerung)_welt).SetzeAnsicht(zoom, verschiebungX, verschiebungY);
        }

        internal bool TryGetKachel(int x, int y, out G5_Kachel kachel)
        {
            return _welt.TryGetKachel(x, y, out kachel);
        }

        internal void SetWerkzeug(G5_Bauwerkzeug werkzeug)
        {
            _aktuellesWerkzeug = werkzeug;
        }

        internal void SetStatusMeldung(string meldung)
        {
            _statusMeldung = meldung ?? string.Empty;
        }

        internal void SetFoerderbaenderAktiv(bool aktiv)
        {
            _foerderbaenderAktiv = aktiv;
        }

        internal void SetAusgewaehltenAusgang(G5_Building gebaeude)
        {
            _ausgewaehlterAusgang = gebaeude;
        }

        internal void SetAusgewaehltesGebaeude(G5_Building gebaeude)
        {
            _ausgewaehltesGebaeude = gebaeude;
        }

        internal void AddFoerderbandRoute(IReadOnlyList<G5_GridPosition> route)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            _foerderbandRouten.Add(new ReadOnlyCollection<G5_GridPosition>(route.ToList()));
        }

        internal void AddGebaeude(G5_Building gebaeude)
        {
            _simulation.AddBuilding(gebaeude);
        }
    }

    internal sealed class G5_GebaeudeInfo : IG5_GebaeudeInfo
    {
        private readonly G5_Building _gebaeude;
        private static readonly IReadOnlyDictionary<G5_Item, int> LeererEingang =
            new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());
        private readonly bool _ausgangAusgewaehlt;
        private readonly bool _gebaeudeAusgewaehlt;

        internal G5_GebaeudeInfo(
            G5_Building gebaeude,
            bool ausgangAusgewaehlt,
            bool gebaeudeAusgewaehlt)
        {
            _gebaeude = gebaeude ?? throw new ArgumentNullException(nameof(gebaeude));
            _ausgangAusgewaehlt = ausgangAusgewaehlt;
            _gebaeudeAusgewaehlt = gebaeudeAusgewaehlt;
        }

        G5_FabrikGebaeudeTyp IG5_GebaeudeInfo.Typ
        {
            get
            {
                if (_gebaeude is G5_Storage)
                {
                    return G5_FabrikGebaeudeTyp.Hub;
                }

                if (_gebaeude is G5_Miner)
                {
                    return G5_FabrikGebaeudeTyp.Foerderer;
                }

                if (_gebaeude is G5_Smelter)
                {
                    return G5_FabrikGebaeudeTyp.Schmelze;
                }

                if (_gebaeude is G5_Constructor)
                {
                    return G5_FabrikGebaeudeTyp.Konstruktor;
                }

                if (_gebaeude is G5_Assembler)
                {
                    return G5_FabrikGebaeudeTyp.Assembler;
                }

                if (_gebaeude is G5_BurningGenerator)
                {
                    return G5_FabrikGebaeudeTyp.Verbrennungsgenerator;
                }

                if (_gebaeude is G5_PowerPole)
                {
                    return G5_FabrikGebaeudeTyp.Strommast;
                }

                if (_gebaeude is G5_Conveyor)
                {
                    return G5_FabrikGebaeudeTyp.Laufband;
                }

                throw new InvalidOperationException("Unbekannter Gebäudetyp in der Fabrikansicht.");
            }
        }

        string IG5_GebaeudeInfo.Name => _gebaeude.Name;
        int IG5_GebaeudeInfo.X => _gebaeude.Position.X;
        int IG5_GebaeudeInfo.Y => _gebaeude.Position.Y;
        int IG5_GebaeudeInfo.Breite => _gebaeude.Width;
        int IG5_GebaeudeInfo.Hoehe => _gebaeude.Height;
        int IG5_GebaeudeInfo.Level => _gebaeude.Level;
        G5_BuildingStatus IG5_GebaeudeInfo.Status => _gebaeude.Status;
        bool IG5_GebaeudeInfo.IstMitStromVersorgt => _gebaeude.IsPowered;
        bool IG5_GebaeudeInfo.HatEingang =>
            _gebaeude is G5_Storage || _gebaeude is G5_ProductionBuilding;
        bool IG5_GebaeudeInfo.HatAusgang =>
            _gebaeude is G5_Storage || _gebaeude is G5_Miner || _gebaeude is G5_ProductionBuilding;
        G5_GridPosition IG5_GebaeudeInfo.EingangsPosition =>
            new G5_GridPosition(_gebaeude.Position.X, _gebaeude.Position.Y + _gebaeude.Height / 2);
        G5_GridPosition IG5_GebaeudeInfo.AusgangsPosition =>
            new G5_GridPosition(_gebaeude.Position.X + _gebaeude.Width - 1,
                _gebaeude.Position.Y + _gebaeude.Height / 2);
        bool IG5_GebaeudeInfo.AusgangAusgewaehlt => _ausgangAusgewaehlt;
        bool IG5_GebaeudeInfo.IstAusgewaehlt => _gebaeudeAusgewaehlt;
        IReadOnlyDictionary<G5_Item, int> IG5_GebaeudeInfo.EingangsInhalt
        {
            get
            {
                if (_gebaeude is G5_ProductionBuilding productionBuilding)
                {
                    return productionBuilding.InputContents;
                }

                if (_gebaeude is G5_Storage storage)
                {
                    return storage.Contents;
                }

                if (_gebaeude is G5_Conveyor conveyor)
                {
                    return conveyor.Contents;
                }

                return LeererEingang;
            }
        }

        IReadOnlyList<string> IG5_GebaeudeInfo.VerfuegbareRezepte =>
            _gebaeude is G5_ProductionBuilding productionBuilding
                ? productionBuilding.Recipes.Select(recipe => recipe.Name).ToArray()
                : Array.Empty<string>();

        string IG5_GebaeudeInfo.AktuellesRezept =>
            (_gebaeude as G5_ProductionBuilding)?.SelectedRecipe?.Name ?? string.Empty;
    }
}
