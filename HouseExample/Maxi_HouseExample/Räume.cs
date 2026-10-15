namespace Maxi_HouseExample;

public class Raum
{
    private readonly Func<bool> _versorgungAn;
    private bool _an;

    internal Raum(string name, Func<bool> versorgungAn)
    {
        Name = name;
        _versorgungAn = versorgungAn;
    }

    public string Name { get; }

    public bool An
    {
        get => _an;
        set
        {
            if (value && !_versorgungAn())
                throw new InvalidOperationException($"{Name} kann ohne Versorgung nicht eingeschaltet werden.");

            _an = value;
        }
    }

    internal void Ausschalten()
    {
        _an = false;
    }
}

public class Bad : Raum
{
    internal Bad(Func<bool> wasserAn) : base("Bad", wasserAn) { }
}

public class Wohnzimmer : Raum
{
    internal Wohnzimmer(Func<bool> heizungAn) : base("Wohnzimmer", heizungAn) { }
}

public class Sauna : Raum
{
    internal Sauna(Func<bool> heizungAn) : base("Sauna", heizungAn) { }
}
