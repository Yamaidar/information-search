using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LemmaSharp;

namespace InformationSearch
{
    public class Tokenizer
    {
        private static int PAGES_COUNT = 100;
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";
        public static void Run()
        {
            var output = "";
            for (var i = 1; i <= PAGES_COUNT; i++)
            {
                var text = File.ReadAllText($"{DIRECTORY_NAME}\\page{i}.txt");
                File.WriteAllText($"{DIRECTORY_NAME}\\tokenized_page{i}.txt", Tokenize(text));
                output += ($"page {i} tokenized;    ");
                if (i % 5 == 0) output += "\n";
            }
            Console.WriteLine(output);
        }
        
        public static string Tokenize(string data)
        {
            var pattern = @"(""[^""]+""|\w+)\s*";
            var group = new List<string>();
            
            MatchCollection mc = Regex.Matches(data, pattern);
            
            foreach (Match m in mc)
            {
                var value = m.Groups[0].Value.Trim('\n', '\t', ' ');

                if (value.Length > 0 && value.Length <= 100)
                {
                    group.Add(value);    
                }
            }

            return String.Join("\n", group);
        }
    }

    public class Lemmatizer
    {
        private static int PAGES_COUNT = 100;
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";

        public static Dictionary<string, ILemmatizer> Lemmatizers = new Dictionary<string, ILemmatizer>()
        {
            {"RUS", new LemmatizerPrebuiltFull(LemmaSharp.LanguagePrebuilt.Russian)},
            {"ENG", new LemmatizerPrebuiltFull(LemmaSharp.LanguagePrebuilt.English)}
        };
        
        public static void Run()
        {
            var output = "";
            for (var i = 1; i <= PAGES_COUNT; i++)
            {
                var text = File.ReadAllText($"{DIRECTORY_NAME}\\tokenized_page{i}.txt");
                File.WriteAllText($"{DIRECTORY_NAME}\\lemmatized_page{i}.txt", 
                    Lemmatize(text));
                output += ($"page {i} lemmatized;    ");
                if (i % 5 == 0) output += "\n";
            }
            Console.WriteLine(output);
        }

        public static string Lemmatize(string data)
        {
            var words = data.Split('\n');
            var lemmas = new List<string>();
            
            foreach (var word in words)
            {
                var lemma = "";
                
                var engRegex = new Regex("[A-Za-z]+");
                var rusRegex = new Regex("[А-Яа-я]+");
                
                if (engRegex.IsMatch(word))
                {
                    lemma = Lemmatizers["ENG"]
                        .Lemmatize(word)
                        .ToLower();
                    lemmas.Add(lemma);
                }
                else if (rusRegex.IsMatch(word))
                {
                    lemma = Lemmatizers["RUS"]
                        .Lemmatize(word)
                        .ToLower();
                    lemmas.Add(lemma);
                }
            }
            
            return String.Join("\n", lemmas);
        }
    }

    public static class StringExtensions
    {
        public static string ToLowerFirstChar(this string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
    }
}