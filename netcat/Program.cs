using System;
using System.IO;
using System.Net.Sockets;

namespace netcat
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Help();
                return;
            }

            if (Console.IsInputRedirected == false)
            {
                Console.WriteLine("No input redirected");
                Help();
                return;
            }

            int port = int.Parse(args[1]);

            using (TcpClient client = new TcpClient(args[0], port))
            {
                using (NetworkStream ns = client.GetStream())
                using (BinaryReader br = new BinaryReader(Console.OpenStandardInput()))
                {
                    while (true)
                    {
                        byte[] buffer = br.ReadBytes(512);
                        if (buffer == null || buffer.Length == 0)
                        {
                            break;
                        }

                        ns.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"(redirect_source) | {appName} [ip] [port]");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\techo \"Hello World\" | {appName} localhost 9900");
        }
    }
}


