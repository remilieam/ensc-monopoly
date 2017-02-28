using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Program
    {
        // Main
        public static void Main(string[] args)
        {
            Console.WriteLine("Vous allez jouer au Monopoly");

            int Reponse = 0;
            bool Approprie = false;

            // Vérification que le joueur entre 1 ou 2
            while (!Approprie)
            {
                Console.WriteLine("\nPour commencer une nouvelle partie, tapez 1");
                Console.WriteLine("Pour reprendre une partie, tapez 2");
                Console.Write("Votre réponse : ");
                try
                {
                    Reponse = int.Parse(Console.ReadLine());
                    if (Reponse == 1 || Reponse == 2)
                    {
                        Approprie = true;
                    }
                }
                catch
                {
                    Console.WriteLine("\nVous n’avez pas tapé 1 ou 2\nVeuillez recommencer");
                }
            }

            // Cas où le joueur veut faire une nouvelle partie
            if (Reponse == 1)
            {
                bool Entier = false;
                int Rep = 0;

                // Vérification que le joueur entre un entier positif supérieur strictement à 1
                while (!Entier)
                {
                    Console.WriteLine("\nCombien de personnes veulent jouer ?");
                    Console.Write("Votre réponse : ");
                    try
                    {
                        Rep = int.Parse(Console.ReadLine());
                        if (Rep > 1)
                        {
                            Entier = true;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("\nVous n’avez pas tapé un entier\nVeuillez recommencer");
                    }
                }

                // Création d’une nouvelle partie
                Partie NouvellePartie = new Partie(Rep);
                Console.ReadKey();
                NouvellePartie.FairePartie();
            }

            // Cas où le joueur veut reprendre une partie
            else if (Reponse == 2)
            {
                bool Existe = false;
                string Nom = "";
                XDocument Fichier = null;

                // Vérification que le nom de la partie à reprendre existe
                while (!Existe)
                {
                    Console.WriteLine("\nEntrer le nom de la partie à récupérer");
                    Console.Write("Votre réponse : ");
                    try
                    {
                        Nom = "..\\..\\FichiersXML\\" + Console.ReadLine() + ".xml";
                        Fichier = XDocument.Load(Nom);
                        Existe = true;
                    }
                    catch
                    {
                        Console.WriteLine("\nLa partie que vous voulez reprendre n’existe pas");
                    }
                }

                // Récupération de la partie
                Partie ReprendrePartie = new Partie(Fichier);
                ReprendrePartie.FairePartie();
            }
        }
    }
}