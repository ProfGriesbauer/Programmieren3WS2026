using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public enum UpgradeCurrency
    {
        Universal,
        Weapon
    }

    public enum WeaponElement
    {
        None,
        Poison,
        Lightning,
        Fire,
        Freeze
    }

    public sealed class ArenaProgression
    {
        public const int StartingUniversalPoints = 10;
        public const int UniversalPointsPerKillMilestone = 1;
        public const int KillsPerUniversalPoint = 5;
        public const int WeaponPointsPerKill = 1;
        public const int AbilitySlotCount = 4;

        private readonly Dictionary<string, int> _weaponPoints =
            new Dictionary<string, int>();
        private readonly Dictionary<string, int> _skillRanks =
            new Dictionary<string, int>();
        private readonly Dictionary<string, int> _weaponUpgradeRanks =
            new Dictionary<string, int>();
        private readonly string[] _entityAbilities =
            new string[AbilitySlotCount];
        private readonly Dictionary<string, string[]> _weaponAbilities =
            new Dictionary<string, string[]>();

        public int UniversalPoints { get; private set; }
        public int TotalKills { get; private set; }
        public IReadOnlyList<string> EntityAbilities
        {
            get { return Array.AsReadOnly(_entityAbilities); }
        }

        public int GetWeaponPoints(string weaponId)
        {
            return _weaponPoints.TryGetValue(weaponId, out int points) ? points : 0;
        }

        public int GetSkillRank(string skillId)
        {
            return _skillRanks.TryGetValue(skillId, out int rank) ? rank : 0;
        }

        public int GetWeaponUpgradeRank(string upgradeId)
        {
            return _weaponUpgradeRanks.TryGetValue(upgradeId, out int rank) ? rank : 0;
        }

        public WeaponElement SelectedMagicWandElement
        {
            get
            {
                return GetSelectedElement(
                    MagicWandUpgradeCatalog.Poison,
                    MagicWandUpgradeCatalog.Lightning,
                    MagicWandUpgradeCatalog.Fire,
                    MagicWandUpgradeCatalog.Freeze);
            }
        }

        public WeaponElement SelectedSwordElement
        {
            get
            {
                return GetSelectedElement(
                    SwordUpgradeCatalog.Poison,
                    SwordUpgradeCatalog.Lightning,
                    SwordUpgradeCatalog.Fire,
                    SwordUpgradeCatalog.Freeze);
            }
        }

        public WeaponElement SelectedBowElement
        {
            get
            {
                return GetSelectedElement(
                    BowUpgradeCatalog.Poison,
                    BowUpgradeCatalog.Lightning,
                    BowUpgradeCatalog.Fire,
                    BowUpgradeCatalog.Freeze);
            }
        }

        public ArenaProgression()
        {
            Reset();
        }

        public void Reset()
        {
            UniversalPoints = StartingUniversalPoints;
            TotalKills = 0;
            _weaponPoints.Clear();
            _skillRanks.Clear();
            _weaponUpgradeRanks.Clear();
            _weaponAbilities.Clear();
            Array.Clear(_entityAbilities, 0, _entityAbilities.Length);
        }

        public void AwardKill(Weapon weapon)
        {
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            TotalKills++;
            string weaponId = weapon.Id == MagicWandUpgradeCatalog.StrikeAbility
                ? WeaponCatalog.MagicWand.Id
                : weapon.Id == SwordUpgradeCatalog.SpinAbility
                    ? WeaponCatalog.Sword.Id
                    : weapon.Id;
            _weaponPoints[weaponId] = GetWeaponPoints(weaponId) + WeaponPointsPerKill;
            if (TotalKills % KillsPerUniversalPoint == 0)
            {
                UniversalPoints += UniversalPointsPerKillMilestone;
            }
        }

        public bool TryUpgradeSkill(string skillId)
        {
            PlayerAbilityDefinition definition =
                PlayerAbilityCatalog.GetDefinition(skillId);
            int currentRank = GetSkillRank(skillId);
            if (currentRank >= definition.MaximumRank)
            {
                return false;
            }

            int cost = definition.GetNextRankCost(currentRank);
            if (UniversalPoints < cost)
            {
                return false;
            }

            UniversalPoints -= cost;
            _skillRanks[skillId] = currentRank + 1;
            if (currentRank == 0)
            {
                AssignToFirstFreeSlot(_entityAbilities, skillId);
            }
            return true;
        }

        public bool TryAssignEntityAbility(int slotIndex, string abilityId)
        {
            ValidateSlot(slotIndex);
            if (string.IsNullOrEmpty(abilityId))
            {
                _entityAbilities[slotIndex] = null;
                return true;
            }

            bool isUnlocked = PlayerAbilityCatalog.Definitions.Any(
                definition => definition.Id == abilityId && GetSkillRank(abilityId) > 0);
            if (!isUnlocked)
            {
                return false;
            }

            AssignToSlot(_entityAbilities, slotIndex, abilityId);
            return true;
        }

        public Weapon GetEntityAbilityWeapon(int slotIndex)
        {
            ValidateSlot(slotIndex);
            string abilityId = _entityAbilities[slotIndex];
            return abilityId == null
                ? null
                : PlayerAbilityCatalog.CreateWeapon(
                    abilityId, GetSkillRank(abilityId));
        }

        public IReadOnlyList<string> GetWeaponAbilities(string weaponId)
        {
            return Array.AsReadOnly(GetWeaponAbilitySlots(weaponId));
        }

        public bool TryAssignWeaponAbility(
            string weaponId, int slotIndex, string abilityId)
        {
            ValidateSlot(slotIndex);
            string[] abilities = GetWeaponAbilitySlots(weaponId);
            if (string.IsNullOrEmpty(abilityId))
            {
                abilities[slotIndex] = null;
                return true;
            }
            if (!IsWeaponAbilityUnlocked(weaponId, abilityId))
            {
                return false;
            }

            AssignToSlot(abilities, slotIndex, abilityId);
            return true;
        }

        public Weapon GetWeaponAbilityWeapon(string weaponId, int slotIndex)
        {
            ValidateSlot(slotIndex);
            string[] abilities = GetWeaponAbilitySlots(weaponId);
            string abilityId = abilities[slotIndex];
            if (abilityId == null)
            {
                return null;
            }
            if (abilityId == MagicWandUpgradeCatalog.StrikeAbility)
            {
                return MagicWandUpgradeCatalog.CreateStrikeWeapon(this);
            }
            if (abilityId == SwordUpgradeCatalog.SpinAbility)
            {
                return SwordUpgradeCatalog.CreateSpinWeapon(this);
            }

            throw new InvalidOperationException(
                "The equipped weapon ability is not supported.");
        }

        public bool CanUpgradeMagicWand(
            string upgradeId, UpgradeCurrency currency, out int cost)
        {
            cost = MagicWandUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.MagicWand.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, MagicWandUpgradeCatalog.IsElement(upgradeId),
                SelectedMagicWandElement);
        }

        public bool TryUpgradeMagicWand(
            string upgradeId, UpgradeCurrency currency)
        {
            if (!CanUpgradeMagicWand(upgradeId, currency, out int cost))
            {
                return false;
            }

            int currentRank = GetWeaponUpgradeRank(upgradeId);
            PurchaseUpgrade(WeaponCatalog.MagicWand.Id, upgradeId, currency, cost);
            if (upgradeId == MagicWandUpgradeCatalog.Strike && currentRank == 0)
            {
                AssignToFirstFreeSlot(
                    GetWeaponAbilitySlots(WeaponCatalog.MagicWand.Id),
                    MagicWandUpgradeCatalog.StrikeAbility);
            }
            return true;
        }

        public bool CanUpgradeBow(
            string upgradeId, UpgradeCurrency currency, out int cost)
        {
            cost = BowUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.Bow.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, BowUpgradeCatalog.IsElement(upgradeId),
                SelectedBowElement);
        }

        public bool TryUpgradeBow(
            string upgradeId, UpgradeCurrency currency)
        {
            if (!CanUpgradeBow(upgradeId, currency, out int cost))
            {
                return false;
            }

            PurchaseUpgrade(WeaponCatalog.Bow.Id, upgradeId, currency, cost);
            return true;
        }

        public bool CanUpgradeSword(
            string upgradeId, UpgradeCurrency currency, out int cost)
        {
            cost = SwordUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.Sword.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, SwordUpgradeCatalog.IsElement(upgradeId),
                SelectedSwordElement);
        }

        public bool TryUpgradeSword(
            string upgradeId, UpgradeCurrency currency)
        {
            if (!CanUpgradeSword(upgradeId, currency, out int cost))
            {
                return false;
            }

            int currentRank = GetWeaponUpgradeRank(upgradeId);
            PurchaseUpgrade(WeaponCatalog.Sword.Id, upgradeId, currency, cost);
            if (upgradeId == SwordUpgradeCatalog.Spin && currentRank == 0)
            {
                AssignToFirstFreeSlot(
                    GetWeaponAbilitySlots(WeaponCatalog.Sword.Id),
                    SwordUpgradeCatalog.SpinAbility);
            }
            return true;
        }

        private WeaponElement GetSelectedElement(
            string poison, string lightning, string fire, string freeze)
        {
            if (GetWeaponUpgradeRank(poison) > 0) return WeaponElement.Poison;
            if (GetWeaponUpgradeRank(lightning) > 0) return WeaponElement.Lightning;
            if (GetWeaponUpgradeRank(fire) > 0) return WeaponElement.Fire;
            if (GetWeaponUpgradeRank(freeze) > 0) return WeaponElement.Freeze;
            return WeaponElement.None;
        }

        private bool CanPurchaseWeaponUpgrade(
            string weaponId, string upgradeId, UpgradeCurrency currency,
            int cost, int maximumRank, string prerequisite,
            bool isElement, WeaponElement selectedElement)
        {
            if (GetWeaponUpgradeRank(upgradeId) >= maximumRank
                || !string.IsNullOrEmpty(prerequisite)
                    && GetWeaponUpgradeRank(prerequisite) == 0)
            {
                return false;
            }

            if ((upgradeId == MagicWandUpgradeCatalog.ElementPower
                    || upgradeId == SwordUpgradeCatalog.ElementPower
                    || upgradeId == BowUpgradeCatalog.ElementPower)
                && selectedElement == WeaponElement.None)
            {
                return false;
            }
            if (isElement && selectedElement != WeaponElement.None
                && GetWeaponUpgradeRank(upgradeId) == 0)
            {
                return false;
            }

            return currency == UpgradeCurrency.Universal
                ? UniversalPoints >= cost
                : GetWeaponPoints(weaponId) >= cost;
        }

        private void PurchaseUpgrade(
            string weaponId, string upgradeId,
            UpgradeCurrency currency, int cost)
        {
            if (currency == UpgradeCurrency.Universal)
            {
                UniversalPoints -= cost;
            }
            else
            {
                _weaponPoints[weaponId] -= cost;
            }

            _weaponUpgradeRanks[upgradeId] = GetWeaponUpgradeRank(upgradeId) + 1;
        }

        private string[] GetWeaponAbilitySlots(string weaponId)
        {
            if (weaponId != WeaponCatalog.MagicWand.Id
                && weaponId != WeaponCatalog.Sword.Id
                && weaponId != WeaponCatalog.Bow.Id)
            {
                throw new ArgumentException(
                    "The weapon has no progression ability slots.", nameof(weaponId));
            }
            if (!_weaponAbilities.TryGetValue(weaponId, out string[] abilities))
            {
                abilities = new string[AbilitySlotCount];
                _weaponAbilities.Add(weaponId, abilities);
            }
            return abilities;
        }

        private bool IsWeaponAbilityUnlocked(string weaponId, string abilityId)
        {
            if (weaponId == WeaponCatalog.MagicWand.Id)
            {
                return abilityId == MagicWandUpgradeCatalog.StrikeAbility
                    && GetWeaponUpgradeRank(MagicWandUpgradeCatalog.Strike) > 0;
            }
            if (weaponId == WeaponCatalog.Sword.Id)
            {
                return abilityId == SwordUpgradeCatalog.SpinAbility
                    && GetWeaponUpgradeRank(SwordUpgradeCatalog.Spin) > 0;
            }
            return false;
        }

        private static void ValidateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= AbilitySlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }
        }

        private static void AssignToFirstFreeSlot(
            string[] slots, string abilityId)
        {
            if (Array.IndexOf(slots, abilityId) >= 0)
            {
                return;
            }

            int emptySlot = Array.IndexOf(slots, null);
            if (emptySlot >= 0)
            {
                slots[emptySlot] = abilityId;
            }
        }

        private static void AssignToSlot(
            string[] slots, int slotIndex, string abilityId)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i != slotIndex && slots[i] == abilityId)
                {
                    slots[i] = null;
                }
            }
            slots[slotIndex] = abilityId;
        }
    }
}
