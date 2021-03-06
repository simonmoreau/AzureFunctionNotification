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
using Microsoft.AspNetCore.Http;

namespace Notifications
{
    public static class HttpTriggerCSharp1
    {
        [FunctionName("HttpTriggerCSharp1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Queue("availableslot", Connection = "AzureWebJobsStorage")] IAsyncCollector<AvailableSlot> availableSlotQueue,
            [Queue("checks", Connection = "AzureWebJobsStorage")] IAsyncCollector<AvailableSlot> checksQueue,
            ILogger log)
        {
            string searchUrl = "https://www.doctolib.fr/vaccination-covid-19/reims";

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

                        if (id != null)
                        {
                            if (id.Contains("search-result-"))
                            {
                                string searchId = id.Replace("search-result-", "");

                                string url = $"https://www.doctolib.fr/search_results/{searchId}.json?limit=6&speciality_id=5494&search_result_format=json";

                                var responseSearch = await httpClient.GetAsync(url);
                                string responseBody = await responseSearch.Content.ReadAsStringAsync();
                                SearchResponse searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseBody);

                                if (searchResponse.availabilities.Count > 0)
                                {
                                    AvailableSlot availableSlot = new AvailableSlot();
                                    availableSlot.name = "A slot is available !";
                                    availableSlot.url = searchUrl;
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

            return new OkObjectResult("responseMessage");
        }
    }
}
