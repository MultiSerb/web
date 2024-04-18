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
        public static List<User> users = new List<User>();

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

                if (resource.Contains("add?username="))
                {
                    string[] user = resource.Split(new string[] { "username=", "name=", "lastname=" }, StringSplitOptions.None);
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);

                    var username = GetPropertyValue(user[1]);
                    var name = GetPropertyValue(user[2]);
                    var lastname = GetPropertyValue(user[3]);

                    Console.WriteLine($"Found username: {username}, Name: {name}, Lastname: {lastname}");

                    sw.Write("<html><body>");
                    if (String.IsNullOrEmpty(username))
                    {
                        sw.WriteLine(GetAllUsers());
                    }
                    else
                    {
                        if (users.Contains(new User { Username = username }))
                        {
                            sw.Write($"<h1>User with:{username} already exists.</h1>");
                        }
                        else
                        {
                            users.Add(new User { Username = username, Name = name, Lastname = lastname });
                            sw.Write($"<h1>Successfully added: {username}</h1>");
                            sw.WriteLine(GetAllUsers());
                        }
                    }
                    sw.WriteLine("<a href=\"/index.html\">Home</a>");
                    sw.WriteLine("</body></html>");
                }
                else if (resource.Contains("find?username="))
                {
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);
                    sw.Write("<html><body>");

                    string[] user = resource.Split(new string[] { "username=" }, StringSplitOptions.None);
                    var username = GetPropertyValue(user[1]);

                    if (String.IsNullOrEmpty(username))
                    {
                        sw.WriteLine(GetAllUsers());
                    }
                    else
                    {
                        if (users.Contains(new User { Username = username }))
                        {
                            User findUser = users.Find(u => u.Equals(username));
                            sw.Write($"<h1>User with:{username} exists.</h1>");
                            sw.Write($"<p>Username:{findUser.Username} Name: {findUser.Name} Lastname:{findUser.Lastname}.</p>");
                        }
                        else
                        {
                            sw.Write($"<h1>User with:{username} does not exist.</h1>");
                        }
                    }
                    sw.WriteLine("<a href=\"/index.html\">Home</a>");
                    sw.WriteLine("</body></html>");
                }
                else if (resource.Contains("delete?username="))
                {
                    string responseText = "HTTP/1.0 200 OK\r\n\r\n";
                    sw.Write(responseText);
                    sw.Write("<html><body>");

                    string[] user = resource.Split(new string[] { "username=" }, StringSplitOptions.None);
                    var username = GetPropertyValue(user[1]);

                    if (String.IsNullOrEmpty(username))
                    {
                        sw.WriteLine(GetAllUsers());
                    }
                    else
                    {
                        if (users.Contains(new User { Username = username }))
                        {
                            users.RemoveAll(u=>u.Equals(username));
                            sw.Write($"<h1>User {username} removed.</h1>");
                        }
                        else
                        {
                            sw.Write($"<h1>User with:{username} does not exist.</h1>");
                        }
                    }

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

            string result = "<ol>";

            if (users.Count == 0)
            {
                result = "<h3> List is empty! </h3>";
                return result;

            }
            foreach (User user in users)
            {
                result += "<li>" + user.Username + "</li>\n";
            }

            result += "</ol>";
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
