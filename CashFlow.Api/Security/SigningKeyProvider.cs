using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Api.Security
{
    // Fornece a mesma chave para quem assina (TokenService) e para quem valida (JwtBearer).
    public sealed class SigningKeyProvider
    {
        private const int TamanhoMinimoBytes = 32; // HS256 exige chave de pelo menos 256 bits

        public SymmetricSecurityKey Key { get; }

        public SigningKeyProvider(IOptions<JwtSettings> options, IHostEnvironment env, ILogger<SigningKeyProvider> logger)
        {
            var chave = options.Value.Key;

            if (string.IsNullOrWhiteSpace(chave))
            {
                // Fora do ambiente de desenvolvimento a API não sobe sem chave configurada (fail fast)
                if (!env.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Jwt:Key não configurada. Defina a variável de ambiente Jwt__Key com pelo menos 32 bytes.");
                }

                // Em desenvolvimento gera uma chave aleatória: o projeto continua plug-and-play,
                // sem segredo commitado. Os tokens deixam de valer quando a API reinicia.
                logger.LogWarning("Jwt:Key não configurada. Usando chave aleatória temporária (somente desenvolvimento).");
                Key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(chave);
            if (bytes.Length < TamanhoMinimoBytes)
            {
                throw new InvalidOperationException($"Jwt:Key precisa ter pelo menos {TamanhoMinimoBytes} bytes.");
            }

            Key = new SymmetricSecurityKey(bytes);
        }
    }
}
