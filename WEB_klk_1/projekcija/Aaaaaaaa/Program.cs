using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace Aaaaaaaa
{
    internal class Program
    {
        private static List<Projekcija> projekcije = new List<Projekcija>();
        public static void StartListening()
        {

            IPAddress ipAddress = IPAddress.Loopback;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);

            // Create a TCP/IP socket.
            Socket serverSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket socket = serverSocket.Accept();

                    Task t = Task.Factory.StartNew(() => Run(socket));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void Run(Socket socket)
        {
            NetworkStream stream = new NetworkStream(socket);
            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream) { NewLine = "\r\n", AutoFlush = true };

            string resource = GetResource(sr);

            if(resource!=null)
            {
                if (resource.Equals(""))
                    resource = "index.html";

                Console.WriteLine("Request from " + socket.RemoteEndPoint + ": " + resource + "\n");

                if (resource.Contains("table?naziv="))
                {
                    string[] tokens = resource.Split(new string[] { "naziv=", "zanr=", "sala=", "datum=", "cena=" }, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var naziv = GetPropertyValue(tokens[1]);
                    var zanr = GetPropertyValue(tokens[2]);
                    var sala = GetPropertyValue(tokens[3]);
                    var datum = GetPropertyValue(tokens[4]);
                    var cena = GetPropertyValue(tokens[5]);

                    sw.Write("<html><body>");
                    if (String.IsNullOrEmpty(naziv))
                    {
                        sw.WriteLine(GetAllProjekcije());
                    }
                    else
                    {
                        if (projekcije.Contains(new Projekcija { Naziv=naziv}))
                        {
                            sw.Write($"<h1>Projekcija {naziv} vec postoji</h1>");
                        }
                        else
                        {
                            int parsedCena;
                            if(!int.TryParse(cena, out parsedCena) || parsedCena<0)
                            {
                                sw.Write("<h1>Uneta cena nije validna!</h1>");
                            }
                            else
                            {
                                projekcije.Add(new Projekcija { Naziv = naziv, Zanr = zanr, Sala = sala, Datum = datum, Cena = parsedCena });
                                sw.Write($"<h1>Successfully added {naziv}!</h1>");
                                sw.WriteLine(GetAllProjekcije());
                            }
                        }
                        sw.WriteLine("<a href=\"/index.html\">Home</a>");
                        sw.WriteLine("</body></html>");
                    }
                }
                else if(resource.Contains("table"))
                {
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    sw.Write("<html><body>");
                    sw.WriteLine(GetAllProjekcije());
                    sw.WriteLine("<a href=\"/index.html\">Home</a>");
                    sw.WriteLine("</body></html>");
                }
                else
                {
                    SendResponse(resource, socket, sw);
                }
            }
            sr.Close();
            sw.Close();
            stream.Close();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static string GetAllProjekcije()
        {
            string result = "<table border=\"1px\">\r\n <tr>\r\n <th>Naziv</th>\r\n <th>Zanr</th>\r\n <th>Sala</th>\r\n <th>Datum</th>\r\n <th>Cena</th>\r\n </tr>";

            if(projekcije.Count==0)
            {
                result = "<h1>Sebastijan</h1>";
                return result;
            }
            foreach(Projekcija p in projekcije)
            {
                result += "<tr><td>" + p.Naziv + "</td><td>" + p.Zanr + "</td><td>" + p.Sala + "</td><td>" + p.Datum + "</td><td>" + p.Cena + "</td></tr>";
            }
            result += "</table>";
            return result;
        }
        private static string GetPropertyValue(string field)
        {
            var newField = field.Split('&')[0];
            newField = Uri.UnescapeDataString(newField);            //Dekordira URL-kodirane karaktere u newField stringu (%20 u prazan string)
            newField = newField.Replace("+", " ");

            return newField;
        }
        private static string GetResource(StreamReader sr)
        {
            string line = sr.ReadLine();

            if (line == null)
                return null;

            String[] tokens = line.Split(' ');

            // prva linija HTTP zahteva: METOD /resurs HTTP/verzija
            // obradjujemo samo GET metodu
            string method = tokens[0];
            if (!method.Equals("GET"))
            {
                return null;
            }

            string rsrc = tokens[1];

            // izbacimo znak '/' sa pocetka
            rsrc = rsrc.Substring(1);

            // ignorisemo ostatak zaglavlja
            string s1;
            while (!(s1 = sr.ReadLine()).Equals(""))
                Console.WriteLine(s1);
            Console.WriteLine("Request: " + line);
            return rsrc;
        }

        private static void SendResponse(string resource, Socket socket, StreamWriter sw)
        {
            // ako u resource-u imamo bilo šta što nije slovo ili cifra, možemo da
            // konvertujemo u "normalan" oblik
            //resource = Uri.UnescapeDataString(resource);

            // pripremimo putanju do našeg web root-a
            resource = "../../../" + resource;
            FileInfo fi = new FileInfo(resource);

            string responseText;
            if (!fi.Exists)
            {
                // ako datoteka ne postoji, vratimo kod za gresku
                responseText = "HTTP/1.0 404 File not found\r\n"
                        + "Content-type: text/html; charset=UTF-8\r\n\r\n<b>404 Нисам нашао:"
                        + fi.Name + "</b>";
                sw.Write(responseText);
                Console.WriteLine("Could not find resource: " + fi.Name);
                return;
            }

            // ispisemo zaglavlje HTTP odgovora
            responseText = "HTTP/1.0 200 OK\r\nContent-type: text/html; charset=UTF-8\r\n\r\n";     //HTML tagovi se koriste samo za formatiranje i strukturiranje
                                                                                                    //sadržaja koji se prikazuje u pregledaču, dok se HTTP zaglavlje koristi
                                                                                                    //za komunikaciju između servera i klijenta. Zbog toga su HTML tagovi
                                                                                                    //relevantni samo u delu odgovora koji sadrži sam HTML sadržaj, a ne u HTTP
                                                                                                    //zaglavlju.
            sw.Write(responseText);

            // a, zatim datoteku
            socket.SendFile(resource);
        }
        static void Main(string[] args)
        {
            StartListening();
        }
    }
}
