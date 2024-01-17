namespace MusicX.Core.Models.Genius;

public record Annotatable(
    string Type,
    string ApiPath,
    ClientTimestamps ClientTimestamps,
    string Context,
    int Id,
    string ImageUrl,
    string LinkTitle,
    string Title,
    string Url
);