using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace go
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowOptions();
                return;
            }

            Shortcuts goInstance = new Shortcuts();
            goInstance.Load();

            if (args[0] == "-a")
            {
                if (args.Length != 3)
                {
                    ShowOptions();
                    return;
                }

                string key = args[1];
                string value = args[2];
                goInstance.Add(key, value);
                goInstance.Save();
                return;
            }

            if (args[0] == "-r")
            {
                if (args.Length != 2)
                {
                    ShowOptions();
                    return;
                }

                string key = args[1];
                goInstance.Remove(key);
                goInstance.Save();
                return;
            }

            if (args.Length == 1)
            {
                string key = args[0];

                if (key == "-l")
                {
                    foreach (var item in goInstance.Map)
                    {
                        Console.WriteLine(item.Key + ": " + item.Value);
                    }

                    return;
                }

                if (goInstance.ContainsKey(key) == false)
                {
                    string candidate = "";

                    foreach (string dirPath in Directory.EnumerateDirectories(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        string dirName = Path.GetFileName(dirPath);
                        if (dirName.Equals(key, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            Console.WriteLine("\"" + dirPath + "\"");
                            return;
                        }
                        else if (dirName.IndexOf(key, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            candidate = dirPath;
                        }
                    }

                    if (string.IsNullOrEmpty(candidate) == false)
                    {
                        Console.WriteLine("\"" + candidate + "\"");
                        return;
                    }

                    ShowOptions();
                    return;
                }
                else
                {
                    string targetPath = goInstance.Get(key);

                    goInstance.Add("prev", Environment.CurrentDirectory);
                    goInstance.Save();
                    Console.WriteLine("\"" + targetPath + "\"");
                    return;
                }
            }
        }

        private static void ShowOptions()
        {
            Console.WriteLine("-a [key] [path]");
            Console.WriteLine("-r [key]");
            Console.WriteLine("[key]");
        }
    }
}
