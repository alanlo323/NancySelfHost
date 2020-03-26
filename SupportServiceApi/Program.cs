using System;
using System.Diagnostics;
using System.Security.Principal;
using Nancy;
using Nancy.Hosting.Self;

namespace SupportServiceApi
{
    class Program
    {
        static string IP = "localhost";
        static int PORT = 9527;

        static void Main(string[] args)
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (!isElevated)
            {
                // relaunch the application with admin rights
                string currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                currentPath += "\\SupportServiceApi.exe";
                var SelfProc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = currentPath,
                    Verb = "runas"
                };
                try
                {
                    Process.Start(SelfProc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(currentPath);
                    Console.WriteLine("Unable to elevate!");
                    Console.WriteLine(ex);
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }

                return;
            }
            else
            {
                string ip = IP;
                int port = PORT;
                try
                {
                    ip = args[0];
                    int.TryParse(args[1], out port);
                }
                catch
                {
                    ip = IP;
                    port = PORT;
                }
                try
                {
                    startHosting(ip, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }
        }

        private static void startHosting(string ip, int port)
        {
            string uri = "http://" + ip + ":" + port;
            using (var host = new NancyHost(
                new HostConfiguration()
                {
                    UrlReservations = new UrlReservations()
                    {
                        CreateAutomatically = true
                    }
                },
                new Uri(uri)))
            {
                host.Start();
                Console.WriteLine("Listening on " + uri);
                Console.WriteLine("Press Esc to stop...");
                do
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                } while (true);
                host.Stop();
            }
        }
    }

    public class GuidGeneratorModule : NancyModule
    {
        public GuidGeneratorModule()
        {
            Get("/", (x) =>
            {
                var dynamicDictionary = (DynamicDictionary)x;
                //string.Concat("Hello ", x.name);
                foreach (var value in dynamicDictionary)
                {
                    Console.WriteLine(value);
                }
                string guid = Guid.NewGuid().ToString();
                Console.WriteLine("Generated Guid:" + guid);
                return guid;
            });
            Get("/{id}", (x) =>
            {
                var dynamicDictionary = (DynamicDictionary)x;
                string guid = Guid.NewGuid().ToString();
                Console.WriteLine("Serving " + x.id + " Generated Guid:" + guid);
                return guid;
            });
        }
    }
}
