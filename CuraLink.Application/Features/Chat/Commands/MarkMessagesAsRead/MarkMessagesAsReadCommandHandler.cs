using CuraLink.Application.Common.Interfaces.Chat;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.MarkMessagesAsRead
{
    public class MarkMessagesAsReadCommandHandler
     : IRequestHandler<
         MarkMessagesAsReadCommand,
         MarkMessagesAsReadResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IChatRealtimeService _chatRealtimeService;

        public MarkMessagesAsReadCommandHandler(
            IApplicationDbContext context,
            IChatRealtimeService chatRealtimeService)
        {
            _context = context;
            _chatRealtimeService = chatRealtimeService;
        }

        public async Task<MarkMessagesAsReadResponse> Handle(
            MarkMessagesAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(
                    a => a.Id == request.AppointmentId,
                    cancellationToken);

            if (appointment is null)
            {
                throw new KeyNotFoundException(
                    "Appointment was not found.");
            }

            var doctorUserId = appointment.Doctor.ApplicationUserId;
            var patientUserId = appointment.Patient.ApplicationUserId;

            if (request.UserId != doctorUserId &&
                request.UserId != patientUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not a participant in this consultation.");
            }

            var unreadMessages = await _context.ChatMessages
                .Where(m =>
                    m.AppointmentId == request.AppointmentId &&
                    m.ReceiverId == request.UserId &&
                    !m.IsRead)
                .ToListAsync(cancellationToken);

            if (unreadMessages.Count == 0)
            {
                return new MarkMessagesAsReadResponse(
                    request.AppointmentId,
                    0,
                    DateTime.UtcNow);
            }

            var readAt = DateTime.UtcNow;

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = readAt;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var senderUserIds = unreadMessages
                .Select(m => m.SenderId)
                .Distinct()
                .ToList();

            foreach (var senderUserId in senderUserIds)
            {
                await _chatRealtimeService.SendReadReceiptAsync(
                    senderUserId,
                    new
                    {
                        appointmentId = request.AppointmentId,
                        messageIds = unreadMessages
                            .Where(m => m.SenderId == senderUserId)
                            .Select(m => m.Id)
                            .ToList(),
                        readAt
                    });
            }

            return new MarkMessagesAsReadResponse(
                request.AppointmentId,
                unreadMessages.Count,
                readAt);
        }
    }
}
