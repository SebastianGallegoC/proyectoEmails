using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailsP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // requiere JWT
    public class ContactsController : ControllerBase
    {
        private readonly ContactService _svc;

        public ContactsController(ContactService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateContactRequest req)
        {
            var created = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _svc.GetAsync(id);
            if (item is null) return NotFound();
            return Ok(item);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ContactResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            var result = await _svc.SearchAsync(q, page, pageSize);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateContactRequest req)
        {
            var updated = await _svc.UpdateAsync(id, req);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}
