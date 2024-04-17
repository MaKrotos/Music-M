namespace MusicX.Core.Models.Genius;

public record GeniusSearchResponse(
IReadOnlyList<Hit> Hits
);