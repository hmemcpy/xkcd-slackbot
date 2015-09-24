using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace XkcdSlackbot
{
    public class SlackRequest
    {
        public string Token { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
        public string Channel_Name { get; set; }
    }

    public class XkcdModule : NancyModule
    {
        public XkcdModule() : base("/xkcd")
        {
            Post["/", runAsync: true] = async (_, ct) =>
            {
                var request = this.Bind<SlackRequest>();
                if (request.Token != ConfigurationManager.AppSettings["SLACK_SLASH_COMMAND_TOKEN"])
                    return 403;

                if (request.Command != "/xkcd") return 400;

                string xkcdUrl;

                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    xkcdUrl = await GetRandomComic();
                }
                else
                {
                    int result;
                    xkcdUrl = int.TryParse(request.Text, out result) ? $"https://xkcd.com/{result}/" : await SearchGoogle(request.Text);
                }

                var data = new
                {
                    channel = "#" + request.Channel_Name,
                    text = xkcdUrl,
                    unfurl_links = true
                };

                using (var wc = new WebClient())
                {
                    await wc.UploadStringTaskAsync(ConfigurationManager.AppSettings["SLACK_WEBHOOK_URL"], JsonConvert.SerializeObject(data));
                }

                return 200;
            };
        }

        private static async Task<string> GetRandomComic()
        {
            int latestComic;
            using (var wc = new WebClient())
            {
                var data = await wc.DownloadStringTaskAsync("https://xkcd.com/info.0.json");
                dynamic result = JObject.Parse(data);
                latestComic = result.num;
            }

            int random = new Random().Next(1, latestComic + 1);
            return $"https://xkcd.com/{random}/";
        }

        private static async Task<string> SearchGoogle(string query)
        {
            string apiKey = ConfigurationManager.AppSettings["GOOGLE_SEARCH_API_KEY"];
            string searchEngineId = ConfigurationManager.AppSettings["GOOGLE_SEARCH_ENGINE_ID"];
            using (var searchService = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer { ApiKey = apiKey }))
            {
                var listRequest = searchService.Cse.List(query);
                listRequest.Cx = searchEngineId;
                var search = await listRequest.ExecuteAsync();

                return search.Items?.First().Link;
            }
        }
    }
}