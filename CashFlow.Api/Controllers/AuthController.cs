using CashFlow.Api.Dtos;
using CashFlow.Api.Entities;
using CashFlow.Api.Infrastructure;
using CashFlow.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)] // limita tentativas de login/cadastro por IP (força bruta)
    public class AuthController : ControllerBase
    {
        private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

        // Usado para gastar o mesmo tempo de hash quando o e-mail não existe,
        // evitando que o tempo de resposta revele quais e-mails estão cadastrados.
        private static readonly Usuario UsuarioFicticio = new();

        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly TokenService _tokens;

        public AuthController(AppDbContext context, IPasswordHasher<Usuario> hasher, TokenService tokens)
        {
            _context = context;
            _hasher = hasher;
            _tokens = tokens;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegistrarRequest request)
        {
            var email = NormalizarEmail(request.Email);

            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            {
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Este e-mail já está cadastrado.");
            }

            var usuario = new Usuario { Email = email };
            usuario.SenhaHash = _hasher.HashPassword(usuario, request.Senha);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new UsuarioResponse(usuario.Id, usuario.Email));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = NormalizarEmail(request.Email);
            var usuario = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == email);

            if (usuario is null)
            {
                _hasher.HashPassword(UsuarioFicticio, request.Senha);
                return CredenciaisInvalidas();
            }

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return CredenciaisInvalidas();
            }

            // Se o algoritmo/iterações do hash ficaram defasados, atualiza o hash no login
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.SenhaHash = _hasher.HashPassword(usuario, request.Senha);
                await _context.SaveChangesAsync();
            }

            return Ok(_tokens.Gerar(usuario));
        }

        // Mesma resposta para "e-mail não existe" e "senha errada" (não revela qual dos dois falhou)
        private ObjectResult CredenciaisInvalidas()
            => Problem(statusCode: StatusCodes.Status401Unauthorized, title: MensagemCredenciaisInvalidas);

        private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
