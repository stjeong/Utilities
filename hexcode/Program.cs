using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace hexcode
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Help();
                return;
            }

            string encodingType = args[0];

            if (encodingType == "-l")
            {
                foreach (var item in Encoding.GetEncodings())
                {
                    Console.WriteLine(item.Name);
                }

                return;
            }
            else if (encodingType == "guid")
            {
                Guid guid = new Guid(args[1]);

                Console.WriteLine(guid.ToString("N"));
                Console.WriteLine(guid.ToString("D"));
                Console.WriteLine(guid.ToString("B"));
                Console.WriteLine(guid.ToString("P"));
                Console.WriteLine(guid.ToString("X"));

                return;
            }

            if (args.Length < 2)
            {
                Help();
                return;
            }

            string text = args[1];

            Encoding encoding = Encoding.GetEncoding(encodingType);

            if (text.IndexOf('-') != -1)
            {
                string[] textBytes = text.Split('-');
                Decode(encoding, textBytes);
            }
            else if (text.IndexOf(',') != -1)
            {
                string[] textBytes = text.Split(',');
                Decode(encoding, textBytes);
            }
            else
            {
                Encode(encoding, text);
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"{appName} -l");
            Console.WriteLine($"{appName} [encoding_type] [text]");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\t{appName} utf-8 \"test\"");
            Console.WriteLine($"\t{appName} utf-8 \"74-65-73-74\"");
            Console.WriteLine($"\t{appName} utf-8 \"0x74-0x65-0x73-0x74\"");
            Console.WriteLine($"\t{appName} utf-8 \"74,65,73,74\"");
            Console.WriteLine($"\t{appName} utf-8 \"0x74,0x65,0x73,0x74\"");
            Console.WriteLine();
            Console.WriteLine($"\t{appName} guid F30A070D-BFFB-46A7-B1D8-8781EF7B698A");
        }

        private static void Decode(Encoding encoding, string[] textBytes)
        {
            List<byte> buffer = new List<byte>();
            foreach (string ch in textBytes)
            {
                string byteText = ch;

                if (byteText.StartsWith("0x") == true)
                {
                    byteText = byteText.Substring(2);
                }

                byte b = byte.Parse(byteText, System.Globalization.NumberStyles.HexNumber);
                buffer.Add(b);
            }

            Console.WriteLine(encoding.GetString(buffer.ToArray()));
        }

        private static void Encode(Encoding encoding, string text)
        {
            byte [] buffer = encoding.GetBytes(text);
            Console.WriteLine(BitConverter.ToString(buffer));
        }
    }
}
