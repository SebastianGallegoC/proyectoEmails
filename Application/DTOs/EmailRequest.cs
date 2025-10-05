using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class EmailRequest
    {
        public List<string> To { get; set; } = new();

        // ✅ Puedes escribir nombres aquí (ej. "walter velasco")
        public List<string>? ToContactNames { get; set; }

        // Compatibilidad si quieres seguir usando IDs
        public List<Guid>? ToContactIds { get; set; }

        public string Subject { get; set; } = string.Empty;
        public string? Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
