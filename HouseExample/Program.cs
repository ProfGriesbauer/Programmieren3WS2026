House myHouse = new House("Musterstraße 12", 2);
myHouse.AddRoom(new Room("Küche", 1));
myHouse.AddRoom(new Room("Bad", 1));
myHouse.AddRoom(new Room("Schlafzimmer", 2));
myHouse.Garden = new Garden(120);
myHouse.Garage = new Garage(1);
myHouse.Attic = new Attic(false);
myHouse.PrintInfo();
