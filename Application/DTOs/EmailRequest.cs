using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public class EmailRequest
    {
        public string To { get; set; } = string.Empty; // Cadena con comas como delimitador
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<IFormFile>? Attachments { get; set; } // Permitir null para evitar problemas de nulabilidad
    }
}