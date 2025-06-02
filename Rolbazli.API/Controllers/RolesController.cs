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

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO createRoleDto)
        {
            if (string.IsNullOrEmpty(createRoleDto.RoleName))
            {
                return BadRequest("Role name is required");
            }

            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
            if (roleExist)
            {
                return BadRequest("Role already exist");
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));
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
