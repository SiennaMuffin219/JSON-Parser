using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JSON_Parser
{
    class Program
    {
        static bool ToRemove(char Lettre1, char Lettre2) => (Lettre1 == '\"' && Lettre2 == '\"' || Lettre1 == '{' && Lettre2 == '}' || Lettre1 == '[' && Lettre2 == ']');

        static void Print(List<char> mot)
        {
            foreach (var item in mot)
            {
                Console.Write(item);
            }
            Console.WriteLine();
        }

        static char GetNextChar(string str, int i)
        {
            if (i < str.Length - 1)
                return str[i + 1];
            else
                return ' ';
        }

        


        static void Main(string[] args)
        {
            if (!File.Exists("Json.txt"))
            {
                File.Create("Json.txt").Close();
                Console.WriteLine("Fichier \"Json.txt\" créé.");
                Console.ReadLine();
            }
            else
            {
                string Json = "";
                using (FileStream Fs = File.OpenRead("Json.txt"))
                {
                    byte[] b = new byte[100];
                    UTF8Encoding temp = new UTF8Encoding(true);

                    while (Fs.Read(b, 0, b.Length) > 0)
                    {
                        Json += temp.GetString(b);
                    }
                }

                List<char> Scope = new List<char>();
                bool inQuotes = false;

                for (int i = 0; i < Json.Length; i++)
                {
                    char Letter = Json[i];
                    if (inQuotes && Letter != '\"')
                        continue;

                    switch (Letter)
                    {
                        case '\"':
                            if (i > 0)
                                if (Json[i - 1] == '\\')
                                    break;

                            inQuotes = !inQuotes;
                            Scope.Add('\"');
                            break;

                        case '{':
                            Scope.Add('{');
                            break;
                        case '}':
                            Scope.Add('}');
                            break;
                        case '[':
                            Scope.Add('[');
                            break;
                        case ']':
                            Scope.Add(']');
                            break;
                    }
                }
                
                while (Scope.Count > 0)
                {
                    for (int i = 0; i < Scope.Count - 1; i++)
                    {
                        if (ToRemove(Scope[i], Scope[i + 1]))
                        {
                            Scope.RemoveRange(i, 2);
                            break;
                        }
                        else if (i == Scope.Count - 2)
                        {
                            Console.WriteLine("Error: Invalid Json");
                            Environment.Exit(-1);
                        }
                    }
                }

                Console.WriteLine("Json Valid");
                Console.ReadLine();

                string Output = "";
                inQuotes = false;
                bool inList = false;
                string Word = "";
                string Name = "";
                JsonObject ObjectList = new JsonObject() { Name = "JSON", Type = ObjectType.Class, InnerObjects = new List<JsonObject>() };
                
                for (int i = 0; i < Json.Length; i++)
                {
                    char Letter = Json[i];
                    if (inQuotes)
                    {
                        if (Letter != '\"')
                        {
                            Word += Letter;
                            continue;
                        }
                    }
                    if (inList && Letter != ']')
                        continue;

                    switch (Letter)
                    {
                        case ',':
                            if (ObjectList.FindLastClass().Type == ObjectType.List)
                            {
                                if (!ObjectList.FindLastClass().InnerObjects.Exists(x => x.Type == ObjectType.Class))
                                    ObjectList.FindLastClass().InnerObjects.Add(new JsonObject() { Name = "_Items", Type = FindType(Word) });
                                inList = true;
                            }
                            else
                                ObjectList.FindLastClass().InnerObjects.Add(new JsonObject() { Name = Name, Type = FindType(Word) });
                            Word = "";
                            Name = "";
                            break;

                        case '\"':
                            inQuotes = !inQuotes;
                            break;

                        case ':':
                            Name = Word;
                            Word = "";
                            break;

                        case '{':
                            ObjectList.FindLastClass().InnerObjects.Add(new JsonObject() { Name = Name, Type = ObjectType.Class, InnerObjects = new List<JsonObject>() });
                            break;

                        case '}':

                            break;

                        case '[':
                            ObjectList.FindLastClass().InnerObjects.Add(new JsonObject() { Name = Name, Type = ObjectType.List, InnerObjects = new List<JsonObject>() });
                            break;

                        case ']':
                            inList = false;
                            break;

                    }
                }

                using (var fs = File.CreateText("Output.version"))
                {
                    fs.Write(Output);
                }
            }
        }

        enum ObjectType
        {
            String,
            Int,
            Float,
            Bool,
            Class,
            List,
            Object,
            Separator
        }

        class JsonObject
        {
            public ObjectType Type;
            public string Name;
            public List<JsonObject> InnerObjects;

            public JsonObject FindLastClass()
            {
                if (Type == ObjectType.Class || Type == ObjectType.List)
                {
                    if (InnerObjects.Count > 0)
                    {
                        if (InnerObjects[InnerObjects.Count - 1].FindLastClass() == null)
                            return this;
                        else
                            return InnerObjects[InnerObjects.Count - 1].FindLastClass();
                    }
                    else
                        return this;
                }
                else
                    return null;
            }
        }

        static ObjectType FindType(string str)
        {
            if (str.StartsWith("\""))
                return ObjectType.String;
            if (str.ToLower() == "true" || str.ToLower() == "false")
                return ObjectType.Bool;
            List<char> nums = new List<char>(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' });
            foreach (var i in str)
            {
                if (!nums.Contains(i))
                    return ObjectType.Object;
            }
            if (str.Contains("."))
                return ObjectType.Float;
            return ObjectType.Int;
        }
    }
}
