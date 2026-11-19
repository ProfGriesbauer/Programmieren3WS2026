using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
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

        private readonly Dictionary<G5_GridPosition, G5_ResourceDeposit> _deposits =
            new Dictionary<G5_GridPosition, G5_ResourceDeposit>();
        private readonly HashSet<G5_Miner> _beltConnectedMiners = new HashSet<G5_Miner>();
        private readonly HashSet<G5_Smelter> _beltConnectedSmelters = new HashSet<G5_Smelter>();
        private readonly SatisfactoryStartScreen _startScreen = new SatisfactoryStartScreen();
        private readonly SatisfactoryGameMentor _mentor = new SatisfactoryGameMentor();
        private G5_FabrikSpielfeld _field;
        private SatisfactoryFieldView _fieldView;
        private G5_Building _buildingToMove;
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
                    _gameStarted = true;
                    ClearField();
                    _mentor.Begin(_deposits[new G5_GridPosition(5, 2)].RemainingAmount.Value);
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
            AddDeposit(5, 2, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, _resourceAmount);
            AddDeposit(5, 9, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, _resourceAmount);
            AddDeposit(17, 2, G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz, _resourceAmount);
            AddDeposit(17, 9, G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz, _resourceAmount);

            AddSmelter(10, 2, "Eisenerz schmelzen");
            AddSmelter(10, 9, "Kupfererz schmelzen");
            _field.SetStatusMeldung(
                "F: Förderer · S: Schmelze · B: Förderband · M: Gebäude versetzen");
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
            if (_deposits.TryGetValue(new G5_GridPosition(5, 2), out G5_ResourceDeposit deposit) &&
                _mentor.ObserveMining(deposit.RemainingAmount.Value))
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
            G5_Bauwerkzeug tool = ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug;
            if (!_mentor.CanPlaceBuilding(tool, x, y, out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            if (tool == G5_Bauwerkzeug.Keines)
            {
                _field.SetStatusMeldung("Wähle F für Förderer, S für Schmelze oder B für Förderband.");
                return;
            }

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
            else if (!IsBuildableArea(x, y, BuildingWidth, BuildingHeight, null))
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
                if (ContainsDeposit(x, y, BuildingWidth, BuildingHeight))
                {
                    _field.SetStatusMeldung("Eine Schmelze kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                building = CreateSmelter(x, y, "Eisenerz schmelzen");
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
            List<G5_Building> buildings = _field.InterneGebaeude.ToList();
            List<G5_Conveyor> conveyors = buildings.OfType<G5_Conveyor>().ToList();
            foreach (G5_Conveyor conveyor in conveyors)
            {
                conveyor.Disconnect();
            }

            _beltConnectedMiners.Clear();
            _beltConnectedSmelters.Clear();
            HashSet<G5_Conveyor> visited = new HashSet<G5_Conveyor>();

            foreach (G5_Conveyor start in conveyors)
            {
                if (!visited.Add(start))
                {
                    continue;
                }

                List<G5_Conveyor> component = GetConveyorComponent(start, conveyors, visited);
                List<G5_Building> endpoints = buildings
                    .Where(building => !(building is G5_Conveyor) &&
                        component.Any(conveyor => AreAdjacent(building, conveyor)))
                    .ToList();
                if (endpoints.Count != 2)
                {
                    continue;
                }

                List<G5_Miner> miners = endpoints.OfType<G5_Miner>().ToList();
                List<G5_Smelter> smelters = endpoints.OfType<G5_Smelter>().ToList();
                if (miners.Count != 1 || smelters.Count != 1)
                {
                    continue;
                }

                G5_Miner miner = miners[0];
                G5_Smelter smelter = smelters[0];
                if (smelter.SelectedRecipe == null ||
                    !smelter.SelectedRecipe.Ingredients.ContainsKey(miner.Resource) ||
                    !TryFindConveyorPath(miner, smelter, component, out List<G5_Building> path) ||
                    path.Count != component.Count + 2)
                {
                    continue;
                }

                for (int index = 1; index < path.Count - 1; index++)
                {
                    ((G5_Conveyor)path[index]).Connect(
                        path[index - 1],
                        path[index + 1]);
                }

                _beltConnectedMiners.Add(miner);
                _beltConnectedSmelters.Add(smelter);
            }
        }

        private static List<G5_Conveyor> GetConveyorComponent(
            G5_Conveyor start,
            IReadOnlyList<G5_Conveyor> conveyors,
            HashSet<G5_Conveyor> visited)
        {
            List<G5_Conveyor> component = new List<G5_Conveyor> { start };
            Queue<G5_Conveyor> queue = new Queue<G5_Conveyor>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                G5_Conveyor current = queue.Dequeue();
                foreach (G5_Conveyor candidate in conveyors)
                {
                    if (AreAdjacent(current, candidate) && visited.Add(candidate))
                    {
                        component.Add(candidate);
                        queue.Enqueue(candidate);
                    }
                }
            }

            return component;
        }

        private static bool TryFindConveyorPath(
            G5_Building source,
            G5_Building destination,
            IReadOnlyList<G5_Conveyor> component,
            out List<G5_Building> path)
        {
            path = null;
            Queue<G5_Conveyor> queue = new Queue<G5_Conveyor>();
            Dictionary<G5_Conveyor, G5_Conveyor> previous =
                new Dictionary<G5_Conveyor, G5_Conveyor>();
            foreach (G5_Conveyor conveyor in component)
            {
                if (AreAdjacent(source, conveyor))
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

                foreach (G5_Conveyor candidate in component)
                {
                    if (!previous.ContainsKey(candidate) && AreAdjacent(current, candidate))
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
                    TransferOutputsToHub(miner);
                }
            }

            foreach (G5_Smelter smelter in _field.InterneGebaeude.OfType<G5_Smelter>())
            {
                TransferOutputsToHub(smelter);
            }

            foreach (G5_Smelter smelter in _field.InterneGebaeude.OfType<G5_Smelter>())
            {
                G5_Recipe recipe = smelter.SelectedRecipe;
                if (recipe == null || _beltConnectedSmelters.Contains(smelter))
                {
                    continue;
                }

                foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
                {
                    G5_ItemTransfer.Transfer(_field.Hub, smelter, ingredient.Key, ingredient.Value);
                }
            }
        }

        private void TransferOutputsToHub(G5_Building source)
        {
            KeyValuePair<G5_Item, int>[] contents = source.TransferableContents.ToArray();
            foreach (KeyValuePair<G5_Item, int> item in contents)
            {
                G5_ItemTransfer.Transfer(source, _field.Hub, item.Key, item.Value);
            }
        }
    }

}
