namespace CashFlow.Api.Security
{
    // Adiciona headers de segurança em todas as respostas, inclusive nas de erro.
    // O OnStarting roda logo antes de enviar a resposta, então os headers não se perdem
    // quando o tratador de exceções limpa a resposta.
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                // Impede o navegador de "adivinhar" o tipo do conteúdo (MIME sniffing)
                headers["X-Content-Type-Options"] = "nosniff";

                // A API não deve ser exibida em iframe (clickjacking)
                headers["X-Frame-Options"] = "DENY";

                // A API só devolve JSON: nenhum script, estilo ou frame deve ser carregado a partir dela
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

                // Não vaza a URL de origem para outros sites
                headers["Referrer-Policy"] = "no-referrer";

                // Desliga recursos do navegador que a aplicação não usa
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

                headers["Cross-Origin-Opener-Policy"] = "same-origin";

                // Dados financeiros não devem ficar em cache de navegador ou proxy
                if (!headers.ContainsKey("Cache-Control"))
                {
                    headers["Cache-Control"] = "no-store";
                }

                return Task.CompletedTask;
            });

            return _next(context);
        }
    }

    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
            => app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
