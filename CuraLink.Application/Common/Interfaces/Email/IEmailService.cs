using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendDoctorActivationEmailAsync(
            string email,
            string doctorName);

        Task SendDoctorRejectionEmailAsync(
            string email,
            string doctorName,
            string rejectionReason);

        Task SendAssistantInvitationEmailAsync(
      string email,
      string clinicName,
      string token);
    }
}