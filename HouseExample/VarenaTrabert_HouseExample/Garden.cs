public class Garden
{
    public double AreaInSquareMeters { get; set; }
    public Pool? Pool { get; set; }
    public List<Tree> Trees { get; } = [];

    public Garden(double areaInSquareMeters)
    {
        AreaInSquareMeters = areaInSquareMeters;
    }

    public void AddTree(Tree tree)
    {
        Trees.Add(tree);
    }
}
