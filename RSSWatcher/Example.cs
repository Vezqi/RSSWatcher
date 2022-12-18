using RssWatcher;

// Create a new RssFeedMonitor instance with the URL of the RSS feed to monitor
var rssFeedMonitor = new RssWatcher.RssWatcher("https://old.reddit.com/r/AskReddit/new.rss", 15000);
Console.WriteLine("Running");

// Subscribe to the RssFeedChanged event to be notified when the data on the RSS feed changes
rssFeedMonitor.RssFeedChanged += RssFeedChanged;

void RssFeedChanged(object sender, RssFeedChangedEventArgs e)
{
    Console.WriteLine($"Found {e.NewRssFeedData.Count()} new item(s):");
    var counter = 1;
    foreach(var item in e.NewRssFeedData)
    {
        Console.WriteLine($"{counter}: {item.Title.Text}");
        counter++;
    }
}

// Keeps the program running indefinitely
Task.Delay(-1).GetAwaiter().GetResult();