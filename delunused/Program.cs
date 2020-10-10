using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace delunused
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Help();
                return;
            }

            EnumerateDirectories(args[0]);
        }
        private static void EnumerateDirectories(string rootPath)
        {
            string[] directories;

            try
            {
                directories = Directory.GetDirectories(rootPath);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            foreach (string dirPath in directories)
            {
                string dirName = Path.GetFileName(dirPath);
                if (IsUnusedDirectory(dirPath, dirName) == true)
                {
                    try
                    {
                        Directory.Delete(dirPath, true);
                        Console.WriteLine("Deleted: " + dirPath);
                    }
                    catch (System.UnauthorizedAccessException e)
                    {
                        Console.WriteLine("UnauthorizedAccessException: " + dirPath);
                        Console.WriteLine(e.ToString());
                    }
                }
                else
                {
                    EnumerateDirectories(dirPath);
                }
            }
        }

        private static void Help()
        {
            string appName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine($"{appName} [dir_path]");
            Console.WriteLine($"Deletes folders: ");
            Console.WriteLine($"\tOBJ.X86, OBJ.X64, WINDOWS_NT.X64.DEBUG, TESTRESULTS, _UPGRADEREPORT_FILES, _VTI_CNF");
            Console.WriteLine($"\tOBJ, BIN // if .csproj in parent folder and has sub directory 'debug'");
            Console.WriteLine($"\t\t // if .csproj in parent folder and has sub/sub directory 'x86/debug'");
            Console.WriteLine($"\t\t // if has no files");
            Console.WriteLine($"\tDEBUG, RELEASE // if .vcxproj in parent folder and has no sub directory");
            Console.WriteLine($"\tARM, ANYCPU, X86, X64 // if no files in 'debug' sub directory");
            Console.WriteLine($"\t\t\t      // if no files in 'release' sub directory");
            Console.WriteLine($"\tPACKAGES // if .sln in parent folder");
            Console.WriteLine($"\t\t // if NUGET.CONFIG file in parent folder");
            Console.WriteLine($"\t\t // if has 'Newtonsoft.Json' sub directory");
            Console.WriteLine($"\tV16   // if '.suo' file exists");
            Console.WriteLine($"ex:");
            Console.WriteLine($"\t{appName} d:\\");
        }

        private static bool IsUnusedDirectory(string dirPath, string dirName)
        {
            switch (dirName.ToUpper())
            {
                case "OBJ.X86":
                case "OBJ.X64":
                case "WINDOWS_NT.X64.DEBUG":
                case "TESTRESULTS":
                case "_UPGRADEREPORT_FILES":
                case "_VTI_CNF":
                    return true;

                case "V16":
                    if (HasFile(dirPath, ".SUO") == true)
                    {
                        return true;
                    }
                    break;

                case "OBJ":
                case "BIN":
                    if (HasFileExtInParent(dirPath, "CSPROJ") == true &&
                        HasSubPath(dirPath, "DEBUG") == true)
                    {
                        return true;
                    }

                    if (HasFileExtInParent(dirPath, "CSPROJ") == true &&
                        HasSubAndSubPath(dirPath, "X86", "DEBUG") == true)
                    {
                        return true;
                    }

                    if (HasNoFiles(dirPath) == true)
                    {
                        return true;
                    }
                    break;

                case "DEBUG":
                case "RELEASE":
                    if (HasFileExtInParent(dirPath, "VCXPROJ") == true &&
                        HasNoSubDirectory(dirPath) == true)
                    {
                        return true;
                    }
                    break;

                case "ARM":
                case "ANYCPU":
                case "X86":
                case "X64":
                    {
                        string debugPath = Path.Combine(dirPath, "DEBUG");
                        string relPath = Path.Combine(dirPath, "RELEASE");

                        if (Directory.Exists(debugPath) == true && HasNoSubDirectory(debugPath) == true)
                        {
                            return true;
                        }

                        if (Directory.Exists(relPath) == true && HasNoSubDirectory(relPath) == true)
                        {
                            return true;
                        }
                    }
                    break;

                case "PACKAGES":
                    if (HasFileExtInParent(dirPath, "SLN") == true)
                    {
                        return true;
                    }

                    if (HasFileInParent(dirPath, "NUGET.CONFIG") == true)
                    {
                        return true;
                    }

                    if (HasSubPath(dirPath, "Newtonsoft.Json") == true)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static bool HasFile(string dirPath, string fileName)
        {
            string filePath = Path.Combine(dirPath, fileName);
            return File.Exists(filePath);
        }

        private static bool HasNoFiles(string dirPath)
        {
            return Directory.GetFiles(dirPath, "*.*").Length == 0;
        }

        private static bool HasFileInParent(string dirPath, string fileName)
        {
            string parentPath = Path.GetDirectoryName(dirPath);
            return Directory.GetFiles(parentPath, fileName).Length >= 1;
        }

        private static bool HasNoSubDirectory(string dirPath)
        {
            string [] subDirs = Directory.GetDirectories(dirPath);
            if (subDirs.Length == 0)
            {
                return true;
            }

            if (subDirs.Length == 1)
            {
                string ext = subDirs[0].ToUpper();
                if (ext == ".TLOG")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSubAndSubPath(string dirPath, string depth1Path, string depth2Path)
        {
            string sub1Path = Path.Combine(dirPath, depth1Path);
            string sub2Path = Path.Combine(sub1Path, depth2Path);
            return (Directory.Exists(sub1Path) && Directory.Exists(sub2Path));
        }

        private static bool HasSubPath(string dirPath, string subDirName)
        {
            string sub1Path = Path.Combine(dirPath, subDirName);
            return Directory.Exists(sub1Path);
        }

        private static bool HasFileExtInParent(string dirPath, string fileExt)
        {
            string parentPath = Path.GetDirectoryName(dirPath);
            return Directory.GetFiles(parentPath, $"*.{fileExt}").Length >= 1;
        }
    }
}
