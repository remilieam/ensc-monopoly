using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Terrain : Case
    {
        public readonly string Couleur;
        public readonly double[] Loyer;
        public readonly double Hypotheque;
        public readonly double PrixMaison;
        public readonly double PrixHotel;
        public int NbMaisons { get; private set; }
        public bool Hotel { get; private set; }
        public bool Constructible { get; private set; }
        public Joueur Proprietaire { get; private set; }

        // Constructeur des terrains
        public Terrain(string nom, double prix, string couleur, double[] loyer, double hypotheque,
            double prixM, double prixH, int nbM, bool hotel, bool constructible, Joueur proprio)
            : base(nom, prix)
        {
            Couleur = couleur;
            Loyer = new double[loyer.Length];
            Loyer = loyer;
            Hypotheque = hypotheque;
            PrixMaison = prixM;
            PrixHotel = prixH;
            NbMaisons = nbM;
            Hotel = hotel;
            Constructible = constructible;
            Proprietaire = proprio;
        }

        // Méthode pour acheter ou non un terrain
        public bool Acheter(Joueur J)
        {
            // Calcul du nombre de terrains de la même couleur possédés par le joueur
            int Nb = 0;
            for (int i = 0; i < J.Terrains.Count; i++)
            {
                if (J.Terrains[i].Couleur == Couleur)
                {
                    Nb += 1;
                }
            }

            int Rep = 0;
            bool Entier = false;

            // Tant que le joueur n’a pas entré 1 ou 2
            while (!Entier)
            {
                Console.WriteLine("\nVoulez-vous acheter ce terrain ?");
                Console.WriteLine("\nPrix : {0} EUR", Prix);
                Console.WriteLine("Couleur : " + Couleur);
                Console.WriteLine("Nombre de terrains de la même couleur : " + Nb);
                Console.WriteLine("\nTapez 1 pour Oui\nTapez 2 pour Non");
                Console.Write("Votre réponse : ");
                Entier = Erreur(Console.ReadLine(), out Rep);
                if (!Entier) { Console.WriteLine("\nVous n’avez pas tapé 1 ou 2\nVeuillez recommencer"); }
            }

            // Cas où le joueur veut acheter le terrain et a assez d’argent
            if (Rep == 1 && J.Argent - Prix >= 0)
            {
                J.AjouterTerrain(this);
                J.RetraitArgent(Prix);
                Proprietaire = J;
                J.MAJCouleur();
                return true;
            }

            // Cas où le joueur veut acheter le terrain mais n’a pas assez d’argent
            else if (Rep == 1 && J.Argent - Prix < 0)
            {
                Console.WriteLine("\nVous n’avez pas suffisamment d’argent pour faire cet achat");
                return false;
            }

            // Cas où le joueur ne veut pas acheter le terrain
            else if (Rep == 2)
            {
                return false;
            }

            return false;
        }

        // Méthode pour enchérir sur un terrain
        public void Enchere(List<Joueur> JS)
        {
            bool Fin = false;
            double Montant = 0;
            Joueur Acheteur = null;

            // Tant qu’on n’a pas quitté l’enchère
            while (!Fin)
            {
                // On compte le nombre de refus à chaque tour de table
                int NbRefus = 0;

                // On regarde qui veut enchérir parmi tous les joueurs de la table
                for (int i = 0; i < JS.Count; i++)
                {
                    int Rep = 0;
                    bool Entier = false;
                    Console.Clear();
                    Console.WriteLine("============ {0} ============", JS[i].Nom);

                    // Tant que le joueur n’a pas entré 1 ou 2
                    while (!Entier)
                    {
                        // Calcul du nombre de terrains de la même couleur possédés par le joueur
                        int Nb = 0;
                        for (int j = 0; j < JS[i].Terrains.Count; j++)
                        {
                            if (JS[i].Terrains[j].Couleur == Couleur)
                            {
                                Nb += 1;
                            }
                        }

                        Console.WriteLine("\nVoulez-vous enchérir sur {0} ?", NomCase);
                        Console.WriteLine("\nEnchère actuelle : {0} EUR", Montant);
                        Console.WriteLine("Prix initial : {0} EUR", Prix);
                        Console.WriteLine("Votre solde : {0} EUR", JS[i].Argent);
                        Console.WriteLine("Couleur : " + Couleur);
                        Console.WriteLine("Nombre de terrain de la même couleur : " + Nb);
                        Console.WriteLine("\nTapez 1 pour Oui\nTapez 2 pour Non");
                        Console.Write("Votre réponse : ");
                        Entier = Erreur(Console.ReadLine(), out Rep);
                    }

                    // Cas où le joueur veut enchérir et peut
                    if (Rep == 1 && JS[i].Argent - Montant > 0)
                    {
                        double Enc = 0;
                        bool Reel = false;

                        // Vérification que le joueur rentre un réel, plus grand que l’enchère actuelle et qu’il peut dépenser cette somme
                        while (!Reel)
                        {
                            Console.Write("\nEntrer une enchère supérieure à {0} EUR : ", Montant);
                            try
                            {
                                Enc = double.Parse(Console.ReadLine());
                                if (Enc > Montant && JS[i].Argent - Enc >= 0)
                                {
                                    Montant = Enc;
                                    Reel = true;
                                    Acheteur = JS[i];
                                }
                            }
                            catch
                            {
                                Console.WriteLine("\nVous n’avez pas tapé 1 ou 2");
                                Console.WriteLine("OU vous n’avez pas entrée une enchère assez élevée");
                                Console.WriteLine("OU vons n’avez pas assez d’argent pour affectuer une telle enchère\nVeuillez recommencer");
                            }
                        }
                    }

                    // Cas où le joueur veut enchérir mais ne peut pas par manque d’argent
                    else if (Rep == 1 && JS[i].Argent - Montant <= 0)
                    {
                        Console.WriteLine("\nVons n’avez pas assez d’argent pour participer à cette enchère");
                        NbRefus += 1;
                    }

                    // Cas où le joueur ne veut pas enchérir
                    else if (Rep == 2)
                    {
                        NbRefus += 1;
                    }
                }

                // Cas où un seul joueur parmi tous les joueurs enchérit lors d’un tour de table
                if (NbRefus == JS.Count - 1 || (NbRefus == JS.Count && Montant != 0))
                {
                    // C’est la fin de l’enchère
                    Fin = true;

                    // Attribution du terrain au joueur qui a enchéri la plus grosse somme
                    Console.Clear();
                    Console.WriteLine("============ {0} ============", Acheteur.Nom);
                    Console.WriteLine("\nFélicitations ! Vous avez remporté l’enchère");
                    Acheteur.RetraitArgent(Montant);
                    Acheteur.AjouterTerrain(this);
                    Proprietaire = Acheteur;
                    Acheteur.MAJCouleur();
                }

                // Cas où aucun joueur parmi les joueurs ne peut enchérir
                else if (NbRefus == JS.Count && Montant == 0)
                {
                    // C’est la fin de l’enchère et on n’a pas de propriétaire pour ce terrain
                    Fin = true;
                    Proprietaire = null;
                }
            }
        }

        // Méthode pour payer ou non le loyer du terrain
        public void Payer(Joueur J)
        {
            // Cas où le joueur n’est pas le propriétaire du terrain
            if (Proprietaire != J)
            {
                Console.WriteLine("\nCe terrain appartient à " + Proprietaire.Nom);

                // Cas où le terrain possède un hôtel
                if (Hotel)
                {
                    // Mouvement d’argent entre le propriétaire et le joueur, et vérification que ce dernier n’a pas perdu
                    Proprietaire.AjoutArgent(J.RetraitArgent(Loyer[5]));
                    if (J.Perdu) { J.TransfererPossessions(Proprietaire); }
                }

                // Cas où le terrain ne possède pas d’hôtel
                else
                {
                    double Versement = Loyer[NbMaisons];

                    // Vérification pour savoir s’il ne faut pas multiplier le loyer par 2
                    if (NbMaisons == 0)
                    {
                        bool FamilleCouleur = false;

                        for (int i = 0; i < J.Couleurs.Count; i++)
                        {
                            if (Couleur == J.Couleurs[i])
                            {
                                FamilleCouleur = true;
                            }
                        }

                        if (FamilleCouleur)
                        {
                            Versement = Loyer[NbMaisons] * 2;
                        }
                    }

                    // Mouvement d’argent entre le propriétaire et le joueur, et vérification que ce dernier n’a pas perdu
                    Proprietaire.AjoutArgent(J.RetraitArgent(Versement));
                    if (J.Perdu) { J.TransfererPossessions(Proprietaire); }
                }
            }

            // Cas où le joueur est le propriétaire du terrain
            else
            {
                Console.WriteLine("\nCe terrain vous appartient");
            }
        }

        // Méthode de gestion des erreurs
        public bool Erreur(string Ans, out int Rep)
        {
            try
            {
                Rep = int.Parse(Ans);
                if (Rep == 1 || Rep == 2)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                Rep = 0;
                return false;
            }
        }

        // Méthode pour sauvegarder les terrains
        public void Sauver(XDocument Doc)
        {
            Doc.Root.Element("Plateau").Add(
                new XElement("Case",
                    new XAttribute("Type", "Terrain"),
                    new XElement("NomCase", NomCase),
                    new XElement("Prix", Prix),
                    new XElement("Couleur", Couleur),
                    new XElement("LoyerTerrain",
                        new XElement("Nu", Loyer[0]),
                        new XElement("M1", Loyer[1]),
                        new XElement("M2", Loyer[2]),
                        new XElement("M3", Loyer[3]),
                        new XElement("M4", Loyer[4]),
                        new XElement("H", Loyer[5])
                        ),
                    new XElement("Hypotheque", Hypotheque),
                    new XElement("PrixMaison", PrixMaison),
                    new XElement("PrixHotel", PrixHotel),
                    new XElement("NbMaisons", NbMaisons),
                    new XElement("Hotel", Hotel),
                    new XElement("Constructible", Constructible),
                    new XElement("Proprietaire", (Proprietaire != null) ? Proprietaire.Nom : null)
                    ));
        }

        // Méthode pour déterminer les autres terrains de la même couleur
        private List<Terrain> FamilleCouleur(Joueur J)
        {
            List<Terrain> Famille = new List<Terrain>();

            for (int i = 0; i < J.Terrains.Count; i++)
            {
                if (J.Terrains[i].Couleur == Couleur)
                {
                    Famille.Add(J.Terrains[i]);
                }
            }

            return Famille;
        }

        // Méthode pour réinitialiser la constructibilité des terrains
        private void ReinitialiserConstructible(Joueur J)
        {
            List<Terrain> Famille = new List<Terrain>(FamilleCouleur(J));
            bool MAJ = true; // Pour savoir s’il faut procéder à la mise à jour
            bool Hotel = true; // Pour savoir si tous les terrains ont un hôtel

            // Parcours de tous les terrains de la même couleur
            for (int i = 0; i < Famille.Count; i++)
            {
                // Vérification que tous les terrains sont insconstructibles
                if (Famille[i].Constructible)
                {
                    MAJ = false;
                }

                // Vérification que tous les terrains n’aient pas tous un hôtel
                if (!Famille[i].Hotel)
                {
                    Hotel = false;
                }
            }

            // Si tous les terrains de la famille de couleur sont insconstructibles et n’ont pas tous un hôtel
            if (MAJ && !Hotel)
            {
                // … On réinitialise le booléen de constructibilité
                for (int i = 0; i < Famille.Count; i++)
                {
                    Famille[i].Constructible = true;
                }
            }
        }

        // Méthode pour réinitialiser la vente des terrains
        private void ReinitialiserVendable(Joueur J)
        {
            List<Terrain> Famille = new List<Terrain>(FamilleCouleur(J));
            bool MAJ = true; // Pour savoir s’il faut procéder à la mise à jour
            bool Nu = true; // Pour savoir si tous les terrains sont nus

            // Parcours de tous les terrains de la même couleur
            for (int i = 0; i < Famille.Count; i++)
            {
                // Vérification que tous les terrains sont constructibles
                if (!Famille[i].Constructible)
                {
                    MAJ = false;
                }

                // Vérification que tous les terrains ne soient pas tous nus
                if (Famille[i].NbMaisons != 0)
                {
                    Nu = false;
                }
            }

            // Si tous les terrains de la famille de couleur sont constructibles et ne sont pas tous nus
            if (MAJ && !Nu)
            {
                // … On réinitialise le booléen d’inconstructibilité
                for (int i = 0; i < Famille.Count; i++)
                {
                    Famille[i].Constructible = false;
                }
            }
        }

        // Méthode pour construire une maison
        public void ConstruireMaison(Joueur J)
        {
            // Vérification pour savoir s’il faut faire une mise à jour
            ReinitialiserConstructible(J);

            // Cas où le joueur peut construire une maison
            if (Constructible && NbMaisons < 4 && J.Argent - PrixMaison >= 0)
            {
                NbMaisons += 1;
                Constructible = false;
                J.RetraitArgent(PrixMaison);
                Console.WriteLine("Appuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }

            // Cas où le joueur ne peut pas construire de maisons
            else
            {
                Console.WriteLine("\nVous ne pouvez pas construire de maisons sur ce terrain");
                Console.WriteLine("\nAppuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }
        }

        // Méthode pour construire un hôtel
        public void ConstruireHotel(Joueur J)
        {
            // Vérification pour savoir s’il faut faire une mise à jour
            ReinitialiserConstructible(J);

            // Cas où le joueur peut construire un hôtel
            if (Constructible && NbMaisons == 4 && J.Argent - PrixHotel >= 0)
            {
                Hotel = true;
                Constructible = false;
                NbMaisons = 0;
                J.RetraitArgent(PrixHotel);
                Console.WriteLine("\nAppuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }

            // Cas où le joueur ne peut pas construire d’hôtels
            else
            {
                Console.WriteLine("\nVous ne pouvez pas construire d’hôtels sur ce terrain");
                Console.WriteLine("Appuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }
        }

        // Méthode pour vendre une maison
        public void VendreMaison(Joueur J)
        {
            // Vérification pour savoir s’il faut faire une mise à jour
            ReinitialiserVendable(J);

            // Cas où le joueur peut vendre une maison
            if (!Constructible && !Hotel)
            {
                NbMaisons -= 1;
                Constructible = true;
                J.AjoutArgent(PrixMaison / 2);
                Console.WriteLine("\nAppuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }

            // Cas où le joueur ne peut pas vendre de maison
            else
            {
                Console.WriteLine("\nVous ne pouvez pas vendre de maisons sur ce terrain");
                Console.WriteLine("Appuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }
        }

        // Méthode pour vendre un hôtel
        public void VendreHotel(Joueur J)
        {
            // Vérification pour savoir s’il faut faire une mise à jour
            ReinitialiserVendable(J);

            int Rep = 0;
            bool Entier = false;

            // Tant que le joueur n’a pas entré 1 ou 2
            while (!Entier)
            {
                Console.WriteLine("\nTapez 1 si vous souhaitez vendre votre hôtel à moitié prix et récupérer les 4 maisons");
                Console.WriteLine("\nTapez 2 si vous souhaitez vendre votre hôtel à moitié prix et récupérer la moitié du prix des 4 maisons");
                Console.Write("Votre réponse : ");
                Entier = Erreur(Console.ReadLine(), out Rep);
                if (!Entier) { Console.WriteLine("\nVous n’avez pas tapé 1 ou 2\nVeuillez recommencer"); }
            }

            // Cas où le joueur peut vendre son hôtel et a tapé 1
            if (Rep == 2 && !Constructible && Hotel)
            {
                Hotel = false;
                NbMaisons = 4;
                Constructible = true;
                J.AjoutArgent(PrixHotel / 2);
                Console.WriteLine("\nAppuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }

            // Cas où le joueur peut vendre son hôtel et a tapé 2
            else if (Rep == 1 && !Constructible && Hotel)
            {
                Hotel = false;
                Constructible = true;
                J.AjoutArgent(PrixHotel / 2 + 2 * PrixMaison);
                Console.WriteLine("\nAppuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }

            // Cas où le joueur ne peut pas vendre d’hôtels
            else
            {
                Console.WriteLine("\nVous ne pouvez pas vendre d’hôtels sur ce terrain");
                Console.WriteLine("Appuyez sur une touche pour continuer");
                Console.ReadKey();
                Console.Write("\r" + " " + "\r");
            }
        }

        // Méthode pour modifier le propriétaire
        public void ModifierProprio(Joueur J)
        {
            Proprietaire = J;
            NbMaisons = 0;
            Hotel = false;
        }
    }
}