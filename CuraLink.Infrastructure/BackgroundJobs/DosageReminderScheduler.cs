using CuraLink.Application.Common.Interfaces.BackgroundJobs;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.BackgroundJobs
{
    public class DosageReminderScheduler : IDosageReminderScheduler
    {
        public Task ScheduleAsync(
            Guid dosageScheduleId,
            DateTime scheduledAtUtc)
        {
            var delay = scheduledAtUtc - DateTime.UtcNow;

            if (delay <= TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            BackgroundJob.Schedule<DosageReminderJob>(
                job => job.Execute(dosageScheduleId),
                delay);

            return Task.CompletedTask;
        }
    }
}
