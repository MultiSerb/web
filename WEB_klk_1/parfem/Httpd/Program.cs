using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Httpd
{
    class Program
    {
        public static List<Parfem> parfemi = new List<Parfem>();
        public static List<Parfem> parfemiFind = new List<Parfem>();
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
            if (resource != null)
            {
                if (resource.Equals(""))
                    resource = "index.html";

                Console.WriteLine("Request from " + socket.RemoteEndPoint + ": "
                        + resource + "\n");

                // TODO: Add your code here...
                if (resource.StartsWith("index.html"))
                {
                    SendResponse(resource, socket, sw);
                }
                else if (resource.StartsWith("dodaj?id="))
                {
                    string[] user = resource.Split(new string[] { "id=", "naziv=", "nota=", "cena=", "akcija=" }, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var id = GetPropertyValue(user[1]);
                    var naziv = GetPropertyValue(user[2]);
                    var nota = GetPropertyValue(user[3]);
                    int cena = Int32.Parse(GetPropertyValue(user[4]));
                    var akcija = "";

                    if(user.Length < 5)
                    {
                        akcija = GetPropertyValue(user[5]);
                    }

                    if (resource.Contains("akcija"))
                    {
                        akcija = "Da";
                    }
                    else
                    {
                        akcija = "Ne";
                    }

                    sw.Write("<html><body>");
                    if (String.IsNullOrEmpty(id))
                    {
                        sw.Write(Tabela());
                    }
                    else
                    {
                        bool nadjen = false;
                        foreach(Parfem p in parfemi)
                        {
                            if (p.Id.Equals(id))
                            {
                                sw.Write($"Parfem sa ID-om: {id} vec postoji.");
                                nadjen = true;
                                break;
                            }
                        }
                        if (nadjen == false)
                        {
                            parfemi.Add(new Parfem { Id = id, Naziv = naziv, Nota = nota, Cena = cena, Akcija=akcija });
                            
                        }
                        sw.Write(Tabela());
                      //  sw.Write(Lista());

                    }
                    sw.Write("</body></html>");
                    
                }
                else if (resource.Contains("find?"))
                {
                    parfemiFind.Clear();
                    string[] rez = resource.Split(new string[] { "cena=" }, StringSplitOptions.None);
                    int cena = Int32.Parse(GetPropertyValue(rez[1]));

                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);
                    sw.Write("<html><body>");

                    if(parfemi.Count == 0)
                    {
                        sw.Write("<h3>Niste uneli nista</h3>");
                    }
                    else
                    {
                        foreach (Parfem p in parfemi)
                        {
                            if (p.Cena <= cena)
                            {
                                parfemiFind.Add(p);
                            }

                        }
                        if(parfemiFind.Count <= 0)
                        {
                            sw.Write($"<h3>Parfem sa manjom cenom od {cena} ne postoji</h3>");
                        }else
                        {
                            sw.Write($"<h3>Pronadjen/i parfem/i sa nizom cenom od {cena}</h3>");
                            sw.Write(TabelaCena());
                        }
                    }
                    sw.Write("</body></html>");
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
            //return 0;
        }

        private static string GetPropertyValue(string field)
        {
            var newField = field.Split('&')[0];
            newField = Uri.UnescapeDataString(newField);
            newField = newField.Replace("+", " ");

            return newField;
        }

        private static string Tabela()
        {
            string result = "";
            result += "<table border=\"2\">";

            result += "<tr ><th  colspan=\"5\" >Parfemi</th></tr>";
            result += "<tr><th>Id</th><th>Naziv</th><th>Mirisna nota</th><th>Cena</th><th>Na akciji?</th></tr>";

            //int brojac = 1;
            foreach(Parfem p in parfemi)
            {
                result += $"<tr> <td>{p.Id}</td> <td>{p.Naziv}</td> <td>{p.Nota}</td> <td>{p.Cena}</td> <td>{p.Akcija}</td> </tr>";
            }
            result += "</table>";
            result += "<a href=\"/index.html\">Nazad</a>";

            return result;
        }

        private static string TabelaCena()
        {
            string result = "";
            result += "<table border=\"2\">";

            result += "<tr ><th  colspan=\"5\" >Parfemi</th></tr>";
            result += "<tr><th>Id</th><th>Naziv</th><th>Mirisna nota</th><th>Cena</th><th>Na akciji?</th></tr>";

            //int brojac = 1;
            foreach (Parfem p in parfemiFind)
            {
                result += $"<tr> <td>{p.Id}</td> <td>{p.Naziv}</td> <td>{p.Nota}</td> <td>{p.Cena}</td> <td>{p.Akcija}</td> </tr>";
            }
            result += "</table>";
            result += "<a href=\"/index.html\">Nazad</a>";

            return result;
        }

        private static string Lista()
        {
            string rez = "";
            rez += "<ol>";

            foreach(Parfem p in parfemi)
            {
                rez += $"<li> {p.Naziv} {p.Cena} </li>";
            }
            rez += "</ol>";
            rez += "<a href=\"/index.html\">Nazad</a>";

            return rez;
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
            responseText = "HTTP/1.0 200 OK\r\nContent-type: text/html; charset=UTF-8\r\n\r\n";
            sw.Write(responseText);

            // a, zatim datoteku
            socket.SendFile(resource);
        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
