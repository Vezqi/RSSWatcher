using System.Timers;
using System.Xml;
using System.ServiceModel.Syndication;

namespace RssWatcher
{
    // Event arguments class for the RssFeedChanged event
    public class RssFeedChangedEventArgs : EventArgs
    {
        public string RssFeedUrl { get; set; }
        public IEnumerable<SyndicationItem> RssFeedData { get; set; }
    }

    public class RssWatcher
    {
        private string rssFeedUrl { get; }
        public System.Timers.Timer rssFeedTimer { get; }
        private IEnumerable<SyndicationItem> previousRssFeedData;
        private HttpClient httpClient;

        // Event for when the data on the RSS feed changes
        public event EventHandler<RssFeedChangedEventArgs> RssFeedChanged;

        public RssWatcher(string rssFeedUrl, int pollIntervalMs)
        {
            this.rssFeedUrl = rssFeedUrl;
            this.httpClient = new HttpClient();

            // Set the initial value of previousRssFeedData to the current data from the RSS feed
            previousRssFeedData = GetRssFeedData(rssFeedUrl);

            // Set up the timer to check for updates to the RSS feed every 60 seconds
            rssFeedTimer = new System.Timers.Timer(pollIntervalMs);
            rssFeedTimer.Start();
            rssFeedTimer.AutoReset = true;
            rssFeedTimer.Elapsed += CheckRssFeedForUpdates;

        }

        // Checks the RSS feed for updates and fires the RssFeedChanged event if there is new data
        private async void CheckRssFeedForUpdates(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Call to CheckRssFeedForUpdates");
            try
            {
                // Get the current data from the RSS feed
                var currentRssFeedData = GetRssFeedData(rssFeedUrl);
                var currentRssFeedId = currentRssFeedData.First().Id;

                // If the current data's ID is different from the previous data's ID, fire the RssFeedChanged event
                if (currentRssFeedId != this.previousRssFeedData.First().Id)
                {
                    Console.WriteLine("Feeds are different!");
                    var args = new RssFeedChangedEventArgs();
                    args.RssFeedUrl = rssFeedUrl;
                    args.RssFeedData = currentRssFeedData;
                    RssFeedChanged?.Invoke(this, args);
                    // Save the current data as the previous data for the next time the RSS feed is checked
                    this.previousRssFeedData = currentRssFeedData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking RSS feed for updates: {ex.Message}");
            }
        }

        // Method to get the data from an RSS feed as a string
        private IEnumerable<SyndicationItem> GetRssFeedData(string rssFeedUrl)
        {
            // Do this to avoid making this return a Task.
            Console.WriteLine("Call to GetRssFeedData");
            var streamGetTask = Task.Run(() => httpClient.GetStreamAsync(this.rssFeedUrl));
            streamGetTask.Wait();
            var response = streamGetTask.Result;

            var reader = XmlReader.Create(response);
            var feed = SyndicationFeed.Load(reader);
            return feed.Items;
        }
    }
}