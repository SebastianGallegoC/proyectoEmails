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
            if (await _repo.EmailExistsAsync(req.Email, null, ct))
                throw new InvalidOperationException("Ya existe un contacto con ese correo.");

            var entity = new Contact
            {
                Name = req.Name.Trim(),
                Email = req.Email.Trim(),
                Phone = req.Phone?.Trim(),
                Notes = req.Notes?.Trim(),
                IsFavorite = req.IsFavorite,
                IsBlocked = req.IsBlocked
            };

            var saved = await _repo.AddAsync(entity, ct);
            return Map(saved);
        }

        public async Task<ContactResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var c = await _repo.GetByIdAsync(id, ct);
            return c is null ? null : Map(c);
        }

        public async Task<PagedResult<ContactResponse>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(q, page, pageSize, ct);
            return new PagedResult<ContactResponse>
            {
                Items = items.Select(Map).ToArray(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ContactResponse> UpdateAsync(Guid id, UpdateContactRequest req, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Contacto no encontrado.");

            if (await _repo.EmailExistsAsync(req.Email, id, ct))
                throw new InvalidOperationException("Ya existe otro contacto con ese correo.");

            entity.Name = req.Name.Trim();
            entity.Email = req.Email.Trim();
            entity.Phone = req.Phone?.Trim();
            entity.Notes = req.Notes?.Trim();
            entity.IsFavorite = req.IsFavorite;
            entity.IsBlocked = req.IsBlocked;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity, ct);
            return Map(entity);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);

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
