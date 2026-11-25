using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    // Dev tool tab "Gegner-Editor": lists the enemy kinds (Assets/Enemies/Types), edits
    // every value of a kind in a form (values, reward, behaviour, look, spawning and a list
    // of attacks), shows a preview with the problems of the current values, saves,
    // duplicates, deletes and spawns the current version beside the player.
    // Every field is optional: empty means the standard value shown next to it.
    internal sealed class ArenaEnemyEditorControl : UserControl
    {
        private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
        private static readonly Brush TextBrush = new SolidColorBrush(Color.FromRgb(183, 197, 212));
        private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(255, 120, 120));
        private static readonly Brush PanelBrush = new SolidColorBrush(Color.FromRgb(30, 40, 52));
        private const double PreviewSize = 260;
        private const double SpriteViewHeight = 170;
        // The look view enlarges the enemy up to this factor (game at zoom 1 = 1).
        private const double SpriteViewScale = 3;
        private const double LabelColumnWidth = 230;
        private const double InputColumnWidth = 180;
        internal const string IdLabel = "Id (Dateiname)";
        internal const string RegionsLabel = "Spawn-Regionen";

        private readonly ArenaField _field;
        private readonly ListBox _typeList;
        private readonly Canvas _spriteView;
        private readonly Canvas _preview;
        private readonly TextBlock _previewInfo;
        private readonly TextBlock _status;
        private readonly StackPanel _attackPanel;
        private readonly List<FieldEditor<EnemyTypeDefinition>> _editors =
            new List<FieldEditor<EnemyTypeDefinition>>();
        private readonly List<AttackEditor> _attacks = new List<AttackEditor>();
        private readonly Dictionary<string, BitmapSource> _previewFrames =
            new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
        private string _loadedId;
        private bool _dirty;
        private bool _loading;
        private bool _suppressListSelection;

        public ArenaEnemyEditorControl(ArenaField field)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(PreviewSize + 24) });

            _typeList = new ListBox
            {
                Margin = new Thickness(4),
                Background = PanelBrush,
                Foreground = Brushes.White
            };
            _typeList.SelectionChanged += OnTypeSelected;
            root.Children.Add(_typeList);

            StackPanel form = new StackPanel { Margin = new Thickness(8, 4, 8, 4) };
            BuildRows(form, CreateTypeFields(), _editors);
            form.Children.Add(CreateHeading("Angriffe"));
            form.Children.Add(new TextBlock
            {
                Text = "Ohne Eintrag kämpft die Art mit den Standardklauen. Von den bereiten "
                    + "Angriffen, die zum Abstand passen, wird einer nach Gewichtung gewählt.",
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            });
            _attackPanel = new StackPanel();
            form.Children.Add(_attackPanel);
            Button addAttack = CreateButton("+ Angriff");
            addAttack.HorizontalAlignment = HorizontalAlignment.Left;
            addAttack.Click += (sender, args) => AddAttack();
            form.Children.Add(addAttack);
            ScrollViewer formScroll = new ScrollViewer
            {
                Content = form,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4)
            };
            Grid.SetColumn(formScroll, 1);
            root.Children.Add(formScroll);

            StackPanel previewPanel = new StackPanel { Margin = new Thickness(8, 4, 4, 4) };
            previewPanel.Children.Add(CreateHeading("Vorschau"));
            _spriteView = CreatePreviewCanvas(SpriteViewHeight);
            _spriteView.Margin = new Thickness(0, 0, 0, 6);
            previewPanel.Children.Add(_spriteView);
            _preview = CreatePreviewCanvas(PreviewSize);
            previewPanel.Children.Add(_preview);
            _previewInfo = new TextBlock
            {
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ScrollViewer previewScroll = new ScrollViewer
            {
                Content = previewPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            previewPanel.Children.Add(_previewInfo);
            Grid.SetColumn(previewScroll, 2);
            root.Children.Add(previewScroll);

            StackPanel footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4, 2, 4, 0)
            };
            AddButton(footer, "Neu", StartNewType);
            AddButton(footer, "Duplizieren", DuplicateType);
            AddButton(footer, "Löschen", DeleteType);
            AddButton(footer, "Speichern", () => SaveType());
            AddButton(footer, "Gegner neben mir spawnen", SpawnTestEnemy);
            AddButton(footer, "Alle Gegner entfernen", RemoveAllEnemies);
            _status = new TextBlock
            {
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            footer.Children.Add(_status);
            Grid.SetRow(footer, 1);
            Grid.SetColumnSpan(footer, 3);
            root.Children.Add(footer);

            Content = root;
            RefreshTypeList(EnemyType.StandardId);
            if (_field.EnemyTypeProblems.Count > 0)
            {
                SetStatus("Hinweise beim Laden: " + String.Join(" | ", _field.EnemyTypeProblems));
            }
        }

        // Asks the user; tests replace it to answer without a dialog.
        internal Func<string, bool> Confirm { get; set; } = message =>
            MessageBox.Show(message, "Gegner-Editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        internal Action<string> ShowError { get; set; } = message =>
            MessageBox.Show(message, "Gegner-Editor",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        internal string LoadedId { get { return _loadedId; } }
        internal bool IsDirty { get { return _dirty; } }
        internal string StatusText { get { return _status.Text; } }
        internal string PreviewText { get { return _previewInfo.Text; } }
        internal int AttackCount { get { return _attacks.Count; } }

        // ---- field lists ----

        private static IEnumerable<FieldSpec<EnemyTypeDefinition>> CreateTypeFields()
        {
            EnemyType s = EnemyType.Standard;
            const string basics = "Grunddaten";
            yield return Fields<EnemyTypeDefinition>.Text(basics, IdLabel, "Pflicht: Buchstaben, Ziffern, - und _",
                d => d.Id, (d, v) => d.Id = v, "Name der JSON-Datei unter Assets/Enemies/Types.");
            yield return Fields<EnemyTypeDefinition>.Text(basics, "Anzeigename", "Standard: = Id",
                d => d.Name, (d, v) => d.Name = v);
            yield return Fields<EnemyTypeDefinition>.Text(basics, "Beschreibung", "Notiz für das Team",
                d => d.Description, (d, v) => d.Description = v);
            yield return Fields<EnemyTypeDefinition>.YesNo(basics, "Aktiv (spawnt im Spiel)", true,
                d => d.Enabled, (d, v) => d.Enabled = v);

            const string values = "Werte";
            yield return Fields<EnemyTypeDefinition>.Int(values, "Lebenspunkte", s.MaxHitPoints,
                d => d.MaxHitPoints, (d, v) => d.MaxHitPoints = v);
            yield return Fields<EnemyTypeDefinition>.Number(values, "Tempo (× Spielertempo)", s.SpeedFactor,
                d => d.SpeedFactor, (d, v) => d.SpeedFactor = v);
            yield return Fields<EnemyTypeDefinition>.Number(values, "Größe", s.Size,
                d => d.Size, (d, v) => d.Size = v, "Sprite und Hitbox skalieren mit.");
            yield return Fields<EnemyTypeDefinition>.YesNo(values, "Lässt sich schubsen", s.CanBePushed,
                d => d.CanBePushed, (d, v) => d.CanBePushed = v);
            yield return Fields<EnemyTypeDefinition>.YesNo(values, "Blockiert Bewegung", s.BlocksMovement,
                d => d.BlocksMovement, (d, v) => d.BlocksMovement = v,
                "Nein: andere laufen hindurch, z. B. damit der Spieler auf eine Mine treten kann.");
            yield return Fields<EnemyTypeDefinition>.Number(values, "Rückstoßresistenz (0–1)", s.KnockbackResistance,
                d => d.KnockbackResistance, (d, v) => d.KnockbackResistance = v,
                "0 = voller Rückstoß, 1 = keiner.");
            yield return Fields<EnemyTypeDefinition>.Number(values, "Schadensfaktor", s.DamageTakenMultiplier,
                d => d.DamageTakenMultiplier, (d, v) => d.DamageTakenMultiplier = v,
                "Unter 1 = Panzerung, über 1 = verwundbar.");
            yield return Fields<EnemyTypeDefinition>.Number(values, "Regeneration (LP/s)", s.RegenerationPerSecond,
                d => d.RegenerationPerSecond, (d, v) => d.RegenerationPerSecond = v);
            yield return Fields<EnemyTypeDefinition>.Flags(values, "Immun gegen", "Standard: nichts",
                EnemyType.StatusNames, d => d.StatusImmunities, (d, v) => d.StatusImmunities = v);

            const string reward = "Belohnung";
            yield return Fields<EnemyTypeDefinition>.Int(reward, "Zählt als Kills (Score)", s.ScoreValue,
                d => d.ScoreValue, (d, v) => d.ScoreValue = v,
                "Für Score, Bestenliste und Spawnbedingungen „ab Kills“.");
            yield return Fields<EnemyTypeDefinition>.Int(reward, "Waffenpunkte pro Kill", s.WeaponPointsPerKill,
                d => d.WeaponPointsPerKill, (d, v) => d.WeaponPointsPerKill = v);
            yield return Fields<EnemyTypeDefinition>.Int(reward, "Kills für Skillpunkte", s.KillsTowardSkillPoint,
                d => d.KillsTowardSkillPoint, (d, v) => d.KillsTowardSkillPoint = v,
                "Je " + ArenaProgression.KillsPerUniversalPoint + " gibt es einen Skillpunkt.");

            const string behavior = "Verhalten";
            yield return Fields<EnemyTypeDefinition>.Choice(behavior, "Verhaltenstyp", "Standard: Verfolger",
                new[]
                {
                    new KeyValuePair<string, string>("Chaser", "Verfolger (bis zum Kontakt)"),
                    new KeyValuePair<string, string>("Ranged", "Fernkämpfer (hält Abstand)"),
                    new KeyValuePair<string, string>("Turret", "Turm (bewegt sich nicht)"),
                    new KeyValuePair<string, string>("Coward", "Feigling (flieht bei wenig Leben)")
                },
                d => d.BehaviorType, (d, v) => d.BehaviorType = v);
            yield return Fields<EnemyTypeDefinition>.Number(behavior, "Wunschabstand (Fernkämpfer)", s.PreferredDistance,
                d => d.PreferredDistance, (d, v) => d.PreferredDistance = v);
            yield return Fields<EnemyTypeDefinition>.Number(behavior, "Flucht unter Leben in % (Feigling)",
                s.FleeBelowHealthPercent,
                d => d.FleeBelowHealthPercent, (d, v) => d.FleeBelowHealthPercent = v);
            yield return Fields<EnemyTypeDefinition>.Choice(behavior, "Zielwahl",
                "Standard: Spieler/Spiegelbilder in Sicht",
                new[]
                {
                    new KeyValuePair<string, string>("Player", "Spieler/Spiegelbilder in Sicht"),
                    new KeyValuePair<string, string>("RandomEntity",
                        "zufällige Entity (auch Gegner, ohne Sichtgrenze)")
                },
                d => d.TargetSelection, (d, v) => d.TargetSelection = v);
            yield return Fields<EnemyTypeDefinition>.Number(behavior, "Neues Zufallsziel nach (s)",
                s.RetargetSeconds,
                d => d.RetargetSeconds, (d, v) => d.RetargetSeconds = v);
            yield return Fields<EnemyTypeDefinition>.Choice(behavior, "Bewegungsart",
                "Standard: laufen",
                new[]
                {
                    new KeyValuePair<string, string>("Walk", "laufen"),
                    new KeyValuePair<string, string>("Hop", "hüpfen (Sprünge mit Pausen)")
                },
                d => d.MovementStyle, (d, v) => d.MovementStyle = v);
            yield return Fields<EnemyTypeDefinition>.Number(behavior, "Sprungdauer (s)", s.HopSeconds,
                d => d.HopSeconds, (d, v) => d.HopSeconds = v);
            yield return Fields<EnemyTypeDefinition>.Number(behavior, "Pause zwischen Sprüngen (s)",
                s.HopPauseSeconds,
                d => d.HopPauseSeconds, (d, v) => d.HopPauseSeconds = v);

            const string perception = "Wahrnehmung und Alarm";
            yield return Fields<EnemyTypeDefinition>.Number(perception, "Sichtweite", s.SightRange,
                d => d.SightRange, (d, v) => d.SightRange = v);
            yield return Fields<EnemyTypeDefinition>.Number(perception, "Sichtbonus nach Entdeckung",
                s.SightBonusAfterDetection,
                d => d.SightBonusAfterDetection, (d, v) => d.SightBonusAfterDetection = v);
            yield return Fields<EnemyTypeDefinition>.YesNo(perception, "Greift Spiegelbilder an",
                s.TargetsMirrorImages,
                d => d.TargetsMirrorImages, (d, v) => d.TargetsMirrorImages = v);
            yield return Fields<EnemyTypeDefinition>.Number(perception, "Alarmdauer nach Treffer (s)",
                s.AlertDurationSeconds,
                d => d.AlertDurationSeconds, (d, v) => d.AlertDurationSeconds = v,
                "So lange weiß der Gegner nach einem direkten Treffer, wo der Spieler ist.");
            yield return Fields<EnemyTypeDefinition>.Number(perception, "Alarmradius", s.AlertRadius,
                d => d.AlertRadius, (d, v) => d.AlertRadius = v,
                "Artgenossen in diesem Radius werden gewarnt (Kettenreaktion).");
            yield return Fields<EnemyTypeDefinition>.YesNo(perception, "Warnt Artgenossen", s.WarnsOthers,
                d => d.WarnsOthers, (d, v) => d.WarnsOthers = v);
            yield return Fields<EnemyTypeDefinition>.YesNo(perception, "Lässt sich warnen", s.CanBeWarned,
                d => d.CanBeWarned, (d, v) => d.CanBeWarned = v);

            const string wander = "Streunen";
            yield return Fields<EnemyTypeDefinition>.Number(wander, "Streun-Tempo (× eigenes Tempo)",
                s.WanderSpeedFactor,
                d => d.WanderSpeedFactor, (d, v) => d.WanderSpeedFactor = v);
            yield return Fields<EnemyTypeDefinition>.Number(wander, "Richtungswechsel frühestens (s)",
                s.WanderMinSeconds,
                d => d.WanderMinSeconds, (d, v) => d.WanderMinSeconds = v);
            yield return Fields<EnemyTypeDefinition>.Number(wander, "Richtungswechsel spätestens (s)",
                s.WanderMaxSeconds,
                d => d.WanderMaxSeconds, (d, v) => d.WanderMaxSeconds = v,
                null, "4 (mind. „frühestens“)");

            const string sounds = "Sounds";
            yield return Fields<EnemyTypeDefinition>.Choice(sounds, "Gelegentlicher Sound",
                "Standard: keiner",
                ArenaAudio.EnemySoundNames.Select(name => new KeyValuePair<string, string>(name, name)),
                d => d.IdleSound, (d, v) => d.IdleSound = v,
                "Spielt immer wieder nach einer zufälligen Pause (z. B. ein quakender Frosch).");
            yield return Fields<EnemyTypeDefinition>.Number(sounds, "Pause frühestens (s)",
                s.IdleSoundMinSeconds,
                d => d.IdleSoundMinSeconds, (d, v) => d.IdleSoundMinSeconds = v);
            yield return Fields<EnemyTypeDefinition>.Number(sounds, "Pause spätestens (s)",
                s.IdleSoundMaxSeconds,
                d => d.IdleSoundMaxSeconds, (d, v) => d.IdleSoundMaxSeconds = v);

            const string look = "Aussehen";
            yield return Fields<EnemyTypeDefinition>.Choice(look, "Spritesheet",
                "Standard: " + EnemyType.DefaultSpriteSheet,
                GetSpriteSheets().Select(file => new KeyValuePair<string, string>(file, file)),
                d => d.SpriteSheet, (d, v) => d.SpriteSheet = v,
                "PNG mit gleichnamiger LibreSprite-JSON in Assets/Enemies; Hitbox wird daraus gemessen.");
            yield return Fields<EnemyTypeDefinition>.YesNo(look, "Lebensleiste anzeigen", s.ShowHealthBar,
                d => d.ShowHealthBar, (d, v) => d.ShowHealthBar = v);
            yield return Fields<EnemyTypeDefinition>.Text(look, "Tönung (#RRGGBB)", "Standard: keine",
                d => d.Tint, (d, v) => d.Tint = v);
            yield return Fields<EnemyTypeDefinition>.Number(look, "Deckkraft (0,05–1)", s.Opacity,
                d => d.Opacity, (d, v) => d.Opacity = v);

            const string spawning = "Spawning";
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Anzahl zu Run-Beginn", s.StartCount,
                d => d.StartCount, (d, v) => d.StartCount = v);
            yield return Fields<EnemyTypeDefinition>.Number(spawning, "Spawn-Takt (s)", s.SpawnIntervalSeconds,
                d => d.SpawnIntervalSeconds, (d, v) => d.SpawnIntervalSeconds = v,
                "Alle so viele Sekunden Spielzeit gibt es einen Spawn-Versuch.");
            yield return Fields<EnemyTypeDefinition>.Number(spawning, "Erster Versuch nach (s)", null,
                d => d.FirstSpawnDelaySeconds, (d, v) => d.FirstSpawnDelaySeconds = v,
                null, "= Spawn-Takt");
            yield return Fields<EnemyTypeDefinition>.Number(spawning, "Spawn-Wahrscheinlichkeit (%)",
                s.SpawnChancePercent,
                d => d.SpawnChancePercent, (d, v) => d.SpawnChancePercent = v,
                "Chance, dass ein Spawn-Versuch wirklich einen Gegner (bzw. eine Gruppe) bringt.");
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Max. gleichzeitig lebend", s.MaxAlive,
                d => d.MaxAlive, (d, v) => d.MaxAlive = v,
                "Zusätzlich gilt die Gesamtgrenze von " + ArenaField.MaximumEnemyCount
                    + " Gegnern (im Code).");
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Gruppengröße min.", s.GroupSizeMin,
                d => d.GroupSizeMin, (d, v) => d.GroupSizeMin = v);
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Gruppengröße max.", null,
                d => d.GroupSizeMax, (d, v) => d.GroupSizeMax = v, null, "= Gruppengröße min.");
            yield return Fields<EnemyTypeDefinition>.Number(spawning, "Mindestabstand zum Spieler", null,
                d => d.MinDistanceToPlayer, (d, v) => d.MinDistanceToPlayer = v,
                null, "= Sichtweite + " + EnemyType.SpawnVisibilityMargin);
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Ab Kills", s.MinKills,
                d => d.MinKills, (d, v) => d.MinKills = v);
            yield return Fields<EnemyTypeDefinition>.Int(spawning, "Bis Kills (ausschließlich)", null,
                d => d.MaxKills, (d, v) => d.MaxKills = v, null, "unbegrenzt");
            yield return Fields<EnemyTypeDefinition>.Number(spawning, "Ab Spielzeit (s)", s.MinRunSeconds,
                d => d.MinRunSeconds, (d, v) => d.MinRunSeconds = v);
            yield return Fields<EnemyTypeDefinition>.Flags(spawning, RegionsLabel,
                "Standard: überall",
                GetRegionOptions(),
                d => d.Regions, (d, v) => d.Regions = v,
                "Regionen aus dem Tab „Spawn-Regionen“ (alle Maps). Fehlt eine Region auf "
                    + "der gespielten Map, zählt sie dort nicht; ohne passende Region: überall.");
            yield return Fields<EnemyTypeDefinition>.Choice(spawning, "Spielerbedingung",
                "Standard: immer",
                new[]
                {
                    new KeyValuePair<string, string>("Always", "immer"),
                    new KeyValuePair<string, string>("PlayerInsideRegion",
                        "nur wenn der Spieler in einer der Regionen ist"),
                    new KeyValuePair<string, string>("PlayerOutsideRegion",
                        "nur wenn der Spieler außerhalb ist")
                },
                d => d.PlayerCondition, (d, v) => d.PlayerCondition = v,
                "Bezieht sich auf die gewählten Spawn-Regionen der gespielten Map.");
        }

        private static readonly string[] AnglePatterns = { EnemyAttack.PatternCone, EnemyAttack.PatternArc };
        private static readonly string[] DurationPatterns =
        {
            EnemyAttack.PatternCone, EnemyAttack.PatternArc, EnemyAttack.PatternSpin,
            EnemyAttack.PatternWave
        };
        private static readonly string[] ProjectilePatterns = { EnemyAttack.PatternProjectile };

        private static IEnumerable<FieldSpec<EnemyAttackDefinition>> CreateAttackFields()
        {
            const string fromWeapon = "Standard: aus der Waffe";
            const string weapon = "Waffe und Muster";
            yield return Fields<EnemyAttackDefinition>.Choice(weapon, "Waffe",
                "Standard: " + WeaponCatalog.EnemyClaws.Name,
                EnemyAttack.BaseWeapons.Select(item =>
                    new KeyValuePair<string, string>(item.Id, item.Name)),
                a => a.BaseWeapon, (a, v) => a.BaseWeapon = v,
                "Fertige Waffe als Vorlage; alle weiteren Werte überschreiben sie.");
            yield return Fields<EnemyAttackDefinition>.Choice(weapon, "Ziele",
                "Standard: Spielerseite",
                new[]
                {
                    new KeyValuePair<string, string>("Opponents", "Spielerseite"),
                    new KeyValuePair<string, string>("Everyone",
                        "alle (auch andere Gegner; trifft die nächste Entity im Radius)")
                },
                a => a.Targets, (a, v) => a.Targets = v);
            yield return Fields<EnemyAttackDefinition>.Choice(weapon, "Muster",
                "Standard: das der Waffe",
                EnemyAttack.Patterns.Select(pattern =>
                    new KeyValuePair<string, string>(pattern, EnemyAttack.GetPatternName(pattern))),
                a => a.Pattern, (a, v) => a.Pattern = v);
            yield return Fields<EnemyAttackDefinition>.Number(weapon, "Reichweite / Radius", null,
                a => a.Range, (a, v) => a.Range = v, null, fromWeapon);
            yield return Fields<EnemyAttackDefinition>.Number(weapon, "Winkel (°)", null,
                a => a.AngleDegrees, (a, v) => a.AngleDegrees = v,
                "Öffnung des Kegels bzw. überstrichener Bogen.", fromWeapon, AnglePatterns);
            yield return Fields<EnemyAttackDefinition>.Number(weapon, "Dauer (s)", null,
                a => a.DurationSeconds, (a, v) => a.DurationSeconds = v, null, fromWeapon,
                DurationPatterns);
            yield return Fields<EnemyAttackDefinition>.Number(weapon, "Projektiltempo", null,
                a => a.ProjectileSpeed, (a, v) => a.ProjectileSpeed = v, null, fromWeapon,
                ProjectilePatterns);
            yield return Fields<EnemyAttackDefinition>.Number(weapon, "Projektilgröße", null,
                a => a.ProjectileSize, (a, v) => a.ProjectileSize = v, null, fromWeapon,
                ProjectilePatterns);

            const string hit = "Treffer";
            yield return Fields<EnemyAttackDefinition>.Int(hit, "Schaden", null,
                a => a.Damage, (a, v) => a.Damage = v, null, fromWeapon);
            yield return Fields<EnemyAttackDefinition>.Number(hit, "Abklingzeit (s)", null,
                a => a.CooldownSeconds, (a, v) => a.CooldownSeconds = v, null, fromWeapon);
            yield return Fields<EnemyAttackDefinition>.Number(hit, "Rückstoß", null,
                a => a.Knockback, (a, v) => a.Knockback = v, null, fromWeapon);
            yield return Fields<EnemyAttackDefinition>.Int(hit, "Treffer pro Ziel", null,
                a => a.MaxHitsPerTarget, (a, v) => a.MaxHitsPerTarget = v, null, fromWeapon);

            const string effects = "Zusatzeffekte (leer = keiner)";
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Gift (Schaden/s)", null,
                a => a.PoisonDamagePerSecond, (a, v) => a.PoisonDamagePerSecond = v, null, "kein Gift");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Giftdauer (s)", 3,
                a => a.PoisonSeconds, (a, v) => a.PoisonSeconds = v);
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Feuer (Schaden/s)", null,
                a => a.FireDamagePerSecond, (a, v) => a.FireDamagePerSecond = v, null, "kein Feuer");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Feuerdauer (s)", 3,
                a => a.FireSeconds, (a, v) => a.FireSeconds = v);
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Frost (s)", null,
                a => a.FreezeSeconds, (a, v) => a.FreezeSeconds = v, null, "kein Frost");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Betäubung (s)", null,
                a => a.StunSeconds, (a, v) => a.StunSeconds = v, null, "keine Betäubung");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Verlangsamung: Resttempo (%)",
                null, a => a.SlowPercent, (a, v) => a.SlowPercent = v, null, "keine Verlangsamung");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Verlangsamungsdauer (s)", 2,
                a => a.SlowSeconds, (a, v) => a.SlowSeconds = v);
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Schwäche: Restschaden (%)",
                null, a => a.WeaknessPercent, (a, v) => a.WeaknessPercent = v, null, "keine Schwäche");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Schwächedauer (s)", 3,
                a => a.WeaknessSeconds, (a, v) => a.WeaknessSeconds = v);
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Lebensraub (%)", null,
                a => a.LifeStealPercent, (a, v) => a.LifeStealPercent = v, null, "kein Lebensraub");
            yield return Fields<EnemyAttackDefinition>.YesNo(effects, "Verschlingen", false,
                a => a.Devour, (a, v) => a.Devour = v,
                "Zieht jedes getroffene Ziel heran und tötet es sofort (Frosch-Zunge).");
            yield return Fields<EnemyAttackDefinition>.Number(effects, "Heranziehdauer (s)", 0.4,
                a => a.DevourPullSeconds, (a, v) => a.DevourPullSeconds = v);

            yield return Fields<EnemyAttackDefinition>.YesNo(effects, "Tödlich", false,
                a => a.Lethal, (a, v) => a.Lethal = v,
                "Jedes getroffene Ziel stirbt sofort (Godmode schützt).");

            const string sequence = "Auslöser und Ablauf";
            yield return Fields<EnemyAttackDefinition>.Choice(sequence, "Auslöser",
                "Standard: Abstand",
                new[]
                {
                    new KeyValuePair<string, string>("Distance", "Abstand (Einsatz ab/bis)"),
                    new KeyValuePair<string, string>("Contact", "Kontakt (Hitboxen berühren sich)")
                },
                a => a.Trigger, (a, v) => a.Trigger = v);
            yield return Fields<EnemyAttackDefinition>.Choice(sequence, "Ausgelöst von",
                "Standard: wer getroffen werden kann",
                new[]
                {
                    new KeyValuePair<string, string>("Targets", "wer getroffen werden kann"),
                    new KeyValuePair<string, string>("Player", "nur der Spieler"),
                    new KeyValuePair<string, string>("PlayerSide", "Spieler und Spiegelbilder"),
                    new KeyValuePair<string, string>("Anyone", "jeder")
                },
                a => a.TriggeredBy, (a, v) => a.TriggeredBy = v);
            yield return Fields<EnemyAttackDefinition>.Text(sequence, "Vorlauf",
                "Clip:Sekunden[:Sound]; … (leer = sofort)",
                a => FormatWindup(a.Windup), (a, v) => a.Windup = ParseWindup(v),
                "Clips des Spritesheets, die nacheinander laufen, bevor der Angriff kommt, "
                    + "z. B. „Aktive:0,3; Hello:0,7:MineTurtleHello“. Sounds: "
                    + String.Join(", ", ArenaAudio.EnemySoundNames) + ".");
            yield return Fields<EnemyAttackDefinition>.Choice(sequence, "Sound beim Angriff",
                "Standard: keiner",
                ArenaAudio.EnemySoundNames.Select(name => new KeyValuePair<string, string>(name, name)),
                a => a.Sound, (a, v) => a.Sound = v);
            yield return Fields<EnemyAttackDefinition>.YesNo(sequence, "Stirbt nach dem Angriff", false,
                a => a.KillsAttacker, (a, v) => a.KillsAttacker = v,
                "Selbstzerstörung: der Gegner stirbt, sobald der Angriff vorbei ist.");

            const string choice = "Einsatz";
            yield return Fields<EnemyAttackDefinition>.Number(choice, "Einsatz ab Abstand", 0,
                a => a.UseFromDistance, (a, v) => a.UseFromDistance = v,
                "Abstand zur Hitbox des Ziels.");
            yield return Fields<EnemyAttackDefinition>.Number(choice, "Einsatz bis Abstand", null,
                a => a.UseUpToDistance, (a, v) => a.UseUpToDistance = v, null, "= Reichweite");
            yield return Fields<EnemyAttackDefinition>.Number(choice, "Gewichtung", 1,
                a => a.Weight, (a, v) => a.Weight = v,
                "Wie oft der Angriff gewählt wird, wenn mehrere passen.");
        }

        private static IEnumerable<KeyValuePair<string, string>> GetRegionOptions()
        {
            return ArenaMapRegionRepository.GetAllRegionNames()
                .Select(name => new KeyValuePair<string, string>(name, name));
        }

        // The regions tab saved: offer the current region names.
        internal void RefreshRegionNames()
        {
            _editors.First(editor => editor.Spec.Label == RegionsLabel)
                .SetFlagOptions(GetRegionOptions().ToArray());
        }

        // Windup steps as text: "Clip:Seconds[:Sound]; ..." with a decimal comma.
        internal static string FormatWindup(List<EnemyWindupStepDefinition> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return null;
            }
            return String.Join("; ", steps.Select(step =>
                (step?.Clip ?? "") + ":"
                    + (step?.Seconds.HasValue == true ? Format(step.Seconds.Value) : "")
                    + (String.IsNullOrEmpty(step?.Sound) ? "" : ":" + step.Sound)));
        }

        // Unreadable seconds become NaN, which the kind's check reports.
        internal static List<EnemyWindupStepDefinition> ParseWindup(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            List<EnemyWindupStepDefinition> steps = new List<EnemyWindupStepDefinition>();
            foreach (string part in text.Split(';'))
            {
                if (String.IsNullOrWhiteSpace(part))
                {
                    continue;
                }
                string[] pieces = part.Split(':');
                double? seconds = null;
                if (pieces.Length > 1 && pieces[1].Trim().Length > 0)
                {
                    seconds = Double.TryParse(pieces[1].Trim().Replace(',', '.'),
                        NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out double value) ? value : double.NaN;
                }
                steps.Add(new EnemyWindupStepDefinition
                {
                    Clip = pieces[0].Trim(),
                    Seconds = seconds,
                    Sound = pieces.Length > 2 && pieces[2].Trim().Length > 0
                        ? pieces[2].Trim()
                        : null
                });
            }
            return steps;
        }

        // PNG sheets in Assets/Enemies that have a LibreSprite JSON of the same name.
        internal static IReadOnlyList<string> GetSpriteSheets()
        {
            try
            {
                string directory = PainterG9.FindAssetDirectory("Enemies");
                return Directory.GetFiles(directory, "*.png")
                    .Where(path => File.Exists(System.IO.Path.ChangeExtension(path, ".json")))
                    .Select(System.IO.Path.GetFileName)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                return new[] { EnemyType.DefaultSpriteSheet };
            }
        }

        // ---- building rows ----

        private void BuildRows<T>(Panel form, IEnumerable<FieldSpec<T>> specs,
            List<FieldEditor<T>> editors)
        {
            string currentGroup = null;
            Grid grid = null;
            foreach (FieldSpec<T> spec in specs)
            {
                if (spec.Group != currentGroup)
                {
                    currentGroup = spec.Group;
                    form.Children.Add(CreateHeading(spec.Group));
                    grid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(LabelColumnWidth) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(InputColumnWidth) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    form.Children.Add(grid);
                }

                FieldEditor<T> editor = new FieldEditor<T>(spec);
                editor.Changed += OnFieldChanged;
                editors.Add(editor);

                int row = grid.RowDefinitions.Count;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                editor.Label = new TextBlock
                {
                    Text = spec.Label,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 3, 8, 3),
                    ToolTip = spec.Tooltip
                };
                Grid.SetRow(editor.Label, row);
                grid.Children.Add(editor.Label);
                Grid.SetRow(editor.Control, row);
                Grid.SetColumn(editor.Control, 1);
                grid.Children.Add(editor.Control);
                editor.Standard = new TextBlock
                {
                    Text = spec.StandardText,
                    Foreground = TextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 3, 0, 3),
                    TextWrapping = TextWrapping.Wrap,
                    ToolTip = spec.Tooltip
                };
                Grid.SetRow(editor.Standard, row);
                Grid.SetColumn(editor.Standard, 2);
                grid.Children.Add(editor.Standard);
            }
        }

        private void OnFieldChanged()
        {
            if (_loading)
            {
                return;
            }
            _dirty = true;
            foreach (AttackEditor attack in _attacks)
            {
                attack.UpdateVisibleFields();
            }
            UpdatePreview();
        }

        // ---- attacks ----

        internal void AddAttack()
        {
            AddAttackEditor(new EnemyAttackDefinition());
            OnFieldChanged();
        }

        private AttackEditor AddAttackEditor(EnemyAttackDefinition definition)
        {
            AttackEditor attack = new AttackEditor();
            StackPanel content = new StackPanel();
            DockPanel header = new DockPanel { Margin = new Thickness(0, 0, 0, 2) };
            Button remove = CreateButton("Angriff entfernen");
            remove.Click += (sender, args) => RemoveAttack(attack);
            DockPanel.SetDock(remove, Dock.Right);
            header.Children.Add(remove);
            attack.Title = new TextBlock
            {
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            header.Children.Add(attack.Title);
            content.Children.Add(header);
            BuildRows(content, CreateAttackFields(), attack.Editors);
            attack.Panel = new Border
            {
                Child = content,
                BorderBrush = new SolidColorBrush(Color.FromRgb(77, 94, 113)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 4, 0, 4)
            };
            _attacks.Add(attack);
            _attackPanel.Children.Add(attack.Panel);
            bool wasLoading = _loading;
            _loading = true;
            try
            {
                foreach (FieldEditor<EnemyAttackDefinition> editor in attack.Editors)
                {
                    editor.Load(definition);
                }
            }
            finally
            {
                _loading = wasLoading;
            }
            RenumberAttacks();
            attack.UpdateVisibleFields();
            return attack;
        }

        internal void RemoveAttackAt(int index)
        {
            RemoveAttack(_attacks[index]);
        }

        private void RemoveAttack(AttackEditor attack)
        {
            _attacks.Remove(attack);
            _attackPanel.Children.Remove(attack.Panel);
            RenumberAttacks();
            OnFieldChanged();
        }

        private void RenumberAttacks()
        {
            for (int i = 0; i < _attacks.Count; i++)
            {
                _attacks[i].Title.Text = "Angriff " + (i + 1);
            }
        }

        internal void SetAttackFieldValue(int index, string label, string value)
        {
            _attacks[index].Editors.First(editor => editor.Spec.Label == label).Value = value;
        }

        internal bool IsAttackFieldVisible(int index, string label)
        {
            return _attacks[index].Editors.First(editor => editor.Spec.Label == label)
                .Control.Visibility == Visibility.Visible;
        }

        // ---- reading and loading the form ----

        // The definition the form describes; parse errors are collected in errors.
        private EnemyTypeDefinition ReadForm(List<string> errors)
        {
            EnemyTypeDefinition definition = new EnemyTypeDefinition();
            foreach (FieldEditor<EnemyTypeDefinition> editor in _editors)
            {
                AddError(errors, null, editor.Store(definition));
            }
            if (_attacks.Count > 0)
            {
                definition.Attacks = new List<EnemyAttackDefinition>();
                for (int i = 0; i < _attacks.Count; i++)
                {
                    EnemyAttackDefinition attack = new EnemyAttackDefinition();
                    foreach (FieldEditor<EnemyAttackDefinition> editor in _attacks[i].Editors)
                    {
                        AddError(errors, "Angriff " + (i + 1) + ": ", editor.Store(attack));
                    }
                    definition.Attacks.Add(attack);
                }
            }
            return definition;
        }

        private static void AddError(List<string> errors, string prefix, string error)
        {
            if (error != null)
            {
                errors.Add(prefix + error);
            }
        }

        private void LoadForm(EnemyTypeDefinition definition, string loadedId)
        {
            _loading = true;
            try
            {
                foreach (FieldEditor<EnemyTypeDefinition> editor in _editors)
                {
                    editor.Load(definition);
                }
                _attacks.Clear();
                _attackPanel.Children.Clear();
                foreach (EnemyAttackDefinition attack in definition.Attacks
                    ?? new List<EnemyAttackDefinition>())
                {
                    AddAttackEditor(attack ?? new EnemyAttackDefinition());
                }
            }
            finally
            {
                _loading = false;
            }
            _loadedId = loadedId;
            _dirty = false;
            UpdatePreview();
        }

        // ---- list ----

        private void RefreshTypeList(string selectId)
        {
            _suppressListSelection = true;
            try
            {
                _typeList.ItemsSource = EnemyTypeRepository.GetTypeIds();
                _typeList.SelectedItem = _typeList.Items.Cast<string>().FirstOrDefault(
                    id => String.Equals(id, selectId, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                _suppressListSelection = false;
            }
            if (_typeList.SelectedItem is string id)
            {
                LoadType(id);
            }
        }

        private void OnTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressListSelection || !(_typeList.SelectedItem is string id)
                || String.Equals(id, _loadedId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (_dirty && !Confirm("Nicht gespeicherte Änderungen verwerfen?"))
            {
                _suppressListSelection = true;
                _typeList.SelectedItem = _typeList.Items.Cast<string>().FirstOrDefault(
                    item => String.Equals(item, _loadedId, StringComparison.OrdinalIgnoreCase));
                _suppressListSelection = false;
                return;
            }
            LoadType(id);
        }

        internal void SelectType(string id)
        {
            _typeList.SelectedItem = _typeList.Items.Cast<string>().FirstOrDefault(
                item => String.Equals(item, id, StringComparison.OrdinalIgnoreCase));
        }

        private void LoadType(string id)
        {
            try
            {
                LoadForm(EnemyTypeRepository.LoadDefinition(id), id);
                SetStatus("„" + id + "“ geladen.");
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail("„" + id + "“ kann nicht geladen werden: " + exception.Message);
            }
        }

        // ---- actions ----

        private void StartNewType()
        {
            if (_dirty && !Confirm("Nicht gespeicherte Änderungen verwerfen?"))
            {
                return;
            }
            LoadForm(new EnemyTypeDefinition { Id = FreeId("NeuerGegner") }, null);
            _dirty = true;
            ClearListSelection();
            SetStatus("Neue Gegnerart: Id eintragen, Werte anpassen (leer = Standard), speichern.");
        }

        private void DuplicateType()
        {
            List<string> errors = new List<string>();
            EnemyTypeDefinition copy = ReadForm(errors);
            if (errors.Count > 0)
            {
                Fail(String.Join(Environment.NewLine, errors));
                return;
            }
            string baseId = EnemyType.IsValidId(copy.Id) ? copy.Id : "Gegner";
            // Room for "_Kopie" and a number within the 40 characters of an id.
            copy.Id = FreeId((baseId.Length > 32 ? baseId.Substring(0, 32) : baseId) + "_Kopie");
            if (!String.IsNullOrWhiteSpace(copy.Name))
            {
                copy.Name += " (Kopie)";
            }
            LoadForm(copy, null);
            _dirty = true;
            ClearListSelection();
            SetStatus("Kopie als „" + copy.Id + "“ – zum Behalten speichern.");
        }

        private void DeleteType()
        {
            if (_loadedId == null)
            {
                RefreshTypeList(EnemyType.StandardId);
                SetStatus("Ungespeicherte Gegnerart verworfen.");
                return;
            }
            bool isStandard = String.Equals(
                _loadedId, EnemyType.StandardId, StringComparison.OrdinalIgnoreCase);
            if (!Confirm(isStandard
                ? "Alle Anpassungen des Standardgegners zurücksetzen?"
                : "Gegnerart „" + _loadedId + "“ löschen? Laufende Gegner dieser Art bleiben, "
                    + "neue spawnen nicht mehr."))
            {
                return;
            }
            try
            {
                EnemyTypeRepository.DeleteDefinition(_loadedId);
                _field.ReloadEnemyTypes();
                string deleted = _loadedId;
                _loadedId = null;
                _dirty = false;
                RefreshTypeList(EnemyType.StandardId);
                SetStatus(isStandard ? "Standardgegner zurückgesetzt." : "„" + deleted + "“ gelöscht.");
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail(exception.Message);
            }
        }

        // Saves the form; false when it holds errors or the user cancels.
        internal bool SaveType()
        {
            List<string> errors = new List<string>();
            EnemyTypeDefinition definition = ReadForm(errors);
            if (!EnemyType.IsValidId(definition.Id))
            {
                errors.Add("Id: nur Buchstaben, Ziffern, - und _ (1–40 Zeichen).");
            }
            List<string> problems = new List<string>();
            if (errors.Count == 0)
            {
                EnemyType.Resolve(definition, problems);
                AddSpriteProblem(definition, problems);
            }
            if (errors.Count > 0 || problems.Count > 0)
            {
                Fail("Nicht gespeichert:" + Environment.NewLine
                    + String.Join(Environment.NewLine, errors.Concat(problems)));
                return false;
            }

            bool exists = EnemyTypeRepository.GetTypeIds().Any(
                id => String.Equals(id, definition.Id, StringComparison.OrdinalIgnoreCase));
            if (exists && !String.Equals(definition.Id, _loadedId,
                    StringComparison.OrdinalIgnoreCase)
                && !Confirm("„" + definition.Id + "“ existiert bereits. Überschreiben?"))
            {
                return false;
            }

            try
            {
                EnemyTypeRepository.SaveDefinition(definition);
                _field.ReloadEnemyTypes();
                _dirty = false;
                RefreshTypeList(definition.Id);
                SetStatus("Gespeichert: Assets\\Enemies\\Types\\" + definition.Id
                    + ".json – gilt ab dem nächsten Spawn.");
                return true;
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail(exception.Message);
                return false;
            }
        }

        // Spawns the form's current (also unsaved) version beside the player.
        internal void SpawnTestEnemy()
        {
            List<string> errors = new List<string>();
            EnemyTypeDefinition definition = ReadForm(errors);
            if (!EnemyType.IsValidId(definition.Id))
            {
                errors.Add("Id: nur Buchstaben, Ziffern, - und _ (1–40 Zeichen).");
            }
            if (errors.Count > 0)
            {
                Fail(String.Join(Environment.NewLine, errors));
                return;
            }
            if (!_field.HasActiveGame || _field.IsGameOver)
            {
                Fail("Test-Spawn braucht einen laufenden Run (Spiel starten, dann Dev Tool öffnen).");
                return;
            }
            if (!_field.SpawnEnemyNearPlayer(EnemyType.Resolve(definition)))
            {
                Fail("Kein Platz neben dem Spieler oder Gesamtgrenze von "
                    + ArenaField.MaximumEnemyCount + " Gegnern erreicht.");
                return;
            }
            SetStatus("„" + definition.Id + "“ steht neben dir – Dev Tool schließen und Spiel fortsetzen.");
        }

        private void RemoveAllEnemies()
        {
            if (!_field.HasActiveGame)
            {
                Fail("Kein laufender Run.");
                return;
            }
            _field.RemoveAllEnemies();
            SetStatus("Alle Gegner entfernt.");
        }

        private static void AddSpriteProblem(EnemyTypeDefinition definition, List<string> problems)
        {
            if (!String.IsNullOrWhiteSpace(definition.SpriteSheet)
                && !GetSpriteSheets().Contains(definition.SpriteSheet.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                problems.Add(definition.Id + ": Spritesheet „" + definition.SpriteSheet
                    + "“ fehlt in Assets/Enemies (PNG und JSON nötig).");
            }
        }

        // ---- preview ----

        private void UpdatePreview()
        {
            _preview.Children.Clear();
            _spriteView.Children.Clear();
            List<string> errors = new List<string>();
            EnemyTypeDefinition definition = ReadForm(errors);
            if (!EnemyType.IsValidId(definition.Id))
            {
                definition.Id = "Vorschau";
                errors.Add("Id fehlt oder enthält ungültige Zeichen.");
            }
            List<string> problems = new List<string>();
            EnemyType type = EnemyType.Resolve(definition, problems);
            AddSpriteProblem(definition, problems);

            double sight = type.SightRange + type.SightBonusAfterDetection;
            double reach = type.Attacks.Max(attack => attack.Weapon.AttackPattern.Range);
            double extent = Math.Max(Math.Max(sight, type.AlertRadius),
                Math.Max(reach, type.Size * 1.5));
            double scale = (PreviewSize / 2 - 8) / Math.Max(1, extent);
            double center = PreviewSize / 2;
            AddCircle(type.SightRange * scale, Color.FromArgb(160, 255, 214, 102), false);
            AddCircle(sight * scale, Color.FromArgb(90, 255, 214, 102), true);
            AddCircle(type.AlertRadius * scale, Color.FromArgb(160, 255, 110, 110), true);
            AddCircle(reach * scale, Color.FromArgb(180, 120, 190, 255), false);
            if (type.Behavior == EnemyBehavior.Ranged)
            {
                AddCircle(type.PreferredDistance * scale, Color.FromArgb(140, 120, 190, 255), true);
            }

            Rect hitbox = PursuerEnemy.GetHitboxOffsetForSize(type.Size, type.SpriteSheet);
            AddEnemy(_preview, center, center, scale);
            // Look view: the enemy enlarged (shrunk only when it would not fit).
            double spriteScale = Math.Min(SpriteViewScale,
                (SpriteViewHeight - 10) / Math.Max(1, type.Size * 2));
            AddEnemy(_spriteView, PreviewSize / 2, SpriteViewHeight / 2, spriteScale);
            _spriteView.Children.Add(new TextBlock
            {
                Text = "Maßstab " + Format(spriteScale) + "×",
                Foreground = TextBrush,
                FontSize = 11,
                Margin = new Thickness(4, 2, 0, 0)
            });

            List<string> lines = new List<string>
            {
                type.Name + (type.Enabled ? "" : " (inaktiv)"),
                type.MaxHitPoints + " LP · Tempo " + Format(type.SpeedFactor * 100) + " %"
                    + " · Größe " + Format(type.Size),
                "Verhalten: " + GetBehaviorName(type.Behavior)
                    + " · Belohnung " + type.ScoreValue + " Kill(s), "
                    + type.WeaponPointsPerKill + " Waffenpunkt(e)",
                "Spawn alle " + Format(type.SpawnIntervalSeconds) + " s mit "
                    + Format(type.SpawnChancePercent) + " %, max. " + type.MaxAlive
                    + ", Gruppe " + type.GroupSizeMin + "–" + type.GroupSizeMax
            };
            for (int i = 0; i < type.Attacks.Count; i++)
            {
                Weapon weapon = type.Attacks[i].Weapon;
                lines.Add("Angriff " + (i + 1) + ": " + weapon.Name + ", "
                    + EnemyAttack.GetPatternName(EnemyAttack.GetPatternOf(weapon.AttackPattern))
                    + ", Reichweite " + Format(weapon.AttackPattern.Range)
                    + ", alle " + Format(weapon.CooldownSeconds) + " s");
            }
            lines.Add("Gelb: Sicht (gestrichelt mit Bonus) · Rot: Alarmradius · "
                + "Blau: Reichweite (gestrichelt: Wunschabstand) · Grün: Hitbox");
            string summary = String.Join(Environment.NewLine, lines);
            IEnumerable<string> messages = errors.Concat(problems);
            _previewInfo.Text = messages.Any()
                ? summary + Environment.NewLine + Environment.NewLine
                    + "Probleme:" + Environment.NewLine + String.Join(Environment.NewLine, messages)
                : summary;
            _previewInfo.Foreground = messages.Any() ? ErrorBrush : TextBrush;

            void AddEnemy(Canvas target, double centerX, double centerY, double factor)
            {
                BitmapSource frame = LoadPreviewFrame(type.SpriteSheet);
                if (frame != null)
                {
                    double height = Math.Max(4, type.Size * 2 * factor);
                    double width = height * frame.PixelWidth / frame.PixelHeight;
                    Image image = new Image
                    {
                        Source = frame,
                        Width = width,
                        Height = height,
                        Opacity = type.Opacity
                    };
                    Canvas.SetLeft(image, centerX - width / 2);
                    Canvas.SetTop(image, centerY - height / 2);
                    target.Children.Add(image);
                    if (type.Tint.HasValue)
                    {
                        Rectangle tint = new Rectangle
                        {
                            Width = width,
                            Height = height,
                            Fill = new SolidColorBrush(type.Tint.Value),
                            OpacityMask = new ImageBrush(frame),
                            Opacity = 0.55 * type.Opacity
                        };
                        Canvas.SetLeft(tint, centerX - width / 2);
                        Canvas.SetTop(tint, centerY - height / 2);
                        target.Children.Add(tint);
                    }
                }
                Rectangle hitboxRectangle = new Rectangle
                {
                    Width = Math.Max(1, hitbox.Width * factor),
                    Height = Math.Max(1, hitbox.Height * factor),
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(hitboxRectangle, centerX + hitbox.X * factor);
                Canvas.SetTop(hitboxRectangle, centerY + hitbox.Y * factor);
                target.Children.Add(hitboxRectangle);
            }

            void AddCircle(double radius, Color color, bool dashed)
            {
                Ellipse circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1.5,
                    StrokeDashArray = dashed ? new DoubleCollection { 4, 3 } : null
                };
                Canvas.SetLeft(circle, center - radius);
                Canvas.SetTop(circle, center - radius);
                _preview.Children.Add(circle);
            }
        }

        private static string GetBehaviorName(EnemyBehavior behavior)
        {
            switch (behavior)
            {
                case EnemyBehavior.Ranged: return "Fernkämpfer";
                case EnemyBehavior.Turret: return "Turm";
                case EnemyBehavior.Coward: return "Feigling";
                default: return "Verfolger";
            }
        }

        private BitmapSource LoadPreviewFrame(string spriteSheet)
        {
            if (_previewFrames.TryGetValue(spriteSheet, out BitmapSource cached))
            {
                return cached;
            }
            BitmapSource frame = null;
            try
            {
                frame = new SpriteSheetAnimator(
                    PainterG9.FindAssetPath("Enemies", spriteSheet),
                    PainterG9.FindAssetPath("Enemies",
                        System.IO.Path.GetFileNameWithoutExtension(spriteSheet) + ".json"))
                    .GetFrame("IdleFront");
            }
            catch (Exception exception) when (IsFileProblem(exception)
                || exception is KeyNotFoundException || exception is InvalidOperationException)
            {
                frame = null;
            }
            _previewFrames[spriteSheet] = frame;
            return frame;
        }

        // ---- helpers ----

        private string FreeId(string baseId)
        {
            IReadOnlyList<string> ids = EnemyTypeRepository.GetTypeIds();
            string id = baseId;
            for (int number = 2; ids.Any(existing =>
                String.Equals(existing, id, StringComparison.OrdinalIgnoreCase)); number++)
            {
                id = baseId + number;
            }
            return id;
        }

        private void ClearListSelection()
        {
            _suppressListSelection = true;
            _typeList.SelectedItem = null;
            _suppressListSelection = false;
        }

        private void SetStatus(string message)
        {
            _status.Foreground = TextBrush;
            _status.Text = message;
        }

        private void Fail(string message)
        {
            _status.Foreground = ErrorBrush;
            _status.Text = message.Replace(Environment.NewLine, " ");
            ShowError(message);
        }

        private static string Format(double value)
        {
            return value.ToString("0.###", GermanCulture);
        }

        private static bool IsFileProblem(Exception exception)
        {
            return exception is IOException || exception is UnauthorizedAccessException
                || exception is JsonException || exception is InvalidDataException
                || exception is ArgumentException || exception is NotSupportedException;
        }

        private static Canvas CreatePreviewCanvas(double height)
        {
            Canvas canvas = new Canvas
            {
                Width = PreviewSize,
                Height = height,
                Background = new SolidColorBrush(Color.FromRgb(13, 19, 27)),
                ClipToBounds = true
            };
            // Enlarged pixel-art sprites stay sharp.
            RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);
            return canvas;
        }

        private static TextBlock CreateHeading(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 8, 0, 4)
            };
        }

        private static Button CreateButton(string label)
        {
            return new Button
            {
                Content = label,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static void AddButton(Panel panel, string label, Action action)
        {
            Button button = CreateButton(label);
            button.Click += (sender, args) => action();
            panel.Children.Add(button);
        }

        // Tests set form values by label.
        internal void SetFieldValue(string label, string value)
        {
            FieldEditor<EnemyTypeDefinition> editor =
                _editors.FirstOrDefault(item => item.Spec.Label == label);
            if (editor == null)
            {
                throw new ArgumentException("Unbekanntes Feld: " + label, nameof(label));
            }
            editor.Value = value;
        }

        internal string GetFieldValue(string label)
        {
            return _editors.First(item => item.Spec.Label == label).Value;
        }

        // ---- attack editor ----

        private sealed class AttackEditor
        {
            public List<FieldEditor<EnemyAttackDefinition>> Editors { get; } =
                new List<FieldEditor<EnemyAttackDefinition>>();
            public Border Panel { get; set; }
            public TextBlock Title { get; set; }

            // Shows only the rows that matter for the chosen (or the weapon's) pattern.
            public void UpdateVisibleFields()
            {
                string pattern = Editors.First(editor => editor.Spec.Label == "Muster").Value;
                if (String.IsNullOrEmpty(pattern))
                {
                    Weapon weapon = EnemyAttack.FindBaseWeapon(
                        Editors.First(editor => editor.Spec.Label == "Waffe").Value)
                        ?? WeaponCatalog.EnemyClaws;
                    pattern = EnemyAttack.GetPatternOf(weapon.AttackPattern);
                }
                foreach (FieldEditor<EnemyAttackDefinition> editor in Editors)
                {
                    editor.SetVisible(editor.Spec.Patterns == null
                        || editor.Spec.Patterns.Contains(pattern));
                }
            }
        }

        // ---- field descriptions ----

        private enum FieldKind
        {
            Text,
            Integer,
            Number,
            YesNo,
            Choice,
            Flags
        }

        // One form row: how it reads and writes its value in the target (a kind or an
        // attack) and which standard value applies when it stays empty.
        private sealed class FieldSpec<T>
        {
            public string Group { get; set; }
            public string Label { get; set; }
            public string StandardText { get; set; }
            public string Tooltip { get; set; }
            public FieldKind Kind { get; set; }
            // Attack rows: the patterns the row applies to; null = all.
            public string[] Patterns { get; set; }
            public bool StandardBool { get; set; }
            // Choice and flag rows: stored value and shown text.
            public IReadOnlyList<KeyValuePair<string, string>> Options { get; set; }
            public Func<T, string> GetText { get; set; }
            public Action<T, string> SetText { get; set; }
            public Func<T, int?> GetInt { get; set; }
            public Action<T, int?> SetInt { get; set; }
            public Func<T, double?> GetNumber { get; set; }
            public Action<T, double?> SetNumber { get; set; }
            public Func<T, bool?> GetBool { get; set; }
            public Action<T, bool?> SetBool { get; set; }
            public Func<T, List<string>> GetList { get; set; }
            public Action<T, List<string>> SetList { get; set; }
        }

        private static class Fields<T>
        {
            public static FieldSpec<T> Text(string group, string label, string standardText,
                Func<T, string> get, Action<T, string> set, string tooltip = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, StandardText = standardText,
                    Tooltip = tooltip, Kind = FieldKind.Text, GetText = get, SetText = set
                };
            }

            public static FieldSpec<T> Int(string group, string label, int? standard,
                Func<T, int?> get, Action<T, int?> set,
                string tooltip = null, string standardText = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, Tooltip = tooltip, Kind = FieldKind.Integer,
                    StandardText = standardText != null && standardText.StartsWith("Standard")
                        ? standardText
                        : "Standard: " + (standardText ?? standard.ToString()),
                    GetInt = get, SetInt = set
                };
            }

            public static FieldSpec<T> Number(string group, string label, double? standard,
                Func<T, double?> get, Action<T, double?> set,
                string tooltip = null, string standardText = null, string[] patterns = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, Tooltip = tooltip, Kind = FieldKind.Number,
                    StandardText = standardText != null && standardText.StartsWith("Standard")
                        ? standardText
                        : "Standard: " + (standardText
                            ?? (standard.HasValue ? Format(standard.Value) : "")),
                    GetNumber = get, SetNumber = set, Patterns = patterns
                };
            }

            public static FieldSpec<T> YesNo(string group, string label, bool standard,
                Func<T, bool?> get, Action<T, bool?> set, string tooltip = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, Tooltip = tooltip, Kind = FieldKind.YesNo,
                    StandardText = "Standard: " + (standard ? "ja" : "nein"),
                    StandardBool = standard, GetBool = get, SetBool = set
                };
            }

            public static FieldSpec<T> Choice(string group, string label, string standardText,
                IEnumerable<KeyValuePair<string, string>> options,
                Func<T, string> get, Action<T, string> set, string tooltip = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, StandardText = standardText, Tooltip = tooltip,
                    Kind = FieldKind.Choice, Options = options.ToArray(),
                    GetText = get, SetText = set
                };
            }

            public static FieldSpec<T> Flags(string group, string label, string standardText,
                IEnumerable<KeyValuePair<string, string>> options,
                Func<T, List<string>> get, Action<T, List<string>> set, string tooltip = null)
            {
                return new FieldSpec<T>
                {
                    Group = group, Label = label, StandardText = standardText, Tooltip = tooltip,
                    Kind = FieldKind.Flags, Options = options.ToArray(),
                    GetList = get, SetList = set
                };
            }
        }

        // The input control of one form row.
        private sealed class FieldEditor<T>
        {
            private readonly TextBox _textBox;
            private readonly ComboBox _choice;
            private readonly List<CheckBox> _flags = new List<CheckBox>();
            private WrapPanel _flagPanel;
            // Stored keys that are not offered (e.g. a region of a deleted map) stay.
            private List<string> _unknownFlags = new List<string>();
            private string _unknownChoice;
            private bool _settingValue;

            public FieldEditor(FieldSpec<T> spec)
            {
                Spec = spec;
                switch (spec.Kind)
                {
                    case FieldKind.YesNo:
                        _choice = CreateChoice(new[]
                        {
                            "Standard (" + (spec.StandardBool ? "ja" : "nein") + ")", "ja", "nein"
                        });
                        Control = _choice;
                        break;
                    case FieldKind.Choice:
                        _choice = CreateChoice(new[] { "Standard" }
                            .Concat(spec.Options.Select(option => option.Value)).ToArray());
                        Control = _choice;
                        break;
                    case FieldKind.Flags:
                        _flagPanel = new WrapPanel { Margin = new Thickness(0, 2, 0, 2) };
                        AddFlagBoxes(spec.Options);
                        Control = _flagPanel;
                        break;
                    default:
                        _textBox = spec.Kind == FieldKind.Text
                            ? new TextBox()
                            : new ArenaIntegerTextBox
                            {
                                AllowDecimalSeparator = spec.Kind == FieldKind.Number
                            };
                        _textBox.Margin = new Thickness(0, 2, 0, 2);
                        _textBox.VerticalContentAlignment = VerticalAlignment.Center;
                        _textBox.ToolTip = spec.Tooltip;
                        _textBox.TextChanged += (sender, args) => Changed?.Invoke();
                        Control = _textBox;
                        break;
                }
            }

            public event Action Changed;
            public FieldSpec<T> Spec { get; }
            public FrameworkElement Control { get; }
            public TextBlock Label { get; set; }
            public TextBlock Standard { get; set; }

            private ComboBox CreateChoice(string[] items)
            {
                ComboBox box = new ComboBox
                {
                    ItemsSource = items,
                    SelectedIndex = 0,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                box.SelectionChanged += (sender, args) =>
                {
                    if (!_settingValue)
                    {
                        _unknownChoice = null;
                    }
                    Changed?.Invoke();
                };
                return box;
            }

            private void AddFlagBoxes(IEnumerable<KeyValuePair<string, string>> options)
            {
                foreach (KeyValuePair<string, string> option in options)
                {
                    CheckBox box = new CheckBox
                    {
                        Content = option.Value,
                        Tag = option.Key,
                        Foreground = Brushes.White,
                        Margin = new Thickness(0, 2, 10, 2)
                    };
                    box.Checked += (sender, args) => Changed?.Invoke();
                    box.Unchecked += (sender, args) => Changed?.Invoke();
                    _flags.Add(box);
                    _flagPanel.Children.Add(box);
                }
            }

            // Replaces the offered keys and keeps what was checked.
            public void SetFlagOptions(IReadOnlyList<KeyValuePair<string, string>> options)
            {
                string value = Value;
                _flags.Clear();
                _flagPanel.Children.Clear();
                Spec.Options = options;
                AddFlagBoxes(options);
                Value = value;
            }

            public void SetVisible(bool visible)
            {
                Visibility visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                Control.Visibility = visibility;
                if (Label != null)
                {
                    Label.Visibility = visibility;
                }
                if (Standard != null)
                {
                    Standard.Visibility = visibility;
                }
            }

            // The raw value of the row: text, "ja"/"nein", the option key or the checked
            // keys separated by commas; "" = standard.
            public string Value
            {
                get
                {
                    switch (Spec.Kind)
                    {
                        case FieldKind.YesNo:
                            return _choice.SelectedIndex <= 0 ? "" : (string)_choice.SelectedItem;
                        case FieldKind.Choice:
                            if (_choice.SelectedIndex <= 0)
                            {
                                return _unknownChoice ?? "";
                            }
                            return Spec.Options[_choice.SelectedIndex - 1].Key;
                        case FieldKind.Flags:
                            return String.Join(",", _flags.Where(box => box.IsChecked == true)
                                .Select(box => (string)box.Tag).Concat(_unknownFlags));
                        default:
                            return _textBox.Text;
                    }
                }
                set
                {
                    string text = value ?? "";
                    switch (Spec.Kind)
                    {
                        case FieldKind.YesNo:
                            _choice.SelectedIndex = text == "ja" ? 1 : text == "nein" ? 2 : 0;
                            break;
                        case FieldKind.Choice:
                            int index = Spec.Options.ToList().FindIndex(option =>
                                String.Equals(option.Key, text, StringComparison.OrdinalIgnoreCase));
                            // An unknown stored value (e.g. a missing sprite) is kept so that
                            // saving does not silently drop it; the preview reports it.
                            _settingValue = true;
                            try
                            {
                                _unknownChoice = index < 0 && text.Length > 0 ? text : null;
                                _choice.SelectedIndex = index + 1;
                            }
                            finally
                            {
                                _settingValue = false;
                            }
                            break;
                        case FieldKind.Flags:
                            HashSet<string> keys = new HashSet<string>(
                                text.Split(',').Select(key => key.Trim())
                                    .Where(key => key.Length > 0),
                                StringComparer.OrdinalIgnoreCase);
                            foreach (CheckBox box in _flags)
                            {
                                box.IsChecked = keys.Remove((string)box.Tag);
                            }
                            _unknownFlags = keys.ToList();
                            break;
                        default:
                            _textBox.Text = text;
                            break;
                    }
                }
            }

            public void Load(T target)
            {
                switch (Spec.Kind)
                {
                    case FieldKind.Text:
                    case FieldKind.Choice:
                        Value = Spec.GetText(target);
                        break;
                    case FieldKind.Integer:
                        int? integer = Spec.GetInt(target);
                        Value = integer.HasValue
                            ? integer.Value.ToString(CultureInfo.InvariantCulture)
                            : "";
                        break;
                    case FieldKind.Number:
                        double? number = Spec.GetNumber(target);
                        Value = number.HasValue ? Format(number.Value) : "";
                        break;
                    case FieldKind.Flags:
                        List<string> list = Spec.GetList(target);
                        Value = list == null ? "" : String.Join(",", list);
                        break;
                    default:
                        bool? flag = Spec.GetBool(target);
                        Value = flag.HasValue ? (flag.Value ? "ja" : "nein") : "";
                        break;
                }
            }

            // Writes the row into the target; returns an error text or null.
            public string Store(T target)
            {
                string text = (Value ?? "").Trim();
                switch (Spec.Kind)
                {
                    case FieldKind.Text:
                    case FieldKind.Choice:
                        Spec.SetText(target, text.Length == 0 ? null : text);
                        return null;
                    case FieldKind.Flags:
                        Spec.SetList(target, text.Length == 0
                            ? null
                            : text.Split(',').ToList());
                        return null;
                    case FieldKind.Integer:
                        if (text.Length == 0)
                        {
                            Spec.SetInt(target, null);
                            return null;
                        }
                        if (Int32.TryParse(text, NumberStyles.None,
                            CultureInfo.InvariantCulture, out int integer))
                        {
                            Spec.SetInt(target, integer);
                            return null;
                        }
                        return Spec.Label + ": „" + text + "“ ist keine ganze Zahl.";
                    case FieldKind.Number:
                        if (text.Length == 0)
                        {
                            Spec.SetNumber(target, null);
                            return null;
                        }
                        if (Double.TryParse(text.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture, out double number))
                        {
                            Spec.SetNumber(target, number);
                            return null;
                        }
                        return Spec.Label + ": „" + text + "“ ist keine Zahl.";
                    default:
                        Spec.SetBool(target, text == "ja" ? true : text == "nein" ? (bool?)false : null);
                        return null;
                }
            }
        }
    }
}
