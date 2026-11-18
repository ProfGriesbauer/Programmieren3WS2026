using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    internal sealed class ArenaProgressionMenuRenderer
    {
        private static readonly Brush TextBrush =
            new SolidColorBrush(Color.FromRgb(225, 233, 241));
        private static readonly Brush ButtonBrush =
            new SolidColorBrush(Color.FromRgb(55, 70, 88));
        private static readonly Brush DisabledBrush =
            new SolidColorBrush(Color.FromRgb(65, 68, 72));

        public UIElement CreateSkillTree(ArenaField field, Action refresh)
        {
            ArenaProgression progression = field.Progression;
            StackPanel content = CreateContent(
                "SPIELER-SKILLTREE",
                "Universelle Punkte: " + progression.UniversalPoints
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
                AddButton(skill,
                    (rank == 0 ? "Freischalten" : "Verbessern")
                        + " (" + cost + " universelle Punkte)",
                    rank < definition.MaximumRank
                        && progression.UniversalPoints >= cost,
                    () =>
                    {
                        progression.TryUpgradeSkill(definition.Id);
                        refresh();
                    });
                root.Items.Add(skill);
            }
            tree.Items.Add(root);
            content.Children.Add(tree);

            AddText(content, "Spielerfähigkeiten");
            AddAbilitySlots(content, progression.EntityAbilities,
                GetUnlockedEntityAbilities(progression),
                progression.TryAssignEntityAbility, "Spielerfähigkeit");
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
                "Universelle Punkte: " + progression.UniversalPoints
                    + "  |  Waffenpunkte: " + weaponPoints
                    + "  |  LMB: Grundangriff, RMB: Waffenfähigkeit 1. "
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
                bool canPurchaseWeapon = CanUpgrade(
                    progression, upgrade.Id, UpgradeCurrency.Weapon, isSword, isBow);
                bool canPurchaseUniversal = CanUpgrade(
                    progression, upgrade.Id, UpgradeCurrency.Universal, isSword, isBow);

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

                AddPurchaseButton(node, field, upgrade, UpgradeCurrency.Weapon,
                    canPurchaseWeapon, cost, refresh);
                AddPurchaseButton(node, field, upgrade, UpgradeCurrency.Universal,
                    canPurchaseUniversal, cost, refresh);

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
                    weaponId, slot, ability), "Waffenfähigkeit");
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

            foreach (ArenaAction action in ArenaKeyBindings.Actions)
            {
                ArenaAction selectedAction = action;
                AddButton(content,
                    ArenaKeyBindings.GetActionName(action) + ": "
                        + field.KeyBindings.GetDisplayName(action),
                    true, () => beginCapture(selectedAction));
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
            Func<int, string, bool> assign, string label)
        {
            List<AbilityOption> options = new List<AbilityOption>
            {
                new AbilityOption(string.Empty, "Leer")
            };
            options.AddRange(unlocked);

            for (int slot = 0; slot < ArenaProgression.AbilitySlotCount; slot++)
            {
                int selectedSlot = slot;
                StackPanel row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 3, 0, 3)
                };
                row.Children.Add(new TextBlock
                {
                    Text = label + " " + (slot + 1),
                    Width = 145,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = TextBrush
                });
                ComboBox selector = new ComboBox
                {
                    Width = 200,
                    ItemsSource = options,
                    DisplayMemberPath = "Name",
                    SelectedValuePath = "Id",
                    SelectedValue = equipped[slot] ?? string.Empty
                };
                selector.SelectionChanged += (sender, args) =>
                {
                    string selected = selector.SelectedValue as string;
                    assign(selectedSlot,
                        string.IsNullOrEmpty(selected) ? null : selected);
                };
                row.Children.Add(selector);
                panel.Children.Add(row);
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

        private static bool CanUpgrade(
            ArenaProgression progression, string id,
            UpgradeCurrency currency, bool isSword, bool isBow)
        {
            if (isSword)
            {
                return progression.CanUpgradeSword(id, currency, out _);
            }
            if (isBow)
            {
                return progression.CanUpgradeBow(id, currency, out _);
            }
            return progression.CanUpgradeMagicWand(id, currency, out _);
        }

        private static void AddPurchaseButton(
            ItemsControl content, ArenaField field,
            WeaponUpgradeDefinition upgrade, UpgradeCurrency currency,
            bool enabled, int cost, Action refresh)
        {
            string currencyName = currency == UpgradeCurrency.Weapon
                ? "Waffenpunkten"
                : "universellen Punkten";
            AddButton(content,
                "Kaufen mit " + currencyName + " (" + cost + ")",
                enabled, () =>
                {
                    field.TryUpgradeWeapon(upgrade.Id, currency);
                    refresh();
                });
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

        private static void AddButton(
            DependencyObject panel, string label, bool enabled, Action action)
        {
            Button button = new Button
            {
                Content = label,
                Height = 36,
                Margin = new Thickness(0, 3, 0, 3),
                Background = enabled ? ButtonBrush : DisabledBrush,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 13,
                IsEnabled = enabled,
                Cursor = Cursors.Hand
            };
            button.Click += (sender, args) => action();
            AddElement(panel, button);
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
