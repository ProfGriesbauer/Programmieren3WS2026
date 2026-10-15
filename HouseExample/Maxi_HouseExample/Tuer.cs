namespace Maxi_HouseExample;

public class Tuer
{
    public bool Offen { get; private set; }

    public void Oeffnen()
    {
        Offen = true;
    }

    public void Schliessen()
    {
        Offen = false;
    }
}
