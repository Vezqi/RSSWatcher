using System.Timers;
using System.Xml;
using System.ServiceModel.Syndication;

namespace RssWatcher
{
    public class RssFeedChangedEventArgs : EventArgs
    {
        public string RssFeedUrl { get; set; }
        public IEnumerable<SyndicationItem>? RssFeedData { get; set; }
        public IEnumerable<SyndicationItem>? NewRssFeedData { get; set; }
    }

    public class RssWatcher
    {
        private string rssFeedUrl { get; }
        public System.Timers.Timer pollScheduler { get; }
        private HttpClient httpClient;

        private IEnumerable<SyndicationItem> previousRssFeedData;
        private string recentId;

        // Event for when the data on the RSS feed changes
        public event EventHandler<RssFeedChangedEventArgs> RssFeedChanged;

        public RssWatcher(string rssFeedUrl, int pollIntervalMs)
        {
            this.rssFeedUrl = rssFeedUrl;
            this.httpClient = new HttpClient();

            // Set the initial value of previousRssFeedData to the current data from the RSS feed. (And the most recent ID to recentId)
            this.previousRssFeedData = this.GetRssFeedData(rssFeedUrl);
            this.recentId = this.previousRssFeedData.First().Id;

            // Set up the timer to check for updates to the RSS feed every 60 seconds
            this.pollScheduler = new System.Timers.Timer(pollIntervalMs);
            this.pollScheduler.Start();
            this.pollScheduler.AutoReset = true;
            this.pollScheduler.Elapsed += this.CheckRssFeedForUpdates;

        }

        // Checks the RSS feed for updates and fires the RssFeedChanged event if there is new data
        private void CheckRssFeedForUpdates(object sender, ElapsedEventArgs? e)
        {
            try
            {
                // Get the current data from the RSS feed
                var currentRssFeedData = this.GetRssFeedData(rssFeedUrl);
                var currentRssFeedId = currentRssFeedData.First().Id;

                // If the current data's ID is different from the previous data's ID, fire the RssFeedChanged event
                if (currentRssFeedId != this.previousRssFeedData.First().Id)
                {
                    var args = new RssFeedChangedEventArgs();
                    args.RssFeedUrl = rssFeedUrl;
                    args.RssFeedData = currentRssFeedData;

                    /*
                        GetNewItems(): How this works is:
                        1) Check for new data. If we have new data, then we are going to take the previously stored recentId variable and locate its position in our new data.
                        2) Next, we are going to take <index> # of elements from our new data.
                        3) We are going to set the event args' NewRssFeedData field to the "added" data to the feed, and then invoke the event.
                        4) After we do this, we can set the most recent ID to be the ID from the very first element in our NEW RSS feed data.
                     */

                    // Save the current data as the previous data for the next time the RSS feed is checked
                    this.previousRssFeedData = currentRssFeedData;

                    // Set our diff data and the invoke the event.
                    args.NewRssFeedData = this.GetNewItems();
                    this.RssFeedChanged?.Invoke(this, args);

                    // Set our recentID to the ID of the first ID of our NEW data.
                    this.recentId = currentRssFeedData.First().Id;
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
            var streamGetTask = Task.Run(() => httpClient.GetStreamAsync(this.rssFeedUrl));
            streamGetTask.Wait();

            var response = streamGetTask.Result;
            var reader = XmlReader.Create(response);
            var feed = SyndicationFeed.Load(reader);

            return feed.Items;
        }

        // Method to get the new items in the RSS feed since the last update.
        private IEnumerable<SyndicationItem> GetNewItems()
        {
  
            var index = this.previousRssFeedData.ToList()
                .FindIndex(item => item.Id == this.recentId);

            // If the index cannot be found, then just return the previous data.
            if (index == -1)
                return this.previousRssFeedData;

            // For example, { 1, 2, 3, 4, 5 } . If our index is 3, and we do Take(3), it will give us { 3, 2, 1 }.
            return this.previousRssFeedData.Take(index).Reverse();
        }

    }
}