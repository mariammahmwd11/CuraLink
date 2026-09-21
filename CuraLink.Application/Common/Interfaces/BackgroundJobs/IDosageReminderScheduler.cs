using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.BackgroundJobs
{
    public interface IDosageReminderScheduler
    {
        Task ScheduleAsync(
            Guid dosageScheduleId,
            DateTime scheduledAtUtc);
    }
}
