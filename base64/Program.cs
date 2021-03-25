using System;
using System.IO;
using System.Text;

namespace base64
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

            if (args.Length == 1)
            {
                byte[] orgBuffer = File.ReadAllBytes(args[0]);
                Console.WriteLine(Convert.ToBase64String(orgBuffer, Base64FormattingOptions.InsertLineBreaks));
            }
            else if (args.Length == 2)
            {
                if (args[0] != "-d" && args[0] != "--decode")
                {
                    Help();
                    return;
                }

                string encodedText = File.ReadAllText(args[1]);
                byte [] buffer = Convert.FromBase64String(encodedText);
                Console.WriteLine(Encoding.ASCII.GetString(buffer));
            }
        }

        private static void Help()
        {
            Console.WriteLine(@"Usage: base64 [OPTION]... [FILE]
Base64 encode or decode FILE.

Mandatory arguments to long options are mandatory for short options too.
  -d, --decode          decode data
      --help     display this help and exit

The data are encoded as described for the base64 alphabet in RFC 4648.
When decoding, the input may contain newlines in addition to the bytes of
the formal base64 alphabet.
");
        }
    }
}
