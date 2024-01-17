namespace MusicX.Core.Models.Genius;

public record CustomPerformance(
    string Label,
    IReadOnlyList<Artist> Artists
);