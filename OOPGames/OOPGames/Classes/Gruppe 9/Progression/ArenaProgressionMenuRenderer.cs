using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOPGames
{
    internal sealed class ArenaProgressionMenuRenderer
    {
        // Scroll positions per menu page (and per scroll area on it) while the game runs, so a
        // purchase, which rebuilds the page, does not jump back to the top and a page reopens
        // where it was left.
        private readonly Dictionary<string, double> _scrollOffsets =
            new Dictionary<string, double>(StringComparer.Ordinal);
        private double _skillTreeZoom = 0.65;
        private double _skillTreeHorizontalOffset;
        private double _skillTreeVerticalOffset;
        private bool _hasSkillTreeViewport;
        private ScrollViewer _skillTreePreviewScrollViewer;
        private bool _isPanningSkillTree;
        private MouseButton _skillTreePanButton;

        // Menu click sound for windows outside the game canvas (skill point question).
        public Action PlayMenuClick { get; set; }
        private static readonly Brush TextBrush =
            new SolidColorBrush(Color.FromRgb(225, 233, 241));
        private const double ButtonHeight = 36;
        private const double SplitRowWidth = 300;
        private const double MinimumSkillTreeZoom = 0.3;
        private const double MaximumSkillTreeZoom = 2.5;
        private const string MaxedOutLabel = "Maximale Stufe erreicht";

        internal void CaptureSkillTreeViewport()
        {
            if (_skillTreePreviewScrollViewer == null)
            {
                return;
            }
            _skillTreeHorizontalOffset =
                _skillTreePreviewScrollViewer.HorizontalOffset;
            _skillTreeVerticalOffset =
                _skillTreePreviewScrollViewer.VerticalOffset;
            _hasSkillTreeViewport = true;
        }

        public UIElement CreateSkillTree(
            ArenaField field, Action refresh, double availableWidth,
            double availableHeight, BitmapSource startNodeSprite)
        {
            ArenaProgression progression = field.Progression;
            StackPanel content = CreateContent(
                "SPIELER-SKILLTREE",
                (progression.IsGodMode
                    ? "Skillpunkte: unbegrenzt"
                    : "Skillpunkte: " + progression.UniversalPoints)
                    + "  |  Kills: " + progression.TotalKills
                    + "  |  1 Punkt je 5 Kills. Spielerfähigkeiten sind unabhängig von Waffen.");
            content.Children.Add(CreateSkillTreePreview(
                field, refresh, availableWidth,
                Math.Max(360, availableHeight - 140), startNodeSprite));

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
                ArenaSkillTreeRepository.GetPlayerStatName(
                    ArenaProgression.MaximumHealth),
                "Erhöht die maximale Gesundheit um "
                    + ArenaProgression.HealthIncreasePerRank + ".",
                field.Player.MaxHitPoints,
                ArenaProgression.HealthIncreasePerRank, refresh);
            AddPlayerStatUpgrade(
                stats, field, ArenaProgression.MaximumSpeed,
                ArenaSkillTreeRepository.GetPlayerStatName(
                    ArenaProgression.MaximumSpeed),
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
                    progression.CanUpgradeSkill(definition.Id),
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
            StackPanel details = new StackPanel();
            details.Children.Add(tree);
            AddText(details, "Spielerfähigkeiten");
            AddAbilitySlots(details, progression.EntityAbilities,
                GetUnlockedEntityAbilities(progression),
                progression.TryAssignEntityAbility, "Spielerfähigkeit", refresh);
            content.Children.Add(new Expander
            {
                Header = "Skill-Details und Zusatzupgrades",
                IsExpanded = false,
                Content = details,
                Foreground = TextBrush,
                Margin = new Thickness(0, 8, 0, 0)
            });
            ScrollViewer page = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = content
            };
            RememberScrollPositions(page, "skills");
            return page;
        }

        private UIElement CreateSkillTreePreview(
            ArenaField field, Action refresh,
            double availableWidth, double availableHeight,
            BitmapSource startNodeSprite)
        {
            ArenaSkillTreeDocument tree = field.Progression.SkillTree;
            double boardWidth = Math.Max(1024,
                tree.Nodes.Count == 0 ? 1024
                    : tree.Nodes.Max(node =>
                        node.X + ArenaSkillTreeVisuals.NodeWidth + 40));
            double boardHeight = Math.Max(520,
                tree.Nodes.Count == 0 ? 520
                    : tree.Nodes.Max(node =>
                        node.Y + ArenaSkillTreeVisuals.NodeHeight + 40));
            Canvas board = new Canvas
            {
                Width = boardWidth,
                Height = boardHeight,
                Background = new SolidColorBrush(Color.FromRgb(13, 19, 27))
            };
            ArenaSkillTreeVisuals.DrawConnections(
                board, tree,
                connection =>
                {
                    ArenaSkillTreeNode target = tree.Nodes.First(
                        node => ArenaSkillTreeRepository.GetNodeId(node)
                            == connection.ToSkillId);
                    return !target.IsFork
                        && field.Progression.GetSkillRank(target.SkillId)
                            > field.Progression.GetSkillNodeRankStart(
                                ArenaSkillTreeRepository.GetNodeId(target))
                        ? Brushes.MediumSeaGreen
                        : Brushes.DeepSkyBlue;
                },
                forksAsMarkers: true);

            IReadOnlyList<ArenaSkillInfo> skills =
                ArenaSkillTreeRepository.GetImplementedSkills();
            foreach (ArenaSkillTreeNode node in tree.Nodes)
            {
                if (node.IsFork)
                {
                    FrameworkElement forkMarker =
                        ArenaSkillTreeVisuals.CreateForkMarkerVisual(
                            node.ForceDecision,
                            node.ForceDecision
                                && field.Progression.HasChosenForcedForkBranch(
                                    ArenaSkillTreeRepository.GetNodeId(node)),
                            field.Progression.GetForkInputsRemaining(
                                ArenaSkillTreeRepository.GetNodeId(node)));
                    Canvas.SetLeft(forkMarker,
                        node.X + (ArenaSkillTreeVisuals.NodeWidth
                            - forkMarker.Width) / 2);
                    Canvas.SetTop(forkMarker,
                        node.Y + (ArenaSkillTreeVisuals.NodeHeight
                            - forkMarker.Height) / 2);
                    board.Children.Add(forkMarker);
                    continue;
                }

                if (ArenaSkillTreeRepository.IsStartNode(node.SkillId))
                {
                    Border startTile =
                        ArenaSkillTreeVisuals.CreateStartNodeTile(startNodeSprite);
                    startTile.ToolTip =
                        ArenaSkillTreeRepository.StartNodeInfo.Description;
                    Canvas.SetLeft(startTile, node.X);
                    Canvas.SetTop(startTile, node.Y);
                    board.Children.Add(startTile);
                    continue;
                }

                ArenaSkillInfo skill = skills.FirstOrDefault(
                    item => item.Id == node.SkillId);
                if (skill == null)
                {
                    throw new InvalidDataException(
                        "The selected skill tree contains an unknown skill.");
                }

                string nodeId = ArenaSkillTreeRepository.GetNodeId(node);
                int rank = field.Progression.GetSkillRank(node.SkillId);
                int rankStart =
                    field.Progression.GetSkillNodeRankStart(nodeId);
                int rankStages =
                    field.Progression.GetSkillNodeRankStages(nodeId);
                int rankLimit = rankStart + rankStages;
                bool nodeUnlocked = rank > rankStart;
                bool isStat = node.SkillId == ArenaProgression.MaximumHealth
                    || node.SkillId == ArenaProgression.MaximumSpeed;
                int maximumRank = isStat
                    ? field.Progression.GetPlayerStatMaximumRank(node.SkillId)
                    : PlayerAbilityCatalog.GetDefinition(node.SkillId).MaximumRank;
                int nextCost = isStat
                    ? field.Progression.GetPlayerStatNextRankCost(node.SkillId)
                    : PlayerAbilityCatalog.GetDefinition(node.SkillId)
                        .GetNextRankCost(rank);
                bool hasPrerequisites =
                    field.Progression.HasSkillNodePrerequisites(nodeId);
                bool isMaxed = rank >= Math.Min(maximumRank, rankLimit);
                bool waitingForEarlierRanks = rank < rankStart;
                bool canPurchase = isStat
                    ? field.Progression.CanUpgradePlayerStatNode(nodeId)
                    : field.Progression.CanUpgradeSkillNode(nodeId);

                string subtitle = isMaxed
                    ? "Rang " + rank + "/" + maximumRank + " · Stufen "
                        + (rankStart + 1) + "–" + rankLimit
                    : waitingForEarlierRanks
                        ? "Wartet auf vorherige Stufen · Rang " + rank + "/"
                            + maximumRank
                    : !hasPrerequisites
                        ? "Gesperrt · " + nextCost + " Punkte"
                    : !canPurchase
                        ? "Nicht genug Skillpunkte · " + nextCost + " Punkte"
                    : rank > 0
                        ? "Rang " + rank + "/" + maximumRank
                            + " · Stufen " + (rankStart + 1) + "–" + rankLimit
                            + " · " + nextCost + " Punkte"
                        : "Freischaltbar · Stufen " + (rankStart + 1)
                            + "–" + rankLimit + " · " + nextCost + " Punkte";
                ArenaSkillTreeTileState tileState = isMaxed
                    ? ArenaSkillTreeTileState.Unlocked
                    : canPurchase
                        ? nodeUnlocked
                            ? ArenaSkillTreeTileState.Unlocked
                            : ArenaSkillTreeTileState.Available
                        : ArenaSkillTreeTileState.Locked;

                Button nodeButton = new Button
                {
                    Width = ArenaSkillTreeVisuals.NodeWidth,
                    Height = ArenaSkillTreeVisuals.NodeHeight,
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    FocusVisualStyle = null,
                    ToolTip = skill.Description
                        + (rank == 0 && !hasPrerequisites
                            ? "\nGesperrt: erst die Voraussetzungen freischalten."
                            : "\nNächste Stufe: " + nextCost + " Skillpunkte."),
                    IsEnabled = !isMaxed && canPurchase,
                    Content = ArenaSkillTreeVisuals.CreateNodeTile(
                        skill.Name, subtitle, tileState)
                };
                nodeButton.Click += (sender, args) =>
                {
                    if (isStat)
                    {
                        field.Progression.TryUpgradePlayerStatNode(nodeId);
                    }
                    else
                    {
                        field.Progression.TryUpgradeSkillNode(nodeId);
                    }
                    refresh();
                };
                Canvas.SetLeft(nodeButton, node.X);
                Canvas.SetTop(nodeButton, node.Y);
                board.Children.Add(nodeButton);
            }

            if (!_hasSkillTreeViewport)
            {
                _skillTreeZoom = Math.Max(
                    0.65, Math.Min(1, availableWidth / boardWidth));
            }
            board.LayoutTransform = new ScaleTransform(
                _skillTreeZoom, _skillTreeZoom);
            ScrollViewer previewScroll = new ScrollViewer
            {
                Content = board,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll = false,
                PanningMode = PanningMode.None,
                Background = new SolidColorBrush(Color.FromRgb(13, 19, 27))
            };
            _skillTreePreviewScrollViewer = previewScroll;
            previewScroll.PreviewMouseWheel += (sender, args) =>
                ZoomSkillTreeAtPointer(
                    previewScroll, board, args);
            previewScroll.PreviewMouseDown += (sender, args) =>
                BeginSkillTreePan(previewScroll, args);
            previewScroll.PreviewMouseMove += (sender, args) =>
                ContinueSkillTreePan(previewScroll, args);
            previewScroll.PreviewMouseUp += (sender, args) =>
                EndSkillTreePan(previewScroll, args);
            previewScroll.LostMouseCapture += (sender, args) =>
                ResetSkillTreePan(previewScroll);
            previewScroll.ScrollChanged += (sender, args) =>
            {
                if (_hasSkillTreeViewport)
                {
                    _skillTreeHorizontalOffset = previewScroll.HorizontalOffset;
                    _skillTreeVerticalOffset = previewScroll.VerticalOffset;
                }
            };
            previewScroll.Loaded += (sender, args) =>
                previewScroll.Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        if (_hasSkillTreeViewport)
                        {
                            previewScroll.ScrollToHorizontalOffset(
                                _skillTreeHorizontalOffset);
                            previewScroll.ScrollToVerticalOffset(
                                _skillTreeVerticalOffset);
                        }
                        else
                        {
                            previewScroll.ScrollToHorizontalOffset(Math.Max(
                                0, (previewScroll.ExtentWidth
                                    - previewScroll.ViewportWidth) / 2));
                            previewScroll.ScrollToVerticalOffset(Math.Max(
                                0, (previewScroll.ExtentHeight
                                    - previewScroll.ViewportHeight) / 2));
                            _hasSkillTreeViewport = true;
                        }
                    }));

            Grid preview = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 19, 27)),
                ClipToBounds = true
            };
            preview.Children.Add(previewScroll);
            Border navigationHint = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(210, 8, 13, 19)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(7, 4, 7, 4),
                Margin = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                IsHitTestVisible = false,
                Child = new TextBlock
                {
                    Text = "Mausrad: scrollen · Shift+Mausrad: zoomen · Ziehen/Mausradklick: verschieben",
                    Foreground = Brushes.White,
                    FontSize = 11
                }
            };
            preview.Children.Add(navigationHint);

            return new Border
            {
                Width = availableWidth,
                Height = availableHeight,
                BorderBrush = new SolidColorBrush(Color.FromRgb(77, 94, 113)),
                BorderThickness = new Thickness(1),
                Child = preview
            };
        }

        private void ZoomSkillTreeAtPointer(
            ScrollViewer scrollViewer, Canvas board, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                return;
            }
            double oldZoom = _skillTreeZoom;
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
            double newZoom = Math.Max(
                MinimumSkillTreeZoom,
                Math.Min(MaximumSkillTreeZoom, oldZoom * zoomFactor));
            if (Math.Abs(newZoom - oldZoom) < 0.0001)
            {
                e.Handled = true;
                return;
            }

            Point pointer = e.GetPosition(scrollViewer);
            double contentX = (scrollViewer.HorizontalOffset + pointer.X) / oldZoom;
            double contentY = (scrollViewer.VerticalOffset + pointer.Y) / oldZoom;
            _skillTreeZoom = newZoom;
            board.LayoutTransform = new ScaleTransform(newZoom, newZoom);
            scrollViewer.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() =>
                {
                    scrollViewer.ScrollToHorizontalOffset(
                        contentX * newZoom - pointer.X);
                    scrollViewer.ScrollToVerticalOffset(
                        contentY * newZoom - pointer.Y);
                }));
            e.Handled = true;
        }

        private void BeginSkillTreePan(
            ScrollViewer scrollViewer, MouseButtonEventArgs e)
        {
            bool isMiddleClick = e.ChangedButton == MouseButton.Middle;
            bool isLeftDrag = e.ChangedButton == MouseButton.Left
                && !IsInsideSkillTreeControl(e.OriginalSource as DependencyObject);
            if ((!isMiddleClick && !isLeftDrag) || _isPanningSkillTree)
            {
                return;
            }

            _isPanningSkillTree = true;
            _skillTreePanButton = e.ChangedButton;
            _skillTreePanStart = e.GetPosition(scrollViewer);
            _skillTreePanStartHorizontalOffset = scrollViewer.HorizontalOffset;
            _skillTreePanStartVerticalOffset = scrollViewer.VerticalOffset;
            scrollViewer.Cursor = Cursors.SizeAll;
            scrollViewer.CaptureMouse();
            e.Handled = true;
        }

        private Point _skillTreePanStart;
        private double _skillTreePanStartHorizontalOffset;
        private double _skillTreePanStartVerticalOffset;

        private void ContinueSkillTreePan(
            ScrollViewer scrollViewer, MouseEventArgs e)
        {
            if (!_isPanningSkillTree)
            {
                return;
            }
            bool buttonStillPressed = _skillTreePanButton == MouseButton.Middle
                ? e.MiddleButton == MouseButtonState.Pressed
                : e.LeftButton == MouseButtonState.Pressed;
            if (!buttonStillPressed)
            {
                ResetSkillTreePan(scrollViewer);
                return;
            }

            Point current = e.GetPosition(scrollViewer);
            scrollViewer.ScrollToHorizontalOffset(
                _skillTreePanStartHorizontalOffset
                    - (current.X - _skillTreePanStart.X));
            scrollViewer.ScrollToVerticalOffset(
                _skillTreePanStartVerticalOffset
                    - (current.Y - _skillTreePanStart.Y));
            e.Handled = true;
        }

        private void EndSkillTreePan(
            ScrollViewer scrollViewer, MouseButtonEventArgs e)
        {
            if (_isPanningSkillTree && e.ChangedButton == _skillTreePanButton)
            {
                ResetSkillTreePan(scrollViewer);
                e.Handled = true;
            }
        }

        private void ResetSkillTreePan(ScrollViewer scrollViewer)
        {
            if (!_isPanningSkillTree)
            {
                return;
            }
            _isPanningSkillTree = false;
            scrollViewer.Cursor = null;
            if (scrollViewer.IsMouseCaptured)
            {
                scrollViewer.ReleaseMouseCapture();
            }
        }

        private static bool IsInsideSkillTreeControl(DependencyObject source)
        {
            while (source != null)
            {
                if (source is ButtonBase || source is ScrollBar || source is Thumb)
                {
                    return true;
                }
                source = source is Visual || source is Visual3D
                    ? VisualTreeHelper.GetParent(source)
                    : LogicalTreeHelper.GetParent(source);
            }
            return false;
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
            if (!field.IsGodMode && !ArenaPreferences.Default.AskBeforeSkillPointPayment)
            {
                AddText(content, "Fehlende Waffenpunkte werden ohne Rückfrage mit Skillpunkten bezahlt.");
                AddButton(content, "Rückfrage vor Skillpunkt-Zahlung wieder einschalten", true,
                    () =>
                    {
                        ArenaPreferences.Default.AskBeforeSkillPointPayment = true;
                        refresh();
                    });
            }

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

                AddPurchaseButton(PlayMenuClick, node, field, upgrade,
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
            ScrollViewer page = CreateScrollablePage(content);
            RememberScrollPositions(page, "weapon:" + weaponId);
            return page;
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
            Action playMenuClick, ItemsControl content, ArenaField field,
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
                    // also when the weapon has no weapon points left at all, unless the
                    // player chose "Nicht erneut fragen".
                    if (skillPointsToSpend > 0
                        && ArenaPreferences.Default.AskBeforeSkillPointPayment
                        && !SkillPointPaymentDialog.Confirm(
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
                            ArenaPreferences.Default, playMenuClick))
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

        // Restores the remembered offsets of every scroll area on the page once it is laid
        // out, then keeps them up to date while the player scrolls.
        private void RememberScrollPositions(FrameworkElement page, string pageKey)
        {
            page.Loaded += (sender, e) => page.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded, new Action(() =>
                {
                    int index = 0;
                    foreach (ScrollViewer viewer in FindScrollViewers(page))
                    {
                        string key = pageKey + "#" + index++;
                        if (_scrollOffsets.TryGetValue(key, out double offset))
                        {
                            viewer.ScrollToVerticalOffset(offset);
                        }
                        viewer.ScrollChanged += (o, args) =>
                            _scrollOffsets[key] = viewer.VerticalOffset;
                    }
                }));
        }

        private static IEnumerable<ScrollViewer> FindScrollViewers(DependencyObject element)
        {
            if (element is ScrollViewer viewer)
            {
                yield return viewer;
            }
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                foreach (ScrollViewer child in FindScrollViewers(
                    VisualTreeHelper.GetChild(element, i)))
                {
                    yield return child;
                }
            }
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
