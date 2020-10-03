using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace wol
{
    class Program
    {
        const int WOL_PACKET_LEN = 102;

        // Wake-on-Lan (WoL) in C#
        // https://www.fluxbytes.com/csharp/wake-lan-wol-c/
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Help();
                return;
            }

            byte[] wolBuffer = GetWolPacket(args[0]);
            if (wolBuffer == null || wolBuffer.Length != WOL_PACKET_LEN)
            {
                Help();
                return;
            }

            UdpClient udp = new UdpClient() { EnableBroadcast = true };

            foreach (IPAddress ipAddress in GetDirectedBroadcastAddresses())
            {
                Send(udp, ipAddress.ToString(), 7, wolBuffer);
                Send(udp, ipAddress.ToString(), 9, wolBuffer);
            }

            Send(udp, "255.255.255.255", 7, wolBuffer);
            Send(udp, "255.255.255.255", 9, wolBuffer);
        }

        static void Send(UdpClient udp, string address, int port, byte[] buffer)
        {
            udp.Send(buffer, buffer.Length, address, port);
            Console.WriteLine($"Sent - {address}:{port}");
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"{appName} [mac_address]");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\t{appName} 01-00-00-00-00-02");
        }

        private static IPAddress[] GetDirectedBroadcastAddresses()
        {
            List<IPAddress> list = new List<IPAddress>();

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (item.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                UnicastIPAddressInformationCollection unicasts = item.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation unicast in unicasts)
                {
                    IPAddress ipAddress = unicast.Address;

                    if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    byte[] addressBytes = ipAddress.GetAddressBytes();
                    byte[] subnetBytes = unicast.IPv4Mask.GetAddressBytes();

                    if (addressBytes.Length != subnetBytes.Length)
                    {
                        continue;
                    }

                    byte[] broadcastAddress = new byte[addressBytes.Length];
                    for (int i = 0; i < broadcastAddress.Length; i++)
                    {
                        broadcastAddress[i] = (byte)(addressBytes[i] | (subnetBytes[i] ^ 255));
                    }

                    list.Add(new IPAddress(broadcastAddress));
                }
            }

            return list.ToArray();
        }

        private static byte[] GetWolPacket(string macAddress)
        {
            byte[] datagram = new byte[WOL_PACKET_LEN];
            byte[] macBuffer = StringToBytes(macAddress);

            MemoryStream ms = new MemoryStream(datagram);
            BinaryWriter bw = new BinaryWriter(ms);

            for (int i = 0; i < 6; i++)
            {
                bw.Write((byte)0xff);
            }

            for (int i = 0; i < 16; i++)
            {
                bw.Write(macBuffer, 0, macBuffer.Length);
            }

            Debug.Assert(ms.Length == WOL_PACKET_LEN);

            return datagram;
        }

        private static byte[] StringToBytes(string macAddress)
        {
            macAddress = Regex.Replace(macAddress, "[-|:]", ""); // Remove any semicolons or minus characters present in our MAC address
            byte[] buffer = new byte[macAddress.Length / 2];

            for (int i = 0; i < macAddress.Length; i += 2)
            {
                string digit = macAddress.Substring(i, 2);
                buffer[i / 2] = byte.Parse(digit, NumberStyles.HexNumber);
            }

            return buffer;
        }
    }
}
