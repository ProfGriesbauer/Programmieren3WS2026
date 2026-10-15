namespace PatrickHouseExample;

public class Fenster
{
    public bool IstOffen { get; private set; }
    public bool Sicherheitsmodus { get; set; }

    public void Oeffnen()
    {
        if (!Sicherheitsmodus)
            IstOffen = true;
    }

    public void Schliessen()
    {
        IstOffen = false;
    }

    public void Wechseln()
    {
        if (IstOffen)
        {
            Schliessen();
        }
        else
        {
            Oeffnen();
        }
    }

    public override string ToString()
    {
        return IstOffen ? "Offen" : "Zu";
    }
}
