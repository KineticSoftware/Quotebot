using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Quotebot.Services
{
    internal class ItsWednesdayMyDudesService
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordConfiguration _discordConfiguration;
        private readonly ILogger<ItsWednesdayMyDudesService> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly YouTubeService _youtubeService;
        private PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(5));
        private Task? _task;
        

        public ItsWednesdayMyDudesService(
            DiscordSocketClient client,
            DiscordConfiguration discordConfiguration, 
            YoutubeConfiguration configuration,
            ILogger<ItsWednesdayMyDudesService> logger,
            CancellationTokenSource cancellationTokenSource)
        {
            _client = client;
            _discordConfiguration = discordConfiguration;
            _logger = logger;
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
            _logger.LogInformation($"Begin {nameof(WhenItsWednesdayMyDudes)}");
            using (_timer = new(untilWednesday))
            {
                _logger.LogInformation($"Waiting {nameof(WhenItsWednesdayMyDudes)} ...");
                await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token);
                
                _logger.LogInformation($"Done waiting {nameof(WhenItsWednesdayMyDudes)} ...");

                PlaylistItem? video = await GetRandomWednesdayYouTubeVideos();
                if (video is not null)
                {
                    _logger.LogInformation($"Got video {video.ContentDetails.VideoId}");
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
            _logger.LogInformation($"Announcing the Holy Day in {generalChannelId}");

            IMessageChannel channel = _client.GetChannel(generalChannelId) as IMessageChannel ??
                                      throw new ArgumentNullException($"Channel Id {generalChannelId} was not found");
            await channel.SendMessageAsync($"It is Wednesday my dudes. aaaaaaaaaaaeee! https://www.youtube.com/watch?v={video.ContentDetails.VideoId}");

            _logger.LogInformation($"Done Announcing the Holy Day in {generalChannelId}");
        }

        private TimeSpan GetNextWednesday()
        {
            _logger.LogInformation($"Begin {nameof(GetNextWednesday)}");
            DateTime mountainStandardTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.Now.UtcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
            _logger.LogInformation($"Current Mountain Standard time {mountainStandardTime}");
            DateOnly nextWednesday = NextCalendarDate(mountainStandardTime, DayOfWeek.Wednesday);
            TimeOnly sixAm = TimeOnly.FromTimeSpan(new(17, 0, 0));

            DateTime nextWedsSixAm = nextWednesday.ToDateTime(sixAm);

            var result = nextWedsSixAm.Subtract(DateTimeOffset.Now.DateTime);

            _logger.LogInformation($"Time until next Wednesday {result:d' days 'hh\\:mm\\.ss}");
            _logger.LogInformation($"End {nameof(GetNextWednesday)}");
            return result;
        }

        private DateOnly NextCalendarDate(DateTimeOffset from, DayOfWeek dayOfTheWeek)
        {
            DateOnly nextDay = DateOnly.FromDateTime(from.Date).AddDays(0); // set to 1 to resume normal operations. 
            int delta = ((int) dayOfTheWeek - (int) nextDay.DayOfWeek + 7) % 7;
            nextDay = nextDay.AddDays(delta);

            _logger.LogInformation($"Next Wednesday date: {nextDay.ToString("D")}");
            return nextDay;
        }

        private async Task<PlaylistItem?> GetRandomWednesdayYouTubeVideos()
        {
            _logger.LogInformation($"End {nameof(GetRandomWednesdayYouTubeVideos)}");
            PlaylistItemsResource.ListRequest listRequest = _youtubeService.PlaylistItems.List("contentDetails") ?? throw new Exception("PlaylistItems List request was null");
            listRequest.PlaylistId = @"PLy3-VH7qrUZ5IVq_lISnoccVIYZCMvi-8";

            List<PlaylistItem> results = new();
            do
            {
                _logger.LogInformation($"Begin {nameof(listRequest.ExecuteAsync)}");
                var playlistItemsListResponse = await listRequest.ExecuteAsync();
                _logger.LogInformation($"Finished {nameof(listRequest.ExecuteAsync)}");
                if (playlistItemsListResponse is null)
                    return null;

                results.AddRange(playlistItemsListResponse.Items);
                listRequest.PageToken = playlistItemsListResponse.NextPageToken;

            } while (!string.IsNullOrWhiteSpace(listRequest.PageToken));

            int rand = Random.Shared.Next(0, results.Count);
            _logger.LogInformation($"End {nameof(GetRandomWednesdayYouTubeVideos)}");
            return results[rand];
        }
    }
}
