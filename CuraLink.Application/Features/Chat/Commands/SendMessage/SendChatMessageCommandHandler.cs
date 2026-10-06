using CuraLink.Application.Common.Interfaces.Chat;
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.ChatMessages;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Chat.Commands.SendMessage
{
    public class SendChatMessageCommandHandler
        : IRequestHandler<SendChatMessageCommand, SendChatMessageResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IChatRealtimeService _chatRealtimeService;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public SendChatMessageCommandHandler(
            IApplicationDbContext context,
            IChatRealtimeService chatRealtimeService,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _context = context;
            _chatRealtimeService = chatRealtimeService;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        public async Task<SendChatMessageResponse> Handle(
            SendChatMessageCommand request,
            CancellationToken cancellationToken)
        {
            // =========================================================
            // 1. Validate message content
            // =========================================================

            var content = request.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "Message content cannot be empty.");
            }

            if (content.Length > 2000)
            {
                throw new ArgumentException(
                    "Message cannot exceed 2000 characters.");
            }

            // =========================================================
            // 2. Get appointment with doctor and patient
            // =========================================================

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

            // =========================================================
            // 3. Get participants' user IDs
            // =========================================================

            var senderUserId = request.SenderUserId;

            var doctorUserId =
                appointment.Doctor.ApplicationUserId;

            var patientUserId =
                appointment.Patient.ApplicationUserId;

            string receiverUserId;

            // =========================================================
            // 4. Determine receiver
            // =========================================================

            if (senderUserId == patientUserId)
            {
                // Patient -> Doctor
                receiverUserId = doctorUserId;
            }
            else if (senderUserId == doctorUserId)
            {
                // Doctor -> Patient
                receiverUserId = patientUserId;
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "You are not a participant in this consultation.");
            }

            // =========================================================
            // 5. Validate appointment status
            // =========================================================

            if (appointment.Status != AppointmentStatus.Paid)
            {
                throw new InvalidOperationException(
                    "Chat is only available for an active consultation.");
            }

            // =========================================================
            // 6. Get sender information
            // =========================================================

            var senderUser = await _userRepository.GetByIdAsync(
                senderUserId,
                cancellationToken);

            if (senderUser is null)
            {
                throw new KeyNotFoundException(
                    "Sender user was not found.");
            }

            var senderName =
                $"{senderUser.FirstName} {senderUser.LastName}".Trim();

            // Add Dr. prefix when sender is the doctor
            if (senderUserId == doctorUserId)
            {
                senderName = $"Dr. {senderName}";
            }

            // =========================================================
            // 7. Create chat message
            // =========================================================

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

            // =========================================================
            // 8. Save message
            // =========================================================

            await _context.SaveChangesAsync(cancellationToken);

            // =========================================================
            // 9. Build realtime response
            // =========================================================

            var response = new SendChatMessageResponse(
                message.Id,
                message.AppointmentId,
                message.SenderId,
                message.ReceiverId,
                message.Content,
                message.SentAt,
                message.IsRead);

            // =========================================================
            // 10. Send message through SignalR
            // =========================================================

            await _chatRealtimeService.SendMessageAsync(
                receiverUserId,
                response);

            // =========================================================
            // 11. Send notification
            // =========================================================

            await _notificationService.SendAsync(
                receiverUserId,
                "New Message",
                $"You have a new message from {senderName}.",
                cancellationToken);

            // =========================================================
            // 12. Return response
            // =========================================================

            return response;
        }
    }
}