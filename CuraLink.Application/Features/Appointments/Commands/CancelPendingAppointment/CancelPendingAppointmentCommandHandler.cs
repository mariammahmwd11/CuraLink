using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Commands.CancelPendingAppointment
{
    public class CancelPendingAppointmentCommandHandler
     : IRequestHandler<CancelPendingAppointmentCommand, bool>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IApplicationDbContext _context;

        public CancelPendingAppointmentCommandHandler(
            IPatientRepository patientRepository, IApplicationDbContext context)
        {
            _patientRepository = patientRepository;
            _context = context;
        }

        public async Task<bool> Handle(CancelPendingAppointmentCommand request, CancellationToken ct)
        {
            var patient = await _patientRepository
                .GetByApplicationUserIdAsync(request.ApplicationUserId, ct);
            if (patient is null) return false;

            var appointment = await _context.Appointments.FirstOrDefaultAsync(
                a => a.Id == request.AppointmentId &&
                     a.PatientId == patient.Id &&
                     a.Status == AppointmentStatus.Pending, ct);

            if (appointment is null) return false;

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
