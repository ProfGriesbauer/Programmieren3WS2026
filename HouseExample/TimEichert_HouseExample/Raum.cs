namespace TimEichert_HouseExample;

public class Raum : IHeizung, ILicht
{
    public string name = string.Empty;
    public double flaeche;
    public bool HeizungIstAn { get; private set; }
    public bool LichtIstAn { get; private set; }
    public double LichtleistungWatt => LichtIstAn ? flaeche * 5 : 0;

    public void HeizungAnmachen()
    {
        HeizungIstAn = true;
    }

    public void HeizungAusschalten()
    {
        HeizungIstAn = false;
    }

    public void LichtAnmachen()
    {
        LichtIstAn = true;
    }

    public void LichtAusschalten()
    {
        LichtIstAn = false;
    }
}