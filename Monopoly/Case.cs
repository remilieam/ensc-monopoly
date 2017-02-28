using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Monopoly
{
    class Case
    {
        // Attributs
        public readonly string NomCase;
        public readonly double Prix;

        // Constructeur complet
        public Case(string nom, double prix)
        {
            NomCase = nom;
            Prix = prix;
        }

        // Méthode pour sauvegarder les cases
        public void SauverCase(XDocument Doc)
        {
            Doc.Root.Element("Plateau").Add(
                new XElement("Case",
                    new XAttribute("Type", "Case"),
                    new XElement("NomCase", NomCase),
                    new XElement("Prix", Prix)
                    ));
        }
    }
}