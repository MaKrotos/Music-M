namespace MusicX.Core.Models.Genius;

public record PrimaryTag(
    string Type,
    int Id,
    string Name,
    bool Primary,
    string Url
);