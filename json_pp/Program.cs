using Newtonsoft.Json;
using System;
using System.IO;

namespace json_pp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (BclExtension.ConsoleHelper.IsInputHandleRedirected() == false)
            {
                Console.WriteLine("No input redirected");
                Help();
                return;
            }

            bool verbose = false;

            if (args.Length == 1 && args[0] == "-v")
            {
                verbose = true;
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryReader br = new BinaryReader(Console.OpenStandardInput()))
            {
                while (true)
                {
                    byte[] buffer = br.ReadBytes(512);
                    if (buffer == null || buffer.Length == 0)
                    {
                        break;
                    }

                    ms.Write(buffer, 0, buffer.Length);
                }

                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                using (StringWriter sw = new StringWriter())
                {
                    string text = sr.ReadToEnd();

                    if (verbose == true)
                    {
                        Console.WriteLine(text);
                    }

                    using (StringReader textReader = new StringReader(text))
                    {
                        Newtonsoft.Json.JsonTextReader jtr = new Newtonsoft.Json.JsonTextReader(textReader);
                        var jsonWriter = new JsonTextWriter(sw) { Formatting = Formatting.Indented };
                        jsonWriter.WriteToken(jtr);
                        Console.WriteLine(sw.ToString());
                    }
                }
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"(redirect_source) | {appName} [-v]");
            Console.WriteLine($"ex:");
            Console.WriteLine("\techo \"{ \\\"foo\\\": 500 }\" | " + appName);
        }
    }
}


