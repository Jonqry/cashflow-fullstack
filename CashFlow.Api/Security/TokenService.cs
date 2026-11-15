using System.Security.Claims;
using CashFlow.Api.Dtos;
using CashFlow.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Api.Security
{
    public sealed class TokenService
    {
        private readonly JwtSettings _settings;
        private readonly SigningKeyProvider _keys;
        private readonly JsonWebTokenHandler _handler = new();

        public TokenService(IOptions<JwtSettings> options, SigningKeyProvider keys)
        {
            _settings = options.Value;
            _keys = keys;
        }

        public TokenResponse Gerar(Usuario usuario)
        {
            var agora = DateTime.UtcNow;
            var expiraEm = agora.AddMinutes(_settings.ExpiracaoMinutos);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                IssuedAt = agora,
                NotBefore = agora,
                Expires = expiraEm,
                // Só o necessário no token: id, e-mail e um identificador único (jti). Nada de senha ou dado sensível.
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                ]),
                SigningCredentials = new SigningCredentials(_keys.Key, SecurityAlgorithms.HmacSha256)
            };

            return new TokenResponse(_handler.CreateToken(descriptor), expiraEm);
        }
    }
}
