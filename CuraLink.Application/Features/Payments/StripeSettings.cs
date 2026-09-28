using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Payments
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
        public string SuccessUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
    }
}
