using System;
using System.Linq;

namespace OOPGames
{
    // Kümmert sich um Futterrezepte, Stallaktionen und zeitabhängige Tierprodukte.
    public sealed partial class HayDayField
    {
        private void ProduceFeed(int recipe)
        {
            string product;
            int wheat;
            int corn;
            int carrots = 0;
            int soybeans = 0;
            if (recipe == 0)
            {
                product = Feed;
                wheat = 1;
                corn = 1;
            }
            else if (recipe == 1)
            {
                product = CowFeed;
                wheat = 2;
                corn = 1;
            }
            else if (recipe == 2)
            {
                product = SheepFeed;
                wheat = 1;
                corn = 2;
            }
            else if (recipe == 3)
            {
                product = PigFeed;
                wheat = 0;
                corn = 1;
                carrots = 1;
            }
            else
            {
                product = GoatFeed;
                wheat = 1;
                corn = 0;
                soybeans = 1;
            }

            if (Inventory[product] >= BarnCapacity)
            {
                SetStatus("Der Lagerplatz für " + product + " ist voll.");
            }
            else if (Inventory[Wheat] >= wheat && Inventory[Corn] >= corn
                && Inventory[Carrots] >= carrots && Inventory[Soybeans] >= soybeans)
            {
                if (wheat > 0) RemoveInventory(Wheat, wheat);
                if (corn > 0) RemoveInventory(Corn, corn);
                if (carrots > 0) RemoveInventory(Carrots, carrots);
                if (soybeans > 0) RemoveInventory(Soybeans, soybeans);
                Inventory[product]++;
                string ingredients = string.Join(" und ", new[]
                {
                    wheat > 0 ? wheat + " Weizen" : null,
                    corn > 0 ? corn + " Mais" : null,
                    carrots > 0 ? carrots + " Karotten" : null,
                    soybeans > 0 ? soybeans + " Soja" : null
                }.Where(value => value != null));
                SetStatus(ingredients + " zu " + product + " verarbeitet.");
            }
            else
            {
                string missing = string.Join(" und ", new[]
                {
                    Inventory[Wheat] < wheat ? wheat + " Weizen" : null,
                    Inventory[Corn] < corn ? corn + " Mais" : null,
                    Inventory[Carrots] < carrots ? carrots + " Karotten" : null,
                    Inventory[Soybeans] < soybeans ? soybeans + " Soja" : null
                }.Where(value => value != null));
                SetStatus("Für " + product + " fehlen " + missing + ".");
            }
        }

        private void ProduceCheese()
        {
            if (Inventory[Cheese] >= BarnCapacity)
            {
                SetStatus("Der Lagerplatz für Käse ist voll.");
            }
            else if (RemoveInventory(Milk, 2))
            {
                Inventory[Cheese]++;
                SetStatus("2 Milch zu 1 Käse verarbeitet.");
            }
            else
            {
                SetStatus("Die Molkerei braucht 2 Milch.");
            }
        }

        public bool FeedAnimal(string kind)
        {
            string feedItem;
            int animalCount;
            switch (kind)
            {
                case "Coop":
                    feedItem = Feed;
                    animalCount = ChickenCount;
                    break;
                case "CowBarn":
                    feedItem = CowFeed;
                    animalCount = CowCount;
                    break;
                case "PigBarn":
                    feedItem = PigFeed;
                    animalCount = PigCount;
                    break;
                case "SheepBarn":
                    feedItem = SheepFeed;
                    animalCount = SheepCount;
                    break;
                case "GoatBarn":
                    feedItem = GoatFeed;
                    animalCount = GoatCount;
                    break;
                default:
                    return false;
            }

            if (animalCount <= 0)
            {
                SetStatus("In diesem Stall sind keine Tiere.");
                return false;
            }

            string productLabel;
            int readyProductCount;
            switch (kind)
            {
                case "Coop":
                    productLabel = "Eier";
                    readyProductCount = _eggReady;
                    break;
                case "CowBarn":
                    productLabel = "Milch";
                    readyProductCount = _milkReady;
                    break;
                case "PigBarn":
                    productLabel = "Fleisch";
                    readyProductCount = _porkReady;
                    break;
                case "SheepBarn":
                    productLabel = "Wolle";
                    readyProductCount = _woolReady;
                    break;
                case "GoatBarn":
                    productLabel = "Ziegenmilch";
                    readyProductCount = _goatMilkReady;
                    break;
                default:
                    return false;
            }
            if (readyProductCount > 0)
            {
                SetStatus("Bitte zuerst " + readyProductCount + " " + productLabel +
                    " einsammeln.");
                return false;
            }

            int remainingFeedSeconds;
            switch (kind)
            {
                case "Coop": remainingFeedSeconds = _feedProgressSeconds; break;
                case "CowBarn": remainingFeedSeconds = _cowFeedProgressSeconds; break;
                case "PigBarn": remainingFeedSeconds = _pigFeedProgressSeconds; break;
                case "SheepBarn": remainingFeedSeconds = _sheepFeedProgressSeconds; break;
                case "GoatBarn": remainingFeedSeconds = _goatFeedProgressSeconds; break;
                default: remainingFeedSeconds = 0; break;
            }
            if (remainingFeedSeconds > 0)
            {
                SetStatus("Die Tiere sind noch gefüttert.");
                return false;
            }

            if (Inventory[feedItem] < animalCount)
            {
                SetStatus("Für " + animalCount + " Tiere werden " + animalCount +
                    " " + feedItem + " benötigt. Es sind nur " + Inventory[feedItem] + " vorhanden.");
                return false;
            }

            RemoveInventory(feedItem, animalCount);
            switch (kind)
            {
                case "Coop":
                    _feedProgressSeconds = 300;
                    break;
                case "CowBarn":
                    _cowFeedProgressSeconds = 300;
                    break;
                case "PigBarn":
                    _pigFeedProgressSeconds = 300;
                    break;
                case "SheepBarn":
                    _sheepFeedProgressSeconds = 300;
                    break;
                case "GoatBarn":
                    _goatFeedProgressSeconds = 300;
                    break;
            }
            SetStatus(animalCount + " " + feedItem + " für " + animalCount +
                " Tiere verteilt.");
            IsDirty = true;
            return true;
        }

        public string GetAnimalProductionStatus(string kind)
        {
            int animalCount;
            string productLabel;
            int ready;
            int feedProgressSeconds;

            switch (kind)
            {
                case "Coop":
                    animalCount = ChickenCount;
                    productLabel = "Eier";
                    ready = _eggReady;
                    feedProgressSeconds = _feedProgressSeconds;
                    break;
                case "CowBarn":
                    animalCount = CowCount;
                    productLabel = "Milch";
                    ready = _milkReady;
                    feedProgressSeconds = _cowFeedProgressSeconds;
                    break;
                case "PigBarn":
                    animalCount = PigCount;
                    productLabel = "Fleisch";
                    ready = _porkReady;
                    feedProgressSeconds = _pigFeedProgressSeconds;
                    break;
                case "SheepBarn":
                    animalCount = SheepCount;
                    productLabel = "Wolle";
                    ready = _woolReady;
                    feedProgressSeconds = _sheepFeedProgressSeconds;
                    break;
                case "GoatBarn":
                    animalCount = GoatCount;
                    productLabel = "Ziegenmilch";
                    ready = _goatMilkReady;
                    feedProgressSeconds = _goatFeedProgressSeconds;
                    break;
                default:
                    return "";
            }

            if (animalCount <= 0)
            {
                return "Keine Tiere";
            }

            if (ready > 0)
            {
                return ready + " " + productLabel + " bereit | Einsammeln";
            }

            if (feedProgressSeconds <= 0)
            {
                return "Hungrig";
            }

            int remainingSeconds = feedProgressSeconds;
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            string fedStatus = "Gefüttert " + minutes.ToString("0") + ":" +
                seconds.ToString("00");
            return fedStatus;
        }

        public bool CollectAnimalProduct(string productItem, string label)
        {
            int ready;
            switch (productItem)
            {
                case Eggs: ready = _eggReady; break;
                case Milk: ready = _milkReady; break;
                case Pork: ready = _porkReady; break;
                case Wool: ready = _woolReady; break;
                case GoatMilk: ready = _goatMilkReady; break;
                default: return false;
            }

            if (ready <= 0)
            {
                SetStatus("Noch keine " + label + " bereit.");
                return false;
            }

            int capacity = CropKinds.Contains(productItem) ? SiloCapacity : BarnCapacity;
            int current = Inventory.ContainsKey(productItem) ? Inventory[productItem] : 0;
            int space = Math.Max(0, capacity - current);
            int collected = Math.Min(ready, space);
            if (collected <= 0 || !AddInventory(productItem, collected))
            {
                SetStatus("Kein Platz für " + label + " in der Scheune.");
                return false;
            }

            int remaining = ready - collected;
            switch (productItem)
            {
                case Eggs: _eggReady = remaining; break;
                case Milk: _milkReady = remaining; break;
                case Pork: _porkReady = remaining; break;
                case Wool: _woolReady = remaining; break;
                case GoatMilk: _goatMilkReady = remaining; break;
            }

            SetStatus(remaining > 0
                ? collected + " " + label + " eingesammelt. " + remaining +
                    " bleiben im Stall; schaffe erst Lagerplatz."
                : collected + " " + label + " eingesammelt.");
            IsDirty = true;
            return true;
        }

        private void AdvanceAnimalProduction(int elapsed, int animalCount,
            string productItem, ref int feedProgressSeconds)
        {
            if (animalCount <= 0 || feedProgressSeconds <= 0)
            {
                return;
            }

            // Die Produktion endet mit der Fütterungszeit; danach wartet das Produkt auf Abholung.
            int feedActiveSeconds = Math.Min(elapsed, feedProgressSeconds);
            feedProgressSeconds -= feedActiveSeconds;
            if (feedProgressSeconds == 0 && feedActiveSeconds > 0)
            {
                switch (productItem)
                {
                    case Eggs: _eggReady = Math.Max(_eggReady, animalCount); break;
                    case Milk: _milkReady = Math.Max(_milkReady, animalCount); break;
                    case Pork: _porkReady = Math.Max(_porkReady, animalCount); break;
                    case Wool: _woolReady = Math.Max(_woolReady, animalCount); break;
                    case GoatMilk: _goatMilkReady = Math.Max(_goatMilkReady, animalCount); break;
                }
                IsDirty = true;
            }

            if (feedActiveSeconds > 0)
            {
                IsDirty = true;
            }
        }
    }
}
