using TimEichert_HouseExample;

House_general haus = new House_general("Mein Haus", "beige", "Musterstraße", 12);

Stockwerk erdgeschoss = haus.StockwerkHinzufuegen(0);
Wohnzimmer wohnzimmer = new Wohnzimmer { name = "Wohnzimmer", flaeche = 25 };
Kueche kueche = new Kueche { name = "Küche", flaeche = 15 };
erdgeschoss.RaumHinzufuegen(wohnzimmer);
erdgeschoss.RaumHinzufuegen(kueche);

Stockwerk obergeschoss = haus.StockwerkHinzufuegen(1);
Schlafzimmer schlafzimmer = new Schlafzimmer { name = "Schlafzimmer", flaeche = 18 };
Bad badezimmer = new Bad { name = "Badezimmer", flaeche = 8 };
obergeschoss.RaumHinzufuegen(schlafzimmer);
obergeschoss.RaumHinzufuegen(badezimmer);

IHeizung heizung = wohnzimmer;
heizung.HeizungAnmachen();

ILicht licht = wohnzimmer;
licht.LichtAnmachen();

IWasser wasser = badezimmer;
wasser.WasserLaufenLassen();

ILicht kuechenLicht = kueche;
kuechenLicht.LichtAnmachen();

IWasser kuechenWasser = kueche;
kuechenWasser.WasserLaufenLassen();

haus.HausAnzeigen();
