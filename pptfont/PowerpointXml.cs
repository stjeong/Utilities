using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace PPTXml
{
    public class PowerpointXml : IDisposable
    {
        string _orgFilePath = "";
        string _extracted = "";
        L2Dictionary _slideFont = new L2Dictionary();
        L2Dictionary _etcFont = new L2Dictionary();

        public IEnumerable<string> ListFonts(bool slideOnly = true)
        {
            L2Dictionary dict;

            if (slideOnly == false)
            {
                dict = _slideFont.CreateNewMergeFrom(_etcFont);
            }
            else
            {
                dict = _slideFont;
            }

            foreach (string fontName in dict.ListLevel2Key())
            {
                yield return fontName;
            }
        }

        public PowerpointXml(string pptxPath)
        {
            if (File.Exists(pptxPath) == false)
            {
                throw new FileNotFoundException(pptxPath);
            }

            FileInfo f = new FileInfo(pptxPath);

            string extracted = Path.Combine(f.Directory.FullName, "temp_extracted");
            DeleteOldDirectory(extracted);

            _extracted = extracted;

            ZipFile.ExtractToDirectory(pptxPath, extracted);

            Enumerate(ListFontFunc);

            _orgFilePath = f.FullName;
        }

        public void SaveFile(out string backupFilePath)
        {
            string dirPath = Path.GetDirectoryName(_orgFilePath);
            string fileName = Path.GetFileNameWithoutExtension(_orgFilePath);
            string extName = Path.GetExtension(_orgFilePath);

            backupFilePath = GetBackupFilePath();
            File.Move(_orgFilePath, backupFilePath);

            ZipFile.CreateFromDirectory(_extracted, _orgFilePath);
        }

        private string GetBackupFilePath()
        {
            int index = 1;

            while (true)
            {
                string orgFilePath = _orgFilePath;
                string newFilePath = null;

                if (index == 1)
                {
                    newFilePath = Path.ChangeExtension(orgFilePath, ".bak");
                }
                else
                {
                    newFilePath = Path.ChangeExtension(orgFilePath, $".{index}.bak");
                }

                if (File.Exists(newFilePath) == false)
                {
                    return newFilePath;
                }

                index++;
            }
        }

        public bool RemoveFont(string fontToRemove)
        {
            return Enumerate(RemoveFontInfo);

            bool RemoveFontInfo(string fileName, XmlNode node, bool isSlide)
            {
                if (node.Attributes == null)
                {
                    return false;
                }

                bool delete = false;

                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "typeface")
                    {
                        string attrValue = attr.Value;
                        if (string.IsNullOrEmpty(attrValue) == true)
                        {
                            continue;
                        }

                        if (attrValue[0] == '+')
                        {
                            continue;
                        }

                        if (attrValue == fontToRemove)
                        {
                            delete = true;
                            break;
                        }
                    }
                }

                if (delete == true)
                {
                    XmlNode parentNode = node.ParentNode;
                    parentNode.RemoveChild(node);
                    return true;
                }

                return false;
            }
        }

        public void Dispose()
        {
            DeleteOldDirectory(_extracted);
        }

        void DeleteOldDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath) == true)
                {
                    Directory.Delete(dirPath, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        bool Enumerate(Func<string, XmlNode, bool, bool> func)
        {
            foreach (string filePath in Directory.EnumerateFiles(_extracted, "*.*", SearchOption.AllDirectories))
            {
                if (IsXmlFile(filePath) == false)
                {
                    continue;
                }

                XmlDocument xmlDoc = new XmlDocument();
                bool changed = false;

                try
                {
                    xmlDoc.Load(filePath);
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNodeList nodeList = xmlDoc.SelectNodes("/descendant-or-self::node()");
                string fileName = Path.GetFileName(filePath);

#if DEBUG
                if (fileName == "slide1.xml")
                {
                    Console.Write("");
                }
#endif

                bool isSlide = Path.GetFileName(Path.GetDirectoryName(filePath)).ToUpper() == "SLIDES";

                foreach (XmlNode node in nodeList)
                {
                    if (func(fileName, node, isSlide) == true)
                    {
                        changed = true;
                    }
                }

                if (changed == true)
                {
                    xmlDoc.Save(filePath);
                    return true;
                }
            }

            return false;
        }

        public bool ListFontFunc(string fileName, XmlNode node, bool isSlide)
        {
            if (node.Attributes == null)
            {
                return false;
            }

            foreach (XmlAttribute attr in node.Attributes)
            {
                if (attr.Name == "typeface")
                {
                    string attrValue = attr.Value;
                    if (string.IsNullOrEmpty(attrValue) == true)
                    {
                        continue;
                    }

                    if (attrValue[0] == '+')
                    {
                        continue;
                    }

                    if (isSlide == true)
                    {
                        _slideFont.Add(fileName, attr.Value);
                    }
                    else
                    {
                        _etcFont.Add(fileName, attr.Value);
                    }
                }
            }

            return false;
        }

        public bool IsXmlFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToUpper();
            switch (ext)
            {
                case ".EMF":
                case ".PNG":
                case ".JPG":
                case ".JPEG":
                case ".WDP":
                    return false;

                default:
                    return true;
            }
        }
    }
}
