using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Gare : Case
    {
        // Attributs
        public readonly double[] Loyer;
        public readonly double Hypotheque;
        public Joueur Proprietaire { get; private set; }

        // Constructeur des gares
        public Gare(string nom, double prix, double[] loyerGare, double hypotheque, Joueur proprio)
            : base(nom, prix)
        {
            Loyer = new double[loyerGare.Length];
            Loyer = loyerGare;
            Hypotheque = hypotheque;
            Proprietaire = proprio;
        }

        // Méthode pour acheter ou non une gare
        public bool Acheter(Joueur J)
        {
            int Rep = 0;
            bool Entier = false;

            // Tant que le joueur n’a pas entré 1 ou 2
            while (!Entier)
            {
                Console.WriteLine("\nVoulez-vous acheter cette gare ?");
                Console.WriteLine("\nPrix : {0} EUR", Prix);
                Console.WriteLine("Nombre de gares que vous possédez : " + J.Gares.Count);
                Console.WriteLine("\nTapez 1 pour Oui\nTapez 2 pour Non");
                Console.Write("Votre réponse : ");
                Entier = Erreur(Console.ReadLine(), out Rep);
            }

            // Cas où le joueur veut acheter la gare et a assez d’argent
            if (Rep == 1 && J.Argent - Prix >= 0)
            {
                J.AjouterGare(this);
                Proprietaire = J;
                J.RetraitArgent(Prix);
                return true;
            }

            // Cas où le joueur veut acheter la gare mais n’a pas assez d’argent
            else if (Rep == 1 && J.Argent - Prix < 0)
            {
                Console.WriteLine("\nVous n’avez pas suffisamment d’argent pour faire cet achat");
                return false;
            }

            // Cas où le joueur ne veut pas acheter la gare
            else if (Rep == 2)
            {
                return false;
            }

            return false;
        }

        // Méthode pour enchérir sur une gare
        public void Enchere(List<Joueur> JS)
        {
            bool FinEnc = false;
            double Montant = 0;
            Joueur Acheteur = null;

            // Tant qu’on n’a pas quitté l’enchère
            while (!FinEnc)
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
                        Console.WriteLine("\nVoulez-vous enchérir sur {0} ?", NomCase);
                        Console.WriteLine("\nEnchère actuelle : {0} EUR", Montant);
                        Console.WriteLine("Prix initial : {0} EUR", Prix);
                        Console.WriteLine("Votre solde : {0} EUR", JS[i].Argent);
                        Console.WriteLine("Nombre de gares que vous possédez : " + JS[i].Gares.Count);
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
                    FinEnc = true;

                    // Attribution de la gare au joueur qui a enchéri la plus grosse somme
                    Console.Clear();
                    Console.WriteLine("============ {0} ============", Acheteur.Nom);
                    Console.WriteLine("\nFélicitations ! Vous avez remporté l’enchère");
                    Acheteur.RetraitArgent(Montant);
                    Acheteur.AjouterGare(this);
                    Proprietaire = Acheteur;
                }

                // Cas où aucun joueur parmi les joueurs ne veut enchérir
                else if (NbRefus == JS.Count && Montant == 0)
                {
                    // C’est la fin de l’enchère et on n’a pas de propriétaire pour cette gare
                    FinEnc = true;
                    Proprietaire = null;
                }
            }
        }

        // Méthode pour payer ou non le loyer de la gare
        public void Payer(Joueur J)
        {
            // Cas où le joueur n’est pas le propriétaire de la gare
            if (Proprietaire != J)
            {
                Console.WriteLine("\nCette gare appartient à " + Proprietaire.Nom);

                // Mouvement d’argent entre le propriétaire et le joueur, et vérification que le joueur n’a pas perdu
                Proprietaire.AjoutArgent(J.RetraitArgent(Loyer[Proprietaire.Gares.Count - 1]));
                if (J.Perdu) { J.TransfererPossessions(Proprietaire); }
            }

            // Cas où le joueur est le propriétaire de la gare
            else
            {
                Console.WriteLine("\nCette gare vous appartient");
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
                Console.WriteLine("\nVous n’avez pas tapé 1 ou 2\nVeuillez recommencer");
                return false;
            }
        }

        // Méthode pour sauvegarder les gares
        public void Sauver(XDocument Doc)
        {
            Doc.Root.Element("Plateau").Add(
                new XElement("Case",
                    new XAttribute("Type", "Gare"),
                    new XElement("NomCase", NomCase),
                    new XElement("Prix", Prix),
                    new XElement("LoyerGare",
                        new XElement("G1", Loyer[0]),
                        new XElement("G2", Loyer[1]),
                        new XElement("G3", Loyer[2]),
                        new XElement("G4", Loyer[3])
                        ),
                    new XElement("Hypotheque", Hypotheque),
                    new XElement("Proprietaire", (Proprietaire != null) ? Proprietaire.Nom : null)
                    ));
        }

        // Méthode pour modifier le propriétaire
        public void ModifierProprio(Joueur J)
        {
            Proprietaire = J;
        }
    }
}