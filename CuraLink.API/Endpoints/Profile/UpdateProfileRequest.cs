namespace CuraLink.API.Endpoints.Profile;

public class UpdateProfileRequest
{
    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public IFormFile? ProfilePhoto { get; set; }
}