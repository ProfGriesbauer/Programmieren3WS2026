// See https://aka.ms/new-console-template for more information
BeesHouse house1 = new BeesHouse();
BeesHouse house2 = new BeesHouse();
house1.name="BeesHouse1";
house1.Hausnummer=1;
house2.name="BeesHouse2";
house2.Hausnummer=2;
object objh = house1;
Console.WriteLine("Hello, World!");
Console.WriteLine(house1.name + "  " + house1.Hausnummer);
Console.WriteLine(house2.name + "  " +house2.Hausnummer);