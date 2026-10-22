namespace TimEichert_HouseExample;

public interface IWasser
{
    bool WasserLaeuft { get; }
    void WasserLaufenLassen();
    void WasserAbstellen();
}