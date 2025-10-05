using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IContactRepository
    {
        Task<Contact> AddAsync(Contact contact, CancellationToken ct = default);
        Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Contact contact, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        Task<(IReadOnlyList<Contact> Items, int Total)> SearchAsync(
            string? q, int page, int pageSize, CancellationToken ct = default);

        Task<IReadOnlyList<string>> GetEmailsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

        // ✅ Resolver por NOMBRE (insensible a may/min; exacto y, si no encuentra, parcial)
        Task<IReadOnlyList<string>> GetEmailsByNamesAsync(
            IEnumerable<string> names,
            bool allowPartialMatch = true,
            CancellationToken ct = default);

        Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
    }
}
