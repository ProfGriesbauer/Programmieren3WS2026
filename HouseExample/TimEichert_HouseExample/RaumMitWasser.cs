namespace TimEichert_HouseExample;

public class RaumMitWasser : Raum, IWasser
{
    public bool WasserLaeuft { get; private set; }

    public void WasserLaufenLassen()
    {
        WasserLaeuft = true;
    }

    public void WasserAbstellen()
    {
        WasserLaeuft = false;
    }
}