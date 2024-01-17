namespace MusicX.Core.Models.Genius;

public record Tag(
    string Type,
    int Id,
    string Name,
    bool Primary,
    string Url
);