# /xkcd for Slack

A slash command for [Slack](https://slack.com/) that displays an [xkcd](https://xkcd.com) comic in the channel, either by comic number (e.g. `/xkcd 702`) or its description (e.g. `/xkcd battery horse`).

![](http://i.imgur.com/YckTtFO.gif)

## Requirements
  1. [Slash Command](https://api.slack.com/slash-commands) for our command (`/xkcd`)
  2. [Incoming Webhook](https://api.slack.com/incoming-webhooks) to display the results of the command
  3. Custom [Google Search Engine](https://cse.google.com) to consume search results via the API
  4. Tiny web application to tie it all together (I'm using a [Nancy](http://nancyfx.org/) application, but can be anything)
  5. Somewhere to host the web application (e.g. Azure or AppHarbor, both offer free tier)

## Getting started
Perform the following steps to set up the command correctly.

### Slash Command
[Create a new Slash Command](https://my.slack.com/services/new/slash-commands/). Call it `/xkcd`, and leave the page open to put in a URL later. Copy the value of the **Token** field, this is your `SLACK_SLASH_COMMAND_TOKEN`.

### Incoming Webhook
[Create a new Incoming Webhook](https://my.slack.com/services/new/incoming-webhook/). You can choose any channel from the list; it doesn't matter. The channel will be overridden on each request with the channel from which the request originated. After creating, you'll see a **Webhook URL** field. This is your `SLACK_WEBHOOK_URL`.

### Google Search Engine
Unfortunately, Google Web Search API was deprecated some time ago, and scraping the results of a search is against the Google TOS (which can get you banned on Google). However, it is possible to use the Google Custom Search API. We can define our own Custom Search Engine (CSE) to only search the xkcd.com site, and that is allowed with a C# API.

Note: CSE allows 100 queries per day for free!

#### Setup
  1. Make sure you're logged in with a Google Account.
  2. Go to https://cse.google.com/cse/create/new
    1. In **Sites to search** type `xkcd.com`
    2. Press **Create**
  3. Press **Control Panel**
  4. Turn off Image search, make sure **Search only included sites** is selected.
  5. Click the **Search engine ID** button, and copy the code. This is your `GOOGLE_SEARCH_ENGINE_ID`.

Finally, to enable access via the C# API, we need to enable the Custom Search API:
  1. Go to https://console.developers.google.com/project
  2. Press **Create Project**, and give it a name `xkcd`
  3. In the newly created project, go to the **API & auth** menu, selecting **APIs**
  4. Under **Other popular APIs** select **Custom Search API**
  5. Press the **Enable API** button
  6. Under **APIs & auth** menu, select **Credentials**
  7. Press **Add credentials** dropdown, and select **API key**
  8. In the new popup, press **Browser key**, give it a name (e.g. `xkcd-key`)
  9. When you're done, you'll get a popup with the API key. This is your `GOOGLE_SEARCH_API_KEY`

Phew!

## Putting it all together

Clone this project, and open it with Visual Studio. This is a Nancy web application, which can be deployed as Azure Web App for free. After deploying it to Azure, go to the [Azure Management Portal](https://manage.windowsazure.com), navigate to **Web Apps**, select your application and go to **Configure**. Scroll down until the **app settings** section, and add the keys as mentioned above:

![](http://i.imgur.com/FvBGwLo.png)

Finally, copy the URL of your Azure application, and paste it into the **Slash Command** page from above, in the **URL** field.

Add in some additional descriptions and usage hints for the command, customize the icon and name of the incoming webhook, and you're all set!
