using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace InformationSearch
{
    public class FrequenciesCalculator
    {
        private static int PAGES_COUNT = 100;
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";
        private static string TF_FILE_NAME = DIRECTORY_NAME + "\\tf.txt";
        private static string IDF_FILE_NAME = DIRECTORY_NAME + "\\idf.txt";
        private static string TF_IDF_FILE_NAME = DIRECTORY_NAME + "\\tf-idf.txt";
        
        private Dictionary<(string, int), double> TFDictionary = new Dictionary<(string, int), double>();
        private Dictionary<string, double> IDFDictionary = new Dictionary<string, double>();
        
        private List<string> Lemmas = new List<string>();

        private FrequenciesCalculator()
        {
            
        }

        public static void CalculateFrequencies()
        {
            var calculator = new FrequenciesCalculator();
            calculator.CalculateTF();
            calculator.CalculateIDF();
            calculator.CalculateTF_IDF();
        }

        private void CalculateTF()
        {
            for (int i = 1; i <= PAGES_COUNT; i++)
            {
                var lemmas = File
                    .ReadAllLines($"{DIRECTORY_NAME}\\lemmatized_page{i}.txt")
                    .ToList();

                var count = lemmas.Count;

                foreach (var lemma in lemmas)
                {
                    if (lemma.Length == 0) 
                        continue;

                    if (!Lemmas.Contains(lemma))
                    {
                        Lemmas.Add(lemma);
                    }

                    if (!TFDictionary.ContainsKey((lemma, i)))
                    {
                        var frequency = (double)lemmas.Count(l => l == lemma) / count;
                        TFDictionary.Add((lemma, i), frequency);
                    }
                }
                if(i % 10 == 0)
                    Console.WriteLine($"Calculated tf for file {i}");
            }

            Lemmas.Sort();
            
            var sb = new StringBuilder();

            var keys = TFDictionary.Keys.OrderBy(key => key.Item1).ToList();

            foreach (var key in keys)
            {
                sb.AppendLine($"{key.Item1} {key.Item2}: {TFDictionary[key]:0.00000}");
            }
            
            File.WriteAllText(TF_FILE_NAME, sb.ToString());
        }
        
        private void CalculateIDF()
        {
            foreach (var lemma in Lemmas)
            {
                var sourcesCount =
                    TFDictionary.Count(pair => pair.Key.Item1 == lemma);

                IDFDictionary[lemma] = Math.Log((double)PAGES_COUNT / sourcesCount, 100);
            }
            
            var sb = new StringBuilder();

            var keys = IDFDictionary.Keys.OrderBy(key => key).ToList();

            foreach (var key in keys)
            {
                sb.AppendLine($"{key}: {IDFDictionary[key]:0.00000}");
            }

            File.WriteAllText(IDF_FILE_NAME, sb.ToString());
            
            Console.WriteLine("Calculated idf");
        }
        
        private void CalculateTF_IDF()
        {
            var tfIdfDictionary = new Dictionary<(string, int), double>();
            
            foreach (var key in TFDictionary.Keys)
            {
                tfIdfDictionary[key] = TFDictionary[key] * IDFDictionary[key.Item1];
            }

            var sb = new StringBuilder();

            var keys = tfIdfDictionary.Keys.OrderBy(key => key.Item1).ToList();

            foreach (var key in keys)
            {
                sb.AppendLine($"{key.Item1} {key.Item2}: {tfIdfDictionary[key]:0.00000}");
            }

            File.WriteAllText(TF_IDF_FILE_NAME, sb.ToString());
            
            Console.WriteLine("Calculated tf-idf");
        }
    }
}