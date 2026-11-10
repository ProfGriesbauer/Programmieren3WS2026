public class RaumMitJalousie : Raum, IRaumJalousie
{
    public bool JalousieGeschlossen { get; set; }

    public RaumMitJalousie(bool lichtAn, decimal temperatur, bool jalousieGeschlossen)
        : base(lichtAn, temperatur)
    {
        JalousieGeschlossen = jalousieGeschlossen;
    }
}
