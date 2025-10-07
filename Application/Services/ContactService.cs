using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ContactService
    {
        private readonly IContactRepository _repo;

        public ContactService(IContactRepository repo)
        {
            _repo = repo;
        }

        public async Task<ContactResponse> CreateAsync(CreateContactRequest req, CancellationToken ct = default)
        {
            var entity = new Contact
            {
                Id = Guid.NewGuid(),
                Name = req.Name.Trim(),
                Email = req.Email.Trim(),
                Phone = req.Phone?.Trim(),
                Notes = req.Notes?.Trim(),
                IsFavorite = req.IsFavorite,
                IsBlocked = req.IsBlocked,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (await _repo.EmailExistsAsync(entity.Email, null, ct))
                throw new InvalidOperationException("Ya existe un contacto con ese email.");

            var created = await _repo.AddAsync(entity, ct);
            return Map(created);
        }

        public async Task<ContactResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item is null ? null : Map(item);
        }

        public Task<PagedResult<ContactResponse>> GetAllAsync(CancellationToken ct = default)
            => SearchAsync(null, 1, 1000, ct);

        public async Task<PagedResult<ContactResponse>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(q, page, pageSize, ct);
            return new PagedResult<ContactResponse>
            {
                Items = items.Select(Map).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ContactResponse> UpdateAsync(Guid id, UpdateContactRequest req, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Contacto no encontrado.");

            var incomingEmail = req.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(incomingEmail) &&
                !incomingEmail.Equals(item.Email, StringComparison.OrdinalIgnoreCase) &&
                await _repo.EmailExistsAsync(incomingEmail, id, ct))
            {
                throw new InvalidOperationException("Ya existe un contacto con ese email.");
            }

            item.Name = req.Name?.Trim() ?? item.Name;
            item.Email = incomingEmail ?? item.Email;
            item.Phone = req.Phone?.Trim();
            item.Notes = req.Notes?.Trim();
            item.IsFavorite = req.IsFavorite;
            item.IsBlocked = req.IsBlocked;
            item.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(item, ct);
            return Map(item);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);

        private static ContactResponse Map(Contact c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Notes = c.Notes,
            IsFavorite = c.IsFavorite,
            IsBlocked = c.IsBlocked,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
