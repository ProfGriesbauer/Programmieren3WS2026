namespace Maxi_HouseExample;

public class Jalousie
{
    public bool Oben { get; private set; }

    public void Hochfahren()
    {
        Oben = true;
    }

    public void Herunterfahren()
    {
        Oben = false;
    }
}
