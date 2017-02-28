using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Joueur
    {
        // Attributs
        public readonly string Nom;
        public double Argent { get; private set; }
        public int Position { get; private set; }
        public List<Terrain> Terrains { get; private set; }
        public List<Gare> Gares { get; private set; }
        public List<ServicePublic> Services { get; private set; }
        public List<string> Couleurs { get; private set; }
        public bool Perdu { get; set; } // Pour savoir si le joueur a perdu
        public bool Chance { get; set; } // Pour savoir si le joueur possède la carte Chance pour sortir de prison
        public bool Communaute { get; set; } // Pour savoir si le joueur possède la carte Caisse de Communauté pour sortir de prison
        public bool Prison { get; set; } // Pour savoir si le joueur est en prison
        public int Double { get; set; } // Pour savoir si le joueur a fait 3 doubles (ou doit sortir de prison)

        // Constructeur avec nom
        public Joueur(string nom)
        {
            Nom = nom;
            Argent = 1500;
            Position = 0;
            Terrains = new List<Terrain>();
            Gares = new List<Gare>();
            Services = new List<ServicePublic>();
            Couleurs = new List<string>();
            Perdu = false;
            Chance = false;
            Communaute = false;
            Prison = false;
            Double = 0;
        }

        // Constructeur complet
        public Joueur(string nom, double argent, int position, bool perdu, bool chance, bool communaute, bool prison, int ddouble)
        {
            Nom = nom;
            Argent = argent;
            Position = position;
            Perdu = perdu;
            Chance = chance;
            Communaute = communaute;
            Prison = prison;
            Double = ddouble;
            Terrains = new List<Terrain>();
            Gares = new List<Gare>();
            Services = new List<ServicePublic>();
            Couleurs = new List<string>();
        }

        // Méthode pour mettre à jour la liste des familles de couleurs qu’il possède
        public void MAJCouleur()
        {
            // Construction d’une liste avec les couleurs des terrains que possède le joueur
            List<string> ListeCouleur = new List<string>();
            for (int i = 0; i < Terrains.Count; i++)
            {
                // Booléen pour vérifier qu’on n’a pas déjà ajouté la couleur à la liste
                bool Deja = false;

                for (int j = 0; j < ListeCouleur.Count; j++)
                {
                    if (Terrains[i].Couleur == ListeCouleur[j])
                    {
                        Deja = true;
                    }
                }

                // Ajout de la couleur si on ne l’a pas déjà ajoutée
                if (!Deja)
                {
                    ListeCouleur.Add(Terrains[i].Couleur);
                }
            }

            // Construction de la liste des occurences des couleurs, associée à la liste précédente
            int[] Occurence = new int[ListeCouleur.Count];
            for (int i = 0; i < Terrains.Count; i++)
            {
                for (int j = 0; j < ListeCouleur.Count; j++)
                {
                    if (Terrains[i].Couleur == ListeCouleur[j])
                    {
                        Occurence[j] += 1;
                    }
                }
            }

            // Mise à jour de la liste des couleurs dont le joueur possède tous les terrains
            Couleurs = new List<string>();
            for (int i = 0; i < Occurence.Length; i++)
            {
                if (Occurence[i] == 3)
                {
                    Couleurs.Add(ListeCouleur[i]);
                }
                if (Occurence[i] == 2 && (ListeCouleur[i] == "Marron" || ListeCouleur[i] == "Bleu"))
                {
                    Couleurs.Add(ListeCouleur[i]);
                }
            }
        }

        // Méthode pour créditer de l’argent sur le compte du joueur
        public void AjoutArgent(double Somme)
        {
            Argent += Somme;
            Console.WriteLine("\nUn crédit de {0} EUR vient d’être effectué sur le compte de {1}", Somme, Nom);
            Console.WriteLine("Solde actuel du compte de {0} : {1} EUR", Nom, Argent);
        }

        // Méthode pour débiter de l’argent sur le compte du joueur
        public double RetraitArgent(double Somme)
        {
            Argent -= Somme;

            // Si la somme à débiter met le joueur dans le négatif
            if (Argent < 0)
            {
                // On lui débite que l’argent qu’il possède
                Somme = Somme + Argent;
                Argent = 0;
                Perdu = true;
            }

            Console.WriteLine("\nUn débit de {0} EUR vient d’être effectué sur le compte de {1}", Somme, Nom);
            Console.WriteLine("Solde actuel du compte de {0} : {1} EUR", Nom, Argent);

            if (Perdu)
            {
                Console.WriteLine("\n{0}, vous avez perdu !", Nom);
            }

            // On retourne la somme d’argent qu’on a pu débiter au joueur
            return Somme;
        }

        // Méthode pour déplacer le joueur d’un nombre de cases donné
        public void Deplacement(Case[] Plateau, int Valeur)
        {
            // Cas où le joueur passe par la case Départ
            if (Position + Valeur > Plateau.Length - 1)
            {
                Console.WriteLine("\nVous venez de passer par la case Départ");
                Position = Position + Valeur - Plateau.Length;
                AjoutArgent(200);
            }

            else
            {
                Position = Position + Valeur;
            }

            Console.WriteLine("\nVous êtes sur la case {0}", Plateau[Position].NomCase);
        }

        // Méthode pour accéder directement à une case donnée
        public void AccesDirect(Case[] Plateau, int NewPosition)
        {
            // Cas où le joueur passe par la case Départ
            if (Position > NewPosition && NewPosition != 10 && NewPosition != 1)
            {
                Console.WriteLine("\nVous venez de passer par la case Départ");
                AjoutArgent(200);
                Position = NewPosition;
            }

            // Cas où le joueur retourne à Belleville
            else if (NewPosition != 10)
            {
                Position = NewPosition;
            }

            // Cas où le joueur va directement en prison
            else if (NewPosition == 10)
            {
                Position = NewPosition;
                Prison = true;
                Double = 0;
            }

            Console.WriteLine("\nVous êtes sur la case {0}", Plateau[Position].NomCase);
        }

        // Méthode pour sauvegarder un joueur
        public void SauverJoueur(XDocument Doc)
        {
            Doc.Root.Element("Joueurs").Add(
                new XElement("Joueur",
                    new XElement("Nom", Nom),
                    new XElement("Argent", Argent),
                    new XElement("Position", Position),
                    new XElement("Perdu", Perdu),
                    new XElement("Chance", Chance),
                    new XElement("Communaute", Communaute),
                    new XElement("Prison", Prison),
                    new XElement("Double", Double)
                    ));
        }

        // Méthode pour ajouter une gare
        public void AjouterGare(Gare C)
        {
            Gares.Add(C);
        }

        // Méthode pour ajouter un service public
        public void AjouterService(ServicePublic C)
        {
            Services.Add(C);
        }

        // Méthode pour ajouter un terrain
        public void AjouterTerrain(Terrain C)
        {
            Terrains.Add(C);
        }

        // Méthode lorsque le joueur a perdu à cause d’un joueur
        public void TransfererPossessions(Joueur J)
        {
            Console.ReadKey();
            Console.WriteLine("\r" + " " + "\r");
            Console.Clear();
            Console.WriteLine("============ {0} ============", J.Nom);

            // Transfert des terrains
            for (int i = 0; i < Terrains.Count; i++)
            {
                Console.WriteLine("\nVous récupérez le terrain " + Terrains[i].NomCase);

                // Vente des maisons, si nécessaire
                if (Terrains[i].NbMaisons != 0)
                {
                    J.AjoutArgent(Terrains[i].NbMaisons * Terrains[i].PrixMaison / 2);
                }

                // Vente des hôtels, si nécessaire
                else if (Terrains[i].Hotel)
                {
                    J.AjoutArgent(Terrains[i].PrixHotel / 2 + 2 * Terrains[i].PrixMaison);
                }

                J.AjouterTerrain(Terrains[i]);
                Terrains[i].ModifierProprio(J);
            }

            // Transfert des gares
            for (int i = 0; i < Gares.Count; i++)
            {
                Console.WriteLine("\nVous récupérez la gare " + Gares[i].NomCase);

                J.AjouterGare(Gares[i]);
                Gares[i].ModifierProprio(J);
            }

            // Transfert des services publics
            for (int i = 0; i < Services.Count; i++)
            {
                Console.WriteLine("\nVous récupérez le service public " + Services[i].NomCase);

                J.AjouterService(Services[i]);
                Services[i].ModifierProprio(J);
            }

            // Transfert des cartes à conserver, si nécessaire
            if (Chance) { J.Chance = true; }
            if (Communaute) { J.Communaute = true; }

            // Mise à jour des couleurs pour le joueur qui récupère les terrains
            J.MAJCouleur();
        }

        // Méthode lorsque le joueur a perdu à cause de la banque
        public void RendrePossessions(Pioche[] P, List<Joueur> JS)
        {
            Console.ReadKey();
            Console.WriteLine("\r" + " " + "\r");

            // Composition de la liste des joueurs restants
            List<Joueur> JSEnc = new List<Joueur>();
            for (int i = 0; i < JS.Count; i++)
            {
                if (JS[i] != this)
                {
                    JSEnc.Add(JS[i]);
                }
            }

            // Enchère sur les terrains et retrait des maisons et hôtels
            for (int i = 0; i < Terrains.Count; i++)
            {
                Terrains[i].Enchere(JSEnc);
            }

            // Enchère sur les gares
            for (int i = 0; i < Gares.Count; i++)
            {
                Gares[i].Enchere(JSEnc);
            }

            // Enchère sur les terrains
            for (int i = 0; i < Services.Count; i++)
            {
                Services[i].Enchere(JSEnc);
            }

            // Rajout des cartes à conserver dans leur pioche respective, si nécessaire
            if (Chance) { P[0].AjouterCarte(P[0].Memoire); }
            if (Communaute) { P[1].AjouterCarte(P[1].Memoire); }
        }
    }
}