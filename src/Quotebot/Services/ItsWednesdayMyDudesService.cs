using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Quotebot.Services
{
    internal class ItsWednesdayMyDudesService
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordConfiguration _discordConfiguration;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly YouTubeService _youtubeService;
        private PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(5));
        private Task? _task;
        

        public ItsWednesdayMyDudesService(
            DiscordSocketClient client, 
            DiscordConfiguration discordConfiguration, 
            YoutubeConfiguration configuration,
            CancellationTokenSource cancellationTokenSource)
        {
            _client = client;
            _discordConfiguration = discordConfiguration;
            _cancellationTokenSource = cancellationTokenSource;

            _youtubeService = new(new BaseClientService.Initializer
            {
                ApiKey = configuration.ApiKey,
                ApplicationName = "QuoteMage"
            });
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
                PlaylistItem? video = await GetRandomWednesdayYouTubeVideos();
                
                if (video is not null)
                {
                    await AnnounceTheHolyDay(video);
                }
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

        private async Task AnnounceTheHolyDay(PlaylistItem video)
        {
            ulong generalChannelId = _discordConfiguration.GeneralChannelId;
            IMessageChannel channel = _client.GetChannel(generalChannelId) as IMessageChannel ??
                                      throw new ArgumentNullException($"Channel Id {generalChannelId} was not found");
            await channel.SendMessageAsync($"It is Wednesday my dudes. aaaaaaaaaaaeee! https://www.youtube.com/watch?v={video.ContentDetails.VideoId}");
        }

        private TimeSpan GetNextWednesday()
        {
            DateOnly nextWednesday = NextCalendarDate(DateTimeOffset.Now, DayOfWeek.Wednesday);
            TimeOnly sixAm = TimeOnly.FromTimeSpan(new(6, 0, 0));

            DateTime nextWedsSixAm = nextWednesday.ToDateTime(sixAm);

            return nextWedsSixAm.Subtract(DateTimeOffset.Now.DateTime);
        }

        private DateOnly NextCalendarDate(DateTimeOffset from, DayOfWeek dayOfTheWeek)
        {
            DateOnly nextDay = DateOnly.FromDateTime(from.Date).AddDays(1);
            int delta = ((int) dayOfTheWeek - (int) nextDay.DayOfWeek + 7) % 7;
            nextDay = nextDay.AddDays(delta);
            return nextDay;
        }

        private async Task<PlaylistItem?> GetRandomWednesdayYouTubeVideos()
        {
            PlaylistItemsResource.ListRequest listRequest = _youtubeService.PlaylistItems.List("contentDetails") ?? throw new Exception("PlaylistItems List request was null");
            listRequest.PlaylistId = @"PLy3-VH7qrUZ5IVq_lISnoccVIYZCMvi-8";

            List<PlaylistItem> results = new();
            do
            {
                var playlistItemsListResponse = await listRequest.ExecuteAsync();
                if (playlistItemsListResponse is null)
                    return null;

                results.AddRange(playlistItemsListResponse.Items);
                listRequest.PageToken = playlistItemsListResponse.NextPageToken;

            } while (!string.IsNullOrWhiteSpace(listRequest.PageToken));

            int rand = Random.Shared.Next(0, results.Count);
            return results[rand];
        }
    }
}
