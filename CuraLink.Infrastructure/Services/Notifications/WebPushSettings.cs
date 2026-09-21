using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services.Notifications
{
    public class WebPushSettings
    {
        public string VapidPublicKey { get; set; } = null!;

        public string VapidPrivateKey { get; set; } = null!;

        public string Subject { get; set; } = null!;
    }
}
