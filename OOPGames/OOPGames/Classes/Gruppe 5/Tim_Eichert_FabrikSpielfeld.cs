using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{
    internal enum G5_Spielphase
    {
        Fabrik,
        Raketenstart,
        Gewonnen
    }

    internal sealed class G5_FabrikSpielfeld : IG5_FabrikSpielInfo, IG5_KartenansichtSteuerung
    {
        internal const int SpielerInventarKapazitaetMax = 50;
        internal const int HandabbauCooldownTicks = 50;
        internal const int ManuellesHandwerkDauerTicks = 50;
        internal const int RaketenstartDauerTicks = 100;
        internal static readonly TimeSpan AbwurfLebensdauer = TimeSpan.FromMinutes(2);

        private readonly G5_WeltSpielfeld _welt;
        private readonly G5_FactorySimulation _simulation;
        private readonly Dictionary<G5_Item, int> _spielerInventar = new Dictionary<G5_Item, int>();
        private readonly Dictionary<G5_Item, long> _abgebauteRohstoffe = new Dictionary<G5_Item, long>();
        private readonly Dictionary<G5_Item, long> _hergestellteProdukte = new Dictionary<G5_Item, long>();
        private readonly List<AbgelegterGegenstand> _abgelegteGegenstaende =
            new List<AbgelegterGegenstand>();
        private G5_Bauwerkzeug _aktuellesWerkzeug;
        private G5_GridPosition _spielerPosition;
        private G5_SpielerBlickrichtung _spielerBlickrichtung;
        private G5_GridPosition? _ausgewaehltesGebaeude;
        private G5_Item? _ausgewaehlterAbwurfGegenstand;
        private string _mengeEingabe = "1";
        private bool _mengeEingabeWurdeGeaendert;
        private string _statusMeldung;
        private bool _baumenueGeoeffnet = true;
        private int _handabbauCooldownTicks;
        private G5_Recipe _laufendesHandwerk;
        private int _verbleibendeHandwerkTicks;
        private int _reservierteHandwerkKapazitaet;
        // Anzahl der Produkte, die nach dem laufenden Handwerk noch hergestellt werden sollen.
        private int _ausstehendeHandwerkAnzahl;
        private int _raketenstartTicks;

        internal G5_FabrikSpielfeld(G5_WeltSpielfeld welt, int spielerX, int spielerY)
        {
            _welt = welt ?? throw new ArgumentNullException(nameof(welt));
            _simulation = new G5_FactorySimulation();
            _simulation.RohstoffGefoerdert += ErfasseAbbau;
            _simulation.ProduktHergestellt += ErfasseProduktion;
            _spielerPosition = new G5_GridPosition(spielerX, spielerY);
            _spielerBlickrichtung = G5_SpielerBlickrichtung.Sueden;
            _statusMeldung = "WASD: bewegen | E: abbauen | Baumenü: Gebäude kaufen und am markierten Feld bauen.";
        }

        internal G5_WeltSpielfeld Welt => _welt;
        internal G5_FactorySimulation Simulation => _simulation;
        internal IReadOnlyList<G5_Building> InterneGebaeude => _simulation.Buildings;
        internal bool BaumenueGeoeffnet => _baumenueGeoeffnet;
        internal bool GebaeuderegisterGeoeffnet => _baumenueGeoeffnet;
        internal G5_Spielphase Spielphase { get; private set; }
        internal int RaketenstartTicks => _raketenstartTicks;
        internal bool StatistikGeoeffnet { get; private set; }
        internal int GebauteGebaeude { get; private set; }
        internal IReadOnlyDictionary<G5_Item, long> AbgebauteRohstoffe =>
            new ReadOnlyDictionary<G5_Item, long>(_abgebauteRohstoffe);
        internal IReadOnlyDictionary<G5_Item, long> HergestellteProdukte =>
            new ReadOnlyDictionary<G5_Item, long>(_hergestellteProdukte);

        internal void ErfasseAbbau(G5_Item item, int menge)
        {
            Zaehle(_abgebauteRohstoffe, item, menge);
        }

        internal void ErfasseProduktion(G5_Item item, int menge)
        {
            Zaehle(_hergestellteProdukte, item, menge);
        }

        private static void Zaehle(Dictionary<G5_Item, long> zaehler, G5_Item item, int menge)
        {
            if (menge <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(menge));
            }

            zaehler[item] = checked((zaehler.TryGetValue(item, out long wert) ? wert : 0) + menge);
        }

        internal void ErfasseGebaeudebau()
        {
            GebauteGebaeude = checked(GebauteGebaeude + 1);
        }

        internal void ToggleStatistik()
        {
            StatistikGeoeffnet = !StatistikGeoeffnet;
        }

        internal bool TryBetankeRakete(G5_Rakete rakete, out string meldung)
        {
            if (!IstAnRakete(rakete))
            {
                meldung = "Stelle dich direkt neben die ausgewählte Rakete.";
                return false;
            }

            int fehlend = G5_Rakete.UranBedarf - rakete.UranVorrat;
            if (fehlend == 0)
            {
                meldung = "Die Rakete ist bereits vollgetankt.";
                return false;
            }

            int menge = Math.Min(fehlend, Gesamtmenge(G5_Item.Uran));
            if (menge == 0)
            {
                meldung = "Zum Betanken wird Uran im Rucksack oder Lager benötigt.";
                return false;
            }

            if (!TryVerbraucheRessourcen(new Dictionary<G5_Item, int> { [G5_Item.Uran] = menge }))
            {
                throw new InvalidOperationException("Das Uran konnte nicht aus Rucksack und Lagern entnommen werden.");
            }

            rakete.Betanken(menge);
            meldung = "Rakete betankt: " + rakete.UranVorrat + "/" + G5_Rakete.UranBedarf + " Uran.";
            return true;
        }

        internal bool TryStarteRakete(G5_Rakete rakete, out string meldung)
        {
            if (!IstAnRakete(rakete))
            {
                meldung = "Stelle dich direkt neben die ausgewählte Rakete.";
                return false;
            }

            if (!rakete.Startbereit)
            {
                meldung = "Die Rakete benötigt " + G5_Rakete.UranBedarf + " Uran zum Start.";
                return false;
            }

            if (Spielphase != G5_Spielphase.Fabrik)
            {
                meldung = "Der Raketenstart läuft bereits.";
                return false;
            }

            Spielphase = G5_Spielphase.Raketenstart;
            StatistikGeoeffnet = false;
            meldung = "Du steigst ein. Der Raketenstart beginnt!";
            return true;
        }

        private bool IstAnRakete(G5_Rakete rakete)
        {
            return rakete != null && _simulation.Buildings.Contains(rakete) &&
                _spielerPosition.ManhattanDistanceTo(rakete.Position) == 1;
        }

        internal void TickRaketenstart()
        {
            if (Spielphase != G5_Spielphase.Raketenstart)
            {
                return;
            }

            _raketenstartTicks++;
            if (_raketenstartTicks >= RaketenstartDauerTicks)
            {
                Spielphase = G5_Spielphase.Gewonnen;
            }
        }

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

        IG5_GebaeudeInfo IG5_FabrikSpielInfo.AusgewaehltesGebaeude
        {
            get
            {
                if (!_ausgewaehltesGebaeude.HasValue)
                {
                    return null;
                }

                G5_Building building = _simulation.Buildings.FirstOrDefault(candidate =>
                    candidate.Position == _ausgewaehltesGebaeude.Value);
                return building == null ? null : new G5_GebaeudeInfo(building);
            }
        }

        G5_GridPosition IG5_FabrikSpielInfo.SpielerPosition => _spielerPosition;
        G5_GridPosition IG5_FabrikSpielInfo.ZielKachel => ZielKachel;
        G5_SpielerBlickrichtung IG5_FabrikSpielInfo.SpielerBlickrichtung => _spielerBlickrichtung;
        IReadOnlyDictionary<G5_Item, int> IG5_FabrikSpielInfo.SpielerInventar =>
            new ReadOnlyDictionary<G5_Item, int>(_spielerInventar);
        IReadOnlyList<G5_AbgelegterGegenstandInfo> IG5_FabrikSpielInfo.AbgelegteGegenstaende =>
            AbgelegteGegenstaende;
        int IG5_FabrikSpielInfo.SpielerInventarKapazitaet => SpielerInventarKapazitaetMax;
        int IG5_FabrikSpielInfo.SpielerInventarBelegt => InventarBelegt;
        bool IG5_FabrikSpielInfo.BaumenueGeoeffnet => _baumenueGeoeffnet;
        string IG5_FabrikSpielInfo.StatusMeldung => _statusMeldung;

        internal G5_GridPosition SpielerPosition => _spielerPosition;
        internal G5_Item? AusgewaehlterAbwurfGegenstand => _ausgewaehlterAbwurfGegenstand;
        internal string MengeEingabe => _mengeEingabe;

        internal IReadOnlyList<G5_AbgelegterGegenstandInfo> AbgelegteGegenstaende
        {
            get
            {
                DateTimeOffset jetzt = DateTimeOffset.UtcNow;
                return _abgelegteGegenstaende
                    .Where(gegenstand => gegenstand.Ablaufzeit > jetzt)
                    .Select(gegenstand => new G5_AbgelegterGegenstandInfo(
                        gegenstand.Gegenstand,
                        gegenstand.Anzahl,
                        gegenstand.Position.X,
                        gegenstand.Position.Y,
                        gegenstand.Ablaufzeit))
                    .ToArray();
            }
        }

        internal G5_GridPosition ZielKachel
        {
            get
            {
                int x = _spielerPosition.X;
                int y = _spielerPosition.Y;
                switch (_spielerBlickrichtung)
                {
                    case G5_SpielerBlickrichtung.Norden:
                        y--;
                        break;
                    case G5_SpielerBlickrichtung.Osten:
                        x++;
                        break;
                    case G5_SpielerBlickrichtung.Sueden:
                        y++;
                        break;
                    case G5_SpielerBlickrichtung.Westen:
                        x--;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new G5_GridPosition(x, y);
            }
        }

        int IG5_FabrikSpielInfo.HubMenge(G5_Item item)
        {
            int playerAmount = Menge(item);
            int storedAmount = _simulation.Buildings.OfType<G5_Storage>()
                .Sum(storage => storage.CountOf(item));
            return checked(playerAmount + storedAmount);
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

        internal bool MoveSpieler(int dx, int dy)
        {
            if (Math.Abs(dx) + Math.Abs(dy) != 1)
            {
                throw new ArgumentException("Spielerbewegungen müssen genau eine Kachel betragen.");
            }

            if (dx > 0) _spielerBlickrichtung = G5_SpielerBlickrichtung.Osten;
            else if (dx < 0) _spielerBlickrichtung = G5_SpielerBlickrichtung.Westen;
            else if (dy > 0) _spielerBlickrichtung = G5_SpielerBlickrichtung.Sueden;
            else _spielerBlickrichtung = G5_SpielerBlickrichtung.Norden;

            int zielX = _spielerPosition.X + dx;
            int zielY = _spielerPosition.Y + dy;
            if (!_welt.TryGetKachel(zielX, zielY, out G5_Kachel kachel) ||
                ((IG5_KachelInfo)kachel).Gelaende != G5_Gelaendeart.Begehbar ||
                _simulation.Buildings.Any(building =>
                    zielX >= building.Position.X &&
                    zielX < building.Position.X + building.Width &&
                    zielY >= building.Position.Y &&
                    zielY < building.Position.Y + building.Height))
            {
                return false;
            }

            _spielerPosition = new G5_GridPosition(zielX, zielY);
            return true;
        }

        internal bool SelectBuildingAt(int x, int y)
        {
            G5_Building building = _simulation.Buildings.FirstOrDefault(candidate =>
                x >= candidate.Position.X &&
                x < candidate.Position.X + candidate.Width &&
                y >= candidate.Position.Y &&
                y < candidate.Position.Y + candidate.Height);
            _ausgewaehltesGebaeude = building == null
                ? (G5_GridPosition?)null
                : building.Position;
            _ausgewaehlterAbwurfGegenstand = null;
            _mengeEingabe = "1";
            _mengeEingabeWurdeGeaendert = false;
            return building != null;
        }

        internal G5_Building AusgewaehltesInternesGebaeude
        {
            get
            {
                if (!_ausgewaehltesGebaeude.HasValue)
                {
                    return null;
                }

                return _simulation.Buildings.FirstOrDefault(building =>
                    building.Position == _ausgewaehltesGebaeude.Value);
            }
        }

        internal void DeselectBuilding()
        {
            _ausgewaehltesGebaeude = null;
        }

        internal int Menge(G5_Item item)
        {
            return _spielerInventar.TryGetValue(item, out int amount) ? amount : 0;
        }

        internal int InventarBelegt => _spielerInventar.Values.Sum();
        internal int VerbleibendeHandabbauCooldownTicks => _handabbauCooldownTicks;

        internal void WaehleAbwurfGegenstand(G5_Item gegenstand)
        {
            _ausgewaehlterAbwurfGegenstand = gegenstand;
            _mengeEingabe = "1";
            _mengeEingabeWurdeGeaendert = false;
        }

        internal void AendereMengeEingabe(string eingabe)
        {
            if (eingabe == "\b")
            {
                if (_mengeEingabe.Length > 0)
                {
                    _mengeEingabe = _mengeEingabe.Substring(
                        0, _mengeEingabe.Length - 1);
                }

                _mengeEingabeWurdeGeaendert = true;
                return;
            }

            if (eingabe == null || eingabe.Length != 1 || !char.IsDigit(eingabe[0]))
            {
                throw new ArgumentException("Die Eingabe muss eine einzelne Ziffer oder Rücktaste sein.", nameof(eingabe));
            }

            if (_mengeEingabe.Length >= 3)
            {
                return;
            }

            _mengeEingabe = _mengeEingabeWurdeGeaendert
                ? _mengeEingabe + eingabe
                : eingabe;
            _mengeEingabeWurdeGeaendert = true;
        }

        internal void BestaetigeMengeEingabe()
        {
            _mengeEingabeWurdeGeaendert = false;
        }

        internal int AnzahlAbgelegterGegenstaende(G5_GridPosition position, DateTimeOffset jetzt)
        {
            EntferneAbgelaufeneGegenstaende(jetzt);
            return _abgelegteGegenstaende
                .Where(gegenstand => gegenstand.Position == position)
                .Sum(gegenstand => gegenstand.Anzahl);
        }

        internal bool TryAblegen(G5_Item gegenstand, int anzahl, DateTimeOffset jetzt)
        {
            if (anzahl <= 0 || Menge(gegenstand) < anzahl)
            {
                return false;
            }

            int verbleibend = Menge(gegenstand) - anzahl;
            if (verbleibend == 0)
            {
                _spielerInventar.Remove(gegenstand);
            }
            else
            {
                _spielerInventar[gegenstand] = verbleibend;
            }

            _abgelegteGegenstaende.Add(new AbgelegterGegenstand(
                gegenstand,
                anzahl,
                _spielerPosition,
                jetzt + AbwurfLebensdauer));
            return true;
        }

        internal int NimmAbgelegteGegenstaendeAuf(G5_GridPosition position, DateTimeOffset jetzt)
        {
            EntferneAbgelaufeneGegenstaende(jetzt);
            int freieKapazitaet = SpielerInventarKapazitaetMax -
                InventarBelegt -
                _reservierteHandwerkKapazitaet;
            int aufgenommen = 0;

            foreach (AbgelegterGegenstand abgelegt in _abgelegteGegenstaende
                .Where(gegenstand => gegenstand.Position == position)
                .ToArray())
            {
                if (freieKapazitaet == 0)
                {
                    break;
                }

                int anzahl = Math.Min(abgelegt.Anzahl, freieKapazitaet);
                _spielerInventar[abgelegt.Gegenstand] =
                    checked(Menge(abgelegt.Gegenstand) + anzahl);
                abgelegt.Anzahl -= anzahl;
                freieKapazitaet -= anzahl;
                aufgenommen += anzahl;
            }

            _abgelegteGegenstaende.RemoveAll(gegenstand => gegenstand.Anzahl == 0);
            return aufgenommen;
        }

        internal void EntferneAbgelaufeneGegenstaende(DateTimeOffset jetzt)
        {
            _abgelegteGegenstaende.RemoveAll(gegenstand => gegenstand.Ablaufzeit <= jetzt);
        }

        private sealed class AbgelegterGegenstand
        {
            internal AbgelegterGegenstand(
                G5_Item gegenstand,
                int anzahl,
                G5_GridPosition position,
                DateTimeOffset ablaufzeit)
            {
                Gegenstand = gegenstand;
                Anzahl = anzahl;
                Position = position;
                Ablaufzeit = ablaufzeit;
            }

            internal G5_Item Gegenstand { get; }
            internal int Anzahl { get; set; }
            internal G5_GridPosition Position { get; }
            internal DateTimeOffset Ablaufzeit { get; }
        }

        internal int FreieAufnahmekapazitaet(G5_Storage ausgeschlossenesLager = null)
        {
            int freieRucksackKapazitaet = SpielerInventarKapazitaetMax -
                InventarBelegt -
                _reservierteHandwerkKapazitaet;
            int freieLagerKapazitaet = _simulation.Buildings
                .OfType<G5_Storage>()
                .Where(storage => !ReferenceEquals(storage, ausgeschlossenesLager))
                .Sum(storage => storage.Capacity - storage.UsedCapacity);
            return checked(freieRucksackKapazitaet + freieLagerKapazitaet);
        }

        internal bool KannEinlagern(int amount, G5_Storage ausgeschlossenesLager = null)
        {
            return amount >= 0 && amount <= FreieAufnahmekapazitaet(ausgeschlossenesLager);
        }

        internal bool KannEinlagern(
            IReadOnlyDictionary<G5_Item, int> items,
            G5_Storage ausgeschlossenesLager = null)
        {
            return items != null &&
                items.Values.All(amount => amount >= 0) &&
                items.Values.Sum() <= FreieAufnahmekapazitaet(ausgeschlossenesLager);
        }

        internal bool TryAddInventar(G5_Item item, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount == 0)
            {
                return true;
            }

            if (!KannEinlagern(amount))
            {
                return false;
            }

            int inRucksack = Math.Min(amount, SpielerInventarKapazitaetMax - InventarBelegt);
            if (inRucksack > 0)
            {
                _spielerInventar[item] = checked(Menge(item) + inRucksack);
            }

            int remaining = amount - inRucksack;
            foreach (G5_Storage storage in _simulation.Buildings.OfType<G5_Storage>())
            {
                int inLager = Math.Min(remaining, storage.Capacity - storage.UsedCapacity);
                if (inLager > 0 && !storage.TryReceive(item, inLager))
                {
                    throw new InvalidOperationException("Gegenstände konnten trotz freier Kapazität nicht eingelagert werden.");
                }

                remaining -= inLager;
                if (remaining == 0)
                {
                    break;
                }
            }

            if (remaining != 0)
            {
                throw new InvalidOperationException("Die Aufnahmekapazität änderte sich während der Einlagerung.");
            }

            return true;
        }

        internal int Gesamtmenge(G5_Item item)
        {
            return checked(Menge(item) + _simulation.Buildings.OfType<G5_Storage>()
                .Sum(storage => storage.CountOf(item)));
        }

        internal bool HatRessourcen(IReadOnlyDictionary<G5_Item, int> kosten)
        {
            return kosten != null && kosten.All(kostenpunkt =>
                kostenpunkt.Value >= 0 && Gesamtmenge(kostenpunkt.Key) >= kostenpunkt.Value);
        }

        internal const int MaxHandwerkAnzahl = 999;

        internal bool ManuellesHandwerkLaeuft => _laufendesHandwerk != null;
        internal G5_Recipe LaufendesHandwerk => _laufendesHandwerk;
        internal int VerbleibendeHandwerkTicks => _verbleibendeHandwerkTicks;

        // Laufendes Produkt plus alle noch wartenden Produkte.
        internal int OffeneHandwerkAnzahl =>
            (ManuellesHandwerkLaeuft ? 1 : 0) + _ausstehendeHandwerkAnzahl;

        internal bool TryStarteManuellesHandwerk(G5_Recipe recipe)
        {
            return TryStarteManuellesHandwerk(recipe, 1);
        }

        // Startet die Herstellung von "anzahl" Produkten nacheinander. Es wird vorab geprüft,
        // ob Zutaten und Rucksackplatz für alle Produkte reichen; verbraucht werden die
        // Zutaten jedoch erst, wenn das jeweilige Produkt an der Reihe ist.
        internal bool TryStarteManuellesHandwerk(G5_Recipe recipe, int anzahl)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (anzahl <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(anzahl));
            }

            if (ManuellesHandwerkLaeuft || MaxHerstellbar(recipe) < anzahl)
            {
                return false;
            }

            if (!TryStarteEinzelnesHandwerk(recipe))
            {
                throw new InvalidOperationException("Handwerkszutaten haben sich während der Herstellung geändert.");
            }

            _ausstehendeHandwerkAnzahl = anzahl - 1;
            return true;
        }

        // Wie viele Produkte dieses Rezepts aktuell mit dem Rucksackinhalt hergestellt werden können.
        internal int MaxHerstellbar(G5_Recipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            int maximum = MaxHandwerkAnzahl;
            foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
            {
                if (ingredient.Value > 0)
                {
                    maximum = Math.Min(maximum, Menge(ingredient.Key) / ingredient.Value);
                }
            }

            // Erzeugt ein Rezept mehr Teile als es verbraucht, begrenzt der freie Rucksackplatz.
            int nettoZuwachs = recipe.OutputAmount - recipe.Ingredients.Values.Sum();
            if (nettoZuwachs > 0)
            {
                int frei = SpielerInventarKapazitaetMax - InventarBelegt - _reservierteHandwerkKapazitaet;
                maximum = Math.Min(maximum, frei / nettoZuwachs);
            }

            return Math.Max(0, maximum);
        }

        private bool TryStarteEinzelnesHandwerk(G5_Recipe recipe)
        {
            int ingredientAmount = recipe.Ingredients.Values.Sum();
            if (recipe.Ingredients.Any(ingredient => Menge(ingredient.Key) < ingredient.Value) ||
                InventarBelegt - ingredientAmount + recipe.OutputAmount > SpielerInventarKapazitaetMax)
            {
                return false;
            }

            if (!TryVerbraucheRessourcen(recipe.Ingredients))
            {
                return false;
            }

            _laufendesHandwerk = recipe;
            _verbleibendeHandwerkTicks = ManuellesHandwerkDauerTicks;
            _reservierteHandwerkKapazitaet = recipe.OutputAmount;
            return true;
        }

        internal G5_Recipe TickManuellesHandwerk()
        {
            if (_laufendesHandwerk == null)
            {
                return null;
            }

            _verbleibendeHandwerkTicks--;
            if (_verbleibendeHandwerkTicks > 0)
            {
                return null;
            }

            G5_Recipe abgeschlossenesRezept = _laufendesHandwerk;
            _reservierteHandwerkKapazitaet -= abgeschlossenesRezept.OutputAmount;
            if (!TryAddInventar(abgeschlossenesRezept.Output, abgeschlossenesRezept.OutputAmount))
            {
                _reservierteHandwerkKapazitaet += abgeschlossenesRezept.OutputAmount;
                throw new InvalidOperationException(
                    "Das hergestellte Produkt konnte trotz reserviertem Rucksackplatz nicht abgelegt werden.");
            }

            _laufendesHandwerk = null;
            _verbleibendeHandwerkTicks = 0;
            ErfasseProduktion(abgeschlossenesRezept.Output, abgeschlossenesRezept.OutputAmount);

            // Nächstes Produkt der gewünschten Anzahl starten. Fehlen inzwischen Zutaten
            // (z. B. weil etwas abgelegt wurde), wird der Rest abgebrochen.
            if (_ausstehendeHandwerkAnzahl > 0)
            {
                _ausstehendeHandwerkAnzahl--;
                if (!TryStarteEinzelnesHandwerk(abgeschlossenesRezept))
                {
                    _ausstehendeHandwerkAnzahl = 0;
                }
            }

            return abgeschlossenesRezept;
        }

        internal bool TryCollectMinerOutput(G5_Miner miner, int requestedAmount, out int amount)
        {
            if (miner == null)
            {
                throw new ArgumentNullException(nameof(miner));
            }

            return TryCollectBuildingOutput(miner, miner.Resource, requestedAmount, out amount);
        }

        internal bool TryCollectBuildingOutput(
            G5_Building building,
            G5_Item item,
            int requestedAmount,
            out int amount)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (!_simulation.Buildings.Contains(building))
            {
                throw new InvalidOperationException("Das Gebäude gehört nicht zu diesem Spielfeld.");
            }

            IReadOnlyDictionary<G5_Item, int> outputContents;
            if (building is G5_Miner miner)
            {
                outputContents = miner.OutputContents;
            }
            else if (building is G5_ProductionBuilding production)
            {
                outputContents = production.OutputContents;
            }
            else
            {
                throw new ArgumentException(
                    "Ausgaben können nur aus Förderern und Produktionsgebäuden abgeholt werden.",
                    nameof(building));
            }

            if (requestedAmount <= 0)
            {
                amount = 0;
                return false;
            }

            outputContents.TryGetValue(item, out int available);
            available = Math.Min(available, requestedAmount);
            amount = Math.Min(available, FreieAufnahmekapazitaet());
            if (amount <= 0)
            {
                return false;
            }

            if (!building.TryExtract(item, amount))
            {
                throw new InvalidOperationException("Die Gebäudeausgabe konnte nicht abgeholt werden.");
            }

            AddInventar(item, amount);
            return true;
        }

        internal bool TryVerbraucheRessourcen(IReadOnlyDictionary<G5_Item, int> kosten)
        {
            if (!HatRessourcen(kosten))
            {
                return false;
            }

            foreach (KeyValuePair<G5_Item, int> kostenpunkt in kosten)
            {
                int remaining = kostenpunkt.Value;
                int fromPlayer = Math.Min(remaining, Menge(kostenpunkt.Key));
                int playerRemaining = Menge(kostenpunkt.Key) - fromPlayer;
                if (playerRemaining == 0)
                {
                    _spielerInventar.Remove(kostenpunkt.Key);
                }
                else
                {
                    _spielerInventar[kostenpunkt.Key] = playerRemaining;
                }

                remaining -= fromPlayer;
                foreach (G5_Storage storage in _simulation.Buildings.OfType<G5_Storage>())
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    int fromStorage = Math.Min(remaining, storage.CountOf(kostenpunkt.Key));
                    if (fromStorage > 0 && !storage.TryExtract(kostenpunkt.Key, fromStorage))
                    {
                        throw new InvalidOperationException("Ressourcen konnten nicht aus dem Lager entnommen werden.");
                    }

                    remaining -= fromStorage;
                }

                if (remaining != 0)
                {
                    throw new InvalidOperationException("Der Ressourcenbestand änderte sich während der Entnahme.");
                }
            }

            return true;
        }

        internal void AddInventar(G5_Item item, int amount)
        {
            if (!TryAddInventar(item, amount))
            {
                throw new InvalidOperationException("Im Rucksack und in den Lagern ist nicht genug Platz.");
            }
        }

        internal bool TryStartHandabbau()
        {
            if (_handabbauCooldownTicks > 0)
            {
                return false;
            }

            _handabbauCooldownTicks = HandabbauCooldownTicks;
            return true;
        }

        internal void TickHandabbauCooldown()
        {
            if (_handabbauCooldownTicks > 0)
            {
                _handabbauCooldownTicks--;
            }
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
            DeselectBuilding();
        }

        internal bool RemoveGebaeude(G5_Building gebaeude)
        {
            if (gebaeude == null || !_simulation.RemoveBuilding(gebaeude))
            {
                return false;
            }

            DeselectBuilding();
            return true;
        }

        internal void ToggleBaumenue()
        {
            _baumenueGeoeffnet = !_baumenueGeoeffnet;
        }

        internal void ToggleGebaeuderegister()
        {
            ToggleBaumenue();
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
                if (_gebaeude is G5_Storage) return G5_FabrikGebaeudeTyp.Lager;
                if (_gebaeude is G5_Rakete) return G5_FabrikGebaeudeTyp.Rakete;
                if (_gebaeude is G5_Miner) return G5_FabrikGebaeudeTyp.Foerderer;
                if (_gebaeude is G5_Smelter) return G5_FabrikGebaeudeTyp.Schmelze;
                if (_gebaeude is G5_Constructor) return G5_FabrikGebaeudeTyp.Konstruktor;
                if (_gebaeude is G5_Assembler) return G5_FabrikGebaeudeTyp.Assembler;
                if (_gebaeude is G5_BurningGenerator) return G5_FabrikGebaeudeTyp.Verbrennungsgenerator;
                if (_gebaeude is G5_PowerPole) return G5_FabrikGebaeudeTyp.Strommast;
                if (_gebaeude is G5_Conveyor) return G5_FabrikGebaeudeTyp.Laufband;
                throw new InvalidOperationException("Unbekannter Gebäudetyp in der Fabrikansicht.");
            }
        }

        string IG5_GebaeudeInfo.Name => _gebaeude.Name;

        string IG5_GebaeudeInfo.Beschreibung
        {
            get
            {
                if (_gebaeude is G5_Miner miner)
                {
                    return "Fördert " + G5_FabrikBezeichnungen.Gegenstand(miner.Resource) +
                        " aus dem Vorkommen.";
                }

                if (_gebaeude is G5_ProductionBuilding production)
                {
                    return production.SelectedRecipe == null
                        ? "Verarbeitet Rohstoffe nach einem gewählten Rezept."
                        : "Verarbeitet Rohstoffe: " + production.SelectedRecipe.Name + ".";
                }

                if (_gebaeude is G5_Conveyor) return "Transportiert Gegenstände zum nächsten Gebäude.";
                if (_gebaeude is G5_BurningGenerator) return "Erzeugt Strom aus Kohle.";
                if (_gebaeude is G5_PowerPole) return "Verbindet Gebäude mit dem Stromnetz.";
                if (_gebaeude is G5_Storage) return "Lagert Gegenstände für deine Fabrik.";
                if (_gebaeude is G5_Rakete rakete)
                {
                    return "Verlässt den Planeten. Treibstoff: " + rakete.UranVorrat +
                        "/" + G5_Rakete.UranBedarf + " Uran.";
                }
                return "Gebäude deiner Fabrik.";
            }
        }

        int IG5_GebaeudeInfo.X => _gebaeude.Position.X;
        int IG5_GebaeudeInfo.Y => _gebaeude.Position.Y;
        int IG5_GebaeudeInfo.Breite => _gebaeude.Width;
        int IG5_GebaeudeInfo.Hoehe => _gebaeude.Height;
        int IG5_GebaeudeInfo.Level => _gebaeude.Level;
        G5_BuildingStatus IG5_GebaeudeInfo.Status => _gebaeude.Status;
        bool IG5_GebaeudeInfo.IstMitStromVersorgt => _gebaeude.IsPowered;

        string IG5_GebaeudeInfo.AusgewaehltesRezept =>
            (_gebaeude as G5_ProductionBuilding)?.SelectedRecipe?.Name ?? string.Empty;

        IReadOnlyList<string> IG5_GebaeudeInfo.VerfuegbareRezepte
        {
            get
            {
                if (!(_gebaeude is G5_ProductionBuilding production))
                {
                    return Array.Empty<string>();
                }

                if (_gebaeude.Level >= 2)
                {
                    return production.Recipes.Select(recipe => recipe.Name).ToArray();
                }

                return production.SelectedRecipe == null
                    ? Array.Empty<string>()
                    : new[] { production.SelectedRecipe.Name };
            }
        }

        IReadOnlyDictionary<G5_Item, int> IG5_GebaeudeInfo.UpgradeKosten =>
            ((IG5_GebaeudeInfo)this).KannVerbessertWerden
                ? G5_FabrikKosten.UpgradePreis(_gebaeude.Level)
                : new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());

        IReadOnlyDictionary<G5_Item, int> IG5_GebaeudeInfo.AusgabeInhalt
        {
            get
            {
                if (_gebaeude is G5_Miner miner)
                {
                    return miner.OutputContents;
                }

                if (_gebaeude is G5_ProductionBuilding production)
                {
                    return production.OutputContents;
                }

                return new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());
            }
        }

        bool IG5_GebaeudeInfo.KannVerbessertWerden =>
            _gebaeude.Level < G5_FabrikKosten.MaxUpgradeLevel &&
            !(_gebaeude is G5_PowerPole) &&
            !(_gebaeude is G5_Rakete);
    }

    internal static class G5_FabrikKosten
    {
        internal const int MaxUpgradeLevel = 3;

        private static readonly IReadOnlyDictionary<G5_Bauwerkzeug, IReadOnlyDictionary<G5_Item, int>> Baupreise =
            new Dictionary<G5_Bauwerkzeug, IReadOnlyDictionary<G5_Item, int>>
            {
                [G5_Bauwerkzeug.Foerderer] = Kosten((G5_Item.Eisenerz, 8), (G5_Item.Holz, 5)),
                [G5_Bauwerkzeug.Schmelze] = Kosten((G5_Item.Eisenbarren, 5), (G5_Item.Eisenplatte, 3), (G5_Item.Beton, 3), (G5_Item.Holz, 4)),
                [G5_Bauwerkzeug.Foerderband] = Kosten((G5_Item.Eisenbarren, 2), (G5_Item.Eisenplatte, 1), (G5_Item.Beton, 1)),
                [G5_Bauwerkzeug.Konstruktor] = Kosten((G5_Item.Eisenbarren, 6), (G5_Item.Eisenplatte, 3), (G5_Item.Eisenstange, 2), (G5_Item.Beton, 3)),
                [G5_Bauwerkzeug.Assembler] = Kosten((G5_Item.Eisenbarren, 10), (G5_Item.Eisenplatte, 6), (G5_Item.Eisenstange, 4), (G5_Item.Beton, 6)),
                [G5_Bauwerkzeug.Verbrennungsgenerator] = Kosten((G5_Item.Eisenbarren, 6), (G5_Item.Eisenplatte, 3), (G5_Item.Beton, 2), (G5_Item.Kohle, 4), (G5_Item.Holz, 4)),
                [G5_Bauwerkzeug.Strommast] = Kosten((G5_Item.Eisenplatte, 1), (G5_Item.Eisenstange, 1), (G5_Item.Beton, 1), (G5_Item.Holz, 2)),
                [G5_Bauwerkzeug.Lager] = Kosten((G5_Item.Eisenplatte, 3), (G5_Item.Eisenstange, 2), (G5_Item.Beton, 4), (G5_Item.Holz, 6)),
                [G5_Bauwerkzeug.Rakete] = Kosten((G5_Item.Eisenplatte, 40), (G5_Item.Beton, 30), (G5_Item.Rotor, 20))
            };

        internal static IReadOnlyDictionary<G5_Item, int> Baupreis(G5_Bauwerkzeug werkzeug)
        {
            return Baupreise.TryGetValue(werkzeug, out IReadOnlyDictionary<G5_Item, int> kosten)
                ? kosten
                : new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());
        }

        internal static IReadOnlyDictionary<G5_Item, int> UpgradePreis(int aktuellesLevel)
        {
            if (aktuellesLevel < 1 || aktuellesLevel >= MaxUpgradeLevel)
            {
                return new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());
            }

            int faktor = aktuellesLevel;
            return Kosten(
                (G5_Item.Eisenerz, 8 * faktor),
                (G5_Item.Kupfererz, 4 * faktor),
                (G5_Item.Holz, 3 * faktor));
        }

        private static IReadOnlyDictionary<G5_Item, int> Kosten(params (G5_Item Item, int Menge)[] kosten)
        {
            return new ReadOnlyDictionary<G5_Item, int>(
                kosten.ToDictionary(eintrag => eintrag.Item, eintrag => eintrag.Menge));
        }
    }
}
