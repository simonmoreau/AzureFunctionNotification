using System;
using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Notifications
{
    public static class Doctolib
    {
        [FunctionName("Doctolib")]
        public async static Task Run([TimerTrigger("%TimerInterval%")] TimerInfo myTimer,
        [Queue("availableslot", Connection = "AzureWebJobsStorage")] IAsyncCollector<AvailableSlot> availableSlotQueue,
        [Queue("checks", Connection = "AzureWebJobsStorage")] IAsyncCollector<AvailableSlot> checksQueue,
        ILogger log)
        {
            log.LogInformation($"C# Doctolib Timer trigger function executed at: {DateTime.Now}");

            string searchUrl = "https://www.doctolib.fr/vaccination-covid-19/75015-paris?ref_visit_motive_ids[]=6970&ref_visit_motive_ids[]=7005&force_max_limit=2";

            using (HttpClient httpClient = new HttpClient())
            {
                // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");

                try
                {
                    string html = await httpClient.GetStringAsync(searchUrl);

                    // var responseSearch = await httpClient.GetByteArrayAsync(searchUrl);
                    // String source = Encoding.GetEncoding("utf-8").GetString(responseSearch, 0, responseSearch.Length - 1);
                    // source = WebUtility.HtmlDecode(source);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    string classToGet = "dl-search-result";

                    HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='" + classToGet + "']");

                    foreach (HtmlNode node in htmlNodes)
                    {
                        string id = node.Attributes["id"]?.Value;
                        HtmlNodeCollection htmlLocationNodes = node.SelectNodes(".//div[@class='dl-text dl-text-body dl-text-s dl-text-regular']");
                        string location = "";

                        foreach (HtmlNode locationNode in htmlLocationNodes)
                        {
                            location = locationNode.InnerText;
                            break;
                        }
                        
                        HtmlNodeCollection htmlLinkNodes = node.SelectNodes(".//a[@class='dl-search-result-name js-search-result-path']");
                        string link = "";

                        foreach (HtmlNode linkNode in htmlLinkNodes)
                        {
                            link = linkNode.Attributes["href"]?.Value;
                            if (link != null)
                            {
                                link = "https://www.doctolib.fr" + link;
                                break;
                            }
                            
                        }

                        if (id != null)
                        {
                            if (id.Contains("search-result-"))
                            {
                                string searchId = id.Replace("search-result-", "");

                                string url = $"https://www.doctolib.fr/search_results/{searchId}.json?ref_visit_motive_ids%5B%5D=6970&ref_visit_motive_ids%5B%5D=7005&speciality_id=5494&search_result_format=json&force_max_limit=2";

                                var responseSearch = await httpClient.GetAsync(url);
                                string responseBody = await responseSearch.Content.ReadAsStringAsync();
                                SearchResponse searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseBody);

                                if (searchResponse.availabilities.Count > 0)
                                {
                                    AvailableSlot availableSlot = new AvailableSlot();
                                    availableSlot.name = "A slot is available ! " + location;
                                    availableSlot.url = link;
                                    await availableSlotQueue.AddAsync(availableSlot);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    AvailableSlot availableSlot = new AvailableSlot();
                    availableSlot.name = "Sorry, nothing ...";
                    await checksQueue.AddAsync(availableSlot);
                }
            }



        }
    }
}
