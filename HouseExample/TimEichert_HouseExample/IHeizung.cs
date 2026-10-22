namespace TimEichert_HouseExample;

public interface IHeizung
{
    bool HeizungIstAn { get; }
    void HeizungAnmachen();
    void HeizungAusschalten();
}