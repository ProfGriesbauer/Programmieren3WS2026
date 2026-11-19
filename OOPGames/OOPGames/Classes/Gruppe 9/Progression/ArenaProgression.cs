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
        public const string MaximumHealth = "maximum-health";
        public const string MaximumSpeed = "maximum-speed";
        public const int PlayerStatMaximumRank = 5;
        public const int HealthIncreasePerRank = 20;
        public const double SpeedIncreasePerRank = 2;

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
        private int _universalPoints;

        public int UniversalPoints
        {
            get { return IsGodMode ? int.MaxValue : _universalPoints; }
        }
        public int TotalKills { get; private set; }
        public bool IsGodMode { get; private set; }
        public IReadOnlyList<string> EntityAbilities
        {
            get { return Array.AsReadOnly(_entityAbilities); }
        }

        public int GetWeaponPoints(string weaponId)
        {
            return IsGodMode
                ? int.MaxValue
                : GetStoredWeaponPoints(weaponId);
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
            _universalPoints = StartingUniversalPoints;
            TotalKills = 0;
            IsGodMode = false;
            _weaponPoints.Clear();
            _skillRanks.Clear();
            _weaponUpgradeRanks.Clear();
            _weaponAbilities.Clear();
            Array.Clear(_entityAbilities, 0, _entityAbilities.Length);
        }

        public void SetGodMode(bool enabled)
        {
            IsGodMode = enabled;
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
            _weaponPoints[weaponId] =
                GetStoredWeaponPoints(weaponId) + WeaponPointsPerKill;
            if (TotalKills % KillsPerUniversalPoint == 0)
            {
                _universalPoints += UniversalPointsPerKillMilestone;
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
            if (!IsGodMode && _universalPoints < cost)
            {
                return false;
            }

            if (!IsGodMode)
            {
                _universalPoints -= cost;
            }
            _skillRanks[skillId] = currentRank + 1;
            if (currentRank == 0)
            {
                AssignToFirstFreeSlot(_entityAbilities, skillId);
            }
            return true;
        }

        public int GetPlayerStatMaximumRank(string statId)
        {
            ValidatePlayerStatId(statId);
            return PlayerStatMaximumRank;
        }

        public int GetPlayerStatNextRankCost(string statId)
        {
            ValidatePlayerStatId(statId);
            return 2 * (GetSkillRank(statId) + 1);
        }

        public bool CanUpgradePlayerStat(string statId)
        {
            ValidatePlayerStatId(statId);
            return GetSkillRank(statId) < PlayerStatMaximumRank
                && (IsGodMode || _universalPoints >= GetPlayerStatNextRankCost(statId));
        }

        public bool TryUpgradePlayerStat(string statId)
        {
            if (!CanUpgradePlayerStat(statId))
            {
                return false;
            }

            if (!IsGodMode)
            {
                _universalPoints -= GetPlayerStatNextRankCost(statId);
            }
            _skillRanks[statId] = GetSkillRank(statId) + 1;
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
            string upgradeId, UpgradeCurrency currency, out int cost,
            bool allowSkillPointFallback = false)
        {
            cost = MagicWandUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.MagicWand.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, MagicWandUpgradeCatalog.IsElement(upgradeId),
                SelectedMagicWandElement, allowSkillPointFallback);
        }

        public bool TryUpgradeMagicWand(
            string upgradeId, UpgradeCurrency currency,
            bool allowSkillPointFallback = false)
        {
            if (!CanUpgradeMagicWand(
                upgradeId, currency, out int cost, allowSkillPointFallback))
            {
                return false;
            }

            int currentRank = GetWeaponUpgradeRank(upgradeId);
            PurchaseUpgrade(
                WeaponCatalog.MagicWand.Id, upgradeId, currency, cost,
                allowSkillPointFallback);
            if (upgradeId == MagicWandUpgradeCatalog.Strike && currentRank == 0)
            {
                AssignToFirstFreeSlot(
                    GetWeaponAbilitySlots(WeaponCatalog.MagicWand.Id),
                    MagicWandUpgradeCatalog.StrikeAbility);
            }
            return true;
        }

        public bool CanUpgradeBow(
            string upgradeId, UpgradeCurrency currency, out int cost,
            bool allowSkillPointFallback = false)
        {
            cost = BowUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.Bow.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, BowUpgradeCatalog.IsElement(upgradeId),
                SelectedBowElement, allowSkillPointFallback);
        }

        public bool TryUpgradeBow(
            string upgradeId, UpgradeCurrency currency,
            bool allowSkillPointFallback = false)
        {
            if (!CanUpgradeBow(
                upgradeId, currency, out int cost, allowSkillPointFallback))
            {
                return false;
            }

            PurchaseUpgrade(
                WeaponCatalog.Bow.Id, upgradeId, currency, cost,
                allowSkillPointFallback);
            return true;
        }

        public bool CanUpgradeSword(
            string upgradeId, UpgradeCurrency currency, out int cost,
            bool allowSkillPointFallback = false)
        {
            cost = SwordUpgradeCatalog.GetNextRankCost(
                this, upgradeId, out int maximumRank, out string prerequisite);
            return CanPurchaseWeaponUpgrade(
                WeaponCatalog.Sword.Id, upgradeId, currency, cost,
                maximumRank, prerequisite, SwordUpgradeCatalog.IsElement(upgradeId),
                SelectedSwordElement, allowSkillPointFallback);
        }

        public bool TryUpgradeSword(
            string upgradeId, UpgradeCurrency currency,
            bool allowSkillPointFallback = false)
        {
            if (!CanUpgradeSword(
                upgradeId, currency, out int cost, allowSkillPointFallback))
            {
                return false;
            }

            int currentRank = GetWeaponUpgradeRank(upgradeId);
            PurchaseUpgrade(
                WeaponCatalog.Sword.Id, upgradeId, currency, cost,
                allowSkillPointFallback);
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
            bool isElement, WeaponElement selectedElement,
            bool allowSkillPointFallback)
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

            if (IsGodMode)
            {
                return true;
            }
            if (currency == UpgradeCurrency.Universal)
            {
                return _universalPoints >= cost;
            }

            int weaponPoints = GetStoredWeaponPoints(weaponId);
            return weaponPoints >= cost
                || allowSkillPointFallback
                    && _universalPoints >= cost - weaponPoints;
        }

        private void PurchaseUpgrade(
            string weaponId, string upgradeId,
            UpgradeCurrency currency, int cost, bool allowSkillPointFallback)
        {
            if (IsGodMode)
            {
                _weaponUpgradeRanks[upgradeId] =
                    GetWeaponUpgradeRank(upgradeId) + 1;
                return;
            }

            if (currency == UpgradeCurrency.Universal)
            {
                _universalPoints -= cost;
            }
            else if (allowSkillPointFallback)
            {
                int weaponPointsSpent = Math.Min(
                    GetStoredWeaponPoints(weaponId), cost);
                _weaponPoints[weaponId] =
                    GetStoredWeaponPoints(weaponId) - weaponPointsSpent;
                _universalPoints -= cost - weaponPointsSpent;
            }
            else
            {
                _weaponPoints[weaponId] =
                    GetStoredWeaponPoints(weaponId) - cost;
            }

            _weaponUpgradeRanks[upgradeId] = GetWeaponUpgradeRank(upgradeId) + 1;
        }

        public void GetWeaponUpgradePayment(
            string weaponId, int cost,
            out int weaponPointsSpent, out int skillPointsSpent)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                throw new ArgumentException("A weapon id is required.", nameof(weaponId));
            }
            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            if (IsGodMode)
            {
                weaponPointsSpent = 0;
                skillPointsSpent = 0;
                return;
            }

            weaponPointsSpent = Math.Min(GetStoredWeaponPoints(weaponId), cost);
            skillPointsSpent = cost - weaponPointsSpent;
        }

        private int GetStoredWeaponPoints(string weaponId)
        {
            return _weaponPoints.TryGetValue(weaponId, out int points)
                ? points
                : 0;
        }

        private static void ValidatePlayerStatId(string statId)
        {
            if (statId != MaximumHealth && statId != MaximumSpeed)
            {
                throw new ArgumentException(
                    "Unknown player stat upgrade: " + statId, nameof(statId));
            }
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
