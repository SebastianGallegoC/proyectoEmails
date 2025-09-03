using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Models
{
    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}