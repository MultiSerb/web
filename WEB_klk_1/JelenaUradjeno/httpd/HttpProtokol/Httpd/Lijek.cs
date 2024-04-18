using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Httpd
{
    public class Lijek
    {
        private int id;
        private string name;
        private int cena;
        private int kolicina;
        private string tip;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int Cena { get => cena; set => cena = value; }
        public int Kolicina { get => kolicina; set => kolicina = value; }
        public string Tip { get => tip; set => tip = value; }
    }
}
