using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ipscan
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Help();
                return;
            }

            string ip_template = args[0];
            int startIp = int.Parse(args[1]);
            int endIp = int.Parse(args[2]);

            List<Task> list = new List<Task>();
            List<IpName> nameList = new List<IpName>();

            for (int i = startIp; i <= endIp; i++)
            {
                string ipAddress = string.Format(ip_template, i);
                Task task = Task.Factory.StartNew(() =>
                {
                    IpName? name = GetPingName(ipAddress);
                    if (name != null)
                    {
                        lock (nameList)
                        {
                            nameList.Add(name.Value);
                        }
                    }
                });

                list.Add(task);
            }

            foreach (Task task in list)
            {
                task.Wait();
            }

            nameList.Sort(new IPv4AddressComparer());

            foreach (IpName txt in nameList)
            {
                Console.WriteLine(txt);
            }
        }

        public class IPv4AddressComparer : IComparer<IpName>
        {
            public int Compare(IpName x, IpName y)
            {
                uint thisIp = GetIp(x);
                uint thatIp = GetIp(y);

                if (thisIp == thatIp)
                {
                    return 0;
                }

                if (thisIp > thatIp)
                {
                    return 1;
                }

                return -1;
            }

            private uint GetIp(IpName item)
            {
                if (IPAddress.TryParse(item.IP, out IPAddress ipAddress) == true)
                {
                    return ToUint32(ipAddress);
                }

                return 0;
            }

            private uint ToUint32(IPAddress addr)
            {
                return BitConverter.ToUInt32(addr.GetAddressBytes().Reverse().ToArray(), 0);
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"{appName} [ip_template] [start] [end]");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\t{appName} 192.168.0.{{0}} 1 254");
        }

        private static IpName? GetPingName(string ipAddress)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "ping",
                Arguments = $"-a {ipAddress} -w 2000 -n 1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                string txt = process.StandardOutput.ReadToEnd();
                if (txt.IndexOf("TTL=") != -1)
                {
                    string startMark = "Pinging ";
                    string endMark = "with 32 bytes of data:";
                    int spos = txt.IndexOf(startMark);
                    int epos = txt.IndexOf(endMark);

                    if (spos != -1 && epos != -1)
                    {
                        string result = txt.Substring(spos + startMark.Length, epos - (spos + startMark.Length)).Trim();
                        string[] ip_name = result.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (ip_name.Length == 2)
                        {
                            return new IpName(ip_name[1].Trim('[', ']'), ip_name[0]);
                        }

                        return new IpName(result, "");
                    }

                    return null;
                }
            }

            return null;
        }

        public struct IpName
        {
            public string IP { get; private set; }
            public string Name { get; private set; }

            public IpName(string ip, string name)
            {
                IP = ip.Trim();
                Name = name.Trim();
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(Name) == true)
                {
                    return IP;
                }

                return $"{IP} {Name}";
            }
        }
    }
}