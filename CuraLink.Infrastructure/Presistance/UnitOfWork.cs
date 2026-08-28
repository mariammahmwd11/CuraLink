using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Infrastructure.Presistance.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
      

        public IDoctorRepository Doctors { get; }

        public IUserRepository Users { get; }
       public IDoctorDocumentRepository DoctorDocuments { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IDoctorRepository doctorRepository,
            IUserRepository userRepository,IDoctorDocumentRepository doctorDocumentRepository)
        {
            _context = context;
            Doctors = doctorRepository;
            Users = userRepository;
            DoctorDocuments = doctorDocumentRepository;
        }
        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
