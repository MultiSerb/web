using System;
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
      public static  List<Lijek>lijekovi=new List<Lijek>();
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

                    Task<int> t = Task.Factory.StartNew(() => Run(socket));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static int Run(Socket socket)
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

                //if (resource.StartsWith("test"))
                //{
                //    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                //    sw.Write(responseText);

                //    sw.Write("<html><body>");
                //    sw.Write("<h1>Srecno sa izradom predispitne obaveze!</h1>");    
                //    sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                //    sw.WriteLine("</body></html>");
                //}

                if (resource.Contains("add?")) {

                    try
                    {
                        string[] user = resource.Split(new string[] { "id=", "naziv=", "cena=", "kolicina=", "tip=" }, StringSplitOptions.None);
                        string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                        sw.Write(responseText);



                        var id = GetPropertyValue(user[1]);
                        var naziv = GetPropertyValue(user[2]);
                        var cena = GetPropertyValue(user[3]);
                        var kolicina = GetPropertyValue(user[4]);
                        var tip = GetPropertyValue(user[5]);

                        Lijek lijek = new Lijek();

                        lijek.Id = int.Parse(id);
                        lijek.Name = naziv;
                        lijek.Cena = int.Parse(cena);
                        lijek.Kolicina = int.Parse(kolicina);
                        lijek.Tip = tip;

                        int b = 0;
                        foreach (Lijek l in lijekovi)
                        {
                            if (l.Id == int.Parse(id))
                            {
                                sw.Write("<html><body>");
                                sw.WriteLine("<h1 style=\"color: red;\">Lijek sa unijetim id=" + id + "vec postoji!</h1>");


                                sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");

                                sw.WriteLine("</body></html>");

                                b++;
                                break;
                            }

                        }

                        if (b == 0)
                        {
                            lijekovi.Add(lijek);

                            //sw.Write("<html><body>");
                            //sw.Write("<h1>Stefan</h1>");
                            //sw.Write("<p>"+id+"</p>");
                            //sw.Write("<p>"+naziv+"</p>");
                            //sw.Write("<p>"+cena+"</p>");
                            //sw.Write("<p>"+kolicina+"</p>");
                            //sw.Write("<p>"+tip+"</p>");

                            sw.Write("<html><body>");
                            sw.WriteLine("<h1>DA VIDIM GDJE JE</h1>");
                            sw.Write(GetAllUsers());

                            sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                            sw.WriteLine("<h1>DA VIDIM GDJE JE</h1>");
                            sw.WriteLine("</body></html>");
                        }

                    }catch (Exception ex)
                    {
                        sw.Write("<html><body>");
                        sw.WriteLine("<h1>DA VIDIM GDJE JE</h1>");
                        sw.Write(GetAllUsers());

                        sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                        sw.WriteLine("<h1>DA VIDIM GDJE JE</h1>");
                        sw.WriteLine("</body></html>");
                    }
                }else if (resource.Contains("pretrazi?tip="))
                {
                    string[] user = resource.Split(new string[] { "tip=" }, StringSplitOptions.None);
                    //string[] tipovi = resource.Split("pretrazi?tip=");
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var tip = GetPropertyValue(user[1]);
                    int b = 0;
                    List<Lijek>lijek=new List<Lijek>();
                    foreach (Lijek l in lijekovi)
                    {
                        if (l.Tip.Equals(tip))
                        {

                            b++;
                            lijek.Add(l);
                        }
                    }



                    if (b == 0)
                    {

                        sw.Write("<html><body>");
                        sw.Write("<h1>Nepostoji Ljjek zadatog tipa</h1>");
                        sw.WriteLine(tip);
                        sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                        sw.WriteLine("</body></html>");

                    }
                    else
                    {
                        sw.Write("<html><body>");
                        sw.Write(GetAllUsers2(lijek));
                        sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                        sw.WriteLine("</body></html>");
                    }
                    
                   

                }else if (resource.Contains("obrisi?"))
                {
                   string[] user = resource.Split(new string[] { "ime=" }, StringSplitOptions.None);
                    
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);
                    int b = 0;
                    var ime = user[1];
                    foreach (Lijek l in lijekovi)
                    {
                        if (l.Name.Equals(ime))
                        {
                            lijekovi.Remove(l);
                            b++;
                            break;
                        }
                    }

                    if (b != 0)
                    {
                        sw.Write("<html><body>");
                        sw.WriteLine("<p style=\"color: blue;\">Lijek " + ime + " je uspjesno obrisan </p>");
                        sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                        sw.WriteLine("</body></html>");
                    }
                    else
                    {
                        sw.Write("<html><body>");
                    sw.WriteLine("<p style=\"color: blue;\">Lijek "+ime+" ne postoji </p>");
                        sw.WriteLine("<a href=\"/index.html\">Pocetna stranica</a>");
                        sw.WriteLine("</body></html>");
                    }

                }
                // TODO: Write code here
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
            return 0;
        }


        private static string GetAllUsers2(List<Lijek>lijek)
        {

            string result = "<table border=\"3\"><th style=\"color: green;\" colspan=\"5\">Lijekovi</th>";
            result += "<tr style=\"font-weight: bold;\"><td>Id\t</td><td>Naziv\t</td><td>Cena\t</td><td>Kolicina\t</td><td>Tip\t</td> </tr>";

            if (lijekovi.Count == 0)
            {
                result = "<h3> List is empty! </h3>";
                return result;

            }
            foreach (Lijek user in lijek)
            {
                result += "<tr><td>" + user.Id + "\t</td><td>" + user.Name + "\t</td><td>" + user.Cena + "\t</td><td>" + user.Kolicina + "\t</td><td>" + user.Tip + "\t</td> </tr>";
            }

            result += "</table>";
            return result;



        }



        private static string GetAllUsers()
        {

            string result = "<table border=\"3\"><th style=\"color: green;\" colspan=\"5\">Lijekovi</th>";
            result += "<tr style=\"font-weight: bold;\"><td>Id\t</td><td>Naziv\t</td><td>Cena\t</td><td>Kolicina\t</td><td>Tip\t</td> </tr>";

            if (lijekovi.Count == 0)
            {
                result = "<h3> List is empty! </h3>";
                return result;

            }
            foreach (Lijek user in lijekovi)
            {
                result += "<tr><td>" + user.Id + "\t</td><td>" + user.Name + "\t</td><td>" + user.Cena + "\t</td><td>" + user.Kolicina + "\t</td><td>" + user.Tip + "\t</td> </tr>";
            }

            result += "</table>";
            return result;

           

        }

        private static string GetPropertyValue(string field)
        {
            var newField = field.Split('&')[0];
            newField = Uri.UnescapeDataString(newField);
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

            return rsrc;
        }

        private static void SendResponse(string resource, Socket socket, StreamWriter sw)
        {
            // zamenimo web separator sistemskim separatorom
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
            responseText = "HTTP/1.0 200 OK\r\n\r\n";
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
