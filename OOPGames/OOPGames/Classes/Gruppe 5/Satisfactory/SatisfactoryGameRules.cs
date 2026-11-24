using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public sealed class SatisfactoryGameRules : IGameRules2
    {
        private const int MapWidth = 120;
        private const int MapHeight = 80;
        private const int BuildingWidth = 1;
        private const int BuildingHeight = 1;
        private const int HudHeight = 56;
        private int _resourceAmount = 300;
        private int _minerCycleTicks = 35;
        private int _smelterCycleTicks = 55;
        private readonly Random _random = new Random();
        private readonly Dictionary<G5_GridPosition, G5_ResourceDeposit> _deposits =
            new Dictionary<G5_GridPosition, G5_ResourceDeposit>();
        private readonly Dictionary<G5_Building, Dictionary<G5_Item, int>> _buildingInvestments =
            new Dictionary<G5_Building, Dictionary<G5_Item, int>>();
        private readonly HashSet<G5_Building> _beltConnectedSources = new HashSet<G5_Building>();
        private readonly HashSet<(G5_ProductionBuilding Building, G5_Item Item)> _beltConnectedInputs =
            new HashSet<(G5_ProductionBuilding Building, G5_Item Item)>();
        private readonly SatisfactoryStartScreen _startScreen = new SatisfactoryStartScreen();
        private readonly SatisfactoryGameMentor _mentor = new SatisfactoryGameMentor();
        private G5_FabrikSpielfeld _field;
        private SatisfactoryFieldView _fieldView;
        private bool _gameStarted;

        public SatisfactoryGameRules()
        {
            ClearField();
        }

        public string Name => "G5 Satisfactory Rules";
        public IGameField CurrentField => _fieldView;
        public bool MovesPossible => _field != null;

        public void DoMove(IPlayMove move)
        {
            if (_field == null)
            {
                return;
            }

            if (!(move is G5_FabrikAktion action))
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
                    _mentor.Skip();
                    ClearField();
                    _field.SetStatusMeldung("WASD: bewegen | E: abbauen | Gebäude am markierten Feld bauen.");
                }

                return;
            }

            switch (action.Aktionstyp)
            {
                case G5_FabrikAktionstyp.SpielerBewegen:
                    MovePlayer(action.X, action.Y);
                    break;
                case G5_FabrikAktionstyp.RohstoffAbbauen:
                    MineFacingResource();
                    break;
                case G5_FabrikAktionstyp.GebaeudeAnsehen:
                    SelectBuilding(action.X, action.Y);
                    break;
                case G5_FabrikAktionstyp.WerkzeugWaehlen:
                    SelectBuildTool(action.Werkzeug);
                    break;
                case G5_FabrikAktionstyp.GebaeudeBauen:
                    PlaceBuilding(action.Werkzeug);
                    break;
                case G5_FabrikAktionstyp.RezeptWaehlen:
                    SelectRecipe(action.Wert);
                    break;
                case G5_FabrikAktionstyp.GebaeudeUpgraden:
                    UpgradeSelectedBuilding();
                    break;
                case G5_FabrikAktionstyp.GebaeudeVerkaufen:
                    SellSelectedBuilding();
                    break;
                case G5_FabrikAktionstyp.BaumenueUmschalten:
                case G5_FabrikAktionstyp.GebaeuderegisterUmschalten:
                    _field.ToggleBaumenue();
                    break;
                case G5_FabrikAktionstyp.TutorialUeberspringen:
                    _mentor.Skip();
                    break;
                default:
                    _field.SetStatusMeldung("Unbekannte Aktion.");
                    break;
            }
        }

        public void ClearField()
        {
            _deposits.Clear();
            _buildingInvestments.Clear();
            _beltConnectedSources.Clear();
            _beltConnectedInputs.Clear();

            G5_WeltSpielfeld world = new G5_WeltSpielfeld(MapWidth, MapHeight);
            int playerX = MapWidth / 2;
            int playerY = MapHeight / 2;
            _field = new G5_FabrikSpielfeld(world, playerX, playerY);
            CreateRandomMap(playerX, playerY);

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

            _fieldView = new SatisfactoryFieldView(_field, _startScreen, _mentor, _gameStarted);
            _field.SetStatusMeldung(
                "WASD: bewegen | E: Rohstoff abbauen | Baumenü: kaufen und am markierten Feld bauen.");
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

            if (_fieldView.TryConsumePendingBuild(out G5_Bauwerkzeug droppedTool))
            {
                PlaceBuilding(droppedTool);
            }

            _field.TickHandabbauCooldown();

            List<G5_Miner> miners = _field.InterneGebaeude.OfType<G5_Miner>().ToList();
            Dictionary<G5_Miner, long?> depositsBeforeTick = miners.ToDictionary(
                miner => miner,
                miner => miner.RemainingDeposit);

            _field.Simulation.Tick();
            UpdateVisibleDeposits(miners, depositsBeforeTick);
            TransferMaterials();
        }

        private void MovePlayer(int dx, int dy)
        {
            if (_field.MoveSpieler(dx, dy))
            {
                _field.SetStatusMeldung("Du schaust auf das markierte Feld. E baut dort Rohstoffe ab.");
            }
            else
            {
                _field.SetStatusMeldung("Dort kannst du nicht weitergehen.");
            }
        }

        private void MineFacingResource()
        {
            G5_GridPosition target = _field.ZielKachel;
            if (!_deposits.TryGetValue(target, out G5_ResourceDeposit deposit))
            {
                _field.SetStatusMeldung("Vor dir gibt es keinen abbaubaren Rohstoff.");
                return;
            }

            if (deposit.RemainingAmount == 0)
            {
                _field.SetStatusMeldung("Dieses Vorkommen ist erschöpft.");
                return;
            }

            if (!_field.KannEinlagern(1))
            {
                _field.SetStatusMeldung("Rucksack und Lager sind voll. Baue ein Lager oder schaffe Platz.");
                return;
            }

            if (!_field.TryStartHandabbau())
            {
                double verbleibendeSekunden = _field.VerbleibendeHandabbauCooldownTicks * 0.04;
                _field.SetStatusMeldung(
                    "Handabbau dauert etwa 2 Sekunden. Bitte noch " +
                    verbleibendeSekunden.ToString("0.0") + " Sekunden warten.");
                return;
            }

            int extracted = deposit.ExtractUpTo(1);
            if (extracted == 0)
            {
                _field.SetStatusMeldung("Dieses Vorkommen ist erschöpft.");
                return;
            }

            if (!_field.Welt.TryReduceRohstoff(target.X, target.Y, extracted))
            {
                throw new InvalidOperationException("Der Ressourcenbestand des Spielfelds konnte nicht aktualisiert werden.");
            }

            _field.AddInventar(deposit.Item, extracted);
            _field.SetStatusMeldung("+" + extracted + " " + ItemName(deposit.Item) + " gesammelt.");
        }

        private void SelectBuilding(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                _field.DeselectBuilding();
                return;
            }

            if (_field.SelectBuildingAt(x, y))
            {
                _field.SetStatusMeldung("Gebäudedetails geöffnet.");
            }
            else
            {
                _field.DeselectBuilding();
                _field.SetStatusMeldung("Kein Gebäude an dieser Stelle.");
            }
        }

        private void SelectBuildTool(G5_Bauwerkzeug tool)
        {
            IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.Baupreis(tool);
            if (cost.Count == 0)
            {
                _field.SetStatusMeldung("Dieses Gebäude kann nicht gebaut werden.");
                return;
            }

            _field.SetWerkzeug(tool);
            _field.SetStatusMeldung(tool + " ausgewählt. Ziehe es auf die markierte Kachel oder drücke Bauen.");
        }

        private void PlaceBuilding(G5_Bauwerkzeug tool)
        {
            if (tool == G5_Bauwerkzeug.Keines ||
                tool == G5_Bauwerkzeug.GebaeudeVerschieben ||
                !Enum.IsDefined(typeof(G5_Bauwerkzeug), tool))
            {
                _field.SetStatusMeldung("Wähle ein Gebäude im Baumenü aus.");
                return;
            }

            IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.Baupreis(tool);
            if (cost.Count == 0)
            {
                _field.SetStatusMeldung("Dieses Gebäude kann nicht gebaut werden.");
                return;
            }

            G5_GridPosition target = _field.ZielKachel;
            if (!IsBuildableArea(target.X, target.Y, BuildingWidth, BuildingHeight, null))
            {
                _field.SetStatusMeldung("Das markierte Feld ist nicht frei oder bebaubar.");
                return;
            }

            G5_Building building;
            if (tool == G5_Bauwerkzeug.Foerderer)
            {
                if (!_deposits.TryGetValue(target, out G5_ResourceDeposit deposit) ||
                    deposit.RemainingAmount == 0)
                {
                    _field.SetStatusMeldung("Ein Förderer muss auf einem verfügbaren Vorkommen stehen.");
                    return;
                }

                building = new G5_Miner(
                    target,
                    deposit,
                    outputCapacity: 20,
                    cycleTicks: _minerCycleTicks,
                    amountPerCycle: 1,
                    powerDemandPerTick: 0);
            }
            else if (ContainsDeposit(target.X, target.Y, BuildingWidth, BuildingHeight))
            {
                _field.SetStatusMeldung("Dieses Gebäude kann nicht auf einem Rohstoffvorkommen stehen.");
                return;
            }
            else if (tool == G5_Bauwerkzeug.Foerderband)
            {
                building = new G5_Conveyor(target, bufferCapacity: 10, itemsPerTick: 1);
            }
            else if (tool == G5_Bauwerkzeug.Schmelze)
            {
                building = CreateSmelter(target.X, target.Y, "Eisenerz schmelzen");
            }
            else if (tool == G5_Bauwerkzeug.Konstruktor)
            {
                G5_Constructor constructor = new G5_Constructor(
                    target,
                    G5_StarterRecipes.CreateConstructionRecipes(
                        ScaleProductionTime(_smelterCycleTicks, 3, 2),
                        powerDemandPerTick: 1),
                    inputCapacity: 30,
                    outputCapacity: 30);
                constructor.SelectRecipe("Eisenplatte herstellen");
                building = constructor;
            }
            else if (tool == G5_Bauwerkzeug.Assembler)
            {
                G5_Assembler assembler = new G5_Assembler(
                    target,
                    G5_StarterRecipes.CreateAssemblyRecipes(
                        ScaleProductionTime(_smelterCycleTicks, 2, 1),
                        powerDemandPerTick: 1),
                    inputCapacity: 30,
                    outputCapacity: 30);
                assembler.SelectRecipe("Rotor herstellen");
                building = assembler;
            }
            else if (tool == G5_Bauwerkzeug.Verbrennungsgenerator)
            {
                building = new G5_BurningGenerator(
                    target,
                    fuelCapacity: 100,
                    maxOutputPerTick: 20,
                    energyPerFuel: new[]
                    {
                        new KeyValuePair<G5_Item, int>(G5_Item.Kohle, 30)
                    });
            }
            else if (tool == G5_Bauwerkzeug.Strommast)
            {
                building = new G5_PowerPole(target, connectionRange: 8);
            }
            else if (tool == G5_Bauwerkzeug.Lager)
            {
                building = new G5_Storage(target, capacity: 500);
            }
            else
            {
                _field.SetStatusMeldung("Dieses Gebäude wird nicht unterstützt.");
                return;
            }

            if (!_field.HatRessourcen(cost))
            {
                _field.SetStatusMeldung("Nicht genügend Rohstoffe: " + FormatCost(cost) + ".");
                return;
            }

            _field.AddGebaeude(building);
            if (!_field.TryVerbraucheRessourcen(cost))
            {
                _field.RemoveGebaeude(building);
                throw new InvalidOperationException("Baukosten haben sich während des Bauens geändert.");
            }

            _buildingInvestments.Add(building, new Dictionary<G5_Item, int>(cost));
            RefreshConveyorConnections();
            _field.SetStatusMeldung(building.Name + " gebaut. Kosten: " + FormatCost(cost) + ".");
        }

        private void SelectRecipe(string recipeName)
        {
            if (!(_field.AusgewaehltesInternesGebaeude is G5_ProductionBuilding production))
            {
                _field.SetStatusMeldung("Wähle zuerst ein Produktionsgebäude aus.");
                return;
            }

            IG5_GebaeudeInfo info = ((IG5_FabrikSpielInfo)_field).AusgewaehltesGebaeude;
            if (!info.VerfuegbareRezepte.Contains(recipeName))
            {
                _field.SetStatusMeldung("Dieses Rezept ist noch gesperrt. Verbessere das Gebäude zuerst.");
                return;
            }

            if (!production.SelectRecipe(recipeName))
            {
                _field.SetStatusMeldung("Dieses Rezept kann in diesem Gebäude nicht verwendet werden.");
                return;
            }

            RefreshConveyorConnections();
            _field.SetStatusMeldung("Rezept geändert: " + recipeName + ".");
        }

        private void UpgradeSelectedBuilding()
        {
            G5_Building building = _field.AusgewaehltesInternesGebaeude;
            IG5_GebaeudeInfo info = ((IG5_FabrikSpielInfo)_field).AusgewaehltesGebaeude;
            if (building == null || info == null || !info.KannVerbessertWerden)
            {
                _field.SetStatusMeldung("Dieses Gebäude kann nicht weiter verbessert werden.");
                return;
            }

            IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.UpgradePreis(building.Level);
            if (!_field.HatRessourcen(cost))
            {
                _field.SetStatusMeldung("Für das Upgrade fehlen: " + FormatCost(cost) + ".");
                return;
            }

            G5_BuildingUpgrade upgrade;
            if (building is G5_Miner)
            {
                upgrade = new G5_BuildingUpgrade(
                    productionTicksReduction: 8,
                    outputCapacityIncrease: 5,
                    outputAmountIncrease: 1);
            }
            else if (building is G5_ProductionBuilding)
            {
                upgrade = new G5_BuildingUpgrade(
                    productionTicksReduction: 10,
                    inputCapacityIncrease: 10,
                    outputCapacityIncrease: 10,
                    powerDemandReduction: 1);
            }
            else if (building is G5_Storage)
            {
                upgrade = new G5_BuildingUpgrade(storageCapacityIncrease: 100);
            }
            else if (building is G5_BurningGenerator)
            {
                upgrade = new G5_BuildingUpgrade(powerOutputIncrease: 10);
            }
            else if (building is G5_Conveyor)
            {
                upgrade = new G5_BuildingUpgrade(
                    outputCapacityIncrease: 5,
                    transportCapacityIncrease: 1);
            }
            else
            {
                _field.SetStatusMeldung("Für dieses Gebäude gibt es kein Upgrade.");
                return;
            }

            if (!_field.TryVerbraucheRessourcen(cost))
            {
                throw new InvalidOperationException("Upgrade-Kosten haben sich während der Verbesserung geändert.");
            }

            building.Upgrade(upgrade);
            if (!_buildingInvestments.TryGetValue(building, out Dictionary<G5_Item, int> investment))
            {
                investment = new Dictionary<G5_Item, int>();
                _buildingInvestments.Add(building, investment);
            }

            AddCost(investment, cost);
            _field.SetStatusMeldung(building.Name + " auf Level " + building.Level + " verbessert.");
        }

        private void SellSelectedBuilding()
        {
            G5_Building building = _field.AusgewaehltesInternesGebaeude;
            if (building == null)
            {
                _field.SetStatusMeldung("Wähle zuerst ein Gebäude aus.");
                return;
            }

            if (!_buildingInvestments.TryGetValue(building, out Dictionary<G5_Item, int> investment))
            {
                investment = new Dictionary<G5_Item, int>();
            }

            G5_Storage storage = building as G5_Storage;
            if (storage != null && storage.UsedCapacity > 0)
            {
                _field.SetStatusMeldung("Leere das Lager, bevor du es verkaufst.");
                return;
            }

            Dictionary<G5_Item, int> refund = investment.ToDictionary(
                item => item.Key,
                item => item.Value / 2);
            if (!_field.KannEinlagern(refund, storage))
            {
                _field.SetStatusMeldung("Rucksack und Lager sind voll. Schaffe Platz für die Rückerstattung.");
                return;
            }

            if (!_field.RemoveGebaeude(building))
            {
                _field.SetStatusMeldung("Das Gebäude konnte nicht verkauft werden.");
                return;
            }

            _buildingInvestments.Remove(building);
            foreach (KeyValuePair<G5_Item, int> item in refund)
            {
                _field.AddInventar(item.Key, item.Value);
            }

            RefreshConveyorConnections();
            _field.SetStatusMeldung("Gebäude verkauft: 50 % der Bau- und Upgrade-Rohstoffe wurden erstattet.");
        }

        private bool IsBuildableArea(int x, int y, int width, int height, G5_Building ignoredBuilding)
        {
            if (x < 0 || y < 0 || x + width > MapWidth || y + height > MapHeight)
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

        private void RefreshConveyorConnections()
        {
            List<G5_Building> buildings = _field.InterneGebaeude.ToList();
            List<G5_Conveyor> conveyors = buildings.OfType<G5_Conveyor>().ToList();
            foreach (G5_Conveyor conveyor in conveyors)
            {
                conveyor.Disconnect();
            }

            _beltConnectedSources.Clear();
            _beltConnectedInputs.Clear();
            HashSet<G5_Conveyor> usedBelts = new HashSet<G5_Conveyor>();

            foreach (G5_Building source in buildings.Where(IsProductionSource))
            {
                if (!TryGetProducedItem(source, out G5_Item item))
                {
                    continue;
                }

                IEnumerable<G5_Building> destinations = buildings.OfType<G5_ProductionBuilding>()
                    .Where(production => production.SelectedRecipe != null &&
                        production.SelectedRecipe.Ingredients.ContainsKey(item))
                    .Cast<G5_Building>()
                    .Concat(buildings.OfType<G5_Storage>());

                foreach (G5_Building destination in destinations)
                {
                    if (!TryFindConveyorPath(source, destination, conveyors, usedBelts, out List<G5_Building> path))
                    {
                        continue;
                    }

                    for (int index = 1; index < path.Count - 1; index++)
                    {
                        G5_Conveyor conveyor = (G5_Conveyor)path[index];
                        conveyor.Connect(path[index - 1], path[index + 1]);
                        usedBelts.Add(conveyor);
                    }

                    _beltConnectedSources.Add(source);
                    if (destination is G5_ProductionBuilding production)
                    {
                        _beltConnectedInputs.Add((production, item));
                    }

                    break;
                }
            }
        }

        private void CreateRandomMap(int playerX, int playerY)
        {
            AddDeposit(playerX + 3, playerY, G5_Rohstofftyp.Eisen, G5_Item.Eisenerz, _resourceAmount);
            AddDeposit(playerX - 3, playerY, G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz, _resourceAmount);
            AddDeposit(playerX, playerY + 3, G5_Rohstofftyp.Holz, G5_Item.Holz, 15);
            AddDeposit(playerX, playerY - 3, G5_Rohstofftyp.Kohle, G5_Item.Kohle, _resourceAmount);

            (G5_Rohstofftyp Type, G5_Item Item)[] resources =
            {
                (G5_Rohstofftyp.Eisen, G5_Item.Eisenerz),
                (G5_Rohstofftyp.Kupfer, G5_Item.Kupfererz),
                (G5_Rohstofftyp.Kohle, G5_Item.Kohle),
                (G5_Rohstofftyp.Kalkstein, G5_Item.Kalkstein),
                (G5_Rohstofftyp.Quarz, G5_Item.Quarz),
                (G5_Rohstofftyp.Schwefel, G5_Item.Schwefel),
                (G5_Rohstofftyp.Uran, G5_Item.Uran)
            };

            int placed = 0;
            for (int attempt = 0; attempt < 12000 && placed < 180; attempt++)
            {
                int x = _random.Next(2, MapWidth - 2);
                int y = _random.Next(2, MapHeight - 2);
                if (Math.Abs(x - playerX) <= 6 && Math.Abs(y - playerY) <= 6)
                {
                    continue;
                }

                G5_GridPosition position = new G5_GridPosition(x, y);
                if (_deposits.ContainsKey(position))
                {
                    continue;
                }

                (G5_Rohstofftyp Type, G5_Item Item) resource = resources[_random.Next(resources.Length)];
                AddDeposit(x, y, resource.Type, resource.Item, _resourceAmount);
                placed++;
            }

            int trees = 0;
            for (int attempt = 0; attempt < 16000 && trees < 320; attempt++)
            {
                int x = _random.Next(1, MapWidth - 1);
                int y = _random.Next(1, MapHeight - 1);
                G5_GridPosition position = new G5_GridPosition(x, y);
                if ((Math.Abs(x - playerX) <= 2 && Math.Abs(y - playerY) <= 2) ||
                    _deposits.ContainsKey(position))
                {
                    continue;
                }

                AddDeposit(x, y, G5_Rohstofftyp.Holz, G5_Item.Holz, _random.Next(8, 16));
                trees++;
            }

            CreateRandomObstacles(playerX, playerY);
        }

        private void CreateRandomObstacles(int playerX, int playerY)
        {
            int placed = 0;
            for (int attempt = 0; attempt < 12000 && placed < 420; attempt++)
            {
                int x = _random.Next(MapWidth);
                int y = _random.Next(MapHeight);
                G5_GridPosition position = new G5_GridPosition(x, y);
                if ((Math.Abs(x - playerX) <= 3 && Math.Abs(y - playerY) <= 3) ||
                    _deposits.ContainsKey(position) ||
                    _field.InterneGebaeude.Any(building => building.Position == position))
                {
                    continue;
                }

                if (_field.Welt.TrySetGelaende(x, y, G5_Gelaendeart.Hindernis))
                {
                    placed++;
                }
            }
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

                if (!_field.Welt.TryReduceRohstoff(
                    miner.Position.X,
                    miner.Position.Y,
                    checked((int)(before.Value - after.Value))))
                {
                    throw new InvalidOperationException("Der Ressourcenbestand des Spielfelds konnte nicht aktualisiert werden.");
                }
            }
        }

        private void TransferMaterials()
        {
            foreach (G5_Miner miner in _field.InterneGebaeude.OfType<G5_Miner>())
            {
                if (!_beltConnectedSources.Contains(miner))
                {
                    TransferOutputs(miner);
                }
            }

            foreach (G5_ProductionBuilding production in _field.InterneGebaeude.OfType<G5_ProductionBuilding>())
            {
                G5_Recipe recipe = production.SelectedRecipe;
                if (recipe == null)
                {
                    continue;
                }

                if (!_beltConnectedSources.Contains(production))
                {
                    TransferOutputs(production);
                }

                foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
                {
                    if (_beltConnectedInputs.Contains((production, ingredient.Key)))
                    {
                        continue;
                    }

                    production.InputContents.TryGetValue(ingredient.Key, out int currentAmount);
                    int amountNeeded = Math.Max(0, ingredient.Value - currentAmount);
                    foreach (G5_Storage storage in _field.InterneGebaeude.OfType<G5_Storage>())
                    {
                        if (amountNeeded == 0)
                        {
                            break;
                        }

                        amountNeeded -= G5_ItemTransfer.Transfer(storage, production, ingredient.Key, amountNeeded);
                    }

                    while (amountNeeded > 0 &&
                        _field.Menge(ingredient.Key) > 0 &&
                        production.CanReceive(ingredient.Key, 1) &&
                        _field.TryVerbraucheRessourcen(SingleItemCost(ingredient.Key)))
                    {
                        if (!production.TryReceive(ingredient.Key, 1))
                        {
                            _field.AddInventar(ingredient.Key, 1);
                            break;
                        }

                        amountNeeded--;
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

                while (generator.FuelContents.Values.Sum() < 5 &&
                    _field.Menge(G5_Item.Kohle) > 0 &&
                    _field.TryVerbraucheRessourcen(SingleItemCost(G5_Item.Kohle)))
                {
                    if (!generator.TryAddFuel(G5_Item.Kohle, 1))
                    {
                        _field.AddInventar(G5_Item.Kohle, 1);
                        break;
                    }
                }
            }
        }

        private void TransferOutputs(G5_Building source)
        {
            foreach (KeyValuePair<G5_Item, int> item in source.TransferableContents.ToArray())
            {
                foreach (G5_Storage storage in _field.InterneGebaeude.OfType<G5_Storage>())
                {
                    if (source.CountOf(item.Key) == 0)
                    {
                        break;
                    }

                    G5_ItemTransfer.Transfer(source, storage, item.Key, source.CountOf(item.Key));
                }

                int remaining = source.CountOf(item.Key);
                int amountToTransfer = Math.Min(remaining, _field.FreieAufnahmekapazitaet());
                if (amountToTransfer > 0 && source.TryExtract(item.Key, amountToTransfer))
                {
                    _field.AddInventar(item.Key, amountToTransfer);
                }
            }
        }

        private static bool IsProductionSource(G5_Building building)
        {
            return building is G5_Miner || building is G5_ProductionBuilding;
        }

        private static bool TryGetProducedItem(G5_Building building, out G5_Item item)
        {
            if (building is G5_Miner miner)
            {
                item = miner.Resource;
                return true;
            }

            if (building is G5_ProductionBuilding production && production.SelectedRecipe != null)
            {
                item = production.SelectedRecipe.Output;
                return true;
            }

            item = default;
            return false;
        }

        private static int ScaleProductionTime(int baseTicks, int numerator, int denominator)
        {
            return checked((baseTicks * numerator + denominator - 1) / denominator);
        }

        private static bool TryFindConveyorPath(
            G5_Building source,
            G5_Building destination,
            IReadOnlyList<G5_Conveyor> conveyors,
            IReadOnlySet<G5_Conveyor> usedBelts,
            out List<G5_Building> path)
        {
            path = null;
            Queue<G5_Conveyor> queue = new Queue<G5_Conveyor>();
            Dictionary<G5_Conveyor, G5_Conveyor> previous = new Dictionary<G5_Conveyor, G5_Conveyor>();
            foreach (G5_Conveyor conveyor in conveyors)
            {
                if (!usedBelts.Contains(conveyor) && AreAdjacent(source, conveyor))
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
                    if (!usedBelts.Contains(candidate) &&
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

        private static IReadOnlyDictionary<G5_Item, int> SingleItemCost(G5_Item item)
        {
            return new Dictionary<G5_Item, int> { [item] = 1 };
        }

        private static void AddCost(Dictionary<G5_Item, int> destination, IReadOnlyDictionary<G5_Item, int> cost)
        {
            foreach (KeyValuePair<G5_Item, int> item in cost)
            {
                destination[item.Key] = checked(
                    destination.TryGetValue(item.Key, out int current) ? current + item.Value : item.Value);
            }
        }

        private static string FormatCost(IReadOnlyDictionary<G5_Item, int> cost)
        {
            return string.Join(", ", cost.Select(item => item.Value + " " + ItemName(item.Key)));
        }

        private static string ItemName(G5_Item item)
        {
            switch (item)
            {
                case G5_Item.Eisenerz: return "Eisenerz";
                case G5_Item.Kupfererz: return "Kupfererz";
                case G5_Item.Kohle: return "Kohle";
                case G5_Item.Holz: return "Holz";
                case G5_Item.Eisenbarren: return "Eisenbarren";
                case G5_Item.Kupferbarren: return "Kupferbarren";
                default: return item.ToString();
            }
        }
    }
}
