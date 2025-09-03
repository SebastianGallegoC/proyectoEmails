using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Models;
//using proyectoEmails.Models;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromForm] EmailRequest request)
    {
        var emailSender = new EmailService();

        try
        {
            await emailSender.SendAndSaveEmailAsync(request.To, request.Subject, request.Body, request.Attachments);
            return Ok("Correo(s) enviado(s) y guardado(s) correctamente.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un error interno: {ex.Message}");
        }
    }
}
