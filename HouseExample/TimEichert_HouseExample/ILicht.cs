namespace TimEichert_HouseExample;

public interface ILicht
{
    bool LichtIstAn { get; }
    void LichtAnmachen();
    void LichtAusschalten();
}