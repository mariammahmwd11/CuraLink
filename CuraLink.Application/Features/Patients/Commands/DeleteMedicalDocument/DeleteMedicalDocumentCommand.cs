using MediatR;

namespace CuraLink.Application.Features.Patients.Commands.DeleteMedicalDocument;

public class DeleteMedicalDocumentCommand : IRequest<bool>
{
    public string UserId { get; set; } = null!;

    public int DocumentId { get; set; }
}