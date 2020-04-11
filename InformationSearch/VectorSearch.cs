using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;

namespace InformationSearch
{
    public class VectorSearch
    {
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";
        private static string TF_IDF_FILE_NAME = DIRECTORY_NAME + "\\tf-idf.txt";
        private static string IDF_FILE_NAME = DIRECTORY_NAME + "\\idf.txt";
        private static int PAGES_COUNT = 100;

        public static string Search(string query)
        {
            var documentsSimilarity = new Dictionary<int, double>();
            
            var lemmas = Lemmatizer.Lemmatize(Tokenizer.Tokenize(query))
                .Split('\n').ToList();

            var dimensions = lemmas.Distinct().ToList();

            var frequencies = new Dictionary<string, double>();

            var text = File.ReadAllLines(IDF_FILE_NAME).ToList();
            
            var idfs = new Dictionary<string, double>();

            foreach (var idf in text)
            {
                var lemma = idf.Substring(0, idf.LastIndexOf(": "));

                var freqString = idf
                    .Replace(lemma + ": ", "")
                    .Replace(",", ".");
                
                var freq = double.Parse(freqString, CultureInfo.InvariantCulture);

                idfs[lemma] = freq;
            }

            dimensions.ForEach(dim =>
            {
                var lemmaTf = (double) lemmas.Count(lemma => lemma == dim) / lemmas.Count;

                frequencies[dim] = lemmaTf * (idfs.ContainsKey(dim) ? idfs[dim] : 1);
            });

            text = File.ReadAllLines(TF_IDF_FILE_NAME).ToList();
            
            var tfIdfs = new Dictionary<(string, int), double>();

            foreach (var tfIdf in text)
            {
                var keys = tfIdf.Substring(0, tfIdf.LastIndexOf(": "));

                var freqString = tfIdf
                    .Replace(keys + ": ", "")
                    .Replace(",", ".");
                
                var freq = double.Parse(freqString, CultureInfo.InvariantCulture);
                var lemma = keys.Substring(0, keys.LastIndexOf(" "));
                var docId = int.Parse(keys.Replace(lemma + " ", ""));

                tfIdfs[(lemma, docId)] = freq;
            }
            
            var querySize = Math.Sqrt(frequencies.Values
                .Select(freq => freq * freq)
                .Sum());

            for (int i = 1; i <= PAGES_COUNT; i++)
            {
                var passing = tfIdfs
                    .Where(tfIdf => tfIdf.Key.Item2 == i)
                    .ToDictionary(arg => arg.Key.Item1, 
                        arg => arg.Value);

                var similarity = CalculateSimilarity(frequencies, passing, querySize);

                if (similarity.CompareTo(1) < 0)
                {
                    documentsSimilarity[i] = similarity;
                }
            }

            documentsSimilarity = documentsSimilarity
                .OrderBy(pair => pair.Value)
                .ToDictionary(a => a.Key,
                a => a.Value);
            
            var sb = new StringBuilder();

            var currentValue = 1;
            var results = documentsSimilarity.Keys.Take(10).ToList();
            
            if (!results.Any())
            {
                sb.AppendLine($"По запросу \"{query}\" ничего не найдено.");
            }
            else
            {
                sb.AppendLine("Результаты запроса.");
                foreach (var key in results)
                {
                    sb.AppendLine($"{currentValue}. Документ {key} ({documentsSimilarity[key]:0.00000})");
                    currentValue++;
                }
            }

            return sb.ToString();
        }

        private static double CalculateSimilarity(Dictionary<string, double> query, 
            Dictionary<string, double> document, double querySize)
        {
            var commonLemmas = query.Keys.Intersect(document.Keys).ToList();

            if (commonLemmas.Count == 0)
            {
                return 1;
            }

            var documentSize = Math.Sqrt(document.Values
                .Select(freq => freq * freq)
                .Sum());
            
            var sum = commonLemmas
                .Select(lemma => query[lemma] * document[lemma])
                .Sum();

            var similarity = sum / (querySize * documentSize);

            return similarity;
        }
    }
}