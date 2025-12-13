using System.Security.Claims;
using ForumApi.Services;
using ForumAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize()]
        [HttpGet("previews")]
        public async Task<IActionResult> GetUserPreviews()
        {
            var users = await _userService.GetAllAsync();

            var previews = users.Select(u => new UserPreviewDto
            {
                Id = u.Id,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl
            });

            return Ok(previews);
        }

        // GET: api/user
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl
            });

            return Ok(userDtos);
        }


        // GET: api/user/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl
            };

            return Ok(userDto);
        }




        // PUT: api/user/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();


            var isAdmin = User.IsInRole("admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            if (currentUserId != id && !isAdmin)
                return Forbid("You can only update your own profile.");


            user.Email = updateUserDto.Email ?? user.Email;
            user.Username = updateUserDto.Username ?? user.Username;
            user.Bio = updateUserDto.Bio ?? user.Bio;
            user.AvatarUrl = updateUserDto.AvatarUrl ?? user.AvatarUrl;

            if (isAdmin && updateUserDto.Role != null)
            {
                user.Role = updateUserDto.Role;
            }

            await _userService.UpdateAsync(user);
            return Ok("User updated successfully.");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var isAdmin = User.IsInRole("admin");
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            if (currentUserId != id && !isAdmin)
                return Forbid("You can only delete your own profile.");
        
            await _userService.DeleteAsync(id);
            return Ok("User deleted successfully.");
        }
    

        [Authorize]
        [HttpGet("claims-test")]
        public IActionResult ClaimsTest()
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            // Or return it in the response so you can see it in Postman / browser
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }

    }
}