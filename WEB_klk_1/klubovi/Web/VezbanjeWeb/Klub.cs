using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VezbanjeWeb
{
    public enum Gradovi {NoviSad, Beograd, Nis }
    public class Klub
    {
        public string Ime { get; set; }
        public string Grad { get; set; }
        public bool Aktivan { get; set; }
        private int brojBodova = 0; 
        public int BrojBodova { get { return brojBodova; } set { brojBodova = value; } }


        public override bool Equals(object obj)
        {
            return obj.Equals(this.Ime);
        }
    }
}
