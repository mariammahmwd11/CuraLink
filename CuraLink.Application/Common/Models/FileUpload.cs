using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Models
{
    public record FileUpload(
     Stream Content,
     string FileName,
     string ContentType);
}
