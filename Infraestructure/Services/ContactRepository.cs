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
        private readonly string _filePath;
        private readonly ILogger<ContactRepository> _logger;
        private static readonly SemaphoreSlim _lock = new(1, 1);

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
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
        }

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private async Task<List<Contact>> ReadAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                using var fs = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return await JsonSerializer.DeserializeAsync<List<Contact>>(fs, _json) ?? new List<Contact>();
            }
            finally { _lock.Release(); }
        }

        private async Task WriteAllAsync(List<Contact> items)
        {
            await _lock.WaitAsync();
            try
            {
                using var fs = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(fs, items, _json);
            }
            finally { _lock.Release(); }
        }

        public async Task<Contact> AddAsync(Contact contact, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            if (list.Any(x => x.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("El correo ya existe.");
            list.Add(contact);
            await WriteAllAsync(list);
            return contact;
        }

        public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task UpdateAsync(Contact contact, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            var idx = list.FindIndex(x => x.Id == contact.Id);
            if (idx < 0) throw new KeyNotFoundException("Contacto no encontrado.");
            list[idx] = contact;
            await WriteAllAsync(list);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            list.RemoveAll(x => x.Id == id);
            await WriteAllAsync(list);
        }

        public async Task<(IReadOnlyList<Contact> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            IEnumerable<Contact> query = list;

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    (x.Name?.ToLowerInvariant().Contains(q) ?? false) ||
                    (x.Email?.ToLowerInvariant().Contains(q) ?? false) ||
                    (x.Phone?.ToLowerInvariant().Contains(q) ?? false));
            }

            var total = query.Count();
            var items = query
                .OrderByDescending(x => x.IsFavorite)
                .ThenBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, total);
        }

        public async Task<IReadOnlyList<string>> GetEmailsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var set = new HashSet<Guid>(ids ?? Array.Empty<Guid>());
            var list = await ReadAllAsync();

            return list.Where(x => set.Contains(x.Id) && !string.IsNullOrWhiteSpace(x.Email))
                       .Select(x => x.Email!)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToList();
        }

        public async Task<IReadOnlyList<string>> GetEmailsByNamesAsync(IEnumerable<string> names, bool allowPartialMatch = true, CancellationToken ct = default)
        {
            var norms = (names ?? Array.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            if (norms.Length == 0) return Array.Empty<string>();

            var list = await ReadAllAsync();

            // 1) coincidencia exacta (case-insensitive)
            var exact = list.Where(c => !string.IsNullOrWhiteSpace(c.Name)
                                     && norms.Contains(c.Name!.Trim().ToLowerInvariant())
                                     && !string.IsNullOrWhiteSpace(c.Email))
                            .Select(c => c.Email!)
                            .ToList();

            if (!allowPartialMatch)
                return exact.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // 2) si faltaron, coincidencia parcial (contains)
            var missing = norms.Where(n =>
                !list.Any(c => !string.IsNullOrWhiteSpace(c.Name)
                            && c.Name!.Trim().ToLowerInvariant() == n)).ToArray();

            var partial = list.Where(c => !string.IsNullOrWhiteSpace(c.Name)
                                       && missing.Any(n => c.Name!.Trim().ToLowerInvariant().Contains(n))
                                       && !string.IsNullOrWhiteSpace(c.Email))
                              .Select(c => c.Email!)
                              .ToList();

            return exact.Concat(partial).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
        {
            var list = await ReadAllAsync();
            return list.Any(x =>
                x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}
