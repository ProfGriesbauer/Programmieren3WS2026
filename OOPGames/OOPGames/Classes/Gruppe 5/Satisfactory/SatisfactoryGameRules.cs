using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // Zentrale Spiellogik: Diese Klasse bestaetigt Aktionen, verwaltet Karte und Tick.
    // Die konkreten Berechnungsklassen fuer Gebaeude, Lager und Rohstoffe stammen aus dem gemeinsamen Fabrikmodell.
    public sealed class SatisfactoryGameRules : IGameRules2
    {
        private const int MapWidth = 50;
        private const int MapHeight = 30;
        private const int HudHeight = 56;
        private const int BuildingWidth = 2;
        private const int BuildingHeight = 2;
        private int _resourceAmount = 300;
        private int _minerCycleTicks = 35;
        private int _smelterCycleTicks = 55;
        private bool _tutorialEnabled = true;
        private readonly Random _random = new Random();

        private readonly Dictionary<G5_GridPosition, G5_ResourceDeposit> _deposits =
            new Dictionary<G5_GridPosition, G5_ResourceDeposit>();
        private readonly HashSet<G5_Miner> _beltConnectedMiners = new HashSet<G5_Miner>();
        private readonly HashSet<G5_Smelter> _beltConnectedSmelters = new HashSet<G5_Smelter>();
        private readonly SatisfactoryStartScreen _startScreen = new SatisfactoryStartScreen();
        private readonly SatisfactoryGameMentor _mentor = new SatisfactoryGameMentor();
        private G5_FabrikSpielfeld _field;
        private SatisfactoryFieldView _fieldView;
        private G5_Building _buildingToMove;
        private G5_GridPosition _tutorialDepositPosition;
        private G5_GridPosition _tutorialCoalDepositPosition;
        private bool _gameStarted;

        public SatisfactoryGameRules()
        {
            ClearField();
        }

        public string Name { get { return "G5 Satisfactory Rules"; } }
        public IGameField CurrentField { get { return _fieldView; } }
        public bool MovesPossible { get { return _field != null; } }

        public void DoMove(IPlayMove move)
        {
            // Der Player liefert Befehle; die Rules sind die einzige Stelle, die deren Gueltigkeit prueft.
            if (_field == null)
            {
                return;
            }

            IG5_FabrikAktion action = move as IG5_FabrikAktion;
            if (action == null)
            {
                _field.SetStatusMeldung("Eingabe konnte nicht verarbeitet werden.");
                return;
            }

            if (!_gameStarted)
            {
                if (action.Aktionstyp == G5_FabrikAktionstyp.GebaeudePlatzieren &&
                    _startScreen.HandleClick(action.X, action.Y))
                {
                    SatisfactoryStartScreenSettings settings = _startScreen.SelectedSettings;
                    _resourceAmount = settings.ResourceAmount;
                    _minerCycleTicks = settings.MinerCycleTicks;
                    _smelterCycleTicks = settings.SmelterCycleTicks;
                    _tutorialEnabled = settings.TutorialEnabled;
                    _gameStarted = true;
                    ClearField();
                    if (_tutorialEnabled)
                    {
                        _mentor.Begin(
                            _deposits[_tutorialDepositPosition].RemainingAmount.Value,
                            _tutorialDepositPosition,
                            _tutorialCoalDepositPosition);
                    }
                    else
                    {
                        _mentor.Skip();
                    }
                }

                return;
            }

            if (action.Aktionstyp == G5_FabrikAktionstyp.TutorialUeberspringen)
            {
                if (_mentor.IsActive)
                {
                    _mentor.Skip();
                    _field.SetStatusMeldung("Tutorial übersprungen. Du kannst jetzt frei spielen.");
                }

                return;
            }

            if (action.Aktionstyp == G5_FabrikAktionstyp.GebaeuderegisterUmschalten)
            {
                _field.ToggleGebaeuderegister();
                return;
            }

            if (action.Aktionstyp == G5_FabrikAktionstyp.GebaeuderegisterScrollen)
            {
                if (_field.GebaeuderegisterGeoeffnet)
                {
                    _field.ScrollGebaeuderegister(action.X);
                }

                return;
            }

            if (action.Aktionstyp == G5_FabrikAktionstyp.WerkzeugWaehlen)
            {
                SelectTool(action.Werkzeug);
            }
            else if (action.Aktionstyp == G5_FabrikAktionstyp.GebaeudePlatzieren)
            {
                if (((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug == G5_Bauwerkzeug.GebaeudeVerschieben)
                {
                    if (_buildingToMove == null)
                    {
                        SelectBuildingToMove(action.X, action.Y);
                    }
                    else
                    {
                        MoveSelectedBuilding(action.X, action.Y);
                    }

                    return;
                }

                if (_mentor.TryInspectHub(action.X, action.Y))
                {
                    _field.SetStatusMeldung(_mentor.Message);
                    return;
                }

                PlaceBuilding(action.X, action.Y);
            }
            else
            {
                _field.SetStatusMeldung("Unbekannte Aktion.");
            }
        }

        public void ClearField()
        {
            // Reset: entfernt alte Simulationen und erstellt eine leere Karte mit Hub und Startschmelzen.
            _deposits.Clear();
            _beltConnectedMiners.Clear();
            _beltConnectedSmelters.Clear();
            _buildingToMove = null;

            G5_WeltSpielfeld world = new G5_WeltSpielfeld(MapWidth, MapHeight);
            world.VerschiebeAnsicht(0, HudHeight);
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (!world.TrySetSichtbarkeit(x, y, true))
                    {
                        throw new InvalidOperationException("Eine Startkachel konnte nicht sichtbar gemacht werden.");
                    }
                }
            }

            _field = new G5_FabrikSpielfeld(world, 1, 1);
            _fieldView = new SatisfactoryFieldView(_field, _startScreen, _mentor, _gameStarted);

            AddSmelter(10, 2, "Eisenerz schmelzen");
            AddSmelter(10, 9, "Kupfererz schmelzen");
            _tutorialDepositPosition = CreateRandomMap();
            _field.SetStatusMeldung(
                "Tab: Bauauswahl · F/S/B/C/A/G/P/L: Bauen · M: Gebäude versetzen");
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void StartedGameCall()
        {
            _gameStarted = false;
            _mentor.Reset();
            _startScreen.ShowHome();
            ClearField();
        }

        public void TickGameCall()
        {
            // Tick: aktualisiert Simulation, Rohstoffanzeigen, automatische Verteilung und Mentorbeobachtung.
            if (_field == null || !_gameStarted)
            {
                return;
            }

            List<G5_Miner> miners = _field.InterneGebaeude.OfType<G5_Miner>().ToList();
            Dictionary<G5_Miner, long?> depositsBeforeTick = miners.ToDictionary(
                miner => miner,
                miner => miner.RemainingDeposit);

            _field.Simulation.Tick();
            UpdateVisibleDeposits(miners, depositsBeforeTick);
            TransferMaterials();
            if (_deposits.TryGetValue(_tutorialDepositPosition, out G5_ResourceDeposit deposit) &&
                _mentor.ObserveMining(deposit.RemainingAmount.Value))
            {
                _field.SetStatusMeldung(_mentor.Message);
            }
            if (_mentor.ObserveCoalStock(
                ((IG5_FabrikSpielInfo)_field).HubMenge(G5_Item.Kohle)))
            {
                _field.SetStatusMeldung(_mentor.Message);
            }
            if (_mentor.ObservePower(
                _field.InterneGebaeude.OfType<G5_Constructor>().Any(constructor =>
                    constructor.Position.X == SatisfactoryTutorialLayout.ConstructorX &&
                    constructor.Position.Y == SatisfactoryTutorialLayout.BuildingY &&
                    constructor.IsPowered),
                _field.InterneGebaeude.OfType<G5_Assembler>().Any(assembler =>
                    assembler.Position.X == SatisfactoryTutorialLayout.AssemblerX &&
                    assembler.Position.Y == SatisfactoryTutorialLayout.BuildingY &&
                    assembler.IsPowered)))
            {
                _field.SetStatusMeldung(_mentor.Message);
            }
            if (_mentor.ObserveConveyorTransport(
                _field.InterneGebaeude.OfType<G5_Conveyor>()
                    .Any(conveyor => conveyor.Status == G5_BuildingStatus.Working)))
            {
                _field.SetStatusMeldung(_mentor.Message);
            }
        }

        private void SelectTool(G5_Bauwerkzeug tool)
        {
            // Werkzeuge werden gewaehlt, bevor ein Klick auf der Karte platziert wird.
            if (!Enum.IsDefined(typeof(G5_Bauwerkzeug), tool))
            {
                _field.SetStatusMeldung("Unbekanntes Bauwerkzeug.");
                return;
            }

            if (!_mentor.TrySelectTool(tool, out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            _buildingToMove = null;
            _field.SetWerkzeug(tool);
            _field.SetStatusMeldung(tool == G5_Bauwerkzeug.GebaeudeVerschieben
                ? "Verschieben ausgewählt: Gebäude und danach das Zielfeld anklicken."
                : tool == G5_Bauwerkzeug.Keines
                    ? "Bauwerkzeug abgewählt."
                    : tool + " ausgewählt. Klicke auf eine passende freie Fläche.");
        }

        private void PlaceBuilding(int x, int y)
        {
            // Platzierung: prueft Mentorvorgabe, Flaeche, Rohstofffeld und erstellt genau das Modell aus dem gemeinsamen Gebaeude-Layer.
            G5_Bauwerkzeug tool = ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug;
            if (!_mentor.CanPlaceBuilding(tool, x, y, out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            if (tool == G5_Bauwerkzeug.Keines)
            {
                _field.SetStatusMeldung("Tab öffnet die Bauauswahl. Dort findest du alle Gebäudetasten.");
                return;
            }

            int buildingWidth = tool == G5_Bauwerkzeug.Foerderband ||
                tool == G5_Bauwerkzeug.Strommast
                ? 1
                : BuildingWidth;
            int buildingHeight = buildingWidth == 1 ? 1 : BuildingHeight;
            G5_Building building;
            if (tool == G5_Bauwerkzeug.Foerderband)
            {
                if (!IsBuildableArea(x, y, 1, 1, null) || ContainsDeposit(x, y, 1, 1))
                {
                    _field.SetStatusMeldung("Ein Förderband braucht ein freies, ebenes Feld.");
                    return;
                }

                building = new G5_Conveyor(
                    new G5_GridPosition(x, y),
                    bufferCapacity: 10,
                    itemsPerTick: 1,
                    powerDemandPerTick: 0);
            }
            else if (!IsBuildableArea(x, y, buildingWidth, buildingHeight, null))
            {
                _field.SetStatusMeldung("Dort ist nicht genug freie, bebaubare Fläche.");
                return;
            }
            else if (tool == G5_Bauwerkzeug.Foerderer)
            {
                G5_GridPosition position = new G5_GridPosition(x, y);
                if (!_deposits.TryGetValue(position, out G5_ResourceDeposit deposit) ||
                    deposit.RemainingAmount == 0)
                {
                    _field.SetStatusMeldung("Ein Förderer muss direkt auf einem Rohstoffvorkommen stehen.");
                    return;
                }

                building = new G5_Miner(
                    position,
                    deposit,
                    outputCapacity: 20,
                    cycleTicks: _minerCycleTicks,
                    amountPerCycle: 1,
                    powerDemandPerTick: 0);
            }
            else if (tool == G5_Bauwerkzeug.Schmelze)
            {
                if (ContainsDeposit(x, y, buildingWidth, buildingHeight))
                {
                    _field.SetStatusMeldung("Eine Schmelze kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                building = CreateSmelter(x, y, "Eisenerz schmelzen");
            }
            else if (tool == G5_Bauwerkzeug.Konstruktor)
            {
                if (ContainsDeposit(x, y, buildingWidth, buildingHeight))
                {
                    _field.SetStatusMeldung("Ein Konstruktor kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                G5_Constructor constructor = new G5_Constructor(
                    new G5_GridPosition(x, y),
                    G5_StarterRecipes.CreateConstructionRecipes(_smelterCycleTicks, powerDemandPerTick: 1),
                    inputCapacity: 30,
                    outputCapacity: 30);
                if (!constructor.SelectRecipe("Eisenplatte herstellen"))
                {
                    throw new InvalidOperationException("Das Standardrezept für den Konstruktor fehlt.");
                }

                building = constructor;
            }
            else if (tool == G5_Bauwerkzeug.Assembler)
            {
                if (ContainsDeposit(x, y, buildingWidth, buildingHeight))
                {
                    _field.SetStatusMeldung("Ein Assembler kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                G5_Assembler assembler = new G5_Assembler(
                    new G5_GridPosition(x, y),
                    G5_StarterRecipes.CreateAssemblyRecipes(_smelterCycleTicks, powerDemandPerTick: 1),
                    inputCapacity: 30,
                    outputCapacity: 30);
                if (!assembler.SelectRecipe("Rotor herstellen"))
                {
                    throw new InvalidOperationException("Das Standardrezept für den Assembler fehlt.");
                }

                building = assembler;
            }
            else if (tool == G5_Bauwerkzeug.Verbrennungsgenerator)
            {
                if (ContainsDeposit(x, y, buildingWidth, buildingHeight))
                {
                    _field.SetStatusMeldung("Ein Generator kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                building = new G5_BurningGenerator(
                    new G5_GridPosition(x, y),
                    fuelCapacity: 100,
                    maxOutputPerTick: 20,
                    energyPerFuel: new[]
                    {
                        new KeyValuePair<G5_Item, int>(G5_Item.Kohle, 30)
                    });
            }
            else if (tool == G5_Bauwerkzeug.Strommast)
            {
                building = new G5_PowerPole(new G5_GridPosition(x, y), connectionRange: 8);
            }
            else if (tool == G5_Bauwerkzeug.Lager)
            {
                if (ContainsDeposit(x, y, buildingWidth, buildingHeight))
                {
                    _field.SetStatusMeldung("Ein Lager kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                building = new G5_Storage(new G5_GridPosition(x, y), capacity: 500);
            }
            else
            {
                _field.SetStatusMeldung("Dieses Bauwerkzeug wird nicht unterstützt.");
                return;
            }

            _field.AddGebaeude(building);
            RefreshConveyorConnections();
            long depositAmount = building is G5_Miner
                ? _deposits[new G5_GridPosition(x, y)].RemainingAmount.Value
                : 0;
            _mentor.BuildingPlaced(tool, x, y, depositAmount);
            _field.SetStatusMeldung(building.Name + " gebaut.");
        }

        private bool IsBuildableArea(int x, int y, int width, int height, G5_Building ignoredBuilding)
        {
            if (x < 0 || y < 0 ||
                x + width > MapWidth ||
                y + height > MapHeight)
            {
                return false;
            }

            for (int tileY = y; tileY < y + height; tileY++)
            {
                for (int tileX = x; tileX < x + width; tileX++)
                {
                    if (!_field.TryGetKachel(tileX, tileY, out G5_Kachel tile) ||
                        ((IG5_KachelInfo)tile).Gelaende != G5_Gelaendeart.Begehbar)
                    {
                        return false;
                    }
                }
            }

            return !OverlapsBuilding(x, y, width, height, ignoredBuilding);
        }

        private bool ContainsDeposit(int x, int y, int width, int height)
        {
            for (int tileY = y; tileY < y + height; tileY++)
            {
                for (int tileX = x; tileX < x + width; tileX++)
                {
                    if (_field.TryGetKachel(tileX, tileY, out G5_Kachel tile) &&
                        ((IG5_KachelInfo)tile).Rohstoff != G5_Rohstofftyp.Keiner)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool OverlapsBuilding(int x, int y, int width, int height, G5_Building ignoredBuilding)
        {
            return _field.InterneGebaeude.Any(existing =>
                !ReferenceEquals(existing, ignoredBuilding) &&
                x < existing.Position.X + existing.Width &&
                existing.Position.X < x + width &&
                y < existing.Position.Y + existing.Height &&
                existing.Position.Y < y + height);
        }

        private void SelectBuildingToMove(int x, int y)
        {
            // Verschieben Start: sucht das angeklickte Gebaeude und merkts sich fuer den zweiten Klick.
            G5_Building building = _field.InterneGebaeude.FirstOrDefault(candidate =>
                x >= candidate.Position.X && x < candidate.Position.X + candidate.Width &&
                y >= candidate.Position.Y && y < candidate.Position.Y + candidate.Height);
            if (building == null)
            {
                _field.SetStatusMeldung("Klicke zuerst auf ein Gebäude, das du versetzen möchtest.");
                return;
            }

            if (!_mentor.TrySelectBuildingToMove(
                building.Position.X,
                building.Position.Y,
                out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            _buildingToMove = building;
            _field.SetStatusMeldung("Jetzt das gewünschte freie Zielfeld anklicken.");
        }

        private void MoveSelectedBuilding(int x, int y)
        {
            // Verschieben Ende: prueft Ziel und aktualisiert Position ueber die gemeinsame Fabriksimulation.
            G5_Building building = _buildingToMove;
            if (!_mentor.CanMoveBuilding(x, y, out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            if (x == building.Position.X && y == building.Position.Y)
            {
                _field.SetStatusMeldung("Das Gebäude steht bereits an diesem Ort.");
                return;
            }

            G5_ResourceDeposit newDeposit = null;
            if (building is G5_Miner miner)
            {
                if (!_deposits.TryGetValue(new G5_GridPosition(x, y), out newDeposit) ||
                    newDeposit.RemainingAmount == 0)
                {
                    _field.SetStatusMeldung("Ein Förderer kann nur auf ein verfügbares Erzvorkommen versetzt werden.");
                    return;
                }

                if (miner.Resource != newDeposit.Item && miner.TransferableContents.Count > 0)
                {
                    _field.SetStatusMeldung("Leere zuerst den Förderer, bevor du ihn auf einen anderen Rohstofftyp versetzt.");
                    return;
                }
            }
            else if (ContainsDeposit(x, y, building.Width, building.Height))
            {
                _field.SetStatusMeldung("Dieses Gebäude kann nicht auf einem Rohstoffvorkommen stehen.");
                return;
            }

            if (!IsBuildableArea(x, y, building.Width, building.Height, building))
            {
                _field.SetStatusMeldung("Das Zielfeld ist nicht frei oder bebaubar.");
                return;
            }

            _field.Simulation.MoveBuilding(building, new G5_GridPosition(x, y));
            if (building is G5_Miner movedMiner)
            {
                movedMiner.SetDeposit(newDeposit);
            }

            _buildingToMove = null;
            _field.SetWerkzeug(G5_Bauwerkzeug.Keines);
            RefreshConveyorConnections();
            _mentor.BuildingMoved();
            _field.SetStatusMeldung(building.Name + " versetzt.");
        }

        private void RefreshConveyorConnections()
        {
            // Verbindungsaufbau: sucht kuerzeste Linien von genau einem Miner zu einer passenden Schmelze.
            List<G5_Building> buildings = _field.InterneGebaeude.ToList();
            List<G5_Conveyor> conveyors = buildings.OfType<G5_Conveyor>().ToList();
            foreach (G5_Conveyor conveyor in conveyors)
            {
                conveyor.Disconnect();
            }

            _beltConnectedMiners.Clear();
            _beltConnectedSmelters.Clear();
            HashSet<G5_Conveyor> verwendeteBänder = new HashSet<G5_Conveyor>();

            foreach (G5_Miner miner in buildings.OfType<G5_Miner>())
            {
                if (!miner.IsPowered)
                {
                    continue;
                }

                foreach (G5_Smelter smelter in buildings.OfType<G5_Smelter>())
                {
                    if (smelter.SelectedRecipe == null ||
                        !smelter.SelectedRecipe.Ingredients.ContainsKey(miner.Resource))
                    {
                        continue;
                    }

                    if (!TryFindConveyorPath(
                        miner,
                        smelter,
                        conveyors,
                        verwendeteBänder,
                        out List<G5_Building> path))
                    {
                        continue;
                    }

                    for (int index = 1; index < path.Count - 1; index++)
                    {
                        G5_Conveyor conveyor = (G5_Conveyor)path[index];
                        conveyor.Connect(path[index - 1], path[index + 1]);
                        verwendeteBänder.Add(conveyor);
                    }

                    _beltConnectedMiners.Add(miner);
                    _beltConnectedSmelters.Add(smelter);
                    break;
                }
            }
        }

        private static bool TryFindConveyorPath(
            G5_Building source,
            G5_Building destination,
            IReadOnlyList<G5_Conveyor> conveyors,
            IReadOnlySet<G5_Conveyor> verwendeteBänder,
            out List<G5_Building> path)
        {
            path = null;
            Queue<G5_Conveyor> queue = new Queue<G5_Conveyor>();
            Dictionary<G5_Conveyor, G5_Conveyor> previous =
                new Dictionary<G5_Conveyor, G5_Conveyor>();
            foreach (G5_Conveyor conveyor in conveyors)
            {
                if (!verwendeteBänder.Contains(conveyor) &&
                    AreAdjacent(source, conveyor))
                {
                    previous.Add(conveyor, null);
                    queue.Enqueue(conveyor);
                }
            }

            G5_Conveyor last = null;
            while (queue.Count > 0)
            {
                G5_Conveyor current = queue.Dequeue();
                if (AreAdjacent(current, destination))
                {
                    last = current;
                    break;
                }

                foreach (G5_Conveyor candidate in conveyors)
                {
                    if (!verwendeteBänder.Contains(candidate) &&
                        !previous.ContainsKey(candidate) &&
                        AreAdjacent(current, candidate))
                    {
                        previous.Add(candidate, current);
                        queue.Enqueue(candidate);
                    }
                }
            }

            if (last == null)
            {
                return false;
            }

            List<G5_Conveyor> reversed = new List<G5_Conveyor>();
            for (G5_Conveyor current = last; current != null; current = previous[current])
            {
                reversed.Add(current);
            }

            reversed.Reverse();
            path = new List<G5_Building> { source };
            path.AddRange(reversed);
            path.Add(destination);
            return true;
        }

        private static bool AreAdjacent(G5_Building first, G5_Building second)
        {
            bool overlapsX = first.Position.X < (long)second.Position.X + second.Width &&
                second.Position.X < (long)first.Position.X + first.Width;
            bool overlapsY = first.Position.Y < (long)second.Position.Y + second.Height &&
                second.Position.Y < (long)first.Position.Y + first.Height;
            return (overlapsX &&
                    (first.Position.Y + first.Height == second.Position.Y ||
                     second.Position.Y + second.Height == first.Position.Y)) ||
                (overlapsY &&
                    (first.Position.X + first.Width == second.Position.X ||
                     second.Position.X + second.Width == first.Position.X));
        }

        private G5_GridPosition CreateRandomMap()
        {
            List<G5_GridPosition> tutorialCandidates = new List<G5_GridPosition>();
            for (int y = 3; y <= MapHeight - 3; y++)
            {
                for (int x = 2; x <= MapWidth - 8; x++)
                {
                    if (IsTutorialRouteAvailable(x, y))
                    {
                        tutorialCandidates.Add(new G5_GridPosition(x, y));
                    }
                }
            }

            if (tutorialCandidates.Count == 0)
            {
                throw new InvalidOperationException("Auf der Karte wurde kein freier Tutorial-Startbereich gefunden.");
            }

            G5_GridPosition tutorialMinerPosition =
                tutorialCandidates[_random.Next(tutorialCandidates.Count)];
            HashSet<G5_GridPosition> reserved = CreateTutorialReservedArea(tutorialMinerPosition);
            AddDeposit(
                tutorialMinerPosition.X,
                tutorialMinerPosition.Y,
                G5_Rohstofftyp.Eisen,
                G5_Item.Eisenerz,
                _resourceAmount);

            bool coalDepositCreated = false;
            for (int attempt = 0; attempt < 2500 && !coalDepositCreated; attempt++)
            {
                int x = _random.Next(2, MapWidth - 2);
                int y = _random.Next(2, MapHeight - 2);
                if (!IsRandomDepositPositionAvailable(x, y, reserved))
                {
                    continue;
                }

                _tutorialCoalDepositPosition = new G5_GridPosition(x, y);
                AddDeposit(x, y, G5_Rohstofftyp.Kohle, G5_Item.Kohle, _resourceAmount);
                AddReservedRectangle(reserved, x, y, BuildingWidth, BuildingHeight);
                coalDepositCreated = true;
            }

            if (!coalDepositCreated)
            {
                throw new InvalidOperationException("Für das Tutorial konnte kein Kohlevorkommen reserviert werden.");
            }

            (G5_Rohstofftyp Rohstoff, G5_Item Item)[] resources =
            {
                (G5_Rohstofftyp.Eisen, G5_Item.Eisenerz),
                (G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz),
                (G5_Rohstofftyp.Kohle, G5_Item.Kohle),
                (G5_Rohstofftyp.Kalkstein, G5_Item.Kalkstein),
                (G5_Rohstofftyp.Quarz, G5_Item.Quarz),
                (G5_Rohstofftyp.Schwefel, G5_Item.Schwefel),
                (G5_Rohstofftyp.Uran, G5_Item.Uran)
            };
            int additionalDepositCount = 23;
            int placedDeposits = 0;
            for (int attempt = 0; attempt < 2500 && placedDeposits < additionalDepositCount; attempt++)
            {
                int x = _random.Next(2, MapWidth - 2);
                int y = _random.Next(2, MapHeight - 2);
                if (!IsRandomDepositPositionAvailable(x, y, reserved))
                {
                    continue;
                }

                (G5_Rohstofftyp Rohstoff, G5_Item Item) resource =
                    resources[_random.Next(resources.Length)];
                AddDeposit(x, y, resource.Rohstoff, resource.Item, _resourceAmount);
                placedDeposits++;
            }

            CreateRandomObstacles(reserved);
            return tutorialMinerPosition;
        }

        private bool IsTutorialRouteAvailable(int x, int y)
        {
            if (!IsBuildableArea(x, y, BuildingWidth, BuildingHeight, null) ||
                !IsBuildableArea(x + 5, y, BuildingWidth, BuildingHeight, null) ||
                AreasOverlap(x, y, BuildingWidth, BuildingHeight, 20, 14, BuildingWidth, BuildingHeight) ||
                AreasOverlap(x + 5, y, BuildingWidth, BuildingHeight, 20, 14, BuildingWidth, BuildingHeight) ||
                AreasOverlap(
                    x,
                    y,
                    BuildingWidth + 5,
                    BuildingHeight,
                    SatisfactoryTutorialLayout.AreaX,
                    SatisfactoryTutorialLayout.AreaY,
                    SatisfactoryTutorialLayout.AreaWidth,
                    SatisfactoryTutorialLayout.AreaHeight))
            {
                return false;
            }

            for (int conveyorX = x + 2; conveyorX <= x + 4; conveyorX++)
            {
                if (!IsBuildableArea(conveyorX, y, 1, 1, null) ||
                    AreasOverlap(conveyorX, y, 1, 1, 20, 14, BuildingWidth, BuildingHeight))
                {
                    return false;
                }
            }

            return true;
        }

        private static HashSet<G5_GridPosition> CreateTutorialReservedArea(G5_GridPosition minerPosition)
        {
            HashSet<G5_GridPosition> reserved = new HashSet<G5_GridPosition>();
            AddReservedRectangle(reserved, minerPosition.X, minerPosition.Y, 2, 2);
            for (int x = minerPosition.X + 2; x <= minerPosition.X + 4; x++)
            {
                AddReservedRectangle(reserved, x, minerPosition.Y, 1, 1);
            }

            AddReservedRectangle(reserved, minerPosition.X + 5, minerPosition.Y, 2, 2);
            AddReservedRectangle(reserved, 20, 14, 2, 2);
            AddReservedRectangle(
                reserved,
                SatisfactoryTutorialLayout.AreaX,
                SatisfactoryTutorialLayout.AreaY,
                SatisfactoryTutorialLayout.AreaWidth,
                SatisfactoryTutorialLayout.AreaHeight);
            return reserved;
        }

        private static void AddReservedRectangle(
            HashSet<G5_GridPosition> reserved,
            int x,
            int y,
            int width,
            int height)
        {
            for (int tileY = y; tileY < y + height; tileY++)
            {
                for (int tileX = x; tileX < x + width; tileX++)
                {
                    reserved.Add(new G5_GridPosition(tileX, tileY));
                }
            }
        }

        private bool IsRandomDepositPositionAvailable(
            int x,
            int y,
            IReadOnlySet<G5_GridPosition> reserved)
        {
            if (!IsBuildableArea(x, y, BuildingWidth, BuildingHeight, null) ||
                ContainsDeposit(x, y, BuildingWidth, BuildingHeight) ||
                AreasOverlap(x, y, BuildingWidth, BuildingHeight, 20, 14, BuildingWidth, BuildingHeight))
            {
                return false;
            }

            for (int tileY = y; tileY < y + BuildingHeight; tileY++)
            {
                for (int tileX = x; tileX < x + BuildingWidth; tileX++)
                {
                    if (reserved.Contains(new G5_GridPosition(tileX, tileY)))
                    {
                        return false;
                    }
                }
            }

            return !_deposits.Keys.Any(deposit =>
                Math.Abs(deposit.X - x) < BuildingWidth &&
                Math.Abs(deposit.Y - y) < BuildingHeight);
        }

        private void CreateRandomObstacles(IReadOnlySet<G5_GridPosition> reserved)
        {
            int placed = 0;
            HashSet<G5_GridPosition> obstaclePositions = new HashSet<G5_GridPosition>();
            for (int attempt = 0; attempt < 1500 && placed < 45; attempt++)
            {
                int x = _random.Next(MapWidth);
                int y = _random.Next(MapHeight);
                G5_GridPosition position = new G5_GridPosition(x, y);
                if (reserved.Contains(position) ||
                    obstaclePositions.Contains(position) ||
                    _deposits.ContainsKey(position) ||
                    OverlapsBuilding(x, y, 1, 1, null) ||
                    _deposits.Keys.Any(deposit =>
                        Math.Abs(deposit.X - x) <= 1 && Math.Abs(deposit.Y - y) <= 1))
                {
                    continue;
                }

                if (_field.Welt.TrySetGelaende(x, y, G5_Gelaendeart.Hindernis))
                {
                    obstaclePositions.Add(position);
                    placed++;
                }
            }
        }

        private static bool AreasOverlap(
            int firstX,
            int firstY,
            int firstWidth,
            int firstHeight,
            int secondX,
            int secondY,
            int secondWidth,
            int secondHeight)
        {
            return firstX < secondX + secondWidth &&
                secondX < firstX + firstWidth &&
                firstY < secondY + secondHeight &&
                secondY < firstY + firstHeight;
        }

        private void AddDeposit(int x, int y, G5_Rohstofftyp resource, G5_Item item, int amount)
        {
            G5_GridPosition position = new G5_GridPosition(x, y);
            G5_ResourceDeposit deposit = new G5_ResourceDeposit(item, amount);
            _deposits.Add(position, deposit);
            if (!_field.Welt.TrySetRohstoff(x, y, resource, amount))
            {
                throw new InvalidOperationException("Ein Rohstoffvorkommen konnte nicht platziert werden.");
            }
        }

        private void AddSmelter(int x, int y, string recipeName)
        {
            G5_Smelter smelter = CreateSmelter(x, y, recipeName);
            _field.AddGebaeude(smelter);
        }

        private G5_Smelter CreateSmelter(int x, int y, string recipeName)
        {
            G5_Smelter smelter = new G5_Smelter(
                new G5_GridPosition(x, y),
                G5_StarterRecipes.CreateSmeltingRecipes(_smelterCycleTicks, powerDemandPerTick: 0),
                inputCapacity: 20,
                outputCapacity: 20);
            if (!smelter.SelectRecipe(recipeName))
            {
                throw new InvalidOperationException("Das Satisfactory-Schmelzrezept ist nicht verfügbar.");
            }

            return smelter;
        }

        private void UpdateVisibleDeposits(
            IReadOnlyList<G5_Miner> miners,
            IReadOnlyDictionary<G5_Miner, long?> depositsBeforeTick)
        {
            foreach (G5_Miner miner in miners)
            {
                long? before = depositsBeforeTick[miner];
                long? after = miner.RemainingDeposit;
                if (!before.HasValue || !after.HasValue || before.Value <= after.Value)
                {
                    continue;
                }

                int extracted = checked((int)(before.Value - after.Value));
                if (!_field.Welt.TryReduceRohstoff(
                    miner.Position.X,
                    miner.Position.Y,
                    extracted))
                {
                    throw new InvalidOperationException("Der Ressourcenbestand des Spielfelds konnte nicht aktualisiert werden.");
                }
            }
        }

        private void TransferMaterials()
        {
            foreach (G5_Miner miner in _field.InterneGebaeude.OfType<G5_Miner>())
            {
                if (!_beltConnectedMiners.Contains(miner))
                {
                    TransferOutputsToStorage(miner);
                }
            }

            G5_ProductionBuilding[] productionBuildings =
                _field.InterneGebaeude.OfType<G5_ProductionBuilding>().ToArray();
            foreach (G5_ProductionBuilding productionBuilding in productionBuildings)
            {
                TransferOutputsToStorage(productionBuilding);
            }

            foreach (G5_ProductionBuilding productionBuilding in productionBuildings)
            {
                G5_Recipe recipe = productionBuilding.SelectedRecipe;
                if (recipe == null ||
                    productionBuilding is G5_Smelter smelter &&
                    _beltConnectedSmelters.Contains(smelter))
                {
                    continue;
                }

                foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
                {
                    productionBuilding.InputContents.TryGetValue(
                        ingredient.Key,
                        out int currentAmount);
                    int amountNeeded = Math.Max(0, ingredient.Value - currentAmount);
                    foreach (G5_Storage storage in _field.InterneGebaeude.OfType<G5_Storage>())
                    {
                        if (amountNeeded == 0)
                        {
                            break;
                        }

                        amountNeeded -= G5_ItemTransfer.Transfer(
                            storage,
                            productionBuilding,
                            ingredient.Key,
                            amountNeeded);
                    }
                }
            }

            foreach (G5_BurningGenerator generator in _field.InterneGebaeude.OfType<G5_BurningGenerator>())
            {
                foreach (G5_Storage storage in _field.InterneGebaeude.OfType<G5_Storage>())
                {
                    while (generator.FuelContents.Values.Sum() < 5 &&
                        storage.TryExtract(G5_Item.Kohle, 1))
                    {
                        if (!generator.TryAddFuel(G5_Item.Kohle, 1) &&
                            !storage.TryRestoreExtracted(G5_Item.Kohle, 1))
                        {
                            throw new InvalidOperationException("Abgezogene Kohle konnte nicht zurückgegeben werden.");
                        }
                    }
                }
            }
        }

        private void TransferOutputsToStorage(G5_Building source)
        {
            KeyValuePair<G5_Item, int>[] contents = source.TransferableContents.ToArray();
            foreach (KeyValuePair<G5_Item, int> item in contents)
            {
                foreach (G5_Storage storage in _field.InterneGebaeude.OfType<G5_Storage>())
                {
                    G5_ItemTransfer.Transfer(source, storage, item.Key, item.Value);
                }
            }
        }
    }

}
