using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Queries.GetChatHistory
{
    public class GetChatHistoryQueryHandler
    : IRequestHandler<GetChatHistoryQuery, List<ChatMessageDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetChatHistoryQueryHandler(
            IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessageDto>> Handle(
            GetChatHistoryQuery request,
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

            var messages = await _context.ChatMessages
                .Where(m => m.AppointmentId == request.AppointmentId)
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto(
                    m.Id,
                    m.AppointmentId,
                    m.SenderId,
                    m.ReceiverId,
                    m.Content,
                    m.SentAt,
                    m.IsRead,
                    m.ReadAt))
                .ToListAsync(cancellationToken);

            return messages;
        }
    }
}
