using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolbazli.API.DTOs;
using Rolbazli.Model.Models;

namespace Rolbazli.API.Controllers
{
    [Authorize(Roles = "Admin")] //Sadece Admin kullanıcısı buraya erişim sağlasın.
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        //[AllowAnonymous]
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO createRoleDto)
        {
            if (string.IsNullOrEmpty(createRoleDto.Name))
            {
                return BadRequest("Role name is required");
            }

            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.Name);
            if (roleExist)
            {
                return BadRequest("Role already exist");
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.Name));
            if (roleResult.Succeeded)
            {
                return Ok(new { message = "Role Created successfully" });
            }

            return BadRequest("Role creation failed.");
        }

        [AllowAnonymous]
        [HttpGet("get-roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = new List<RoleResponseDTO>();

            foreach (var role in roles)
            {
                var userInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleDtos.Add(new RoleResponseDTO
                {
                    Id = role.Id,
                    Name = role.Name,
                    TotalUsers = userInRole.Count
                });
            }
            return Ok(roleDtos);
        }

        // Mevcut bir rolü güncelleme metodu
        [HttpPut("{id}")] // PUT isteği için rota tanımı (örn: api/roles/{id})
        //[Authorize(Roles = "Admin")] // Sadece Admin kullanıcısı rol güncelleyebilir. Ama bunu controller bazında kullanmamız daha mantıklı olur. Her defasında bu attributu yazmayalım diye.
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDTO updateRoleDto)
        {
            // ID boş veya null ise BadRequest dön.
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Role ID is required.");
            }
            // Rol adı boş veya null ise BadRequest dön
            if (string.IsNullOrEmpty(updateRoleDto.Name))
            {
                return BadRequest("Role name is required.");
            }

            // Belirtilen ID'ye sahip rolü bul
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
            {
                return NotFound("Role not found."); // Rol bulunamazsa 404 dön
            }

            // Rol adının değiştirilip değiştirilmediğini kontrol et
            if (role.Name != updateRoleDto.Name)
            {
                // Yeni rol adıyla başka bir rol var mı kontrol et
                var roleExist = await _roleManager.RoleExistsAsync(updateRoleDto.Name);
                if (roleExist)
                {
                    return BadRequest("Role with this name already exists."); // Aynı isimde rol varsa hata dön
                }

                // Rol adını güncelle
                role.Name = updateRoleDto.Name;
                var result = await _roleManager.UpdateAsync(role); // Rolü güncelle
                if (result.Succeeded)
                {
                    // Başarılı olursa 200 OK dön
                    return Ok(new { Message = "Role updated successfully." });
                }
                // Güncelleme başarısız olursa hataları dön
                return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Role update failed.");
            }

            // Rol adı değişmediyse bile başarılı yanıt dönebiliriz.
            return Ok(new { Message = "Role name is the same, no update needed." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoleById(string id)
        {
            //kullanıcının girdiği id'yi bul
            var roleId = await _roleManager.FindByIdAsync(id);
            if (roleId is null)
            {
                return NotFound("Role not found");
            }
            var result = await _roleManager.DeleteAsync(roleId);
            if (result.Succeeded)
            {
                return Ok(new {Message = "Role deleted successfully."});
            }
            return BadRequest("Role deletion failed!..");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO assignRoleDTO)
        {
            var user = await _userManager.FindByIdAsync(assignRoleDTO.UserId);
            if (user is null)
            {
                return NotFound("User not found!..");
            }
            var role = await _roleManager.FindByIdAsync(assignRoleDTO.RoleId);
            if (role is null)
            {
                return NotFound("Role not found!..");
            }
            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (result.Succeeded)
            {
                return Ok(new {Message = "Role assign successfullyy"});
            }
            var error = result.Errors.FirstOrDefault();
            return BadRequest(error!.Description);
        }
    }
}
