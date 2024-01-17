namespace MusicX.Core.Models.Genius;

public record TranslationSong(
    string Type,
    string ApiPath,
    int Id,
    string Language,
    string LyricsState,
    string Path,
    string Title,
    string Url
);