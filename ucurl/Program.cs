using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ucurl
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string cmd = args[0];
            string[] cmdArgs = null;

            if (Path.GetFileNameWithoutExtension(cmd).ToLower() == "curl")
            {
                cmdArgs = PreprocessArgs(args.Skip(1).ToArray());
            }
            else
            {
                cmd = "curl";
                cmdArgs = PreprocessArgs(args.ToArray());
            }
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = cmd;
            psi.UseShellExecute = false;
            psi.Arguments = string.Join(" ", cmdArgs);

            Process proc = Process.Start(psi);

            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;

            proc.WaitForExit();
        }

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static string[] PreprocessArgs(string[] args)
        {
            List<string> list = new List<string>();

            foreach (string arg in args)
            {
                if (arg.IndexOf(' ') == -1)
                {
                    list.Add(arg);
                }
                else
                {
                    if (arg.IndexOf('"') == -1)
                    {
                        list.Add("\"" + arg + "\"");
                    }
                    else
                    {
                        List<string> argEncoded = new List<string>();

                        foreach (char ch in arg)
                        {
                            if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.OtherLetter)
                            {
                                argEncoded.Add("\\u" + ((int)ch).ToString("x4"));
                            }
                            else
                            {
                                argEncoded.Add(ch.ToString());
                            }
                        }

                        string text = string.Join("", argEncoded);
                        text = text.Replace("\"", "\\\"");

                        list.Add("\"" + text + "\"");
                    }
                }
            }

            return list.ToArray();
        }
    }
}
