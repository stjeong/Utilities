using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;

namespace netcat
{
    public class NetCatSettings
    {
        public bool ListenMode;
        public bool KeepAccept;
        public bool UdpMode;
        public bool PrintToConsole;
        public RedirectSource Source = RedirectSource.StandardInput;

        public static string[] ParseOptions(string[] args, NetCatSettings settings)
        {
            List<string> endpoints = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("-") == false)
                {
                    endpoints.Add(arg);
                }

                if (arg.IndexOf("u") != -1)
                {
                    settings.UdpMode = true;
                }

                if (arg.IndexOf("C") != -1)
                {
                    settings.PrintToConsole = true;
                }

                if (arg.IndexOf("l") != -1)
                {
                    settings.ListenMode = true;
                }

                if (arg.IndexOf("k") != -1)
                {
                    settings.KeepAccept = true;
                }
            }

            return endpoints.ToArray();
        }
    }

}

public interface INetWorkWriter : IDisposable
{
    void Write(byte[] data);
}

public class TcpWrapper : INetWorkWriter
{
    TcpClient _client;
    NetworkStream _ns;

    public TcpWrapper(string address, int port)
    {
        _client = new TcpClient(address, port);
        _ns = _client.GetStream();
    }

    public void Dispose()
    {
        _ns.Dispose();

        IDisposable disp = _client as IDisposable;
        if (disp != null)
        {
            disp.Dispose();
        }
    }

    public void Write(byte[] data)
    {
        _ns.Write(data, 0, data.Length);
    }
}

public class UdpWrapper : INetWorkWriter
{
    UdpClient _client;

    public UdpWrapper(string address, int port)
    {
        _client = new UdpClient(address, port);
    }

    public void Dispose()
    {
        IDisposable disp = _client as IDisposable;
        if (disp != null)
        {
            disp.Dispose();
        }
    }
    
    public void Write(byte[] data)
    {
        _client.Send(data, data.Length);
    }
}

public enum RedirectSource
{
    StandardInput,
    StandardError,
}