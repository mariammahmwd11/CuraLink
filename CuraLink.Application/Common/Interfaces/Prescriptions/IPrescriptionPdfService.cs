using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Prescriptions
{
    public interface IPrescriptionPdfService
    {
        Task<byte[]> GenerateAsync(
            Guid prescriptionId,
            CancellationToken cancellationToken = default);
    }
}
