using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public sealed class G5_FabrikRules : IGameRules2
    {
        private const int KartenBreite = 16;
        private const int KartenHoehe = 10;
        private const int HudHoehe = 36;
        private const int HubX = 1;
        private const int HubY = 1;
        private const int GebaeudeBreite = 2;
        private const int GebaeudeHoehe = 2;
        private const int FoerdererZyklusTicks = 35;
        private const int SchmelzZyklusTicks = 55;

        private readonly Dictionary<G5_GridPosition, G5_ResourceDeposit> _vorkommen =
            new Dictionary<G5_GridPosition, G5_ResourceDeposit>();
        private G5_FabrikSpielfeld _feld;

        public G5_FabrikRules()
        {
            ClearField();
        }

        public string Name => "G5 Einfaches Fabrikspiel";

        public IGameField CurrentField => _feld;

        public bool MovesPossible => _feld != null;

        public void StartedGameCall()
        {
        }

        public void TickGameCall()
        {
            if (_feld == null)
            {
                return;
            }

            List<G5_Miner> miner = _feld.InterneGebaeude.OfType<G5_Miner>().ToList();
            Dictionary<G5_Miner, long?> vorkommenVorTick = miner.ToDictionary(
                foerderer => foerderer,
                foerderer => foerderer.RemainingDeposit);

            _feld.Simulation.Tick();
            AktualisiereKachelVorkommen(miner, vorkommenVorTick);
            TransportiereZumHubUndZurSchmelze();
        }

        public void DoMove(IPlayMove move)
        {
            if (_feld == null)
            {
                return;
            }

            if (!(move is IG5_FabrikAktion aktion))
            {
                _feld.SetStatusMeldung("Eingabe konnte nicht verarbeitet werden.");
                return;
            }

            if (aktion.Aktionstyp == G5_FabrikAktionstyp.WerkzeugWaehlen)
            {
                WaehleWerkzeug(aktion.Werkzeug);
                return;
            }

            if (aktion.Aktionstyp == G5_FabrikAktionstyp.GebaeudePlatzieren)
            {
                PlatziereGebaeude(aktion.X, aktion.Y);
                return;
            }

            _feld.SetStatusMeldung("Unbekannte Aktion.");
        }

        public void ClearField()
        {
            _vorkommen.Clear();

            G5_WeltSpielfeld welt = new G5_WeltSpielfeld(KartenBreite, KartenHoehe);
            welt.VerschiebeAnsicht(0, HudHoehe);
            _feld = new G5_FabrikSpielfeld(welt, HubX, HubY);

            FuegeVorkommenHinzu(5, 2, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, 100);
            FuegeVorkommenHinzu(12, 6, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, 100);
            welt.TrySetGelaende(8, 4, G5_Gelaendeart.Hindernis);
            welt.TrySetGelaende(8, 5, G5_Gelaendeart.Hindernis);
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        private void WaehleWerkzeug(G5_Bauwerkzeug werkzeug)
        {
            if (!Enum.IsDefined(typeof(G5_Bauwerkzeug), werkzeug))
            {
                _feld.SetStatusMeldung("Unbekanntes Bauwerkzeug.");
                return;
            }

            _feld.SetWerkzeug(werkzeug);
            _feld.SetStatusMeldung(werkzeug == G5_Bauwerkzeug.Keines
                ? "Bauwerkzeug abgewählt."
                : werkzeug + " ausgewählt. Klicke auf eine passende freie Fläche.");
        }

        private void PlatziereGebaeude(int x, int y)
        {
            G5_Bauwerkzeug werkzeug = ((IG5_FabrikSpielInfo)_feld).AktuellesWerkzeug;
            if (werkzeug == G5_Bauwerkzeug.Keines)
            {
                _feld.SetStatusMeldung("Wähle zuerst F für Förderer oder S für Schmelze.");
                return;
            }

            if (!IstFlaecheBebaubar(x, y))
            {
                _feld.SetStatusMeldung("Dort ist nicht genug freie, bebaubare Fläche.");
                return;
            }

            G5_Building gebaeude;
            if (werkzeug == G5_Bauwerkzeug.Foerderer)
            {
                if (!_vorkommen.TryGetValue(new G5_GridPosition(x, y), out G5_ResourceDeposit vorkommen) ||
                    vorkommen.RemainingAmount == 0)
                {
                    _feld.SetStatusMeldung("Ein Förderer muss direkt auf einem festen Rohstoffvorkommen stehen.");
                    return;
                }

                gebaeude = new G5_Miner(
                    new G5_GridPosition(x, y),
                    vorkommen,
                    outputCapacity: 20,
                    cycleTicks: FoerdererZyklusTicks,
                    amountPerCycle: 1,
                    powerDemandPerTick: 0);
            }
            else if (werkzeug == G5_Bauwerkzeug.Schmelze)
            {
                if (EnthaeltRohstoff(x, y, GebaeudeBreite, GebaeudeHoehe))
                {
                    _feld.SetStatusMeldung("Eine Schmelze kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                G5_Smelter schmelze = new G5_Smelter(
                    new G5_GridPosition(x, y),
                    G5_StarterRecipes.CreateSmeltingRecipes(SchmelzZyklusTicks, powerDemandPerTick: 0),
                    inputCapacity: 20,
                    outputCapacity: 20);
                schmelze.SelectRecipe("Eisenerz schmelzen");
                gebaeude = schmelze;
            }
            else
            {
                _feld.SetStatusMeldung("Dieses Bauwerkzeug wird noch nicht unterstützt.");
                return;
            }

            if (UeberlapptMitGebaeude(x, y, gebaeude.Width, gebaeude.Height))
            {
                _feld.SetStatusMeldung("Gebäude dürfen sich nicht überlappen.");
                return;
            }

            _feld.AddGebaeude(gebaeude);
            _feld.SetStatusMeldung(gebaeude.Name + " gebaut. Gebäude sind in diesem Prototyp kostenlos.");
        }

        private bool IstFlaecheBebaubar(int x, int y)
        {
            IG5_WeltSpielfeld weltInfo = _feld;
            if (x < 0 || y < 0 ||
                x + GebaeudeBreite > weltInfo.Breite ||
                y + GebaeudeHoehe > weltInfo.Hoehe)
            {
                return false;
            }

            for (int tileY = y; tileY < y + GebaeudeHoehe; tileY++)
            {
                for (int tileX = x; tileX < x + GebaeudeBreite; tileX++)
                {
                    if (!_feld.TryGetKachel(tileX, tileY, out G5_Kachel kachel) ||
                        ((IG5_KachelInfo)kachel).Gelaende != G5_Gelaendeart.Begehbar)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool EnthaeltRohstoff(int x, int y, int breite, int hoehe)
        {
            for (int tileY = y; tileY < y + hoehe; tileY++)
            {
                for (int tileX = x; tileX < x + breite; tileX++)
                {
                    if (_feld.TryGetKachel(tileX, tileY, out G5_Kachel kachel) &&
                        ((IG5_KachelInfo)kachel).Rohstoff != G5_Rohstofftyp.Keiner)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool UeberlapptMitGebaeude(int x, int y, int breite, int hoehe)
        {
            foreach (G5_Building vorhanden in _feld.InterneGebaeude)
            {
                bool ueberlappt =
                    x < vorhanden.Position.X + vorhanden.Width &&
                    vorhanden.Position.X < x + breite &&
                    y < vorhanden.Position.Y + vorhanden.Height &&
                    vorhanden.Position.Y < y + hoehe;

                if (ueberlappt)
                {
                    return true;
                }
            }

            return false;
        }

        private void FuegeVorkommenHinzu(
            int x,
            int y,
            G5_Rohstofftyp rohstoff,
            G5_Item item,
            int menge)
        {
            G5_GridPosition position = new G5_GridPosition(x, y);
            G5_ResourceDeposit vorkommen = new G5_ResourceDeposit(item, menge);
            _vorkommen.Add(position, vorkommen);
            _feld.Welt.TrySetRohstoff(x, y, rohstoff, menge);
        }

        private void AktualisiereKachelVorkommen(
            IReadOnlyList<G5_Miner> miner,
            IReadOnlyDictionary<G5_Miner, long?> vorkommenVorTick)
        {
            foreach (G5_Miner foerderer in miner)
            {
                long? vorher = vorkommenVorTick[foerderer];
                long? nachher = foerderer.RemainingDeposit;
                if (!vorher.HasValue || !nachher.HasValue || vorher.Value <= nachher.Value)
                {
                    continue;
                }

                int abgebaut = checked((int)(vorher.Value - nachher.Value));
                _feld.Welt.TryReduceRohstoff(foerderer.Position.X, foerderer.Position.Y, abgebaut);
            }
        }

        private void TransportiereZumHubUndZurSchmelze()
        {
            foreach (G5_Miner foerderer in _feld.InterneGebaeude.OfType<G5_Miner>())
            {
                TransferiereAlleAusgaenge(foerderer);
            }

            foreach (G5_Smelter schmelze in _feld.InterneGebaeude.OfType<G5_Smelter>())
            {
                TransferiereAlleAusgaenge(schmelze);
            }

            foreach (G5_Smelter schmelze in _feld.InterneGebaeude.OfType<G5_Smelter>())
            {
                G5_Recipe rezept = schmelze.SelectedRecipe;
                if (rezept == null)
                {
                    continue;
                }

                foreach (KeyValuePair<G5_Item, int> zutat in rezept.Ingredients)
                {
                    G5_ItemTransfer.Transfer(_feld.Hub, schmelze, zutat.Key, zutat.Value);
                }
            }
        }

        private void TransferiereAlleAusgaenge(G5_Building quelle)
        {
            KeyValuePair<G5_Item, int>[] inhalt = quelle.TransferableContents.ToArray();
            foreach (KeyValuePair<G5_Item, int> stapel in inhalt)
            {
                G5_ItemTransfer.Transfer(quelle, _feld.Hub, stapel.Key, stapel.Value);
            }
        }
    }
}
