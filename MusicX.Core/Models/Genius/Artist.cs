namespace MusicX.Core.Models.Genius;

public record Artist(
    string Type,
    string ApiPath,
    string HeaderImageUrl,
    int Id,
    string ImageUrl,
    string IndexCharacter,
    bool IsMemeVerified,
    bool IsVerified,
    string Name,
    string Slug,
    string Url
);