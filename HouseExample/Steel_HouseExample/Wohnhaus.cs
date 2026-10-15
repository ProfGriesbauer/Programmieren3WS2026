public class Wohnhaus : Haus
{
    public override void AusschaltenOderHerunterfahren()
    {
        foreach (var raum in Räume)
        {
            raum.LichtAn = false;

            if (raum is IRaumJalousie raumMitJalousie)
            {
                raumMitJalousie.JalousieGeschlossen = true;
            }
        }
    }
}
