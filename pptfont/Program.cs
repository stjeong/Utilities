using PPTXml;
using System;

namespace pptfont
{
    class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 1;

            if (args.Length < 2)
            {
                Help();
                return exitCode;
            }

            string mode = args[0];
            string filePath = args[1];
            string fontToRemove = null;

            if (mode == "remove")
            {
                if (args.Length < 3)
                {
                    Help();
                    return exitCode;
                }

                fontToRemove = args[2];
            }

            using (PowerpointXml pptxml = new PowerpointXml(filePath))
            {
                if (mode == "list")
                {
                    foreach (string fontName in pptxml.ListFonts(true))
                    {
                        Console.WriteLine(fontName);
                    }

                    exitCode = 0;
                }
                else if (mode == "remove" && string.IsNullOrEmpty(fontToRemove) == false)
                {
                    if (pptxml.RemoveFont(fontToRemove) == true)
                    {
                        pptxml.SaveFile(out string backupFilePath);
                        Console.WriteLine($"Reanmed old file \"{filePath}\" to {backupFilePath}");
                        Console.WriteLine($"{filePath}: Removed \"{fontToRemove}\"");

                        exitCode = 0;
                    }
                    else
                    {
                        Console.WriteLine("Font not used");
                    }
                }
            }

            return exitCode;
        }

        private static void Help()
        {
            Console.WriteLine($"{nameof(pptfont)} [mode] [file_path_to_pptx] [font_name]");
            Console.WriteLine();
            Console.WriteLine($"<example>");
            Console.WriteLine($"\t1) To list fonts");
            Console.WriteLine($"\t\t{nameof(pptfont)} list test.pptx");
            Console.WriteLine($"\t2) To remove fonts");
            Console.WriteLine($"\t\t{nameof(pptfont)} remove test.pptx \"Ami R\"");
        }
    }
}