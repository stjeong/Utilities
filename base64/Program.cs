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
            else if (args.Length >= 2)
            {
                int cols = 0;
                bool encode = false;
                string filePath = null;

                try
                {
                    if (args[0].StartsWith("--wrap=") == true)
                    {
                        cols = int.Parse(args[0].Split('=')[1]);
                        filePath = args[1];
                        encode = true;
                    }

                    switch (args[0])
                    {
                        case "-d":
                        case "--decode":
                            string encodedText = File.ReadAllText(args[1]);
                            byte[] buffer = Convert.FromBase64String(encodedText);
                            Console.WriteLine(Encoding.ASCII.GetString(buffer));
                            return;

                        case "-w":
                            cols = int.Parse(args[1]);
                            filePath = args[2];
                            encode = true;
                            break;
                    }

                    if (encode == true)
                    {
                        byte[] orgBuffer = File.ReadAllBytes(filePath);
                        string output = Convert.ToBase64String(orgBuffer, Base64FormattingOptions.None);

                        if (cols == 0)
                        {
                            Console.Write(output);
                        }
                        else
                        {
                            for (int i = 0; i < output.Length; i ++)
                            {
                                if ((i % cols) == 0)
                                {
                                    Console.WriteLine();
                                }

                                Console.Write(output[i]);
                            }
                        }

                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                Help();
            }
        }

        private static void Help()
        {
            Console.WriteLine(@"Usage: base64 [OPTION]... [FILE]
Base64 encode or decode FILE.

Mandatory arguments to long options are mandatory for short options too.
  -d, --decode          decode data
  -w, --wrap=COLS       wrap encoded lines after COLS character (default 76).
                          Use 0 to disable line wrapping

The data are encoded as described for the base64 alphabet in RFC 4648.
When decoding, the input may contain newlines in addition to the bytes of
the formal base64 alphabet.
");
        }
    }
}
