namespace MusicX.Core.Models.Genius;

public record Medium3(
    string Attribution,
    string Provider,
    string Type,
    string Url,
    int? Start
);