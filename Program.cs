using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security;

namespace ConvertXML;
class ConvertXML
{
    public static string path = Environment.CurrentDirectory;
    public static string xmlFilePath = "strcombineall.xml";
    public static Boolean forced = false;
    private static List<customTable> charactersName = new List<customTable>(); 
    static string[] attributeNamesToCheck = { "Text", "text", "Name", "name", "Message", "message", "Desc", "desc", "title", "category", "introduction", "itemName", "ItemName", "effect_name", "note", "size0", "size1", "size2", "size3", "size4", "ptn00", "ptn01", "ptn02", "ptn03", "ptn04", "text0", "text1", "text2", "text3", "text4", "PotName", "ItemEffectName", "BookName", "LineHelp" };
    static string pattern = @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]";
    static void Main(string[] args)
    {

        forced = args.Contains("-forced");
        if(args.Length > 0)
            xmlFilePath = args[0];

        if(xmlFilePath.EndsWith(".xml") || xmlFilePath.EndsWith(".txt"))
        {
            if (!File.Exists(xmlFilePath))
            {
                Console.WriteLine("File " + xmlFilePath + " not found. (0x001)");
                Environment.Exit(-1);
                return;
            }
        }
        else
        {
            Console.WriteLine("Only .xml and .txt files are accepted. (" + xmlFilePath + ") (0x003)");
            Environment.Exit(-1);
            return;
        }

        if(args[0].EndsWith(".xml"))
            Extract();
        else if(args[0].EndsWith(".txt"))
            Insert();
        else if(args[0] == "-translate")
            Translate();
        else{
            Console.WriteLine("No argument entered. For safety, you must enter an argument"
            + Environment.NewLine + "-extract : Extract a .xml file located next to the executable."
            + Environment.NewLine + "-insert : Insert a .txt file into a .xml, both located next to the executable."
            + Environment.NewLine + "-translate : Uses the .xml files in the \"TRANSLATE\" folder and inserts them into strcombineall.xml.");
            Environment.Exit(-1);
            return;
        }
    }

    static void Extract(){
        if(File.Exists(xmlFilePath.Replace(".xml", ".txt")) && !forced)
        {
            Console.WriteLine("File extraction " + xmlFilePath.Replace(path, "") + " is ignored because a .txt file already exists. " + Environment.NewLine + "Use -forced as the last argument to force extraction. (0x400)");
            return;
        }
        Console.WriteLine("Extracting " + xmlFilePath.Replace(path, "") + " in progress...");
        XmlDocument xmlDoc = new XmlDocument();
        StreamReader reader = new StreamReader(xmlFilePath);
        xmlDoc.Load(reader);

        #pragma warning disable CS8600
        #pragma warning disable CS8602

        string valueIndex = "";

        if(xmlFilePath.ToLower().StartsWith("strcombineall"))
            valueIndex = "str";
        else
            valueIndex = Path.GetFileNameWithoutExtension(Path.GetFileName(xmlFilePath));

        if(valueIndex.Contains(" "))
        {
            Console.WriteLine("Extraction of the file " + xmlFilePath + " failed because the desired tag contains a space. (0x201)");
            return;
        }

        XmlNodeList nodes = xmlDoc.SelectNodes("Root/" + valueIndex);

        if (nodes == null || nodes.Count == 0)
        {
            Console.WriteLine("Extraction of the file " + xmlFilePath + " failed because no tag was found. (0x200)");
            return;
        }

        MemoryStream memoire = new MemoryStream();
        string final = "";

        XmlNode tmpTest = nodes[0];
        
        foreach (XmlNode node in nodes)
        {
            foreach (string attr in attributeNamesToCheck)
            {
                if(node.Attributes[attr] != null)
                {
                    string texte = node.Attributes[attr].Value;
                    if((texte.Length > 0 && !Regex.IsMatch(texte, pattern) && texte != "dummy" && texte != "[[[NO_DATA]]]" && checkValid(texte)) || forced)
                    {
                        final += texte + Environment.NewLine;
                    }
                }
            }
        }

        byte[] ba = Encoding.Default.GetBytes(final);
        memoire.Write(ba);

        using(StreamWriter writetext = new StreamWriter(xmlFilePath.Replace(".xml", ".txt")))
        {
            writetext.Write(final);
        }

    }

    // TO-DO technically usable
    static void Insert(){
        if(!File.Exists(xmlFilePath.Replace(".txt", ".xml")))
        {
            Console.WriteLine("The .txt file exists, but its .xml file was not found. (0x004)");
            Environment.Exit(-1);
            return;
        }
        Console.WriteLine("Insertion of " + xmlFilePath.Replace(path, "") + " in progress...");
        string[] lines = File.ReadAllLines(xmlFilePath);

        List<string> entry = new List<string>();

        foreach(string s in lines){
            entry.Add(s);
        }

        string valueIndex = "";

        if(xmlFilePath.ToLower().StartsWith("strcombineall"))
            valueIndex = "str";
        else
            valueIndex = Path.GetFileNameWithoutExtension(Path.GetFileName(xmlFilePath));

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath.Replace(".txt", ".xml"));

        XmlNodeList nodes = xmlDoc.SelectNodes("Root/" + valueIndex);

        if (nodes == null || nodes.Count == 0)
        {
            Console.WriteLine("Inserting the file " + xmlFilePath + " failed because no Root/" + valueIndex + " was found. (0x300)");
            return;
        }

        var i = 0;

        foreach (XmlNode node in nodes)
        {

            foreach (string attr in attributeNamesToCheck)
            {
                if(node.Attributes[attr] != null)
                {
                    if(forced || (checkValid(node.Attributes[attr].Value) && node.Attributes[attr].Value.Length > 0 && !Regex.IsMatch(node.Attributes[attr].Value, pattern) && node.Attributes[attr].Value != "dummy" && node.Attributes[attr].Value != "[[[NO_DATA]]]"))
                    {
                        node.Attributes[attr].Value = entry[i];
                        i++;
                    }

                }
            }         
        }

        var finalString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine + "<Root>" + Environment.NewLine;

        using (var writer = System.IO.File.CreateText(xmlFilePath.Replace(".txt", " NEW.xml")))
        {
            foreach (XmlNode node in nodes)
            {
                string text = node.OuterXml;
                finalString += "    " + text + Environment.NewLine;
            }

            finalString += "</Root>" + Environment.NewLine;
            writer.Write(finalString);
        }

        string tmpPath = xmlFilePath.Replace(path, "").Replace(".txt", ".xml");

        if(tmpPath.StartsWith("\\") || tmpPath.StartsWith("/"))
            tmpPath = tmpPath.Substring(1);

        int indexSeparateur = tmpPath.IndexOf('\\');

        if (indexSeparateur != -1)
            tmpPath = Path.Combine("Saves", tmpPath.Substring(indexSeparateur + 1));

        if (File.Exists(path + "\\" + tmpPath)) // If the code is executed in the root folder of the translation, we also replace the .xml in the archive.
        {
            if(File.Exists(path + "\\" + tmpPath + ".BAK"))
                File.Delete(path + "\\" + tmpPath + ".BAK");

            File.Move(path + "\\" + tmpPath, path + "\\" + tmpPath + ".BAK");
            File.Copy(xmlFilePath.Replace(".txt", " NEW.xml"), path + "\\" + tmpPath);
            File.Delete(xmlFilePath.Replace(".txt", " NEW.xml"));

            Console.WriteLine(tmpPath + " found!");
        }

        Console.WriteLine("Reinsertion complete.");
    }

    static bool checkValid(string line)
    {
        string resultat = line;
        if (resultat.Length <= 0) return false;

        resultat = resultat
        .Replace(".", "")
        .Replace(",", "")
        .Replace("?", "")
        .Replace(";", "")
        .Replace(":", "")
        .Replace("/", "")
        .Replace("!", "")
        .Replace("*", "")
        .Replace("%", "")
        .Replace("&", "")
        .Replace("~", "")
        .Replace("'", "")
        .Replace("#", "")
        .Replace("\"", "")
        .Replace("{", "")
        .Replace("(", "")
        .Replace("[", "")
        .Replace("-", "")
        .Replace("|", "")
        .Replace("`", "")
        .Replace("_", "")
        .Replace("\\", "")
        .Replace("@", "")
        .Replace(")", "")
        .Replace("]", "")
        .Replace("=", "")
        .Replace("}", "")
        .Replace("+", "")
        .Replace("-", "")
        .Replace("*", "")
        .Replace("/", "")
        .Replace(" ", "")
        .Replace("0", "")
        .Replace("1", "")
        .Replace("2", "")
        .Replace("3", "")
        .Replace("4", "")
        .Replace("5", "")
        .Replace("6", "")
        .Replace("7", "")
        .Replace("8", "")
        .Replace("9", "")
        ;

        return (resultat.Length > 0);
    }

    static void Translate(){

        Console.WriteLine("Translation of the strcombineall.xml file from the .xml files in \"TRANSLATE\".");

        DirectoryInfo d = new DirectoryInfo(path + "\\TRANSLATE\\");
        XmlDocument xmlDoc;
        XmlNodeList nodes;

        Regex regex = new Regex(@"\p{IsHiragana}|\p{IsKatakana}|\p{IsCJKUnifiedIdeographs}|\p{IsCJKCompatibilityIdeographs}|\p{IsCJKUnifiedIdeographsExtensionA}");
        Regex regexKor = new Regex(@"\p{IsHangulSyllables}|\p{IsHangulCompatibilityJamo}|\p{IsHangulJamo}");

        int i = 0;

        foreach (var file in d.GetFiles("*.xml"))
        {

            xmlDoc = new XmlDocument();
            xmlDoc.Load(path + "\\TRANSLATE\\" + file.Name);
            nodes = xmlDoc.SelectNodes("Root/str");

            foreach (XmlNode node in nodes)
            {
                bool isJapaneseLetter = regex.IsMatch(node.Attributes["Text"].Value);
                bool isKoreanLetter = regexKor.IsMatch(node.Attributes["Text"].Value);

                if(node.Attributes["Text"].Value == "" || isJapaneseLetter || isKoreanLetter)
                    continue;
                
                charactersName.Add(new customTable(node.Attributes["String_No"].Value, node.Attributes["Text"].Value));
                i++;

            }
        }

        Console.WriteLine("Number of texts already translated : " + i.ToString());

        xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath);
        nodes = xmlDoc.SelectNodes("Root/str");

        foreach (XmlNode node in nodes)
        {
            string value = node.Attributes["String_No"].Value;

            var characterNameFound = charactersName.Find(characterName => characterName.Id.Equals(value));

            if(characterNameFound.Name == null)
                continue;

            node.Attributes["Text"].Value = characterNameFound.Name;
        }

        var finalString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine + "<Root>" + Environment.NewLine;
        using (var writer = System.IO.File.CreateText(path + "\\strcombineall.xml"))
        {
            foreach (XmlNode node in nodes)
            {
                string text = node.OuterXml;
                finalString += "    " + text + Environment.NewLine;
            }

            finalString += "</Root>";
            writer.Write(finalString);
            Console.WriteLine("Reinsertion complete.");
        }

    }
}
