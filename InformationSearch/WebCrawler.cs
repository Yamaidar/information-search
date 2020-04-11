using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace InformationSearch
{
    public class WebCrawler
    {
        // Константы
        private static int PAGES = 100;
        private static int SYMBOLS = 1000;
        private static string DIRECTORY_NAME = $"{Directory.GetCurrentDirectory()}\\pages";
        private static string INDEX_FILE_NAME = DIRECTORY_NAME + "\\index.txt";
        private static ConcurrentDictionary<string, string> DownloadedLinks { get; set; } 
            = new ConcurrentDictionary<string, string>();

        private HttpClient Client { get; set; }

        private int FilesCreated { get; set; }

        private List<string> ChildrenUrls { get; set; }

        public WebCrawler()
        {
            Directory.CreateDirectory(DIRECTORY_NAME);

            Client = new HttpClient();
            FilesCreated = 0;
            ChildrenUrls = new List<string>();
        }

        private void WriteIndexFile()
        {
            var sb = new StringBuilder();
            var pairs 
                = DownloadedLinks.OrderBy(pair => int.Parse(pair.Key));
            
            foreach (var pair in pairs)
            {
                sb.AppendLine(pair.Key + ": " + pair.Value);
            }
            
            File.WriteAllText(INDEX_FILE_NAME, sb.ToString());
        }

        public static async Task LaunchWebCrawler(string url)
        {
            var webCrawler = new WebCrawler();
            await webCrawler.ObservePage(url, true);
        }

        public async Task ObservePage(string url, bool isParent = false)
        {
            if (DownloadedLinks.Count >= PAGES)
            {
                return;
            }

            try
            {
                var response = await Client.GetStringAsync(url);

                var html  = new HtmlDocument();
                html.LoadHtml(response);
                
                // Убирает js и css
                var nodes = html.DocumentNode.SelectNodes("//script|//style");

                foreach (var node in nodes)
                    node.ParentNode.RemoveChild(node);
                
                var chunks = new List<string>(); 

                // Извлекает текст
                foreach (var item in html.DocumentNode.DescendantsAndSelf())
                {
                    if (item.NodeType == HtmlNodeType.Text)
                    {
                        var nodeText = item.InnerText
                            .Trim('\n')
                            .Trim('\r')
                            .Trim();
                        if (nodeText != "")
                        {
                            chunks.Add(nodeText);
                        }
                    }
                }

                var text = String.Join(" ", chunks);
                var words = Regex.Split(text, "( )+");

                if (words.Length >= SYMBOLS)
                {
                    await WriteData(text, url);
                }

                if (DownloadedLinks.Count >= PAGES)
                {
                    WriteIndexFile();
                    Environment.Exit(0);
                }

                // Поиск ссылок
                var transitionLinks = new List<string>();
                foreach(HtmlNode link in html.DocumentNode.SelectNodes("//a[@href]"))
                {
                    transitionLinks.Add(link.GetAttributeValue("href", null));
                }
               
                // Убирает ссылки на файлы
                foreach (var value in transitionLinks)
                {
                    //var urlRegex = new Regex("^(http(s)?://)?([\\w-]+.)+[\\w-]+(/[\\w- ./?%&=])?$");
                    var filesRegex = new Regex(@"\/[^\/]+.(png|css|json|svg|js|haml)(\?[\w-]+)?$");

                    if (!filesRegex.IsMatch(value) &&
                        DownloadedLinks.Values.FirstOrDefault(link => 
                            String.Equals(link.Split('?')[0].Trim('/'), 
                                value.Split('?')[0].Trim('/'))) == null)
                    {
                        ChildrenUrls.Add(value);
                    }
                }

                if (!isParent)
                    return;
                
                while (DownloadedLinks.Count < PAGES)
                {
                    var tasks = new List<Task>();

                    var take = Math.Min(PAGES - DownloadedLinks.Count, ChildrenUrls.Count);

                    var takes = ChildrenUrls.Take(take);
                    
                    ChildrenUrls = ChildrenUrls.Skip(take).ToList();

                    foreach (var link in takes)
                    {
                        tasks.Add(ObservePage(link));
                    }
                    
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception e)
            {
                // Do nothing
            }
        }

        public async Task WriteData(string text, string url)
        {
            if (FilesCreated >= PAGES || DownloadedLinks.Values.FirstOrDefault(link => 
                    String.Equals(link.Split('?')[0].Trim('/'), 
                        url.Split('?')[0].Trim('/'))) != null)
            {
                return;
            }
            
            var name = FilesCreated + 1;
            FilesCreated++;
            
            var fileName = $"{DIRECTORY_NAME}\\page{name}.txt";
            using (StreamWriter writer = File.CreateText(fileName))
            {
                await writer.WriteAsync(text);
            }
            
            bool added = false;
            while (!added)
            {
                added = DownloadedLinks.TryAdd(name.ToString(), url);
            }
            
            Console.WriteLine($"page{name} - {url}");
        }
        
    }
}