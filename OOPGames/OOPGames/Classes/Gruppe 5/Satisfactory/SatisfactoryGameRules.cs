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
        private int _constructorCycleTicks = 55;
        private bool _smeltersEnabled;
        private bool _constructorsEnabled;
        private bool _conveyorsEnabled;
        private G5_Building _selectedConveyorSource;
        private G5_Building _selectedBuilding;

        private readonly Dictionary<G5_GridPosition, G5_ResourceDeposit> _deposits =
            new Dictionary<G5_GridPosition, G5_ResourceDeposit>();
        private readonly SatisfactoryStartScreen _startScreen = new SatisfactoryStartScreen();
        private readonly SatisfactoryGameMentor _mentor = new SatisfactoryGameMentor();
        private G5_FabrikSpielfeld _field;
        private SatisfactoryFieldView _fieldView;
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
                    _startScreen.HandleClick(action.BildschirmX, action.BildschirmY))
                {
                    SatisfactoryStartScreenSettings settings = _startScreen.SelectedSettings;
                    _resourceAmount = settings.ResourceAmount;
                    _minerCycleTicks = settings.MinerCycleTicks;
                    _smelterCycleTicks = settings.SmelterCycleTicks;
                    _constructorCycleTicks = settings.ConstructorCycleTicks;
                    _smeltersEnabled = settings.SmeltersEnabled;
                    _constructorsEnabled = settings.ConstructorsEnabled;
                    _conveyorsEnabled = settings.ConveyorsEnabled;
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
                if (_mentor.TryInspectHub(action.X, action.Y))
                {
                    _field.SetStatusMeldung(_mentor.Message);
                    return;
                }

                if (HandleBuildingMenuClick(action))
                {
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
            _field.SetFoerderbaenderAktiv(_conveyorsEnabled);
            _selectedConveyorSource = null;
            _selectedBuilding = null;
            _fieldView = new SatisfactoryFieldView(_field, _startScreen, _mentor, _gameStarted);
            AddDeposit(5, 2, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, _resourceAmount);
            AddDeposit(5, 9, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, _resourceAmount);
            AddDeposit(17, 2, G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz, _resourceAmount);
            AddDeposit(17, 9, G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz, _resourceAmount);

            _field.SetStatusMeldung(GetBuildingControlsStatus());
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
            if (!_conveyorsEnabled)
            {
                TransferMaterials();
            }

            if (_deposits.TryGetValue(new G5_GridPosition(5, 2), out G5_ResourceDeposit deposit) &&
                _mentor.ObserveMining(deposit.RemainingAmount.Value))
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

            if ((tool == G5_Bauwerkzeug.Schmelze || tool == G5_Bauwerkzeug.Kupferschmelze) &&
                !_smeltersEnabled)
            {
                _field.SetStatusMeldung("Schmelzen sind in den Einstellungen deaktiviert.");
                return;
            }

            if (tool == G5_Bauwerkzeug.Konstruktor && !_constructorsEnabled)
            {
                _field.SetStatusMeldung("Konstruktoren sind in den Einstellungen deaktiviert.");
                return;
            }

            if (tool == G5_Bauwerkzeug.Foerderband && !_conveyorsEnabled)
            {
                _field.SetStatusMeldung("Förderbänder sind in den Einstellungen deaktiviert.");
                return;
            }

            _selectedConveyorSource = null;
            _field.SetAusgewaehltenAusgang(null);
            if (tool == G5_Bauwerkzeug.Keines)
            {
                CloseSelectedBuilding();
            }
            _field.SetWerkzeug(tool);
            string status = tool == G5_Bauwerkzeug.Keines
                ? "Bauwerkzeug abgewählt."
                : tool == G5_Bauwerkzeug.Foerderband
                    ? "Förderband-Modus: roten Ausgang, dann blauen Eingang anklicken."
                    : tool + " ausgewählt. Klicke auf eine passende freie Fläche.";
            _field.SetStatusMeldung(status);
        }

        private void PlaceBuilding(int x, int y)
        {
            G5_Bauwerkzeug tool = ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug;
            if (tool == G5_Bauwerkzeug.Foerderband)
            {
                HandleConveyorPortClick(x, y);
                return;
            }

            if (!_mentor.CanPlaceBuilding(tool, x, y, out string mentorMessage))
            {
                _field.SetStatusMeldung(mentorMessage);
                return;
            }

            if (tool == G5_Bauwerkzeug.Keines)
            {
                _field.SetStatusMeldung(_conveyorsEnabled
                    ? "Wähle F, S, K oder C zum Bauen oder B zum Verbinden."
                    : "Wähle zuerst ein verfügbares Bauwerkzeug: F, S, K oder C.");
                return;
            }

            if (!IsBuildableArea(x, y))
            {
                _field.SetStatusMeldung("Dort ist nicht genug freie, bebaubare Fläche.");
                return;
            }

            G5_Building building;
            if (tool == G5_Bauwerkzeug.Foerderer)
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
            else if (tool == G5_Bauwerkzeug.Schmelze || tool == G5_Bauwerkzeug.Kupferschmelze)
            {
                if (!_smeltersEnabled)
                {
                    _field.SetStatusMeldung("Schmelzen sind in den Einstellungen deaktiviert.");
                    return;
                }

                if (ContainsDeposit(x, y))
                {
                    _field.SetStatusMeldung("Eine Schmelze kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                string recipeName = tool == G5_Bauwerkzeug.Kupferschmelze
                    ? "Kupfererz schmelzen"
                    : "Eisenerz schmelzen";
                building = CreateSmelter(x, y, recipeName);
            }
            else if (tool == G5_Bauwerkzeug.Konstruktor)
            {
                if (!_constructorsEnabled)
                {
                    _field.SetStatusMeldung("Konstruktoren sind in den Einstellungen deaktiviert.");
                    return;
                }

                if (ContainsDeposit(x, y))
                {
                    _field.SetStatusMeldung("Ein Konstruktor kann nicht auf einem Rohstofffeld stehen.");
                    return;
                }

                building = CreateConstructor(x, y);
            }
            else
            {
                _field.SetStatusMeldung("Dieses Bauwerkzeug wird nicht unterstützt.");
                return;
            }

            if (OverlapsBuilding(x, y, building.Width, building.Height))
            {
                _field.SetStatusMeldung("Gebäude dürfen sich nicht überlappen.");
                return;
            }

            _field.AddGebaeude(building);
            long depositAmount = building is G5_Miner
                ? _deposits[new G5_GridPosition(x, y)].RemainingAmount.Value
                : 0;
            _mentor.BuildingPlaced(tool, x, y, depositAmount);
            _field.SetStatusMeldung(building.Name + " gebaut.");
        }

        private bool HandleBuildingMenuClick(IG5_FabrikAktion action)
        {
            if (_selectedBuilding != null)
            {
                if (SatisfactoryBuildingMenuLayout.IsCloseButton(action.BildschirmX, action.BildschirmY))
                {
                    CloseSelectedBuilding();
                    return true;
                }

                if (_selectedBuilding is G5_ProductionBuilding productionBuilding &&
                    SatisfactoryBuildingMenuLayout.TryGetRecipeIndex(
                        action.BildschirmX,
                        action.BildschirmY,
                        productionBuilding.Recipes.Count,
                        out int recipeIndex))
                {
                    G5_Recipe recipe = productionBuilding.Recipes[recipeIndex];
                    if (!productionBuilding.SelectRecipe(recipe.Name))
                    {
                        throw new InvalidOperationException("Ein angezeigtes Produktionsrezept konnte nicht gewählt werden.");
                    }

                    _field.SetStatusMeldung("Rezept gewählt: " + recipe.Name + ".");
                    return true;
                }

                if (_conveyorsEnabled &&
                    ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug == G5_Bauwerkzeug.Foerderband &&
                    SatisfactoryBuildingMenuLayout.Contains(action.BildschirmX, action.BildschirmY))
                {
                    return false;
                }

                if (SatisfactoryBuildingMenuLayout.Contains(action.BildschirmX, action.BildschirmY))
                {
                    return true;
                }
            }

            if (((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug != G5_Bauwerkzeug.Foerderband)
            {
                G5_Building building = FindBuildingAt(action.X, action.Y);
                if (building != null)
                {
                    _selectedBuilding = building;
                    _field.SetAusgewaehltesGebaeude(building);
                    _field.SetStatusMeldung(building.Name + " ausgewählt.");
                    return true;
                }
            }

            if (_selectedBuilding != null)
            {
                CloseSelectedBuilding();
            }

            return false;
        }

        private G5_Building FindBuildingAt(int x, int y)
        {
            return _field.InterneGebaeude.FirstOrDefault(building =>
                x >= building.Position.X &&
                x < building.Position.X + building.Width &&
                y >= building.Position.Y &&
                y < building.Position.Y + building.Height);
        }

        private void CloseSelectedBuilding()
        {
            _selectedBuilding = null;
            _field.SetAusgewaehltesGebaeude(null);
        }

        private void HandleConveyorPortClick(int x, int y)
        {
            G5_Building destination = FindInputAt(x, y);
            if (_selectedConveyorSource != null && destination != null)
            {
                if (ReferenceEquals(_selectedConveyorSource, destination))
                {
                    _field.SetStatusMeldung("Wähle einen Eingang eines anderen Gebäudes.");
                    return;
                }

                ConnectConveyorRoute(_selectedConveyorSource, destination);
                return;
            }

            G5_Building source = FindOutputAt(x, y);
            if (source != null)
            {
                _selectedConveyorSource = source;
                _field.SetAusgewaehltenAusgang(source);
                _field.SetStatusMeldung(
                    source.Name + " Ausgang gewählt. Klicke jetzt auf einen blauen Eingang.");
                return;
            }

            _field.SetStatusMeldung(_selectedConveyorSource == null
                ? "Klicke zuerst auf einen roten Gebäudeausgang."
                : "Klicke auf einen blauen Eingang oder wähle einen anderen roten Ausgang.");
        }

        private G5_Building FindOutputAt(int x, int y)
        {
            return _field.InterneGebaeude.FirstOrDefault(building =>
                HasOutputPort(building) &&
                building.Position.X + building.Width - 1 == x &&
                building.Position.Y + building.Height / 2 == y);
        }

        private G5_Building FindInputAt(int x, int y)
        {
            return _field.InterneGebaeude.FirstOrDefault(building =>
                HasInputPort(building) &&
                building.Position.X == x &&
                building.Position.Y + building.Height / 2 == y);
        }

        private static bool HasOutputPort(G5_Building building)
        {
            return building is G5_Storage ||
                building is G5_Miner ||
                building is G5_ProductionBuilding;
        }

        private static bool HasInputPort(G5_Building building)
        {
            return building is G5_Storage || building is G5_ProductionBuilding;
        }

        private void ConnectConveyorRoute(G5_Building source, G5_Building destination)
        {
            List<G5_GridPosition> route = FindConveyorRoute(source, destination);
            if (route == null)
            {
                _field.SetStatusMeldung("Kein freier Förderbandweg zwischen diesen Anschlüssen.");
                return;
            }

            List<G5_Conveyor> conveyors = route
                .Select(position => new G5_Conveyor(position, bufferCapacity: 20, itemsPerTick: 1))
                .ToList();

            for (int i = 0; i < conveyors.Count; i++)
            {
                IG5_ItemEndpoint previous = i == 0 ? source : conveyors[i - 1];
                IG5_ItemEndpoint next = i == conveyors.Count - 1 ? destination : conveyors[i + 1];
                conveyors[i].Connect(previous, next);
            }

            foreach (G5_Conveyor conveyor in conveyors)
            {
                _field.AddGebaeude(conveyor);
            }

            _field.AddFoerderbandRoute(route);
            _selectedConveyorSource = null;
            _field.SetAusgewaehltenAusgang(null);
            _field.SetStatusMeldung("Förderband mit " + conveyors.Count + " Segmenten verbunden.");
        }

        private List<G5_GridPosition> FindConveyorRoute(G5_Building source, G5_Building destination)
        {
            G5_GridPosition start = new G5_GridPosition(
                source.Position.X + source.Width,
                source.Position.Y + source.Height / 2);
            G5_GridPosition goal = new G5_GridPosition(
                destination.Position.X - 1,
                destination.Position.Y + destination.Height / 2);

            if (!IsConveyorTileAvailable(start.X, start.Y) ||
                !IsConveyorTileAvailable(goal.X, goal.Y))
            {
                return null;
            }

            int startIndex = start.Y * MapWidth + start.X;
            int goalIndex = goal.Y * MapWidth + goal.X;
            int[] previous = Enumerable.Repeat(-2, MapWidth * MapHeight).ToArray();
            Queue<int> pending = new Queue<int>();
            previous[startIndex] = -1;
            pending.Enqueue(startIndex);

            int[] deltaX = { 1, -1, 0, 0 };
            int[] deltaY = { 0, 0, 1, -1 };
            while (pending.Count > 0 && previous[goalIndex] == -2)
            {
                int current = pending.Dequeue();
                int currentX = current % MapWidth;
                int currentY = current / MapWidth;

                for (int direction = 0; direction < deltaX.Length; direction++)
                {
                    int nextX = currentX + deltaX[direction];
                    int nextY = currentY + deltaY[direction];
                    if (!IsConveyorTileAvailable(nextX, nextY))
                    {
                        continue;
                    }

                    int nextIndex = nextY * MapWidth + nextX;
                    if (previous[nextIndex] != -2)
                    {
                        continue;
                    }

                    previous[nextIndex] = current;
                    pending.Enqueue(nextIndex);
                }
            }

            if (previous[goalIndex] == -2)
            {
                return null;
            }

            List<G5_GridPosition> route = new List<G5_GridPosition>();
            for (int current = goalIndex; current >= 0; current = previous[current])
            {
                route.Add(new G5_GridPosition(current % MapWidth, current / MapWidth));
            }

            route.Reverse();
            return route;
        }

        private bool IsConveyorTileAvailable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight ||
                !_field.TryGetKachel(x, y, out G5_Kachel tile))
            {
                return false;
            }

            IG5_KachelInfo tileInfo = (IG5_KachelInfo)tile;
            if (tileInfo.Gelaende != G5_Gelaendeart.Begehbar ||
                tileInfo.Rohstoff != G5_Rohstofftyp.Keiner)
            {
                return false;
            }

            return !_field.InterneGebaeude.Any(building =>
                x >= building.Position.X &&
                x < building.Position.X + building.Width &&
                y >= building.Position.Y &&
                y < building.Position.Y + building.Height);
        }

        private bool IsBuildableArea(int x, int y)
        {
            if (x < 0 || y < 0 ||
                x + BuildingWidth > MapWidth ||
                y + BuildingHeight > MapHeight)
            {
                return false;
            }

            for (int tileY = y; tileY < y + BuildingHeight; tileY++)
            {
                for (int tileX = x; tileX < x + BuildingWidth; tileX++)
                {
                    if (!_field.TryGetKachel(tileX, tileY, out G5_Kachel tile) ||
                        ((IG5_KachelInfo)tile).Gelaende != G5_Gelaendeart.Begehbar)
                    {
                        return false;
                    }
                }
            }

            return !OverlapsBuilding(x, y, BuildingWidth, BuildingHeight);
        }

        private bool ContainsDeposit(int x, int y)
        {
            for (int tileY = y; tileY < y + BuildingHeight; tileY++)
            {
                for (int tileX = x; tileX < x + BuildingWidth; tileX++)
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

        private bool OverlapsBuilding(int x, int y, int width, int height)
        {
            return _field.InterneGebaeude.Any(existing =>
                x < existing.Position.X + existing.Width &&
                existing.Position.X < x + width &&
                y < existing.Position.Y + existing.Height &&
                existing.Position.Y < y + height);
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

        private G5_Constructor CreateConstructor(int x, int y)
        {
            G5_Constructor constructor = new G5_Constructor(
                new G5_GridPosition(x, y),
                G5_StarterRecipes.CreateConstructionRecipes(_constructorCycleTicks, powerDemandPerTick: 0),
                inputCapacity: 20,
                outputCapacity: 20);
            if (!constructor.SelectRecipe("Eisenplatte herstellen"))
            {
                throw new InvalidOperationException("Das Satisfactory-Konstruktionsrezept ist nicht verfügbar.");
            }

            return constructor;
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
                TransferOutputsToHub(miner);
            }

            foreach (G5_ProductionBuilding productionBuilding in
                     _field.InterneGebaeude.OfType<G5_ProductionBuilding>())
            {
                TransferOutputsToHub(productionBuilding);
            }

            foreach (G5_ProductionBuilding productionBuilding in
                     _field.InterneGebaeude.OfType<G5_ProductionBuilding>())
            {
                G5_Recipe recipe = productionBuilding.SelectedRecipe;
                if (recipe == null)
                {
                    continue;
                }

                foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
                {
                    G5_ItemTransfer.Transfer(
                        _field.Hub,
                        productionBuilding,
                        ingredient.Key,
                        ingredient.Value);
                }
            }
        }

        private string GetBuildingControlsStatus()
        {
            if (!_gameStarted)
            {
                return "F Förderer; Schmelzen und Konstruktoren in den Einstellungen aktivieren.";
            }

            List<string> controls = new List<string> { "F Förderer" };
            if (_smeltersEnabled)
            {
                controls.Add("S Eisen- oder K Kupferschmelze");
            }

            if (_constructorsEnabled)
            {
                controls.Add("C Konstruktor");
            }

            controls.Add(_conveyorsEnabled
                ? "B Förderbänder manuell verbinden"
                : "Material wird automatisch übergeben");
            return "Verfügbar: " + string.Join(", ", controls) + ".";
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
