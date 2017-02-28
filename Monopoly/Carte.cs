using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Monopoly
{
    class Carte
    {
        // Attributs
        public readonly string Type;
        public readonly string Description;
        public readonly double Montant1;
        public readonly double Montant2;
        public readonly int Position;

        // Constructeur complet
        public Carte(string type, string description, double montant1, double montant2, int position)
        {
            Type = type;
            Description = description;
            Montant1 = montant1;
            Montant2 = montant2;
            Position = position;
        }

        // Affichage de la carte
        public override string ToString()
        {
            string chaine = string.Format("\nVoici la carte piochée : {0}", Description);
            return chaine;
        }
    }
}