using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Services
{
    internal class ItsWednesdayMyDudesService
    {
        private readonly DiscordSocketClient _client;
        
        private readonly CancellationTokenSource _cancellationTokenSource;
        private PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(5));
        private Task? _task;

        public ItsWednesdayMyDudesService(DiscordSocketClient client, CancellationTokenSource cancellationTokenSource)
        {
            _client = client;
            _cancellationTokenSource = cancellationTokenSource;
        }

        private async Task AnnounceTheHolyDay()
        {
            ulong generalChannelId = 1021213113648947232;
            IMessageChannel channel = _client.GetChannel(generalChannelId) as IMessageChannel ?? throw new ArgumentNullException($"Channel Id {generalChannelId} was not found");
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
    }
}
