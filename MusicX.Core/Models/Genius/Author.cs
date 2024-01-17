namespace MusicX.Core.Models.Genius;

public record Author(
    string Type,
    double Attribution,
    object PinnedRole,
    User User
);