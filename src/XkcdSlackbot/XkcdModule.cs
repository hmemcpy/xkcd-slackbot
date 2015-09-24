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

                int result;
                var url = int.TryParse(request.Text, out result) ? $"https://xkcd.com/{result}" : await SearchGoogle(request.Text);

                if (string.IsNullOrEmpty(url)) return 404;

                var data = new
                {
                    text = url,
                    channel = "#" + request.Channel_Name,
                    unfurl_links = true
                };

                using (var wc = new WebClient())
                {
                    await wc.UploadStringTaskAsync(ConfigurationManager.AppSettings["SLACK_WEBHOOK_URL"], JsonConvert.SerializeObject(data));
                }

                return 200;
            };
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