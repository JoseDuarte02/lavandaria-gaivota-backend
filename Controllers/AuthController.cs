// Controllers/AuthController.cs
using LavandariaGaivotaAPI.Data; // Para ApplicationDbContext, se precisar diretamente (geralmente não no controller)
using LavandariaGaivotaAPI.Models; // Para ApplicationUser
using LavandariaGaivotaAPI.Dtos;  // Para os DTOs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace LavandariaGaivotaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Opcional, se for usar Roles
        private readonly IConfiguration _configuration;
        // private readonly SignInManager<ApplicationUser> _signInManager; // Mais para auth baseada em cookie. Para JWT, UserManager é suficiente.

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, // Injete se for usar Roles
            IConfiguration configuration
            /* SignInManager<ApplicationUser> signInManager */)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            // _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) // Validação do DTO (DataAnnotations)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email!);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status409Conflict,
                    new { Status = "Error", Message = "Utilizador com este email já existe!" });
            }

            ApplicationUser user = new()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerDto.Email,
                FullName = registerDto.FullName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password!);

            if (!result.Succeeded)
            {
                // Constrói uma lista de erros do Identity
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                // Retorna um BadRequest (400) com os erros de validação do Identity
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "Falha na criação do utilizador devido a erros de validação.",
                    Errors = identityErrors // Envia a lista de descrições de erro
                });
            }

            // ... (código para adicionar role, se houver) ...

            return StatusCode(StatusCodes.Status201Created,
                new { Status = "Success", Message = "Utilizador criado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email!);
            if (user == null)
            {
                return Unauthorized(new { Status = "Error", Message = "Credenciais inválidas." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password!);
            if (!passwordValid)
            {
                return Unauthorized(new { Status = "Error", Message = "Credenciais inválidas." });
            }

            // Se chegou aqui, o utilizador é válido, gerar o token JWT
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // ID do utilizador
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único para o token
                // Adicione outros claims que queira (ex: Nome, Roles)
                // new Claim(ClaimTypes.Name, user.UserName), // Ou user.FullName se o tiver
            };

            // Opcional: Adicionar roles do utilizador aos claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var tokenValidityInMinutes = Convert.ToInt32(jwtSettings["TokenValidityInMinutes"] ?? "60"); // Default para 60 minutos

            var validAudiences = jwtSettings["ValidAudience"]?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .ToList();


            if (validAudiences != null)
            {
                foreach (var audience in validAudiences)
                {
                    authClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
                }
            }
            var token = new JwtSecurityToken(
    issuer: jwtSettings["ValidIssuer"],
    audience: null, // As audiences estão agora na lista de claims
    expires: DateTime.UtcNow.AddMinutes(tokenValidityInMinutes),
    claims: authClaims,
    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
);

            return Ok(new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email!);
            if (user == null)
            {
                // ATENÇÃO: Por segurança, não revele que o utilizador não existe.
                // Retorne sempre uma mensagem genérica de sucesso.
                return Ok(new { Status = "Success", Message = "Se existir uma conta com este email, um link de recuperação foi enviado." });
            }

            // Controllers/AuthController.cs -> método ForgotPassword
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedEmail = HttpUtility.UrlEncode(user.Email!); // Codifique o email também, por segurança

            // O link que aponta para o frontend, combinando o token na rota e o email na query string
            var resetLink = $"http://localhost:3000/resetar-password/{encodedToken}?email={encodedEmail}";

            // Imprime na consola para teste
            Console.WriteLine("---- Link de Reset de Password (para fins de teste) ----");
            Console.WriteLine(resetLink);
            Console.WriteLine("---------------------------------------------------------");


            // *** AQUI ENTRARIA A LÓGICA DE ENVIO DE EMAIL ***
            // Exemplo com um serviço de email fictício:
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            // Por agora, o link será apenas impresso na consola do backend para que possa testar.

            return Ok(new { Status = "Success", Message = "Se existir uma conta com este email, um link de recuperação foi enviado." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email!);
            if (user == null)
            {
                // Novamente, uma mensagem genérica para não revelar informações
                return BadRequest(new { Status = "Error", Message = "Falha ao redefinir a password. O pedido é inválido." });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token!, resetPasswordDto.NewPassword!);

            if (result.Succeeded)
            {
                return Ok(new { Status = "Success", Message = "Password redefinida com sucesso!" });
            }

            // Se a redefinição falhar (ex: token inválido), retorne os erros
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Status = "Error", Message = "Falha ao redefinir a password.", Errors = errors });
        }

        [Authorize] // Garante que apenas utilizadores autenticados podem aceder
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword!, changePasswordDto.NewPassword!);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { Message = "Falha ao alterar a password.", Errors = errors });
            }

            return Ok(new { Status = "Success", Message = "Password alterada com sucesso." });
        }
    }
}