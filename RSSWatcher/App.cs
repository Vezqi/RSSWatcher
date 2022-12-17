using RssWatcher;

// Create a new RssFeedMonitor instance with the URL of the RSS feed to monitor
var rssFeedMonitor = new RssWatcher.RssWatcher("http://lorem-rss.herokuapp.com/feed", 60000);
Console.WriteLine($"Running: {rssFeedMonitor.rssFeedTimer.Enabled}");

// Subscribe to the RssFeedChanged event to be notified when the data on the RSS feed changes
rssFeedMonitor.RssFeedChanged += RssFeedChanged;

// Method to handle the RssFeedChanged event.
void RssFeedChanged(object sender, RssFeedChangedEventArgs e)
{
    // Print the URL of the RSS feed and the new data from the feed
    Console.WriteLine("RSS feed changed: " + e.RssFeedUrl);
    Console.WriteLine("New RSS feed data: " + e.RssFeedData);
}

// Todo: Program stops terminating if we don't wait for a keystroke/anything. Fix!
Console.ReadLine();