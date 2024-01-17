namespace MusicX.Core.Models.Genius;

public record User(
    string Type,
    string AboutMeSummary,
    string ApiPath,
    Avatar Avatar,
    string HeaderImageUrl,
    string HumanReadableRoleForDisplay,
    int Id,
    int Iq,
    bool IsMemeVerified,
    bool IsVerified,
    string Login,
    string Name,
    string RoleForDisplay,
    string Url,
    CurrentUserMetadata CurrentUserMetadata
);