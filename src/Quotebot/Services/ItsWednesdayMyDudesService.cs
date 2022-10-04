using System.Net.Http.Json;

namespace Quotebot.Services
{
    internal class ItsWednesdayMyDudesService
    {
        private readonly DiscordSocketClient _client;
        private readonly YoutubeConfiguration _configuration;
        private readonly string _youtubeApiKey;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(5));
        private Task? _task;

        public ItsWednesdayMyDudesService(DiscordSocketClient client, YoutubeConfiguration configuration,
            CancellationTokenSource cancellationTokenSource)
        {
            _client = client;
            _configuration = configuration;
            _cancellationTokenSource = cancellationTokenSource;
            _youtubeApiKey = _configuration.ApiKey;

        }

        private async Task AnnounceTheHolyDay()
        {
            ulong generalChannelId = 1021213113648947232;
            IMessageChannel channel = _client.GetChannel(generalChannelId) as IMessageChannel ??
                                      throw new ArgumentNullException($"Channel Id {generalChannelId} was not found");
            await channel.SendMessageAsync("Announcement!");
        }

        public void Initialize()
        {
            var periodRemaining = GetNextWednesday();
            _task = WhenItsWednesdayMyDudes(periodRemaining);
        }

        private async Task WhenItsWednesdayMyDudes(TimeSpan untilWednesday)
        {
            using (_timer = new(untilWednesday))
            {
                await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token);
                await GetWednesdayYouTubeVideos();
                await AnnounceTheHolyDay();
            }

            untilWednesday = GetNextWednesday();
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                await WhenItsWednesdayMyDudes(untilWednesday);
            }

            if (_task is null)
                return;

            await _task;
            _timer.Dispose();

        }

        private TimeSpan GetNextWednesday()
        {
            //return TimeSpan.FromSeconds(5);

            DateOnly nextWednesday = NextCalendarDate(DateTimeOffset.Now, DayOfWeek.Wednesday);
            TimeOnly sixAm = TimeOnly.FromTimeSpan(new(6, 0, 0));

            DateTime nextWedsSixAm = nextWednesday.ToDateTime(sixAm);

            return nextWedsSixAm.Subtract(DateTimeOffset.Now.DateTime);
        }

        private DateOnly NextCalendarDate(DateTimeOffset from, DayOfWeek dayOfTheWeek)
        {
            DateOnly nextDay = DateOnly.FromDateTime(from.Date).AddDays(1);
            var delta = ((int) dayOfTheWeek - (int) nextDay.DayOfWeek + 7) % 7;
            nextDay = nextDay.AddDays(delta);
            return nextDay;
        }


        private async Task<YouTubeResults> GetWednesdayYouTubeVideos()
        {
            Uri baseYoutubeUri = new("https://www.youtube.com/watch?v=");
            using HttpClient httpClient = new()
            {
                BaseAddress = new Uri($"https://www.googleapis.com/")
            };

            try
            {
                var result = await httpClient.GetFromJsonAsync<YouTubeResults>(
                    $"youtube/v3/playlistItems?part=contentDetails&playlistId=PLy3-VH7qrUZ5IVq_lISnoccVIYZCMvi-8&maxResults=50&key={_youtubeApiKey}");

                if (result is null)
                {
                    return new();
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public abstract record YoutubeEntity
        {
            public string kind { get; init; } = string.Empty;
            public string etag { get; init; } = string.Empty;
        }
        public record YouTubeResults : YoutubeEntity
        {
            public string nextPageToken { get; init; } = string.Empty;
            public IEnumerable<YouTubeItem> items { get; init; } = Enumerable.Empty<YouTubeItem>();
        }

        public record YouTubeItem : YoutubeEntity
        {
            public string id { get; init; } = string.Empty;
            public ContentItem contentDetails { get; init; }
        }

        public record ContentItem
        {
            public string videoId { get; init; } = string.Empty;

            public DateTimeOffset videoPublishedAt { get; init; }
        }
    }
}
