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
        private G5_Bauwerkzeug _aktuellesWerkzeug;
        private string _statusMeldung;

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
                    .Select(building => (IG5_GebaeudeInfo)new G5_GebaeudeInfo(building))
                    .ToArray();
                return new ReadOnlyCollection<IG5_GebaeudeInfo>(gebaeude);
            }
        }

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

        internal void AddGebaeude(G5_Building gebaeude)
        {
            _simulation.AddBuilding(gebaeude);
        }
    }

    internal sealed class G5_GebaeudeInfo : IG5_GebaeudeInfo
    {
        private readonly G5_Building _gebaeude;

        internal G5_GebaeudeInfo(G5_Building gebaeude)
        {
            _gebaeude = gebaeude ?? throw new ArgumentNullException(nameof(gebaeude));
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
    }
}
