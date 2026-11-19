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
            _field.SetStatusMeldung("Satisfactory: F Förderer, S Schmelze, dann auf die Karte klicken.");
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

            _field.SetWerkzeug(tool);
            _field.SetStatusMeldung(tool == G5_Bauwerkzeug.Keines
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
                _field.SetStatusMeldung("Wähle zuerst F für Förderer oder S für Schmelze.");
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
            else if (tool == G5_Bauwerkzeug.Schmelze)
            {
                if (ContainsDeposit(x, y))
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
                TransferOutputsToHub(miner);
            }

            foreach (G5_Smelter smelter in _field.InterneGebaeude.OfType<G5_Smelter>())
            {
                TransferOutputsToHub(smelter);
            }

            foreach (G5_Smelter smelter in _field.InterneGebaeude.OfType<G5_Smelter>())
            {
                G5_Recipe recipe = smelter.SelectedRecipe;
                if (recipe == null)
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
