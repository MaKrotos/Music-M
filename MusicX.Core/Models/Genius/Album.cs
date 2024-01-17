namespace MusicX.Core.Models.Genius;

public record Album(
    string Type,
    string ApiPath,
    string CoverArtThumbnailUrl,
    string CoverArtUrl,
    string FullTitle,
    int Id,
    string Name,
    string NameWithArtist,
    ReleaseDateComponents ReleaseDateComponents,
    string ReleaseDateForDisplay,
    string Url,
    Artist Artist
);