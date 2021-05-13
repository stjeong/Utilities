using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace rotatefile
{
    // windows: rotatefile c:\temp *.* 6
    // linux:   ls -d -t1 $PWD/test/* | tail -n +6 | xargs rm
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Help();
                return;
            }

            string targetDir = args[0];
            string filterPattern = args[1];
            int allowLimit = 0;

            if (int.TryParse(args[2], out allowLimit) == false)
            {
                Help();
                return;
            }

            List<FileItem> files = new List<FileItem>();

            foreach (string file in Directory.EnumerateFiles(targetDir, filterPattern))
            {
                string filePath = Path.Combine(targetDir, file);
                DateTime created = File.GetCreationTime(filePath);

                files.Add(new FileItem { Name = filePath, Created = created });
            }

            if (files.Count <= allowLimit)
            {
                return;
            }

            files.Sort((x, y) =>
            {
                return y.Created.CompareTo(x.Created);
            });

            foreach (FileItem item in files.Skip(allowLimit))
            {
                try
                {
                    File.Delete(item.Name);
                    Console.WriteLine($"Deleted: {item.Name}");
                }
                catch { }
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"{appName} [target_dir] [file_filter] [maxcount]");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\t{appName} c:\temp *.dmp 5");
        }
    }

    public class FileItem
    {
        public string Name;
        public DateTime Created;
    }
}
