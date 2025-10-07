using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class EmailRequest
    {
     
        [Display(Name = "Destinatarios directos (To)")]
        public List<string> To { get; set; } = new();

    
        [Display(Name = "Contactos por Id")]
        public List<Guid> ToContactIds { get; set; } = new();

   
        [Display(Name = "Contactos por Nombre")]
        public List<string> ToContactNames { get; set; } = new();

      
        [Required(ErrorMessage = "El campo Subject es obligatorio.")]
        [StringLength(200, ErrorMessage = "El asunto no puede tener más de 200 caracteres.")]
        [Display(Name = "Asunto")]
        public string? Subject { get; set; }

  
        [Display(Name = "Cuerpo del mensaje (Body)")]
        public string? Body { get; set; }

      
        [Display(Name = "Adjuntos")]
        public List<IFormFile>? Attachments { get; set; }
    }
}
