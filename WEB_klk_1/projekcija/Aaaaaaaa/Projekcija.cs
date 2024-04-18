using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aaaaaaaa
{
    class Projekcija
    {
        public string Naziv { get; set; }
        public string Zanr { get; set; }
        public string Sala { get; set; }
        public string Datum { get; set; }
        public int Cena { get; set; }

        public override bool Equals(object obj)
        {
            return obj.Equals(this.Naziv);
        }
    }
}
