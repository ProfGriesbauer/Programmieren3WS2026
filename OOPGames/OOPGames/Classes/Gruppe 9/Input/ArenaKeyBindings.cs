using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace OOPGames
{
    public enum ArenaAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        WeaponAttack,
        WeaponAbility1,
        WeaponAbility2,
        WeaponAbility3,
        WeaponAbility4,
        EntityAbility1,
        EntityAbility2,
        EntityAbility3,
        EntityAbility4,
        Pause,
        ToggleDebugRanges,
        ToggleDebugHud,
        ZoomIn,
        ZoomOut,
        SpawnEnemy
    }

    public enum ArenaInputDevice
    {
        Keyboard,
        Mouse
    }

    public sealed class ArenaInputBinding
    {
        public ArenaInputDevice Device { get; set; }
        public Key Key { get; set; }
        public MouseButton MouseButton { get; set; }

        public string DisplayName
        {
            get
            {
                if (Device == ArenaInputDevice.Mouse)
                {
                    switch (MouseButton)
                    {
                        case MouseButton.Left: return "Linke Maustaste";
                        case MouseButton.Right: return "Rechte Maustaste";
                        case MouseButton.Middle: return "Mittlere Maustaste";
                        case MouseButton.XButton1: return "Maustaste 4";
                        case MouseButton.XButton2: return "Maustaste 5";
                        default: return "Ungültige Maustaste";
                    }
                }

                return Key.ToString();
            }
        }

        public bool MatchesKey(Key key)
        {
            return Device == ArenaInputDevice.Keyboard && Key == key;
        }

        public bool MatchesMouseButton(MouseButton button)
        {
            return Device == ArenaInputDevice.Mouse && MouseButton == button;
        }

        public static ArenaInputBinding ForKey(Key key)
        {
            return new ArenaInputBinding
            {
                Device = ArenaInputDevice.Keyboard,
                Key = key
            };
        }

        public static ArenaInputBinding ForMouseButton(MouseButton button)
        {
            return new ArenaInputBinding
            {
                Device = ArenaInputDevice.Mouse,
                MouseButton = button
            };
        }

        public ArenaInputBinding Copy()
        {
            return new ArenaInputBinding
            {
                Device = Device,
                Key = Key,
                MouseButton = MouseButton
            };
        }
    }

    public sealed class ArenaKeyBindings
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

        private readonly Dictionary<ArenaAction, ArenaInputBinding> _bindings;

        private ArenaKeyBindings(
            Dictionary<ArenaAction, ArenaInputBinding> bindings)
        {
            _bindings = bindings;
        }

        public static string DefaultFilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OOPGames", "ArenaSurvival", "keybindings.json");
            }
        }

        public static IReadOnlyList<ArenaAction> Actions
        {
            get { return Array.AsReadOnly((ArenaAction[])Enum.GetValues(typeof(ArenaAction))); }
        }

        public static ArenaKeyBindings LoadDefault()
        {
            return Load(DefaultFilePath);
        }

        public static ArenaKeyBindings Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A settings path is required.", nameof(filePath));
            }

            Dictionary<ArenaAction, ArenaInputBinding> bindings = CreateDefaults();
            if (!File.Exists(filePath))
            {
                return new ArenaKeyBindings(bindings);
            }

            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath)))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("The saved input bindings are invalid.");
                }

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    if (!TryParseAction(property.Name, out ArenaAction action))
                    {
                        throw new InvalidDataException(
                            "The saved input bindings contain an unknown action.");
                    }

                    ArenaInputBinding binding = ParseBinding(property.Value, action);
                    ValidateBinding(binding);
                    if (binding.Device == ArenaInputDevice.Mouse
                        && !IsMouseBindable(action))
                    {
                        throw new InvalidDataException(
                            "A mouse button is assigned to an unsupported action.");
                    }
                    bindings[action] = binding;
                }
            }

            if (bindings[ArenaAction.WeaponAttack]
                .MatchesMouseButton(MouseButton.Right))
            {
                bindings[ArenaAction.WeaponAttack] =
                    ArenaInputBinding.ForMouseButton(MouseButton.Left);
                if (bindings[ArenaAction.WeaponAbility1]
                    .MatchesMouseButton(MouseButton.Left))
                {
                    bindings[ArenaAction.WeaponAbility1] =
                        ArenaInputBinding.ForMouseButton(MouseButton.Right);
                }
            }

            ValidateUniqueBindings(bindings);
            return new ArenaKeyBindings(bindings);
        }

        public ArenaInputBinding GetBinding(ArenaAction action)
        {
            return _bindings[action].Copy();
        }

        public string GetDisplayName(ArenaAction action)
        {
            return _bindings[action].DisplayName;
        }

        public Key GetKey(ArenaAction action)
        {
            ArenaInputBinding binding = _bindings[action];
            return binding.Device == ArenaInputDevice.Keyboard
                ? binding.Key
                : Key.None;
        }

        public bool IsDown(ArenaAction action)
        {
            ArenaInputBinding binding = _bindings[action];
            if (binding.Device == ArenaInputDevice.Keyboard)
            {
                return binding.Key != Key.None && Keyboard.IsKeyDown(binding.Key);
            }

            switch (binding.MouseButton)
            {
                case MouseButton.Left: return Mouse.LeftButton == MouseButtonState.Pressed;
                case MouseButton.Right: return Mouse.RightButton == MouseButtonState.Pressed;
                case MouseButton.Middle: return Mouse.MiddleButton == MouseButtonState.Pressed;
                case MouseButton.XButton1: return Mouse.XButton1 == MouseButtonState.Pressed;
                case MouseButton.XButton2: return Mouse.XButton2 == MouseButtonState.Pressed;
                default: return false;
            }
        }

        public bool MatchesKey(ArenaAction action, Key key)
        {
            return _bindings[action].MatchesKey(key);
        }

        public bool MatchesMouseButton(ArenaAction action, MouseButton button)
        {
            return _bindings[action].MatchesMouseButton(button);
        }

        public void SetKey(ArenaAction action, Key key)
        {
            SetBinding(action, ArenaInputBinding.ForKey(key));
        }

        public void SetMouseButton(ArenaAction action, MouseButton button)
        {
            SetBinding(action, ArenaInputBinding.ForMouseButton(button));
        }

        public void SetBinding(ArenaAction action, ArenaInputBinding binding)
        {
            if (!Enum.IsDefined(typeof(ArenaAction), action))
            {
                throw new ArgumentOutOfRangeException(nameof(action));
            }
            ValidateBinding(binding);
            if (binding.Device == ArenaInputDevice.Mouse && !IsMouseBindable(action))
            {
                throw new InvalidOperationException(
                    "Maustasten können für Bewegung, Angriffe und Fähigkeiten verwendet werden.");
            }

            foreach (KeyValuePair<ArenaAction, ArenaInputBinding> current in _bindings)
            {
                if (current.Key != action && IsSameInput(current.Value, binding))
                {
                    throw new InvalidOperationException(
                        binding.DisplayName + " ist bereits "
                        + GetActionName(current.Key) + " zugewiesen.");
                }
            }

            _bindings[action] = binding.Copy();
        }

        public void Save()
        {
            Save(DefaultFilePath);
        }

        public void ResetToDefaults()
        {
            _bindings.Clear();
            foreach (KeyValuePair<ArenaAction, ArenaInputBinding> binding in CreateDefaults())
            {
                _bindings.Add(binding.Key, binding.Value);
            }
        }

        public void Save(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A settings path is required.", nameof(filePath));
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(filePath, JsonSerializer.Serialize(_bindings, JsonOptions));
        }

        public static string GetActionName(ArenaAction action)
        {
            switch (action)
            {
                case ArenaAction.MoveUp: return "Nach oben bewegen";
                case ArenaAction.MoveDown: return "Nach unten bewegen";
                case ArenaAction.MoveLeft: return "Nach links bewegen";
                case ArenaAction.MoveRight: return "Nach rechts bewegen";
                case ArenaAction.WeaponAttack: return "Waffen-Grundangriff";
                case ArenaAction.WeaponAbility1: return "Waffenfähigkeit 1";
                case ArenaAction.WeaponAbility2: return "Waffenfähigkeit 2";
                case ArenaAction.WeaponAbility3: return "Waffenfähigkeit 3";
                case ArenaAction.WeaponAbility4: return "Waffenfähigkeit 4";
                case ArenaAction.EntityAbility1: return "Spielerfähigkeit 1";
                case ArenaAction.EntityAbility2: return "Spielerfähigkeit 2";
                case ArenaAction.EntityAbility3: return "Spielerfähigkeit 3";
                case ArenaAction.EntityAbility4: return "Spielerfähigkeit 4";
                case ArenaAction.Pause: return "Pause / Menü";
                case ArenaAction.ToggleDebugRanges: return "Debugbereiche";
                case ArenaAction.ToggleDebugHud: return "Debug-HUD";
                case ArenaAction.ZoomIn: return "Zoom hinein";
                case ArenaAction.ZoomOut: return "Zoom heraus";
                case ArenaAction.SpawnEnemy: return "Testgegner spawnen";
                default: return action.ToString();
            }
        }

        private static ArenaInputBinding ParseBinding(
            JsonElement value, ArenaAction action)
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                if (!Enum.TryParse(value.GetString(), true, out Key legacyKey)
                    || legacyKey == Key.None)
                {
                    throw new InvalidDataException(
                        "A saved key binding contains an invalid key.");
                }

                if (action == ArenaAction.WeaponAttack
                    && legacyKey == Key.Space)
                {
                    return ArenaInputBinding.ForMouseButton(MouseButton.Left);
                }
                return ArenaInputBinding.ForKey(legacyKey);
            }

            ArenaInputBinding binding =
                JsonSerializer.Deserialize<ArenaInputBinding>(
                    value.GetRawText(), JsonOptions);
            if (binding == null)
            {
                throw new InvalidDataException("A saved input binding is empty.");
            }
            return binding;
        }

        private static bool TryParseAction(string value, out ArenaAction action)
        {
            if (Enum.TryParse(value, true, out action)
                && Enum.IsDefined(typeof(ArenaAction), action))
            {
                return true;
            }

            switch (value)
            {
                case "PrimaryAttack":
                    action = ArenaAction.WeaponAttack;
                    return true;
                case "Ability1":
                    action = ArenaAction.EntityAbility1;
                    return true;
                case "Ability2":
                    action = ArenaAction.EntityAbility2;
                    return true;
                case "Ability3":
                    action = ArenaAction.EntityAbility3;
                    return true;
                case "Ability4":
                    action = ArenaAction.EntityAbility4;
                    return true;
                default:
                    action = default(ArenaAction);
                    return false;
            }
        }

        private static Dictionary<ArenaAction, ArenaInputBinding> CreateDefaults()
        {
            return new Dictionary<ArenaAction, ArenaInputBinding>
            {
                { ArenaAction.MoveUp, ArenaInputBinding.ForKey(Key.W) },
                { ArenaAction.MoveDown, ArenaInputBinding.ForKey(Key.S) },
                { ArenaAction.MoveLeft, ArenaInputBinding.ForKey(Key.A) },
                { ArenaAction.MoveRight, ArenaInputBinding.ForKey(Key.D) },
                { ArenaAction.WeaponAttack, ArenaInputBinding.ForMouseButton(MouseButton.Left) },
                { ArenaAction.WeaponAbility1, ArenaInputBinding.ForMouseButton(MouseButton.Right) },
                { ArenaAction.WeaponAbility2, ArenaInputBinding.ForKey(Key.D1) },
                { ArenaAction.WeaponAbility3, ArenaInputBinding.ForKey(Key.D2) },
                { ArenaAction.WeaponAbility4, ArenaInputBinding.ForKey(Key.D3) },
                { ArenaAction.EntityAbility1, ArenaInputBinding.ForKey(Key.Q) },
                { ArenaAction.EntityAbility2, ArenaInputBinding.ForKey(Key.E) },
                { ArenaAction.EntityAbility3, ArenaInputBinding.ForKey(Key.R) },
                { ArenaAction.EntityAbility4, ArenaInputBinding.ForKey(Key.F) },
                { ArenaAction.Pause, ArenaInputBinding.ForKey(Key.Escape) },
                { ArenaAction.ToggleDebugRanges, ArenaInputBinding.ForKey(Key.B) },
                { ArenaAction.ToggleDebugHud, ArenaInputBinding.ForKey(Key.H) },
                { ArenaAction.ZoomIn, ArenaInputBinding.ForKey(Key.K) },
                { ArenaAction.ZoomOut, ArenaInputBinding.ForKey(Key.L) },
                { ArenaAction.SpawnEnemy, ArenaInputBinding.ForKey(Key.N) }
            };
        }

        private static void ValidateBinding(ArenaInputBinding binding)
        {
            if (binding == null
                || !Enum.IsDefined(typeof(ArenaInputDevice), binding.Device))
            {
                throw new ArgumentException("An input binding is invalid.", nameof(binding));
            }

            if (binding.Device == ArenaInputDevice.Keyboard)
            {
                if (!Enum.IsDefined(typeof(Key), binding.Key) || binding.Key == Key.None)
                {
                    throw new ArgumentOutOfRangeException(nameof(binding));
                }
                return;
            }

            if (binding.MouseButton != MouseButton.Left
                && binding.MouseButton != MouseButton.Right
                && binding.MouseButton != MouseButton.Middle
                && binding.MouseButton != MouseButton.XButton1
                && binding.MouseButton != MouseButton.XButton2)
            {
                throw new ArgumentOutOfRangeException(nameof(binding));
            }
        }

        private static bool IsSameInput(
            ArenaInputBinding first, ArenaInputBinding second)
        {
            return first.Device == second.Device
                && (first.Device == ArenaInputDevice.Keyboard
                    ? first.Key == second.Key
                    : first.MouseButton == second.MouseButton);
        }

        private static bool IsMouseBindable(ArenaAction action)
        {
            return action == ArenaAction.MoveUp
                || action == ArenaAction.MoveDown
                || action == ArenaAction.MoveLeft
                || action == ArenaAction.MoveRight
                || action == ArenaAction.WeaponAttack
                || action == ArenaAction.WeaponAbility1
                || action == ArenaAction.WeaponAbility2
                || action == ArenaAction.WeaponAbility3
                || action == ArenaAction.WeaponAbility4
                || action == ArenaAction.EntityAbility1
                || action == ArenaAction.EntityAbility2
                || action == ArenaAction.EntityAbility3
                || action == ArenaAction.EntityAbility4;
        }

        private static void ValidateUniqueBindings(
            Dictionary<ArenaAction, ArenaInputBinding> bindings)
        {
            HashSet<string> assignedInputs = new HashSet<string>();
            foreach (ArenaInputBinding binding in bindings.Values)
            {
                string identity = binding.Device == ArenaInputDevice.Keyboard
                    ? "K:" + (int)binding.Key
                    : "M:" + (int)binding.MouseButton;
                if (!assignedInputs.Add(identity))
                {
                    throw new InvalidDataException(
                        "The saved bindings assign one input to multiple actions.");
                }
            }
        }
    }
}
