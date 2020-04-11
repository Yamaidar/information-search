using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InformationSearch
{
    public class InvertedIndexHandler
    {
        private static int PAGES_COUNT = 100;
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";
        private static string INVERTED_INDEX_FILE_NAME = DIRECTORY_NAME + "\\inverted_index.txt";

        public InvertedIndexHandler()
        {
            
        }

        public static void BuildInvertedIndex()
        { 
            var wordsPositions = new Dictionary<string, List<int>>();    
            
            for (int i = 1; i <= PAGES_COUNT; i++)
            {
                var lemmas = File.ReadAllLines($"{DIRECTORY_NAME}\\lemmatized_page{i}.txt");
                foreach (var lemma in lemmas)
                {
                    if (lemma.Length == 0) 
                        continue;

                    if (!wordsPositions.ContainsKey(lemma)) 
                        wordsPositions[lemma] = new List<int>();

                    if (!wordsPositions[lemma].Contains(i)) 
                        wordsPositions[lemma].Add(i);
                }
            }

            var sb = new StringBuilder();
            var keys = wordsPositions.Keys
                .OrderBy(key => key.Length > 0 ? key[0].ToString() : "")
                .ToList();
            
            foreach (var key in keys)
            {
                var list = wordsPositions[key];
                list = list.OrderBy(docId => docId).ToList();
                
                sb.AppendLine($"{key} {String.Join(" ", list)}");
            }

            File.WriteAllText(INVERTED_INDEX_FILE_NAME, sb.ToString());
        }

        public static List<int> FindWordPositions(string word)
        {
            var wordPositions = new List<int>();

            var lines = File.ReadAllLines(INVERTED_INDEX_FILE_NAME);

            foreach (var line in lines)
            {
                if (line.StartsWith(word + " "))
                {
                    wordPositions.AddRange(line
                        .Replace(word + " ", "")
                        .Split(' ')
                        .Select(docId => int.Parse(docId))
                        .ToList()
                    );
                    break;
                }
            }
            
            return wordPositions;
        }
    }
}