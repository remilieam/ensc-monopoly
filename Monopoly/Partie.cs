using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Partie
    {
        // Attributs
        private List<Joueur> Joueurs { get; set; }
        private Case[] Plateau { get; set; }
        private Pioche[] Pioches { get; set; }

        // Constructeur nouvelle partie
        public Partie(int nbJoueur)
        {
            // Construction des joueurs
            Joueurs = new List<Joueur>();
            for (int i = 0; i < nbJoueur; i++)
            {
                Console.Write("Entrer le nom du joueur : ");
                Joueurs.Add(new Joueur(Console.ReadLine()));
            }

            // Construction des pioches
            Pioches = new Pioche[] { new Pioche(XDocument.Load("..\\..\\FichiersXML\\Chance.xml")), new Pioche(XDocument.Load("..\\..\\FichiersXML\\Communaute.xml")) };

            // Choix du plateau
            int Rep = 0;
            bool Entier = false;
            while (!Entier)
            {
                Console.WriteLine("\nSur quelle plateau voulez-vous jouer ?");
                Console.WriteLine("  Tapez 1 pour la version classique");
                Console.WriteLine("  Tapez 2 pour la version française");
                Console.WriteLine("  Tapez 3 pour la version girondine");
                Console.WriteLine("  Tapez 4 pour la version bordelaise");
                Console.Write("Votre réponse : ");
                Entier = Erreur(Console.ReadLine(), out Rep, 4);
            }

            // Construction du plateau
            string Version = "";
            if (Rep == 1) { Version = "..\\..\\FichiersXML\\PlateauNormal.xml"; }
            if (Rep == 2) { Version = "..\\..\\FichiersXML\\PlateauFrance.xml"; }
            if (Rep == 3) { Version = "..\\..\\FichiersXML\\PlateauGironde.xml"; }
            if (Rep == 4) { Version = "..\\..\\FichiersXML\\PlateauBordeaux.xml"; }
            XDocument Fichier = XDocument.Load(Version);
            XElement Tree = Fichier.Root;
            Plateau = new Case[40];
            Plateau = RecupPlateau(Tree);
        }

        // Constructeur reprendre partie
        public Partie(XDocument doc)
        {
            // Construction des pioches
            Pioches = new Pioche[] { new Pioche(XDocument.Load("..\\..\\FichiersXML\\Chance.xml")), new Pioche(XDocument.Load("..\\..\\FichiersXML\\Communaute.xml")) };

            // Construction du plateau et des joueurs
            XElement Tree = doc.Root;
            Joueurs = new List<Joueur>();
            Plateau = new Case[40];
            Joueurs = RecupJoueurs(Tree.Element("Joueurs"));
            Plateau = RecupPlateau(Tree.Element("Plateau"));

            // On retire les cartes "Prison" dans le cas où un des joueurs la/les possède et on met à jour les couleurs possédées
            for (int i = 0; i < Joueurs.Count; i++)
            {
                Joueurs[i].MAJCouleur();

                if (Joueurs[i].Chance)
                {
                    for (int j = 0; j < Pioches[0].Piopioche.Count; j++)
                    {
                        if (Pioches[0].Piopioche[j].Type == "Prison")
                        {
                            Pioches[0].RetirerCarte(Pioches[0].Piopioche[j]);
                        }
                    }
                }

                if (Joueurs[i].Communaute)
                {
                    for (int j = 0; j < Pioches[1].Piopioche.Count; j++)
                    {
                        if (Pioches[1].Piopioche[j].Type == "Prison")
                        {
                            Pioches[1].RetirerCarte(Pioches[1].Piopioche[j]);
                        }
                    }
                }
            }
        }

        // Méthode pour faire jouer un joueur
        private bool Jouer(Joueur J)
        {
            // Rappel de la personne qui joue et de sa cagnotte
            Console.Clear();
            Console.WriteLine("============ {0} ============", J.Nom);
            Console.WriteLine("\nVotre solde : {0} EUR", J.Argent);

            // Booléens
            bool Rejouer = false; // Pour savoir si le joueur devra rejouer
            bool FinTour = false; // Pour savoir si le joueur peut bouger

            // Lancé des dés
            Console.WriteLine("\nPour lancer les dés, appuyez sur une touche");
            Console.ReadKey();
            Console.Write("\r" + " " + "\r");
            Random Alea = new Random();
            int De1 = Alea.Next(1, 7);
            int De2 = Alea.Next(1, 7);

            if (De1 == De2 && !J.Prison)
            {
                Rejouer = true;
                J.Double += 1;
            }

            Console.WriteLine(@"

                   +---+      +---+
Résultat des dés : | " + De1 + " |  et  | " + De2 + @" |
                   +---+      +---+
");

            // Cas où la personne est en prison
            if (J.Prison)
            {
                // Cas où le joueur a fait un double
                if (De1 == De2 && J.Double < 3)
                {
                    int Rep = 0;
                    bool Entier = false;

                    // Tant que le joueur n’a pas entré 1 ou 2
                    while (!Entier)
                    {
                        Console.WriteLine("\nVoulez-vous sortir de prison ?");
                        Console.WriteLine("Tapez 1 pour Oui\nTapez 2 pour Non");
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep, 2);
                    }

                    if (Rep == 1)
                    {
                        Rejouer = false;
                        J.Prison = false;
                        J.Double = 0;
                    }
                }

                // Cas où le joueur a la carte Chance lui permettant de sortir de prison
                if (J.Prison && J.Chance && J.Double < 3)
                {
                    int Rep = 0;
                    bool Entier = false;

                    // Tant que le joueur n’a pas entré 1 ou 2
                    while (!Entier)
                    {
                        Console.WriteLine("\nVoulez-vous utiliser votre carte Chance pour sortir de prison ?");
                        Console.WriteLine("Tapez 1 pour Oui\nTapez 2 pour Non");
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep, 2);
                    }

                    if (Rep == 1)
                    {
                        J.Prison = false;
                        Pioches[0].AjouterCarte(Pioches[0].Memoire);
                        J.Double = 0;
                    }
                }

                // Cas où le joueur a la carte Caisse de Communauté lui permettant de sortir de prison
                if (J.Prison && J.Communaute && J.Double < 3)
                {
                    int Rep = 0;
                    bool Entier = false;

                    // Tant que le joueur n’a pas entré 1 ou 2
                    while (!Entier)
                    {
                        Console.WriteLine("\nVoulez-vous utiliser votre carte Caisse de Communauté pour sortir de prison ?");
                        Console.WriteLine("Tapez 1 pour Oui\nTapez 2 pour Non");
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep, 2);
                    }

                    if (Rep == 1)
                    {
                        J.Prison = false;
                        Pioches[1].AjouterCarte(Pioches[1].Memoire);
                        J.Double = 0;
                    }
                }

                // Cas où le joueur n’a rien (ou ne veut rien faire)
                if (J.Prison)
                {
                    // Cas où le joueur est en prison depuis moins de 3 tours
                    if (J.Double < 2)
                    {
                        int Rep = 0;
                        bool Entier = false;

                        // Tant que le joueur n’a pas entré 1 ou 2
                        while (!Entier)
                        {
                            Console.WriteLine("\nVoulez-vous payer une amende de 50 EUR pour sortir de prison ?");
                            Console.WriteLine("Tapez 1 pour Oui\nTapez 2 pour Non");
                            Console.Write("Votre réponse : ");
                            Entier = Erreur(Console.ReadLine(), out Rep, 2);
                        }

                        // Cas où le joueur veut payer son amende et peut
                        if (Rep == 1 && J.Argent - 50 >= 0)
                        {
                            J.Prison = false;
                            J.RetraitArgent(50);
                            J.Double = 0;
                        }

                        // Cas où le joueur veut payer son amende et ne peut pas
                        else if (Rep == 1 && J.Argent - 50 < 0)
                        {
                            // Il doit retenter sa chance au prochain tour en espérant toucher de l’argent entre temps
                            Console.WriteLine("\nVous ne pouvez pas payer l’amende");
                            Console.WriteLine("Attendez le prochain tour");
                            J.Double += 1;
                            FinTour = true;
                        }

                        // Cas où le joueur veut retenter sa chance au prochain tour
                        else if (Rep == 2)
                        {
                            J.Double += 1;
                            FinTour = true;
                        }
                    }

                    // Cas où le joueur est en prison depuis 3 tours
                    else
                    {
                        Console.WriteLine("\nVous êtes obligé de payer l’amende de 50 EUR");
                        J.Prison = false;
                        J.Double = 0;
                        J.RetraitArgent(50);

                        // Si le joueur a perdu son tour se finit dès maintenant et il rend ses possessions à la banque
                        if (J.Perdu)
                        {
                            FinTour = true;
                            J.RendrePossessions(Pioches, Joueurs);
                        }
                    }
                }
            }

            // Cas où la personne a fait 3 doubles d’affilé
            if (J.Double == 3 && !J.Prison)
            {
                J.AccesDirect(Plateau, 10);
                FinTour = true;
            }

            // Cas où le joueur peut bouger
            if (!FinTour && !J.Prison)
            {
                // Déplacement et gestion de la case Départ
                J.Deplacement(Plateau, (De1 + De2));

                // Cas où le joueur tombe sur une case Chance
                if (Plateau[J.Position].NomCase == "Chance")
                {
                    Pioches[0].PiocherCarte(J, Plateau, Joueurs);
                    if (J.Perdu) { J.RendrePossessions(Pioches, Joueurs); }
                }

                // Cas où le joueur tombe sur une case Caisse de Communauté
                if (Plateau[J.Position].NomCase == "Caisse de Communauté")
                {
                    if (Pioches[1].PiocherCarte(J, Plateau, Joueurs))
                    {
                        Pioches[0].PiocherCarte(J, Plateau, Joueurs);
                    }
                    if (J.Perdu) { J.RendrePossessions(Pioches, Joueurs); }
                }

                // Cas où le joueur tombe sur des cases où il doit payer des taxes
                if (J.Position == 4 || J.Position == 38)
                {
                    J.RetraitArgent(Plateau[J.Position].Prix);
                    if (J.Perdu) { J.RendrePossessions(Pioches, Joueurs); }
                }

                // Cas où le joueur tombe sur une gare
                else if (Plateau[J.Position] is Gare)
                {
                    Gare GareActu = (Gare)Plateau[J.Position];

                    // Cas où la gare n’appartient à personne
                    if (GareActu.Proprietaire == null)
                    {
                        // Cas où le joueur ne souhaite pas acheter la gare
                        if (!GareActu.Acheter(J))
                        {
                            GareActu.Enchere(Joueurs);
                        }
                    }

                    // Cas où la gare appartient à quelqu’un
                    else
                    {
                        GareActu.Payer(J);
                    }
                }

                // Cas où le joueur tombe sur un service public
                else if (Plateau[J.Position] is ServicePublic)
                {
                    ServicePublic ServiceActu = (ServicePublic)Plateau[J.Position];

                    // Cas où le service public n’appartient à personne
                    if (ServiceActu.Proprietaire == null)
                    {
                        // Cas où le joueur ne souhaite pas acheter le service public
                        if (!ServiceActu.Acheter(J))
                        {
                            ServiceActu.Enchere(Joueurs);
                        }
                    }

                    // Cas où le service public appartient à quelqu’un
                    else
                    {
                        ServiceActu.PayerService(J, De1 + De2);
                    }
                }

                // Cas où le joueur tombe sur un terrain
                else if (Plateau[J.Position] is Terrain)
                {
                    Terrain TerrainActu = (Terrain)Plateau[J.Position];

                    // Cas où le terrain n’appartient à personne
                    if (TerrainActu.Proprietaire == null)
                    {
                        // Cas où le joueur ne souhaite pas acheter le terrain
                        if (!TerrainActu.Acheter(J))
                        {
                            TerrainActu.Enchere(Joueurs);
                        }
                    }

                    // Cas où le terrain appartient à quelqu’un
                    else
                    {
                        TerrainActu.Payer(J);
                    }
                }

                // Cas où le joueur tombe sur le policier
                else if (J.Position == 30)
                {
                    J.AccesDirect(Plateau, 10);
                }
            }

            Console.ReadKey();
            Console.Write("\r" + " " + "\r");
            return Rejouer;
        }

        // Méthode pour acheter des maisons / hôtels
        private void ModifBat(Joueur J)
        {
            int Rep = 0;

            // Tant que le joueur veut encore construire/vendre
            while (Rep != 5)
            {
                bool Entier = false;
                int Rep2 = 0, Rep3 = 0;
                Console.Clear();
                Console.WriteLine("============ {0} ============", J.Nom);
                Console.WriteLine("\nVotre solde : {0} EUR", J.Argent);

                // Tant que le joueur que le joueur n’a pas entré un chiffre entre 1 et 5
                while (!Entier)
                {
                    Console.WriteLine("\nVoulez-vous contruire une maison ou un hôtel ?");
                    Console.WriteLine("Tapez 1 pour construire une maison");
                    Console.WriteLine("Tapez 2 pour construire un hôtel");
                    Console.WriteLine("Tapez 3 pour vendre une maison");
                    Console.WriteLine("Tapez 4 pour vendre tous les hôtels");
                    Console.WriteLine("Tapez 5 si vous ne voulez rien construire");
                    Console.Write("Votre réponse : ");
                    Entier = Erreur(Console.ReadLine(), out Rep, 5);
                }

                // Cas où le joueur veut construire quelque chose
                if (Rep != 5)
                {
                    Entier = false;

                    // Tant que le joueur n’a pas choisi une couleur
                    while (!Entier)
                    {
                        // Choix de la couleur du terrain de construction
                        Console.WriteLine("\nChoisissez la couleur du terrain sur lequel vous voulez contruire ou vendre");
                        for (int i = 0; i < J.Couleurs.Count; i++)
                        {
                            Console.WriteLine("Tapez {0} pour sélectionner la couleur {1}", i + 1, J.Couleurs[i]);
                        }
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep2, J.Couleurs.Count);
                    }

                    Entier = false;
                    List<Terrain> Liste = new List<Terrain>();

                    // Tant que le joueur n’a pas choisi un terrain
                    while (!Entier)
                    {
                        // Choix du terrain de construction
                        Console.WriteLine("\nChoisissez le terrain sur lequel vous voulez construire ou vendre");
                        int c = 0;
                        for (int i = 0; i < J.Terrains.Count; i++)
                        {
                            if (J.Terrains[i].Couleur == J.Couleurs[Rep2 - 1])
                            {
                                c += 1;
                                Console.WriteLine("Tapez {0} pour sélectionner {1}", c, J.Terrains[i].NomCase);
                                Liste.Add(J.Terrains[i]);
                            }
                        }
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep3, c);
                    }

                    // Affichage des caractéristiques du terrain
                    Console.WriteLine("\nTerrain sélectionné : " + Liste[Rep3 - 1].NomCase);
                    Console.WriteLine("Nombre de maisons construites : " + Liste[Rep3 - 1].NbMaisons);
                    Console.Write("Construction d’un hôtel : ");
                    if (Liste[Rep3 - 1].Hotel) { Console.WriteLine("Oui"); }
                    else { Console.WriteLine("Non"); }
                    Console.WriteLine("Prix d’une maison : {0} EUR", Liste[Rep3 - 1].PrixMaison);
                    Console.WriteLine("Prix d’un hôtel : {0} EUR + 4 maisons", Liste[Rep3 - 1].PrixHotel);

                    // Cas où le joueur veut construire une maison
                    if (Rep == 1)
                    {
                        Liste[Rep3 - 1].ConstruireMaison(J);
                    }

                    // Cas où le joueur veut construire un hôtel
                    if (Rep == 2)
                    {
                        Liste[Rep3 - 1].ConstruireHotel(J);
                    }

                    // Cas où le joueur veut vendre une maison
                    if (Rep == 3)
                    {
                        Liste[Rep3 - 1].VendreMaison(J);
                    }

                    // Cas où le joueur veut vendre un hôtel
                    if (Rep == 4)
                    {
                        Liste[Rep3 - 1].VendreHotel(J);
                    }
                }
            }
        }

        // Méthode pour faire tourner la partie
        public void FairePartie()
        {
            bool Fin = false;
            int Tour = 0;

            // Tant qu’on ne quitte pas la partie ou qu’elle n’est pas finie
            while (!Fin)
            {
                // On fait jouer les joueurs les uns après les autres

                // Si la personne qui joue n’est pas en prison, on remet à 0 son compteur de double
                if (!Joueurs[Tour].Prison) { Joueurs[Tour].Double = 0; }

                // Tant que le joueur peut rejouer, il a droit à un nouveau tour
                while (Jouer(Joueurs[Tour]) && !Joueurs[Tour].Perdu)
                {
                    Console.WriteLine("\nC’est à nouveau votre tour");
                    Console.ReadKey();
                    Console.Write("\r" + " " + "\r");
                }

                // À chaque fin de tour, les joueurs peuvent effectuer des constructions, si cela leur est possible
                for (int i = 0; i < Joueurs.Count; i++)
                {
                    if (Joueurs[i].Couleurs.Count != 0)
                    {
                        ModifBat(Joueurs[i]);
                    }
                }

                // Si le joueur a perdu durant son tour, on l’élimine
                if (Joueurs[Tour].Perdu)
                {
                    Joueurs.Remove(Joueurs[Tour]);
                }

                // Cas où il n’y a plus qu’un seul joueur (les autres ayant perdu)
                if (Joueurs.Count == 1)
                {
                    Fin = true;
                    Console.WriteLine("\nPartie terminée !");
                    Console.WriteLine("L’heureux gagnant est : " + Joueurs[0].Nom);
                }

                // Cas où il reste encore plus d’un joueur
                else
                {
                    Tour = (Tour + 1) % Joueurs.Count;
                    Console.WriteLine("\nAppuyez sur “ÉCHAP” pour quitter la partie");
                    Console.WriteLine("Appuyez sur une autre touche pour passer au joueur suivant");
                    ConsoleKey Touche = Console.ReadKey().Key;
                    Console.Write("\r" + " " + "\r");

                    // Cas où l’on veut arrêter la partie
                    if (Touche == ConsoleKey.Escape)
                    {
                        Fin = true;

                        int Rep = 0;
                        bool Entier = false;

                        // Tant que le joueur n’a pas entré 1 ou 2
                        while (!Entier)
                        {
                            Console.Clear();
                            Console.WriteLine("Voulez-vous enregistrer la partie en cour ?");
                            Console.WriteLine("Tapez 1 pour Oui\nTapez 2 pour Non");
                            Console.Write("Votre réponse : ");
                            Entier = Erreur(Console.ReadLine(), out Rep, 2);
                        }

                        // Cas où le joueur veut enregistrer la partie
                        if (Rep == 1)
                        {
                            // Sérialisation des données
                            Console.WriteLine("\nEntrer le nom sous lequel vous voulez sauverder votre partie");
                            Console.Write("Votre réponse : ");
                            Sauver(Console.ReadLine());
                            Console.WriteLine("\nVotre partie a bien été enregistrée !");
                        }

                        Console.WriteLine("\nAu revoir !");
                    }
                }
            }
        }

        // Méthode de gestion des erreurs (dans un intervalle allant de 1 à c)
        private bool Erreur(string Ans, out int Rep, int c)
        {
            try
            {
                Rep = int.Parse(Ans);
                for (int i = 1; i <= c; i++)
                {
                    if (Rep == i)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                Rep = 0;
                Console.WriteLine("\nVous n’avez pas tapé un entier entre 1 et {0}\nVeuillez recommencer", c);
                return false;
            }
        }

        // Méthode pour récupérer le plateau
        private Case[] RecupPlateau(XElement racine)
        {
            List<XElement> Liste = (from c in racine.Descendants("Case") select c).ToList<XElement>();
            Case[] TabCases = new Case[Liste.Count];

            for (int i = 0; i < TabCases.Length; i++)
            {
                string Type = (string)Liste[i].Attribute("Type");

                if (Type == "Case")
                {
                    TabCases[i] = new Case((string)Liste[i].Element("NomCase"), (double)Liste[i].Element("Prix"));
                }

                else if (Type == "Gare")
                {
                    // Vérification pour savoir si la gare appartient à quelqu’un et récupération de l’éventuel propriétaire
                    Joueur Proprio = null;
                    if (Liste[i].Element("Proprietaire") != null)
                    {
                        for (int j = 0; j < Joueurs.Count; j++)
                        {
                            if ((string)Liste[i].Element("Proprietaire") == Joueurs[j].Nom)
                            {
                                Proprio = Joueurs[j];
                            }
                        }
                    }

                    // Construction de la gare
                    TabCases[i] = new Gare(
                        (string)Liste[i].Element("NomCase"),
                        (double)Liste[i].Element("Prix"),
                        new double[] {
                            (double)Liste[i].Element("LoyerGare").Element("G1"), 
                            (double)Liste[i].Element("LoyerGare").Element("G2"), 
                            (double)Liste[i].Element("LoyerGare").Element("G3"), 
                            (double)Liste[i].Element("LoyerGare").Element("G4") },
                        (double)Liste[i].Element("Hypotheque"),
                        (Proprio != null) ? Proprio : null);

                    // Ajout de la gare à la liste de gares du propriétaire
                    if (Proprio != null)
                    {
                        Proprio.AjouterGare((Gare)TabCases[i]);
                    }
                }

                else if (Type == "ServicePublic")
                {
                    // Vérification pour savoir si le service public appartient à quelqu’un et récupération de l’éventuel propriétaire
                    Joueur Proprio = null;
                    if (Liste[i].Element("Proprietaire") != null)
                    {
                        for (int j = 0; j < Joueurs.Count; j++)
                        {
                            if ((string)Liste[i].Element("Proprietaire") == Joueurs[j].Nom)
                            {
                                Proprio = Joueurs[j];
                            }
                        }
                    }

                    // Construction du service public
                    TabCases[i] = new ServicePublic(
                        (string)Liste[i].Element("NomCase"),
                        (double)Liste[i].Element("Prix"),
                        new double[] { (double)Liste[i].Element("LoyerService").Element("S1"), (double)Liste[i].Element("LoyerService").Element("S2") },
                        (double)Liste[i].Element("Hypotheque"),
                        (Proprio != null) ? Proprio : null);

                    // Ajout de la gare à la liste de services publics du propriétaire
                    if (Proprio != null)
                    {
                        Proprio.AjouterService((ServicePublic)TabCases[i]);
                    }
                }

                else if (Type == "Terrain")
                {
                    // Vérification que le terrain appartient à quelqu’un et récupération de l’éventuel propriétaire
                    Joueur Proprio = null;
                    if (Liste[i].Element("Proprietaire") != null)
                    {
                        for (int j = 0; j < Joueurs.Count; j++)
                        {
                            if ((string)Liste[i].Element("Proprietaire") == Joueurs[j].Nom)
                            {
                                Proprio = Joueurs[j];
                            }
                        }
                    }

                    // Construction du terrain
                    TabCases[i] = new Terrain(
                        (string)Liste[i].Element("NomCase"),
                        (double)Liste[i].Element("Prix"),
                        (string)Liste[i].Element("Couleur"),
                        new double[] {
                            (double)Liste[i].Element("LoyerTerrain").Element("Nu"), 
                            (double)Liste[i].Element("LoyerTerrain").Element("M1"), 
                            (double)Liste[i].Element("LoyerTerrain").Element("M2"), 
                            (double)Liste[i].Element("LoyerTerrain").Element("M3"), 
                            (double)Liste[i].Element("LoyerTerrain").Element("M4"), 
                            (double)Liste[i].Element("LoyerTerrain").Element("H")},
                        (double)Liste[i].Element("Hypotheque"),
                        (double)Liste[i].Element("PrixMaison"),
                        (double)Liste[i].Element("PrixHotel"),
                        (Liste[i].Element("NbMaisons") != null) ? (int)Liste[i].Element("NbMaisons") : 0,
                        (Liste[i].Element("Hotel") != null) ? (bool)Liste[i].Element("Hotel") : false,
                        (Liste[i].Element("Constructible")) != null ? (bool)Liste[i].Element("Constructible") : true,
                        (Proprio != null) ? Proprio : null);

                    // Ajout de la gare à la liste de terrains du propriétaire
                    if (Proprio != null)
                    {
                        Proprio.AjouterTerrain((Terrain)TabCases[i]);
                    }
                }
            }

            return TabCases;
        }

        // Méthode pour récupérer les joueurs
        private List<Joueur> RecupJoueurs(XElement racine)
        {
            List<Joueur> ListeJoueurs = (from c in racine.Descendants("Joueur")
                                         select new Joueur(
                                             (string)c.Element("Nom"),
                                             (double)c.Element("Argent"),
                                             (int)c.Element("Position"),
                                             (bool)c.Element("Perdu"),
                                             (bool)c.Element("Chance"),
                                             (bool)c.Element("Communaute"),
                                             (bool)c.Element("Prison"),
                                             (int)c.Element("Double"))).ToList<Joueur>();

            return ListeJoueurs;
        }

        // Méthode pour sauvegarder la partie
        private void Sauver(string nom)
        {
            // Création du document XML
            XDocument Doc = new XDocument(new XElement("Partie", new XElement("Plateau"), new XElement("Joueurs")));

            // Sauvegarde des joueurs (sans ses possessions)
            for (int i = 0; i < Joueurs.Count; i++)
            {
                Joueurs[i].SauverJoueur(Doc);
            }

            // Sauvegarde des possessions en indiquant le nom (en chaîne de caractères) du propriétaire
            for (int i = 0; i < Plateau.Length; i++)
            {
                if (Plateau[i] is Gare) { ((Gare)Plateau[i]).Sauver(Doc); }
                else if (Plateau[i] is Terrain) { ((Terrain)Plateau[i]).Sauver(Doc); }
                else if (Plateau[i] is ServicePublic) { ((ServicePublic)Plateau[i]).Sauver(Doc); }
                else { Plateau[i].SauverCase(Doc); }
            }

            // Enregistrement du fichier XML
            string NomDoc = "..\\..\\FichiersXML\\" + nom + ".xml";
            Doc.Save(NomDoc);
        }
    }
}