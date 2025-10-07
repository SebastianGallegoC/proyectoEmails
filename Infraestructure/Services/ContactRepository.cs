using System.Text.Json;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ContactRepository : IContactRepository
    {
        private readonly ILogger<ContactRepository> _logger;
        private readonly string _filePath;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private readonly SemaphoreSlim _fileLock = new(1, 1);

        public ContactRepository(IConfiguration cfg, ILogger<ContactRepository> logger)
        {
            _logger = logger;

            var configured = cfg["Storage:ContactsPath"];
            var baseDir = AppContext.BaseDirectory;
            var defaultPath = Path.Combine(baseDir, "App_Data", "contacts.json");

            _filePath = string.IsNullOrWhiteSpace(configured)
                ? defaultPath
                : (Path.IsPathRooted(configured) ? configured : Path.Combine(baseDir, configured));

            var dir = Path.GetDirectoryName(_filePath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
            {
                try
                {
                    File.WriteAllText(_filePath, "[]");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo crear el archivo de contactos en {Path}", _filePath);
                    throw;
                }
            }
        }


        public async Task<Contact> AddAsync(Contact contact, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (contact.Id == Guid.Empty) contact.Id = Guid.NewGuid();
            contact.CreatedAt = contact.CreatedAt == default ? DateTime.UtcNow : contact.CreatedAt;
            contact.UpdatedAt = DateTime.UtcNow;

            await _fileLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await ReadAllUnsafeAsync(ct).ConfigureAwait(false);
                if (list.Any(x => x.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("El correo ya existe.");

                list.Add(contact);
                await WriteAllUnsafeAsync(list, ct).ConfigureAwait(false);
                return contact;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var list = await ReadAllAsync(ct).ConfigureAwait(false);
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task UpdateAsync(Contact contact, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await _fileLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await ReadAllUnsafeAsync(ct).ConfigureAwait(false);
                var idx = list.FindIndex(x => x.Id == contact.Id);
                if (idx < 0) throw new KeyNotFoundException("Contacto no encontrado.");

                // email único (excluyendo a sí mismo)
                if (list.Any(x => x.Id != contact.Id && x.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("El correo ya existe.");

                contact.UpdatedAt = DateTime.UtcNow;
                list[idx] = contact;
                await WriteAllUnsafeAsync(list, ct).ConfigureAwait(false);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await _fileLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await ReadAllUnsafeAsync(ct).ConfigureAwait(false);
                var removed = list.RemoveAll(x => x.Id == id);
                if (removed > 0)
                    await WriteAllUnsafeAsync(list, ct).ConfigureAwait(false);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(IReadOnlyList<Contact> Items, int Total)> SearchAsync(
            string? q, int page, int pageSize, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var list = await ReadAllAsync(ct).ConfigureAwait(false);
            IEnumerable<Contact> query = list;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.Email) && x.Email.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.Phone) && x.Phone.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.Notes) && x.Notes.Contains(term, StringComparison.OrdinalIgnoreCase))
                );
            }
            query = query.OrderBy(x => x.Name ?? x.Email).ThenBy(x => x.Email);

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 5000) pageSize = 5000; 

            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return (items, total);
        }

        public async Task<IReadOnlyList<string>> GetEmailsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var set = ids?.ToHashSet() ?? new HashSet<Guid>();
            if (set.Count == 0) return Array.Empty<string>();

            var all = await ReadAllAsync(ct).ConfigureAwait(false);
            return all.Where(c => set.Contains(c.Id))
                      .Select(c => c.Email)
                      .Where(e => !string.IsNullOrWhiteSpace(e))
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList();
        }

        public async Task<IReadOnlyList<string>> GetEmailsByNamesAsync(
            IEnumerable<string> names,
            bool allowPartialMatch = true,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var incoming = (names ?? Array.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (incoming.Count == 0) return Array.Empty<string>();

            var all = await ReadAllAsync(ct).ConfigureAwait(false);

            var exact = all.Where(c => incoming.Contains(c.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                           .Select(c => c.Email)
                           .Where(e => !string.IsNullOrWhiteSpace(e))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();

            if (!allowPartialMatch) return exact;

            var notMatched = incoming.Where(n => !exact.Any(e => all.Any(c => c.Email.Equals(e, StringComparison.OrdinalIgnoreCase) && string.Equals(c.Name, n, StringComparison.OrdinalIgnoreCase))))
                                     .ToList();

            if (notMatched.Count == 0) return exact;

            var partial = all.Where(c => !string.IsNullOrWhiteSpace(c.Name) &&
                                         notMatched.Any(n => c.Name!.Contains(n, StringComparison.OrdinalIgnoreCase)))
                             .Select(c => c.Email)
                             .Where(e => !string.IsNullOrWhiteSpace(e))
                             .Distinct(StringComparer.OrdinalIgnoreCase);

            return exact.Concat(partial)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(email)) return false;

            var all = await ReadAllAsync(ct).ConfigureAwait(false);
            return all.Any(c =>
                c.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        private async Task<List<Contact>> ReadAllAsync(CancellationToken ct)
        {
            await _fileLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                return await ReadAllUnsafeAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<List<Contact>> ReadAllUnsafeAsync(CancellationToken ct)
        {
            try
            {
                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length == 0) return new List<Contact>();
                var list = await JsonSerializer.DeserializeAsync<List<Contact>>(fs, JsonOptions, ct).ConfigureAwait(false);
                return list ?? new List<Contact>();
            }
            catch (JsonException jex)
            {
                _logger.LogWarning(jex, "Archivo JSON inválido en {Path}. Se inicializa como lista vacía.", _filePath);
                return new List<Contact>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo contactos desde {Path}", _filePath);
                throw;
            }
        }

        private async Task WriteAllAsync(List<Contact> list, CancellationToken ct)
        {
            await _fileLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                await WriteAllUnsafeAsync(list, ct).ConfigureAwait(false);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task WriteAllUnsafeAsync(List<Contact> list, CancellationToken ct)
        {
            try
            {
                using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(fs, list, JsonOptions, ct).ConfigureAwait(false);
                await fs.FlushAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escribiendo contactos en {Path}", _filePath);
                throw;
            }
        }
    }
}
