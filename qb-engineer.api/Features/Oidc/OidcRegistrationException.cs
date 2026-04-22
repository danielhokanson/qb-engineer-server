namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Thrown by registration and client-management handlers to surface an RFC 7591 / RFC 7592
/// error envelope. The controller catches this and emits a 400 (or 401 for auth failures)
/// with the <c>error</c> + <c>error_description</c> body the spec expects.
/// </summary>
public class OidcRegistrationException(string error, string description) : Exception(description)
{
    public string Error { get; } = error;
    public string Description { get; } = description;

    public static class Errors
    {
        public const string InvalidRedirectUri = "invalid_redirect_uri";
        public const string InvalidClientMetadata = "invalid_client_metadata";
        public const string InvalidSoftwareStatement = "invalid_software_statement";
        public const string UnapprovedSoftwareStatement = "unapproved_software_statement";
        public const string InvalidToken = "invalid_token";
        public const string AccessDenied = "access_denied";
    }
}
