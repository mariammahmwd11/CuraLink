using CuraLink.Application.Common.Interfaces.Chat;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.ChatMessages;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.SendMessage
{
    public class SendChatMessageCommandHandler
    : IRequestHandler<SendChatMessageCommand, SendChatMessageResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IChatRealtimeService _chatRealtimeService;

        public SendChatMessageCommandHandler(
            IApplicationDbContext context,
            IChatRealtimeService chatRealtimeService)
        {
            _context = context;
            _chatRealtimeService = chatRealtimeService;
        }

        public async Task<SendChatMessageResponse> Handle(
            SendChatMessageCommand request,
            CancellationToken cancellationToken)
        {
            var content = request.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Message content cannot be empty.");

            if (content.Length > 2000)
                throw new ArgumentException(
                    "Message cannot exceed 2000 characters.");

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(
                    a => a.Id == request.AppointmentId,
                    cancellationToken);

            if (appointment is null)
                throw new KeyNotFoundException(
                    "Appointment was not found.");

            var senderUserId = request.SenderUserId;

            var doctorUserId = appointment.Doctor.ApplicationUserId;
            var patientUserId = appointment.Patient.ApplicationUserId;

            string receiverUserId;

            if (senderUserId == patientUserId)
            {
                receiverUserId = doctorUserId;
            }
            else if (senderUserId == doctorUserId)
            {
                receiverUserId = patientUserId;
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "You are not a participant in this consultation.");
            }

            if (appointment.Status != AppointmentStatus.Paid)
            {
                throw new InvalidOperationException(
                    "Chat is only available for an active consultation.");
            }

            var message = new ChatMessage
            {
                AppointmentId = appointment.Id,
                SenderId = senderUserId,
                ReceiverId = receiverUserId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);

            await _context.SaveChangesAsync(cancellationToken);

            var response = new SendChatMessageResponse(
                message.Id,
                message.AppointmentId,
                message.SenderId,
                message.ReceiverId,
                message.Content,
                message.SentAt,
                message.IsRead);

            await _chatRealtimeService.SendMessageAsync(
                receiverUserId,
                response);

            return response;
        }
    }
}
