using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    internal sealed class ArenaProgressionMenuRenderer
    {
        private static readonly Brush TextBrush =
            new SolidColorBrush(Color.FromRgb(225, 233, 241));
        private const double ButtonHeight = 36;
        private const double SplitRowWidth = 300;
        private const string MaxedOutLabel = "Maximale Stufe erreicht";

        public UIElement CreateSkillTree(ArenaField field, Action refresh)
        {
            ArenaProgression progression = field.Progression;
            StackPanel content = CreateContent(
                "SPIELER-SKILLTREE",
                (progression.IsGodMode
                    ? "Skillpunkte: unbegrenzt"
                    : "Skillpunkte: " + progression.UniversalPoints)
                    + "  |  Kills: " + progression.TotalKills
                    + "  |  1 Punkt je 5 Kills. Spielerfähigkeiten sind unabhängig von Waffen.");

            TreeView tree = new TreeView
            {
                MaxHeight = 280,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            TreeViewItem root = new TreeViewItem
            {
                Header = "Spielerfähigkeiten",
                IsExpanded = true
            };
            TreeViewItem stats = new TreeViewItem
            {
                Header = "Spielerwerte",
                IsExpanded = true
            };
            AddPlayerStatUpgrade(
                stats, field, ArenaProgression.MaximumHealth,
                "Maximale Gesundheit",
                "Erhöht die maximale Gesundheit um "
                    + ArenaProgression.HealthIncreasePerRank + ".",
                field.Player.MaxHitPoints,
                ArenaProgression.HealthIncreasePerRank, refresh);
            AddPlayerStatUpgrade(
                stats, field, ArenaProgression.MaximumSpeed,
                "Maximale Geschwindigkeit",
                "Erhöht die Bewegungsgeschwindigkeit um "
                    + ArenaProgression.SpeedIncreasePerRank + ".",
                field.Player.MaxSpeed,
                ArenaProgression.SpeedIncreasePerRank, refresh);
            root.Items.Add(stats);
            foreach (PlayerAbilityDefinition definition in PlayerAbilityCatalog.Definitions)
            {
                int rank = progression.GetSkillRank(definition.Id);
                int cost = definition.GetNextRankCost(rank);
                TreeViewItem skill = new TreeViewItem
                {
                    Header = definition.Name + " — Rang "
                        + rank + "/" + definition.MaximumRank,
                    IsExpanded = true
                };
                AddText(skill, definition.Description);
                bool isMaxed = rank >= definition.MaximumRank;
                AddButton(skill,
                    isMaxed
                        ? MaxedOutLabel
                        : (rank == 0 ? "Freischalten" : "Verbessern")
                            + " (" + cost + " universelle Punkte)",
                    !isMaxed && progression.UniversalPoints >= cost,
                    () =>
                    {
                        progression.TryUpgradeSkill(definition.Id);
                        refresh();
                    },
                    isMaxed);
                AddAbilityModifier(skill, progression, definition.CooldownUpgradeId,
                    "Abklingzeit", "Senkt die Abklingzeit um "
                        + (PlayerAbilityCatalog.CooldownReductionPerRank * 100).ToString("0")
                        + " % je Rang.", rank > 0, refresh);
                if (definition.HasDuration)
                {
                    AddAbilityModifier(skill, progression, definition.DurationUpgradeId,
                        "Dauer", definition.DurationDescription, rank > 0, refresh);
                }
                if (definition.Id == PlayerAbilityCatalog.SlowMotion)
                {
                    AddBulletTimeUpgrade(skill, progression, rank, refresh);
                }
                root.Items.Add(skill);
            }
            tree.Items.Add(root);
            content.Children.Add(tree);

            AddText(content, "Spielerfähigkeiten");
            AddAbilitySlots(content, progression.EntityAbilities,
                GetUnlockedEntityAbilities(progression),
                progression.TryAssignEntityAbility, "Spielerfähigkeit", refresh);
            return CreateScrollablePage(content);
        }

        public UIElement CreateWeaponUpgrades(ArenaField field, Action refresh)
        {
            ArenaProgression progression = field.Progression;
            string weaponId = field.SelectedPlayerWeapon.Id;
            bool isSword = weaponId == WeaponCatalog.Sword.Id;
            bool isBow = weaponId == WeaponCatalog.Bow.Id;
            string weaponName = field.SelectedPlayerWeapon.Name;
            int weaponPoints = progression.GetWeaponPoints(weaponId);
            List<WeaponUpgradeDefinition> upgrades = isSword
                ? new List<WeaponUpgradeDefinition>(SwordUpgradeCatalog.Upgrades)
                : isBow
                    ? new List<WeaponUpgradeDefinition>(BowUpgradeCatalog.Upgrades)
                    : new List<WeaponUpgradeDefinition>(MagicWandUpgradeCatalog.Upgrades);

            StackPanel content = CreateContent(
                weaponName.ToUpperInvariant() + " — WAFFENBAUM",
                (field.IsGodMode
                    ? "Godmode: unbegrenzte Punkte, Upgrades kostenlos."
                    : "Waffenpunkte: " + weaponPoints
                        + "  |  Skillpunkte: " + progression.UniversalPoints
                        + ". Waffenpunkte werden zuerst eingesetzt; fehlende Punkte "
                        + "können nach Bestätigung aus Skillpunkten bezahlt werden.")
                    + " LMB: Grundangriff, RMB: Waffenfähigkeit 1. "
                    + "Elemente wirken auf die Waffenangriffe.");

            TreeView tree = new TreeView
            {
                MaxHeight = 390,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            TreeViewItem root = new TreeViewItem
            {
                Header = weaponName + " — Grundangriff und Upgrades",
                IsExpanded = true
            };
            Dictionary<string, TreeViewItem> nodes =
                new Dictionary<string, TreeViewItem>();

            foreach (WeaponUpgradeDefinition upgrade in upgrades)
            {
                int rank = progression.GetWeaponUpgradeRank(upgrade.Id);
                int cost = GetNextRankCost(
                    progression, upgrade.Id, isSword, isBow,
                    out int maximumRank, out string prerequisite);
                bool canPurchase = field.CanUpgradeWeapon(
                    upgrade.Id, out cost,
                    out int weaponPointsToSpend, out int skillPointsToSpend);

                TreeViewItem node = new TreeViewItem
                {
                    Header = upgrade.Name + " — Rang "
                        + rank + "/" + maximumRank,
                    IsExpanded = true
                };
                AddText(node, upgrade.Description);
                AddText(node, "Nächste Stufe: " + cost + " Punkte"
                    + (string.IsNullOrEmpty(prerequisite)
                        ? string.Empty
                        : " | Voraussetzung: "
                            + GetUpgradeDefinition(prerequisite, isSword, isBow).Name));
                if (isSword
                    ? progression.SelectedSwordElement != WeaponElement.None
                        && IsElement(upgrade.Id, true)
                        && rank == 0
                    : isBow
                        ? progression.SelectedBowElement != WeaponElement.None
                            && BowUpgradeCatalog.IsElement(upgrade.Id)
                            && rank == 0
                        : progression.SelectedMagicWandElement != WeaponElement.None
                            && IsElement(upgrade.Id, false)
                            && rank == 0)
                {
                    AddText(node, "Ein anderer Elementzweig ist bereits gewählt.");
                }

                AddPurchaseButton(node, field, upgrade,
                    canPurchase, cost, weaponPointsToSpend,
                    skillPointsToSpend, rank >= maximumRank, refresh);

                if (!string.IsNullOrEmpty(prerequisite)
                    && nodes.TryGetValue(prerequisite, out TreeViewItem parent))
                {
                    parent.Items.Add(node);
                }
                else
                {
                    root.Items.Add(node);
                }
                nodes.Add(upgrade.Id, node);
            }

            tree.Items.Add(root);
            content.Children.Add(tree);

            AddText(content, "Waffenfähigkeiten — getrennt von Spielerfähigkeiten");
            AddAbilitySlots(content, progression.GetWeaponAbilities(weaponId),
                GetUnlockedWeaponAbilities(progression, weaponId),
                (slot, ability) => progression.TryAssignWeaponAbility(
                    weaponId, slot, ability), "Waffenfähigkeit", refresh);
            return CreateScrollablePage(content);
        }

        public UIElement CreateKeyBindings(
            ArenaField field, ArenaAction? waitingFor,
            Action<ArenaAction> beginCapture, Action refresh)
        {
            StackPanel content = CreateContent(
                "STEUERUNG",
                waitingFor.HasValue
                    ? ArenaKeyBindings.GetActionName(waitingFor.Value)
                        + ": jetzt Taste oder Maustaste drücken (Escape bricht ab)."
                    : "Eintrag wählen. Maustasten funktionieren für Bewegung, Angriffe und Fähigkeiten.");

            // Split rows: action name on the long half, current key on the short half.
            foreach (ArenaAction action in ArenaKeyBindings.Actions)
            {
                ArenaAction selectedAction = action;
                bool isCapturing = waitingFor == action;
                string keyName = isCapturing
                    ? "…"
                    : field.KeyBindings.GetDisplayName(action);
                ArenaUiButton actionButton = CreateSplitButton(
                    ArenaKeyBindings.GetActionName(action),
                    ArenaUiButtonShape.SplitLong, () => beginCapture(selectedAction));
                ArenaUiButton keyButton = CreateSplitButton(
                    keyName, ArenaUiButtonShape.SplitShort,
                    () => beginCapture(selectedAction));
                actionButton.IsChosen = isCapturing;
                keyButton.IsChosen = isCapturing;
                keyButton.ToolTip = field.KeyBindings.GetDisplayName(action);
                System.Windows.Automation.AutomationProperties.SetName(
                    actionButton, ArenaKeyBindings.GetActionName(action) + ": "
                        + field.KeyBindings.GetDisplayName(action));
                content.Children.Add(ArenaUiButton.CreateSplitRow(
                    actionButton, keyButton, SplitRowWidth, ButtonHeight));
            }
            AddButton(content, "Auf Standard zurücksetzen", true, () =>
            {
                field.KeyBindings.ResetToDefaults();
                field.KeyBindings.Save();
                refresh();
            });
            return CreateScrollablePage(content);
        }

        private static List<AbilityOption> GetUnlockedEntityAbilities(
            ArenaProgression progression)
        {
            List<AbilityOption> options = new List<AbilityOption>();
            foreach (PlayerAbilityDefinition definition in PlayerAbilityCatalog.Definitions)
            {
                if (progression.GetSkillRank(definition.Id) > 0)
                {
                    options.Add(new AbilityOption(definition.Id, definition.Name));
                }
            }
            return options;
        }

        private static List<AbilityOption> GetUnlockedWeaponAbilities(
            ArenaProgression progression, string weaponId)
        {
            List<AbilityOption> options = new List<AbilityOption>();
            if (weaponId == WeaponCatalog.Sword.Id
                && progression.GetWeaponUpgradeRank(SwordUpgradeCatalog.Spin) > 0)
            {
                options.Add(new AbilityOption(
                    SwordUpgradeCatalog.SpinAbility, "Drehwirbel"));
            }
            else if (weaponId == WeaponCatalog.MagicWand.Id
                && progression.GetWeaponUpgradeRank(MagicWandUpgradeCatalog.Strike) > 0)
            {
                options.Add(new AbilityOption(
                    MagicWandUpgradeCatalog.StrikeAbility, "Stabschlag"));
            }
            return options;
        }

        private static void AddAbilitySlots(
            StackPanel panel, IReadOnlyList<string> equipped,
            IReadOnlyList<AbilityOption> unlocked,
            Func<int, string, bool> assign, string label, Action refresh)
        {
            List<AbilityOption> options = new List<AbilityOption>
            {
                new AbilityOption(string.Empty, "Leer")
            };
            options.AddRange(unlocked);

            // Long half cycles through the unlocked abilities, short half clears the slot.
            for (int slot = 0; slot < ArenaProgression.AbilitySlotCount; slot++)
            {
                int selectedSlot = slot;
                string equippedId = equipped[slot] ?? string.Empty;
                int current = options.FindIndex(option => option.Id == equippedId);
                ArenaUiButton cycleButton = CreateSplitButton(
                    label + " " + (slot + 1) + ": "
                        + (current >= 0 ? options[current].Name : "Leer"),
                    ArenaUiButtonShape.SplitLong,
                    () =>
                    {
                        for (int step = 1; step < options.Count; step++)
                        {
                            AbilityOption next =
                                options[(Math.Max(current, 0) + step) % options.Count];
                            if (assign(selectedSlot,
                                string.IsNullOrEmpty(next.Id) ? null : next.Id))
                            {
                                break;
                            }
                        }
                        refresh();
                    });
                cycleButton.IsEnabled = unlocked.Count > 0;
                cycleButton.ToolTip = unlocked.Count > 0
                    ? "Klicken, um die nächste freigeschaltete Fähigkeit zu wählen."
                    : "Noch keine Fähigkeit freigeschaltet.";
                ArenaUiButton clearButton = CreateSplitButton(
                    "Leeren", ArenaUiButtonShape.SplitShort,
                    () =>
                    {
                        assign(selectedSlot, null);
                        refresh();
                    });
                clearButton.IsEnabled = !string.IsNullOrEmpty(equippedId);
                panel.Children.Add(ArenaUiButton.CreateSplitRow(
                    cycleButton, clearButton, SplitRowWidth, ButtonHeight));
            }
        }

        private static int GetNextRankCost(
            ArenaProgression progression, string id, bool isSword, bool isBow,
            out int maximumRank, out string prerequisite)
        {
            if (isSword)
            {
                return SwordUpgradeCatalog.GetNextRankCost(
                    progression, id, out maximumRank, out prerequisite);
            }
            if (isBow)
            {
                return BowUpgradeCatalog.GetNextRankCost(
                    progression, id, out maximumRank, out prerequisite);
            }
            return MagicWandUpgradeCatalog.GetNextRankCost(
                progression, id, out maximumRank, out prerequisite);
        }

        private static WeaponUpgradeDefinition GetUpgradeDefinition(
            string id, bool isSword, bool isBow)
        {
            if (isSword)
            {
                return SwordUpgradeCatalog.GetDefinition(id);
            }
            return isBow
                ? BowUpgradeCatalog.GetDefinition(id)
                : MagicWandUpgradeCatalog.GetDefinition(id);
        }

        private static bool IsElement(string id, bool isSword)
        {
            return isSword
                ? SwordUpgradeCatalog.IsElement(id)
                : MagicWandUpgradeCatalog.IsElement(id);
        }

        // Cooldown or duration upgrade below an ability; locked until the ability is unlocked.
        private static void AddAbilityModifier(
            TreeViewItem parent, ArenaProgression progression, string modifierId,
            string name, string description, bool isAbilityUnlocked, Action refresh)
        {
            int rank = progression.GetSkillRank(modifierId);
            int maximumRank = PlayerAbilityCatalog.ModifierMaximumRank;
            bool isMaxed = rank >= maximumRank;
            TreeViewItem node = new TreeViewItem
            {
                Header = name + " — Rang " + rank + "/" + maximumRank,
                IsExpanded = true
            };
            AddText(node, description
                + (isAbilityUnlocked ? string.Empty : " Erst die Fähigkeit freischalten."));
            AddButton(node,
                isMaxed
                    ? MaxedOutLabel
                    : "Verbessern ("
                        + progression.GetAbilityModifierNextRankCost(modifierId)
                        + " Skillpunkte)",
                progression.CanUpgradeAbilityModifier(modifierId),
                () =>
                {
                    progression.TryUpgradeAbilityModifier(modifierId);
                    refresh();
                },
                isMaxed);
            parent.Items.Add(node);
        }

        // Expensive one-off stage of the slow motion that keeps the player at normal speed.
        private static void AddBulletTimeUpgrade(
            TreeViewItem parent, ArenaProgression progression, int slowMotionRank,
            Action refresh)
        {
            bool isUnlocked = progression.HasBulletTime;
            TreeViewItem node = new TreeViewItem
            {
                Header = "Bullet Time — " + (isUnlocked ? "freigeschaltet" : "gesperrt"),
                IsExpanded = true
            };
            AddText(node, "Während der Zeitlupe bewegst du dich, greifst an und erholst "
                + "dich in normalem Tempo, nur die Arena bleibt verlangsamt."
                + (slowMotionRank < PlayerAbilityCatalog.BulletTimeRequiredSlowMotionRank
                    ? " Benötigt Zeitlupe Rang "
                        + PlayerAbilityCatalog.BulletTimeRequiredSlowMotionRank + "."
                    : string.Empty));
            AddButton(node,
                isUnlocked
                    ? MaxedOutLabel
                    : "Freischalten (" + PlayerAbilityCatalog.BulletTimeCost
                        + " Skillpunkte)",
                progression.CanUnlockBulletTime(),
                () =>
                {
                    progression.TryUnlockBulletTime();
                    refresh();
                },
                isUnlocked);
            parent.Items.Add(node);
        }

        private static void AddPlayerStatUpgrade(
            TreeViewItem parent, ArenaField field, string statId,
            string name, string description, double currentValue,
            double increasePerRank, Action refresh)
        {
            ArenaProgression progression = field.Progression;
            int rank = progression.GetSkillRank(statId);
            int maximumRank = progression.GetPlayerStatMaximumRank(statId);
            int cost = progression.GetPlayerStatNextRankCost(statId);
            TreeViewItem node = new TreeViewItem
            {
                Header = name + " — Rang " + rank + "/" + maximumRank,
                IsExpanded = true
            };
            AddText(node, description + " Aktuell: " + currentValue + ".");
            bool isMaxed = rank >= maximumRank;
            AddButton(node,
                isMaxed ? MaxedOutLabel : "Verbessern (" + cost + " Skillpunkte)",
                progression.CanUpgradePlayerStat(statId),
                () =>
            {
                field.TryUpgradePlayerStat(statId);
                refresh();
            },
                isMaxed);
            parent.Items.Add(node);
        }

        private static void AddPurchaseButton(
            ItemsControl content, ArenaField field,
            WeaponUpgradeDefinition upgrade, bool enabled, int cost,
            int weaponPointsToSpend, int skillPointsToSpend,
            bool isMaxed, Action refresh)
        {
            AddButton(content,
                isMaxed
                    ? MaxedOutLabel
                    : field.IsGodMode
                    ? "Kostenlos verbessern (" + cost + ")"
                    : "Verbessern (" + FormatPayment(
                        weaponPointsToSpend, skillPointsToSpend) + ")",
                enabled, () =>
                {
                    // Every purchase that uses skill points needs an explicit confirmation,
                    // also when the weapon has no weapon points left at all.
                    if (skillPointsToSpend > 0
                        && MessageBox.Show(
                            (weaponPointsToSpend == 0
                                ? "Du hast keine Waffenpunkte für "
                                    + field.SelectedPlayerWeapon.Name + ". "
                                : "Es fehlen " + skillPointsToSpend + " Waffenpunkte. ")
                                + "Möchtest du „" + upgrade.Name + "“ mit "
                                + skillPointsToSpend + " Skillpunkt"
                                + (skillPointsToSpend == 1 ? "" : "en")
                                + (weaponPointsToSpend > 0
                                    ? " und " + weaponPointsToSpend + " Waffenpunkt"
                                        + (weaponPointsToSpend == 1 ? "" : "en")
                                    : string.Empty)
                                + " bezahlen?",
                            "Mit Skillpunkten bezahlen?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    field.TryUpgradeWeaponWithSkillPoints(upgrade.Id);
                    refresh();
                },
                isMaxed);
        }

        private static string FormatPayment(int weaponPoints, int skillPoints)
        {
            if (skillPoints <= 0)
            {
                return weaponPoints + " Waffenpunkte";
            }
            return weaponPoints > 0
                ? weaponPoints + " Waffenpunkte + " + skillPoints + " Skillpunkte"
                : skillPoints + " Skillpunkte";
        }

        private static StackPanel CreateContent(string title, string summary)
        {
            StackPanel content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 23,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            AddText(content, summary);
            return content;
        }

        private static ScrollViewer CreateScrollablePage(StackPanel content)
        {
            return new ScrollViewer
            {
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            };
        }

        private static void AddText(DependencyObject panel, string text)
        {
            AddElement(panel, new TextBlock
            {
                Text = text,
                FontSize = 13,
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 5)
            });
        }

        // Disabled upgrade buttons show the locked sprite, fully upgraded ones the maxed-out sprite.
        private static void AddButton(
            DependencyObject panel, string label, bool enabled, Action action,
            bool isMaxed = false)
        {
            ArenaUiButton button = new ArenaUiButton(label, action)
            {
                Height = ButtonHeight,
                MinWidth = 240,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 3, 0, 3),
                FontSize = 13,
                IsEnabled = enabled && !isMaxed,
                State = isMaxed ? ArenaUiButtonState.MaxedOut : ArenaUiButtonState.Normal,
                ToolTip = label
            };
            AddElement(panel, button);
        }

        private static ArenaUiButton CreateSplitButton(
            string label, ArenaUiButtonShape shape, Action action)
        {
            return new ArenaUiButton(label, action, shape)
            {
                FontSize = 13,
                ToolTip = label
            };
        }

        private static void AddElement(DependencyObject parent, UIElement element)
        {
            if (parent is Panel panel)
            {
                panel.Children.Add(element);
            }
            else if (parent is ItemsControl items)
            {
                items.Items.Add(element);
            }
            else
            {
                throw new ArgumentException(
                    "Menu content must be a WPF panel or items control.", nameof(parent));
            }
        }

        private sealed class AbilityOption
        {
            public string Id { get; }
            public string Name { get; }

            public AbilityOption(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
