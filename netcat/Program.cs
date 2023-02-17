using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace netcat
{

    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("netcat [-u -C] [-l -k] [address] [port]");
                Console.WriteLine(" -u UDP mode");
                Console.WriteLine(" -C print to console");
                Console.WriteLine(" -l Listen mode, for inbound connects");
                Console.WriteLine(" -k Keep inbound sockets open for multiple connects");
                return;
            }

            NetCatSettings settings = new NetCatSettings();
            args = NetCatSettings.ParseOptions(args, settings);

            if (settings.ListenMode)
            {
                Listen(settings, args);
            }
            else
            {
                RedirectStream(settings, args);
            }
        }

        private static void RedirectStream(NetCatSettings settings, string[] args)
        {
            if (Console.IsInputRedirected == false)
            {
                if (Console.IsErrorRedirected == true)
                {
                    settings.Source = RedirectSource.StandardError;
                }
                else
                {
                    Console.WriteLine("No input redirected");
                    return;
                }
            }

            string address = args[0];
            int port = int.Parse(args[1]);

            INetWorkWriter writer = null;

            if (settings.UdpMode)
            {
                writer = new UdpWrapper(address, port);
            }
            else
            {
                writer = new TcpWrapper(address, port);
            }

            using (writer)
            using (StreamReader sr = OpenRedirectInput(settings))
            {
                while (true)
                {
                    string text = sr.ReadLine();
                    if (text == null)
                    {
                        break;
                    }

                    if (text == "")
                    {
                        continue;
                    }

                    if (settings.PrintToConsole)
                    {
                        Console.WriteLine(text);
                    }

                    byte[] buffer = Encoding.UTF8.GetBytes(text);
                    writer?.Write(buffer);
                }
            }
        }

        private static void Listen(NetCatSettings settings, string[] args)
        {
            string bindingAddress = "0.0.0.0";
            int port = 0;
            if (args.Length == 1)
            {
                port = int.Parse(args[0]);
            }
            else if (args.Length == 2)
            {
                bindingAddress = args[0];
                port = int.Parse(args[1]);
            }
            else
            {
                Console.WriteLine("Invalid -l options");
                Console.WriteLine("netcat -l [binding_address] [port]");
                Console.WriteLine("netcat -l [port]");
                return;
            }

            if (settings.UdpMode)
            {
                Console.WriteLine("Not support for -l option with -u");
                return;
            }

            TcpListener listener = new TcpListener(System.Net.IPAddress.Parse(bindingAddress), port);
            listener.Start();

            do
            {
                TcpClient client = listener.AcceptTcpClient();

                if (settings.KeepAccept)
                {
                    Task.Run(() =>
                    {
                        ReadClientStream(client);
                    });
                }
                else
                {
                    ReadClientStream(client);
                }

            } while (settings.KeepAccept == true);

            listener.Stop();
        }

        private static void ReadClientStream(TcpClient client)
        {
            using (client)
            using (NetworkStream ns = client.GetStream())
            using (StreamReader sr = new StreamReader(ns))
            {
                while (true)
                {
                    try
                    {
                        int data = sr.Read();
                        if (data < 0)
                        {
                            break;
                        }

                        char ch = (char)data;
                        if (char.IsLetterOrDigit(ch) == true)
                        {
                            Console.Write(ch);
                        }
                        else
                        {
                            Console.Write('.');
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                }
            }
        }

        private static StreamReader OpenRedirectInput(NetCatSettings settings)
        {
            Stream source = null;

            switch (settings.Source)
            {
                case RedirectSource.StandardError:
                    source = Console.OpenStandardError();
                    break;

                default:
                    source = Console.OpenStandardInput();
                    break;
            }

            return new StreamReader(source);
        }
    }
}


