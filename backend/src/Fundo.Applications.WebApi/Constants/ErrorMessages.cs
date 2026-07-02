namespace Fundo.Applications.WebApi.Constants
{
    public static class ErrorMessages
    {
        public const string UnexpectedError = "An unexpected error occurred while processing the request.";

        public const string MissingCorsConfiguration = "Missing Cors configuration section.";

        public const string CorsPolicyNameRequired = "Cors:PolicyName configuration is required.";

        public const string CorsAllowedOriginsRequired = "Cors:AllowedOrigins configuration must include at least one origin.";
    }
}
