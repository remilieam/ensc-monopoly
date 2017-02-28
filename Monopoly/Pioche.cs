using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Pioche
    {
        // Attributs
        public List<Carte> Piopioche { get; private set; }
        private int DessusPioche = 0;
        public Carte Memoire { get; private set; }

        // Constructeur de la pioche
        public Pioche(XDocument Doc)
        {
            // Récupération des cartes
            XElement Racine = Doc.Root;
            Piopioche = new List<Carte>();
            Piopioche = RecupCarte(Racine);
            Piopioche = MelangerCartes(Piopioche);
            Memoire = null;
        }

        // Méthode pour mélanger les cartes
        public List<Carte> MelangerCartes(List<Carte> ListeCartes)
        {
            Random Alea = new Random();
            List<int> ListeNbAlea = new List<int>();
            List<Carte> ListeCartesMelangees = new List<Carte>();

            // Tant qu’on n’a pas mis toutes les cartes de la pioche initiale dans la nouvelle pioche
            while (ListeCartesMelangees.Count != ListeCartes.Count)
            {
                bool Pris = false;
                int Num = Alea.Next(ListeCartes.Count);

                // Vérification que la carte n’a pas déjà été mise dans la nouvelle pioche
                for (int j = 0; j < ListeNbAlea.Count; j++)
                {
                    if (ListeNbAlea[j] == Num)
                    {
                        Pris = true;
                    }
                }

                // Transfert de la carte vers la nouvelle pioche
                if (!Pris)
                {
                    ListeCartesMelangees.Add(ListeCartes[Num]);
                    ListeNbAlea.Add(Num);
                }
            }

            return ListeCartesMelangees;
        }

        // Méthode pour piocher une carte
        public bool PiocherCarte(Joueur J, Case[] Plateau, List<Joueur> Joueurs)
        {
            // Carte tirée
            Carte Carte = Piopioche[DessusPioche % Piopioche.Count];
            Console.WriteLine(Carte);

            // Cas d’une carte à conserver
            if (Carte.Type == "Prison")
            {
                if (Plateau[J.Position].NomCase == "Chance")
                {
                    J.Chance = true;
                }
                else if (Plateau[J.Position].NomCase == "Caisse de Communauté")
                {
                    J.Communaute = true;
                }
                Memoire = Carte;
                Piopioche.Remove(Carte);
            }

            // Cas d’une carte de déplacement
            else if (Carte.Type == "Deplacement")
            {
                // Cas où l’on avance
                if (Carte.Position >= 0)
                {
                    J.AccesDirect(Plateau, Carte.Position);
                }

                // Cas où l’on recule
                else
                {
                    J.Deplacement(Plateau, Carte.Position);
                }
            }

            // Cas d’une carte de débit
            else if (Carte.Type == "Debit")
            {
                J.RetraitArgent(Carte.Montant1);
            }

            // Cas d’une carte de crédit
            else if (Carte.Type == "Credit")
            {
                J.AjoutArgent(Carte.Montant1);
            }

            // Cas d’une carte d’impôt sur les bâtiments
            if (Carte.Type == "TaxeBatiment")
            {
                int NbMaisons = 0;
                int NbHotels = 0;
                double Virement = 0;

                // Calcul du nombre de maisons et d’hôtels possédés par le joueur
                for (int i = 0; i < J.Terrains.Count; i++)
                {
                    NbMaisons += J.Terrains[i].NbMaisons;
                    if (J.Terrains[i].Hotel) { NbHotels += 1; }
                }

                Virement = NbHotels * Carte.Montant2 + NbMaisons * Carte.Montant1;

                if (Virement != 0)
                {
                    J.RetraitArgent(Virement);
                }
            }

            // Cas d’une carte anniversaire
            if (Carte.Type == "Anniversaire")
            {
                double Versement = 0;
                int i = 0;

                while (i < Joueurs.Count && J != Joueurs[i])
                {
                    Versement += Joueurs[i].RetraitArgent(Carte.Montant1);
                    if (Joueurs[i].Perdu) { Joueurs[i].TransfererPossessions(J); }
                    i += 1;
                }

                J.AjoutArgent(Versement);
            }

            // Cas d’une carte alternative
            if (Carte.Type == "AlternativeChance")
            {
                bool Entier = false;
                int Rep = 0;

                while (!Entier)
                {
                    Console.WriteLine("\nTapez 1 pour payer l’amende");
                    Console.WriteLine("Tapez 2 pour tirer une carte Chance");
                    Console.Write("Votre réponse : ");
                    try
                    {
                        Rep = int.Parse(Console.ReadLine());
                        if (Rep == 1 || Rep == 2)
                        {
                            Entier = true;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("\nVous n’avez pas tapé 1 ou 2\nVeuillez recommencer");
                    }
                }

                if (Rep == 1)
                {
                    J.RetraitArgent(Carte.Montant1);
                }

                else if (Rep == 2)
                {
                    return true;
                }
            }

            DessusPioche += 1;
            return false;
        }

        // Méthode pour récupérer les cartes
        public List<Carte> RecupCarte(XElement racine)
        {
            List<Carte> ListeCartes = (from c in racine.Descendants("Carte")
                                       select new Carte(
                                            (string)c.Element("Type"),
                                            (string)c.Element("Description"),
                                            (c.Element("Montant1") != null) ? (double)c.Element("Montant1") : 0,
                                            (c.Element("Montant2") != null) ? (double)c.Element("Montant2") : 0,
                                            (c.Element("Position") != null) ? (int)c.Element("Position") : 0)).ToList<Carte>();

            return ListeCartes;
        }

        // Méthode pour ajouter une carte
        public void AjouterCarte(Carte C)
        {
            Piopioche.Add(C);
        }

        // Méthode pour retirer une carte
        public void RetirerCarte(Carte C)
        {
            Memoire = C;
            Piopioche.Remove(C);
        }
    }
}