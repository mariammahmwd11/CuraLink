using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features
{
    public class UserInfoDto
    {
        public string Id { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
    }
}
