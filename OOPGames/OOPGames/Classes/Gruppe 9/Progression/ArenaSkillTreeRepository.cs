using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OOPGames
{
    internal sealed class ArenaSkillTreeDocument
    {
        public string Name { get; set; }
        public List<ArenaSkillTreeNode> Nodes { get; set; } =
            new List<ArenaSkillTreeNode>();
        public List<ArenaSkillTreeConnection> Connections { get; set; } =
            new List<ArenaSkillTreeConnection>();
    }

    internal sealed class ArenaSkillTreeNode
    {
        public string SkillId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsFork { get; set; }
        public bool ForceDecision { get; set; }
    }

    internal sealed class ArenaSkillTreeConnection
    {
        public string FromSkillId { get; set; }
        public string ToSkillId { get; set; }
    }

    internal sealed class ArenaSkillInfo
    {
        public ArenaSkillInfo(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
    }

    internal static class ArenaSkillTreeRepository
    {
        public const string DefaultTreeName = "Standard";
        public const string StartNodeId = "__tree_start__";
        public const string ForkPaletteId = "__fork_palette__";
        private const string ForkNodePrefix = "__tree_fork_";
        private const string SelectionFileName = "SelectedSkillTree.txt";
        private const double MaximumNodePosition = 4096;
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { WriteIndented = true };
        public static ArenaSkillInfo StartNodeInfo { get; } =
            new ArenaSkillInfo(
                StartNodeId,
                "Start",
                "Baumstart. Direkt verbundene Skills können ohne vorherigen Kauf freigeschaltet werden.");

        public static string TreeDirectory
        {
            get
            {
                string overlayDirectory = PainterG9.FindAssetDirectory("Overlay");
                string assetsDirectory = Directory.GetParent(overlayDirectory).FullName;
                string treeDirectory = Path.Combine(assetsDirectory, "SkillTrees");
                Directory.CreateDirectory(treeDirectory);
                return treeDirectory;
            }
        }

        public static IReadOnlyList<ArenaSkillInfo> GetImplementedSkills()
        {
            List<ArenaSkillInfo> skills = new List<ArenaSkillInfo>
            {
                new ArenaSkillInfo(ArenaProgression.MaximumHealth,
                    "Maximale Gesundheit",
                    "Erhöht die maximale Gesundheit um "
                        + ArenaProgression.HealthIncreasePerRank + "."),
                new ArenaSkillInfo(ArenaProgression.MaximumSpeed,
                    "Maximale Geschwindigkeit",
                    "Erhöht die Bewegungsgeschwindigkeit um "
                        + ArenaProgression.SpeedIncreasePerRank + ".")
            };
            foreach (PlayerAbilityDefinition definition in PlayerAbilityCatalog.Definitions)
            {
                skills.Add(new ArenaSkillInfo(
                    definition.Id, definition.Name, definition.Description));
            }
            return skills;
        }

        public static bool IsStartNode(string skillId)
        {
            return String.Equals(
                skillId, StartNodeId, StringComparison.Ordinal);
        }

        public static bool IsForkNode(ArenaSkillTreeNode node)
        {
            return node != null && node.IsFork;
        }

        public static bool IsForkNode(
            ArenaSkillTreeDocument tree, string skillId)
        {
            return tree.Nodes.Any(node => node.IsFork
                && String.Equals(node.SkillId, skillId, StringComparison.Ordinal));
        }

        public static string CreateForkNodeId()
        {
            return ForkNodePrefix + Guid.NewGuid().ToString("N");
        }

        public static ArenaSkillInfo ForkPaletteInfo { get; } =
            new ArenaSkillInfo(
                ForkPaletteId,
                "Abzweigung",
                "Verzweigt den Freischaltpfad. Optional erzwingt sie die Wahl eines einzigen Ausgangs.");

        public static ArenaSkillInfo GetForkNodeInfo(bool forceDecision)
        {
            return new ArenaSkillInfo(
                "fork",
                "Abzweigung",
                forceDecision
                    ? "Erzwingt eine Entscheidung: Nach dem Freischalten eines Ausgangs werden die anderen gesperrt."
                    : "Verzweigt den Freischaltpfad. Mehrere Ausgänge können freigeschaltet werden.");
        }

        public static ArenaSkillTreeDocument CreateDefaultTree()
        {
            ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
            {
                Name = DefaultTreeName,
                Nodes = new List<ArenaSkillTreeNode>
                {
                    new ArenaSkillTreeNode
                    {
                        SkillId = StartNodeId, X = 80, Y = 420
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = ArenaProgression.MaximumHealth, X = 430, Y = 80
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = ArenaProgression.MaximumSpeed, X = 430, Y = 250
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = PlayerAbilityCatalog.PowerBlast, X = 430, Y = 420
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = PlayerAbilityCatalog.HealingPulse, X = 430, Y = 590
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = PlayerAbilityCatalog.SlowMotion, X = 430, Y = 760
                    }
                },
                Connections = new List<ArenaSkillTreeConnection>
                {
                    CreateStartConnection(ArenaProgression.MaximumHealth),
                    CreateStartConnection(ArenaProgression.MaximumSpeed),
                    CreateStartConnection(PlayerAbilityCatalog.PowerBlast),
                    CreateStartConnection(PlayerAbilityCatalog.HealingPulse),
                    CreateStartConnection(PlayerAbilityCatalog.SlowMotion)
                }
            };
            return tree;
        }

        public static ArenaSkillTreeConnection CreateStartConnection(string skillId)
        {
            return new ArenaSkillTreeConnection
            {
                FromSkillId = StartNodeId,
                ToSkillId = skillId
            };
        }

        public static ArenaSkillTreeDocument LoadSelectedTree()
        {
            string selectionPath = Path.Combine(TreeDirectory, SelectionFileName);
            if (!File.Exists(selectionPath))
            {
                return CreateDefaultTree();
            }

            string fileName = ValidateTreeFileName(
                File.ReadAllText(selectionPath).Trim());
            string treePath = Path.Combine(TreeDirectory, fileName);
            if (!File.Exists(treePath))
            {
                throw new FileNotFoundException(
                    "The selected skill tree does not exist.", treePath);
            }
            return LoadTree(treePath);
        }

        public static IReadOnlyList<string> GetTreeNames()
        {
            List<string> names = new List<string> { DefaultTreeName };
            names.AddRange(Directory.GetFiles(TreeDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
            return names.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        public static ArenaSkillTreeDocument LoadTreeByName(string name)
        {
            if (String.Equals(name, DefaultTreeName, StringComparison.OrdinalIgnoreCase))
            {
                return CreateDefaultTree();
            }

            string fileName = ValidateTreeFileName(name + ".json");
            string path = Path.Combine(TreeDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The skill tree does not exist.", path);
            }
            return LoadTree(path);
        }

        public static string GetSelectedTreeName()
        {
            string selectionPath = Path.Combine(TreeDirectory, SelectionFileName);
            if (!File.Exists(selectionPath))
            {
                return DefaultTreeName;
            }
            string fileName = ValidateTreeFileName(File.ReadAllText(selectionPath).Trim());
            return Path.GetFileNameWithoutExtension(fileName);
        }

        public static void SelectTree(string name)
        {
            if (String.Equals(name, DefaultTreeName, StringComparison.OrdinalIgnoreCase))
            {
                string selectionPath = Path.Combine(TreeDirectory, SelectionFileName);
                if (File.Exists(selectionPath))
                {
                    File.Delete(selectionPath);
                }
                return;
            }

            string fileName = ValidateTreeFileName(name + ".json");
            string path = Path.Combine(TreeDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Save the skill tree before selecting it.", path);
            }
            File.WriteAllText(
                Path.Combine(TreeDirectory, SelectionFileName), fileName);
        }

        public static void SaveTree(string name, ArenaSkillTreeDocument tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }
            string fileName = ValidateTreeFileName(name + ".json");
            tree.Name = Path.GetFileNameWithoutExtension(fileName);
            ValidateTree(tree);

            string path = Path.Combine(TreeDirectory, fileName);
            string temporaryPath = path + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(tree, JsonOptions));
                File.Move(temporaryPath, path, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public static void ValidateTree(ArenaSkillTreeDocument tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }
            if (tree.Nodes == null || tree.Connections == null)
            {
                throw new InvalidDataException(
                    "A skill tree must contain node and connection lists.");
            }

            HashSet<string> validSkillIds = new HashSet<string>(
                GetImplementedSkills().Select(skill => skill.Id),
                StringComparer.Ordinal);
            validSkillIds.Add(StartNodeId);
            Dictionary<string, ArenaSkillTreeNode> nodes =
                new Dictionary<string, ArenaSkillTreeNode>(StringComparer.Ordinal);
            foreach (ArenaSkillTreeNode node in tree.Nodes)
            {
                if (node == null || String.IsNullOrWhiteSpace(node.SkillId)
                    || !(node.IsFork
                        ? node.SkillId.StartsWith(
                            ForkNodePrefix, StringComparison.Ordinal)
                        : validSkillIds.Contains(node.SkillId)
                            && !node.ForceDecision)
                    || double.IsNaN(node.X) || double.IsInfinity(node.X)
                    || double.IsNaN(node.Y) || double.IsInfinity(node.Y)
                    || node.X < 0 || node.Y < 0
                    || node.X > MaximumNodePosition
                    || node.Y > MaximumNodePosition)
                {
                    throw new InvalidDataException(
                        "A skill tree contains an invalid or unknown skill node.");
                }
                if (!nodes.TryAdd(node.SkillId, node))
                {
                    throw new InvalidDataException(
                        "A skill tree may contain each skill only once.");
                }
            }
            if (tree.Nodes.Count(node => IsStartNode(node.SkillId)) > 1)
            {
                throw new InvalidDataException(
                    "A skill tree can contain only one start node.");
            }

            Dictionary<string, List<string>> prerequisites =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string skillId in nodes.Keys)
            {
                prerequisites.Add(skillId, new List<string>());
            }
            foreach (ArenaSkillTreeConnection connection in tree.Connections)
            {
                if (connection == null
                    || !nodes.ContainsKey(connection.FromSkillId)
                    || !nodes.ContainsKey(connection.ToSkillId)
                    || IsStartNode(connection.ToSkillId)
                    || String.Equals(
                        connection.FromSkillId, connection.ToSkillId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "A skill tree contains an invalid connection.");
                }
                List<string> parents = prerequisites[connection.ToSkillId];
                if (parents.Contains(connection.FromSkillId, StringComparer.Ordinal))
                {
                    throw new InvalidDataException(
                        "A skill tree contains a duplicate connection.");
                }
                parents.Add(connection.FromSkillId);
            }
            ValidateAcyclic(prerequisites);
        }

        public static bool ArePrerequisitesMet(
            ArenaSkillTreeDocument tree, string skillId, Func<string, int> getRank)
        {
            return ArePrerequisitesMet(tree, skillId, getRank, (forkId) => null);
        }

        public static bool ArePrerequisitesMet(
            ArenaSkillTreeDocument tree,
            string skillId,
            Func<string, int> getRank,
            Func<string, string> getChosenForkBranch)
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }
            if (getRank == null)
            {
                throw new ArgumentNullException(nameof(getRank));
            }
            if (getChosenForkBranch == null)
            {
                throw new ArgumentNullException(nameof(getChosenForkBranch));
            }
            if (IsStartNode(skillId) || IsForkNode(tree, skillId))
            {
                return false;
            }

            return AreNodePrerequisitesMet(
                tree, skillId, getRank, getChosenForkBranch,
                new HashSet<string>(StringComparer.Ordinal));
        }

        private static bool IsNodeAvailable(
            ArenaSkillTreeDocument tree,
            string nodeId,
            Func<string, int> getRank,
            Func<string, string> getChosenForkBranch,
            ISet<string> visited)
        {
            if (IsStartNode(nodeId))
            {
                return true;
            }

            ArenaSkillTreeNode node = tree.Nodes.FirstOrDefault(
                item => String.Equals(item.SkillId, nodeId, StringComparison.Ordinal));
            if (node == null)
            {
                return false;
            }
            if (!node.IsFork)
            {
                return getRank(nodeId) > 0;
            }
            return AreNodePrerequisitesMet(
                tree, nodeId, getRank, getChosenForkBranch, visited);
        }

        private static bool AreNodePrerequisitesMet(
            ArenaSkillTreeDocument tree,
            string nodeId,
            Func<string, int> getRank,
            Func<string, string> getChosenForkBranch,
            ISet<string> visited)
        {
            if (!visited.Add(nodeId))
            {
                return false;
            }
            try
            {
                bool hasStartNode = tree.Nodes.Any(
                    node => IsStartNode(node.SkillId));
                bool hasIncomingConnection = false;
                foreach (ArenaSkillTreeConnection connection in tree.Connections)
                {
                    if (!String.Equals(
                        connection.ToSkillId, nodeId, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    hasIncomingConnection = true;
                    ArenaSkillTreeNode source = tree.Nodes.First(
                        item => item.SkillId == connection.FromSkillId);
                    if (source.IsFork
                        && source.ForceDecision
                        && getChosenForkBranch(source.SkillId) is string chosenBranch
                        && !String.Equals(
                            chosenBranch, nodeId, StringComparison.Ordinal))
                    {
                        return false;
                    }
                    if (!IsNodeAvailable(
                        tree, source.SkillId, getRank,
                        getChosenForkBranch, visited))
                    {
                        return false;
                    }
                }
                return hasIncomingConnection || !hasStartNode;
            }
            finally
            {
                visited.Remove(nodeId);
            }
        }

        private static ArenaSkillTreeDocument LoadTree(string path)
        {
            ArenaSkillTreeDocument tree;
            try
            {
                tree = JsonSerializer.Deserialize<ArenaSkillTreeDocument>(
                    File.ReadAllText(path));
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    "The skill tree file contains invalid JSON.", exception);
            }
            if (tree == null)
            {
                throw new InvalidDataException("The skill tree file is empty.");
            }
            ValidateTree(tree);
            if (!tree.Nodes.Any(node => IsStartNode(node.SkillId)))
            {
                AddStartNodeToLegacyTree(tree);
                ValidateTree(tree);
            }
            tree.Name = Path.GetFileNameWithoutExtension(path);
            return tree;
        }

        private static void AddStartNodeToLegacyTree(ArenaSkillTreeDocument tree)
        {
            string[] initialSkillIds = tree.Nodes
                .Where(node => !tree.Connections.Any(
                    connection => connection.ToSkillId == node.SkillId))
                .Select(node => node.SkillId)
                .ToArray();

            double width = ArenaSkillTreeVisuals.NodeWidth;
            double height = ArenaSkillTreeVisuals.NodeHeight;
            const double spacing = 32;
            double startX = 80;
            double startY = 80;
            bool positionFound = false;
            for (double y = 80; !positionFound && y + height <= MaximumNodePosition;
                y += height + spacing)
            {
                for (double x = 80; x + width <= MaximumNodePosition;
                    x += width + spacing)
                {
                    bool overlapsNode = tree.Nodes.Any(node =>
                        x < node.X + width + spacing
                        && x + width + spacing > node.X
                        && y < node.Y + height + spacing
                        && y + height + spacing > node.Y);
                    if (!overlapsNode)
                    {
                        startX = x;
                        startY = y;
                        positionFound = true;
                        break;
                    }
                }
            }

            tree.Nodes.Add(new ArenaSkillTreeNode
            {
                SkillId = StartNodeId,
                X = startX,
                Y = startY
            });
            tree.Connections.AddRange(initialSkillIds.Select(
                CreateStartConnection));
        }

        private static string ValidateTreeFileName(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)
                || !String.Equals(Path.GetFileName(fileName), fileName,
                    StringComparison.Ordinal)
                || !String.Equals(Path.GetExtension(fileName), ".json",
                    StringComparison.OrdinalIgnoreCase)
                || String.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName))
                || Path.GetFileNameWithoutExtension(fileName).IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0
                || String.Equals(
                    Path.GetFileNameWithoutExtension(fileName),
                    DefaultTreeName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Choose a skill tree JSON file from the SkillTrees folder.",
                    nameof(fileName));
            }
            return fileName;
        }

        private static void ValidateAcyclic(
            IReadOnlyDictionary<string, List<string>> prerequisites)
        {
            Dictionary<string, int> states =
                new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string skillId in prerequisites.Keys)
            {
                Visit(skillId, prerequisites, states);
            }
        }

        private static void Visit(
            string skillId,
            IReadOnlyDictionary<string, List<string>> prerequisites,
            IDictionary<string, int> states)
        {
            if (states.TryGetValue(skillId, out int state))
            {
                if (state == 1)
                {
                    throw new InvalidDataException(
                        "A skill tree cannot contain circular prerequisites.");
                }
                if (state == 2)
                {
                    return;
                }
            }

            states[skillId] = 1;
            foreach (string prerequisite in prerequisites[skillId])
            {
                Visit(prerequisite, prerequisites, states);
            }
            states[skillId] = 2;
        }
    }
}
