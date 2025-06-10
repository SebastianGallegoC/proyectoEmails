using Npgsql;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Domain.Interfaces
{
    public class EmailRepository : IEmailRepository
    {
        private readonly string _connectionString;

       
        public EmailRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task SaveEmailAsync(string to,string subject, string body)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
               "INSERT INTO \"GuardadoInformacion\" (\"Destinatario\", \"Asunto\", \"Contenido\") VALUES (@to, @subject, @body)",
    connection);

            command.Parameters.AddWithValue("to", to);
            command.Parameters.AddWithValue("subject", subject);
            command.Parameters.AddWithValue("body", body);

            await command.ExecuteNonQueryAsync();
        }
    }
}
