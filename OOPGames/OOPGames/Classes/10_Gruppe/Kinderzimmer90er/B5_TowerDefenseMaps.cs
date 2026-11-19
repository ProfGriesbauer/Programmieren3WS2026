using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace OOPGames
{
    // A path crossing with a bridge deck: enemies inside Area that move along the bridge direction are drawn
    // on top of the bridge, all others underneath (see karten/LIESMICH_Karten.txt).
    public sealed class TowerDefenseCrossing
    {
        public TowerDefenseCrossing(Rect area, bool horizontal)
        {
            Area = area;
            Horizontal = horizontal;
        }

        public Rect Area { get; private set; }
        public bool Horizontal { get; private set; }

        public bool IsOnBridge(double x, double y, double directionX, double directionY)
        {
            if (!Area.Contains(x, y)) return false;
            return Horizontal ? Math.Abs(directionX) > Math.Abs(directionY) : Math.Abs(directionY) > Math.Abs(directionX);
        }
    }

    // One map from Assets/karten/<Id>.json. All coordinates are pixels of the 1920x1056 map images.
    public sealed class TowerDefenseMapDefinition
    {
        public TowerDefenseMapDefinition(string id, string name, string difficulty, Point mainBuilding,
            IList<Point> towerSpots, IList<IList<Point>> routes, IList<int> routeTiles, IList<TowerDefenseCrossing> crossings)
        {
            Id = id;
            Name = name;
            Description = difficulty;
            MainBuilding = mainBuilding;
            TowerSpots = towerSpots;
            Routes = routes;
            RouteTiles = routeTiles;
            Crossings = crossings;
        }

        // File prefix of the map images (<Id>_boden.png, _deko.png, _bruecken.png, _komplett.png).
        public string Id { get; private set; }
        public string Name { get; private set; }
        // Difficulty text from the map file, e.g. "leicht" or "sehr schwer".
        public string Description { get; private set; }
        // Pivot of the castle sprite = the gate, where every route ends.
        public Point MainBuilding { get; private set; }
        // Pivot points for the tower sprites, one per pedestal.
        public IList<Point> TowerSpots { get; private set; }
        // One waypoint list per spawn.
        public IList<IList<Point>> Routes { get; private set; }
        public IList<int> RouteTiles { get; private set; }
        public IList<TowerDefenseCrossing> Crossings { get; private set; }

        public bool IsOnBridge(double x, double y, double directionX, double directionY)
        {
            foreach (TowerDefenseCrossing crossing in Crossings)
                if (crossing.IsOnBridge(x, y, directionX, directionY)) return true;
            return false;
        }
    }

    public static class TowerDefenseMaps
    {
        public const int MapWidth = 1920;
        public const int MapHeight = 1056;
        public const int TileSize = 96;

        // Unlock order = file order (map1 ... map5): the routes get shorter and more spawns appear.
        static readonly IList<TowerDefenseMapDefinition> _Maps = Load();

        public static IList<TowerDefenseMapDefinition> Maps { get { return _Maps; } }

        // Extra enemy strength per map (multiplies the difficulty multiplier). Tuned with the balancing bot so the
        // maps get harder in order. Retuned after the stronger upgrades, the mastery levels and the damped gold.
        static readonly double[] _Strength = { 1.05, 1.29, 1.08, 0.85, 1.2 };

        public static double EnemyStrength(int mapIndex)
        {
            return mapIndex >= 0 && mapIndex < _Strength.Length ? _Strength[mapIndex] : 1.0;
        }

        static IList<TowerDefenseMapDefinition> Load()
        {
            string folder = Path.Combine(TowerDefenseAssets.Folder, "karten");
            string[] files = Directory.Exists(folder) ? Directory.GetFiles(folder, "map*.json") : new string[0];
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            List<TowerDefenseMapDefinition> maps = new List<TowerDefenseMapDefinition>();
            foreach (string file in files) maps.Add(Parse(file));
            if (maps.Count == 0) throw new FileNotFoundException("No Tower Defense maps found in " + folder);
            return maps;
        }

        static TowerDefenseMapDefinition Parse(string file)
        {
            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(file)))
            {
                JsonElement root = document.RootElement;
                List<IList<Point>> routes = new List<IList<Point>>();
                List<int> tiles = new List<int>();
                foreach (JsonElement route in root.GetProperty("routen").EnumerateArray())
                {
                    List<Point> points = new List<Point>();
                    foreach (JsonElement point in route.GetProperty("waypoints").EnumerateArray()) points.Add(ToPoint(point));
                    routes.Add(points);
                    tiles.Add(route.GetProperty("kacheln").GetInt32());
                }

                List<TowerDefenseCrossing> crossings = new List<TowerDefenseCrossing>();
                foreach (JsonElement crossing in root.GetProperty("kreuzungen").EnumerateArray())
                {
                    JsonElement area = crossing.GetProperty("bereich");
                    crossings.Add(new TowerDefenseCrossing(
                        new Rect(new Point(area[0].GetDouble(), area[1].GetDouble()), new Point(area[2].GetDouble(), area[3].GetDouble())),
                        crossing.GetProperty("oben").GetString() == "horizontal"));
                }

                List<Point> spots = new List<Point>();
                foreach (JsonElement spot in root.GetProperty("podeste").EnumerateArray()) spots.Add(ToPoint(spot.GetProperty("turm_pivot")));

                return new TowerDefenseMapDefinition(
                    root.GetProperty("id").GetString(), root.GetProperty("name").GetString(),
                    root.GetProperty("schwierigkeit").GetString(),
                    ToPoint(root.GetProperty("schloss").GetProperty("pivot")), spots, routes, tiles, crossings);
            }
        }

        static Point ToPoint(JsonElement array)
        {
            return new Point(array[0].GetDouble(), array[1].GetDouble());
        }
    }
}
