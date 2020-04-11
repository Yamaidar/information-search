using System;
using System.Threading.Tasks;

namespace InformationSearch
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. WebCrawler
            /*var url = "https://habr.com/ru/post/334126/";
            await WebCrawler.LaunchWebCrawler(url);*/
            
            // 2. Tokenizer + Lemmatizaer
            /*Console.WriteLine("\n");
            Tokenizer.Run();
            Lemmatizer.Run();*/

            // 3. Bool Search
            // InvertedIndexHandler.BuildInvertedIndex();
            
            /*while (true)
            {
                Console.Write("Введите запрос: ");
                Console.WriteLine(BoolInterpreter.Execute(Console.ReadLine()));
            }*/

            // 4. TF, IDF, TF-IDF
            //FrequenciesCalculator.CalculateFrequencies();
        }
    }
}