using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services.Email
{
    public class EmailSettings
    {
        public string ApiKey { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public string LoginUrl { get; set; } = null!;
        public string AssistantInvitationUrl { get; set; } = null!;
    }
}
