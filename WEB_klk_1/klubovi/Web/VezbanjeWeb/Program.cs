using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using VezbanjeWeb;

namespace Httpd
{
    class Program
    {
        public static int bod = -1;
        public static List<Klub> klubovi = new List<Klub>();

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
            //string userAgent = HttpContext.Current.Request.UserAgent;
            if (resource != null)
            {
                if (resource.Equals(""))
                    resource = "index.html";

                Console.WriteLine("Request from " + socket.RemoteEndPoint + ": "
                        + resource + "\n");


                //if (userAgent.Contains("Trident") || userAgent.Contains("MSIE"))
                //{
                //    Console.WriteLine("Evidencija klubova nije moguća iz Internet Explorer web browsera.");
                //    Console.WriteLine("Molimo koristite drugi web pregledač za dodavanje klubova.");
                //}
                if (resource.Contains("add?naziv="))
                {
                    string[] klub = resource.Split(new string[] { "naziv=", "gradovi=", "aktivan=" }, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var naziv = GetPropertyValue(klub[1]);
                    var grad = GetPropertyValue(klub[2]);
                    var aktivanStr = "off";
                    try
                    {
                        aktivanStr = GetPropertyValue(klub[3]);
                    }catch (Exception e) { }
                    
                    var aktivan = false;
                    if (aktivanStr == "on")
                        aktivan = true;
                    var bodovi = 0;

                    if (bod != -1)
                    {
                        bodovi = bod;
                    }

                    Console.WriteLine($"KLUB: naziv: {naziv}, grad: {grad}, bodovi: {bodovi} aktivan: {aktivan}");

                    sw.Write("<html><body>");
                    if (String.IsNullOrEmpty(naziv))
                    {
                        sw.WriteLine(GetAllUsers());
                    }
                    else
                    {
                        if (klubovi.Contains(new Klub { Ime = naziv }))
                        {
                            sw.Write($"<h1>Klub with:{naziv} already exists.</h1>");
                        }
                        else
                        {
                            klubovi.Add(new Klub { Ime = naziv, Grad = grad, BrojBodova = bodovi, Aktivan = aktivan });
                            //sw.Write($"<h1>Successfully added: {naziv}</h1>");
                            sw.Write("<h1 style=\"color:blue\">Tabela</h1>");
                            sw.WriteLine(GetAllUsers());
                        }
                    }
                    sw.WriteLine("<a href=\"/index.html\">Dodaj novi klub</a><br/><br/>");
                    sw.WriteLine("<a href=\"/vodeciKlub\">Prikazi vodeci klub</a>");
                    sw.WriteLine("<h1 style=\"color:blue\">Unesite bodove</h1>");

                    sw.WriteLine("<form accept-charset=\"UTF - 8\" action=\"http://localhost:8080/add\">");
                    sw.WriteLine("<table><tr><td>Klub </td> <td>");


                    sw.WriteLine("<select name=\"klub\">");

                    foreach (Klub klub1 in klubovi)
                    {
                        sw.WriteLine("<option value=\"" + klub1.Ime + "\">" + klub1.Ime + "</option>");
                    }

                    sw.WriteLine("<br/></select></td></tr><tr><td>Bodovi</td><td><input type=\"number\" name=\"bod\"></td>");
                    sw.WriteLine("<tr><td></td><td><input type=\"submit\" value=\"Unesi\" /></td></tr></table></form>");
                    sw.WriteLine("</body></html>");
                }
                else if (resource.Contains("add?klub="))
                {
                    string[] klub = resource.Split(new string[] { "klub=", "bod="}, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var imeKluba = GetPropertyValue(klub[1]);
                    var bodovi = GetPropertyValue(klub[2]);

                    Console.WriteLine($"KLUB: {imeKluba}, bodovi: {bodovi}");

                    sw.Write("<html><body>");
                    if (String.IsNullOrEmpty(imeKluba))
                    {
                        sw.WriteLine(GetAllUsers());
                    }
                    else
                    {
                        foreach(Klub klub1 in klubovi)
                        {
                            if(imeKluba.Equals(klub1.Ime.ToString()))
                            {
                                klub1.BrojBodova = Int32.Parse(bodovi);
                                sw.WriteLine("<h1>Uspesno uneti bodovi</h1>");
                                break;
                            }
                        }
                    }
                    sw.WriteLine(GetAllUsers());
                    sw.WriteLine("<a href=\"/index.html\">Dodaj novi klub</a><br/><br/>");
                    sw.WriteLine("<a href=\"/vodeciKlub\">Prikazi vodeci klub</a>");
                    sw.WriteLine("<h1>Unesite bodove</h1>");

                    sw.WriteLine("<form accept-charset=\"UTF - 8\" action=\"http://localhost:8080/add\">");
                    sw.WriteLine("<table><tr><td>Klub </td> <td>");


                    sw.WriteLine("<select name=\"klub\">");

                    foreach (Klub klub1 in klubovi)
                    {
                        sw.WriteLine("<option value=\"" + klub1.Ime + "\">" + klub1.Ime + "</option>");
                    }

                    sw.WriteLine("<br/></select></td></tr><tr><td>Bodovi</td><td><input type=\"number\" name=\"bod\"></td>");
                    sw.WriteLine("<tr><td></td><td><input type=\"submit\" value=\"Unesi\" /></td></tr></table></form>");
                    sw.WriteLine("</body></html>");
                }
                else if (resource.Contains("izmeniPodatke?"))
                {
                    string[] klub = resource.Split(new string[] { "naziv=", "gradovi=", "aktivan=", "bodovi=" }, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var naziv = GetPropertyValue(klub[1]);
                    var grad = GetPropertyValue(klub[2]);
                    var aktivan = GetPropertyValue(klub[3]);
                    var check = "";
                    if(aktivan.Equals("True"))
                    {
                        check = "checked";
                    }
                    var bodovi = GetPropertyValue(klub[4]);

                    bod = Int32.Parse(bodovi);


                    Console.WriteLine($"KLUB: naziv: {naziv}, grad: {grad}, bodovi: {bodovi} aktivan: {aktivan}");

                    var index = 0;
                    foreach (Klub klub1 in klubovi)
                    {
                        if(klub1.Ime.Equals(naziv))
                        {
                            break;
                        }
                        index++;
                    }
                    klubovi.RemoveAt(index);

                    sw.WriteLine("<html><body>");

                    sw.WriteLine("<h1 style=\"color:green\">Izmena podataka</h1>");
                    sw.WriteLine("<form accept-charset=\"UTF-8\" action=\"http://localhost:8080/add\"><table><tr><td>Naziv:</td>" +
                        "<td><input value=\"" + naziv + "\" type=\"text\" name=\"naziv\"></td></tr><tr><td>Grad:</td><td><select value=\"" + grad + "\" name=\"gradovi\"><option value=\"Novi Sad\">Novi Sad</option>" +
                        "<option value=\"Beograd\">Beograd</option><option value=\"Nis\">Nis</option></select></td></tr><tr><td>Aktivan:</td><td><input " + check + " type=\"checkbox\" name=\"aktivan\"></td>" +
                        "</tr><tr><td></td><td><input type=\"submit\" value=\"Izmeni\" /></td></tr></table></form>");


                    sw.WriteLine("</body></html>");
                }
                else if(resource.Contains("vodeciKlub"))
                {
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var vodeciBod = -1;
                    var imeKluba = "Nema klubova";
                    foreach(Klub klub in klubovi)
                    {
                        if(vodeciBod < klub.BrojBodova)
                        {
                            vodeciBod = klub.BrojBodova;
                            imeKluba = klub.Ime;
                        }
                    }

                    sw.WriteLine("<html><body>");
                    sw.WriteLine("<p>Trenutno vodeci klub: <b>" + imeKluba + "</b></p>");
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
            //return 0;
        }

        private static string GetPropertyValue(string field)
        {
            var newField = field.Split('&')[0];
            newField = Uri.UnescapeDataString(newField);
            newField = newField.Replace("+", " ");

            return newField;
        }

        private static string GetAllUsers()
        {
            int br = 1;
            string result = "<table border=\"1\">";

            if (klubovi.Count == 0)
            {
                result = "<h3> List is empty! </h3>";
                return result;

            }
            result += "<tr align=\"center\">" + "<td>#</td>"
                                 + "<td>Klub</td>"
                                 + "<td>Bodovi</td>"
                                 + "<td>Akcije</td>"
                        + "</tr>";
            foreach (Klub klub in klubovi)
            {
                //result += "<li>" + klub.Ime + " " + klub.Grad + " " + klub.Aktivan + "</li>\n";
                
                result += "<tr align=\"center\">" + "<td>" + br++.ToString() + "</td>"
                                 + "<td>" + klub.Ime + "</td>"
                                 + "<td>" + klub.BrojBodova + "</td>"
                                 + "<td><a href=\"/izmeniPodatke?naziv=" + klub.Ime + "&gradovi=" + klub.Grad + "&aktivan=" + klub.Aktivan + "&bodovi=" + klub.BrojBodova + "\">Izmeni podatke</a><br/><br/></td>"
                        + "</tr>";
            }

            result += "</table><br/>";

            return result;
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
