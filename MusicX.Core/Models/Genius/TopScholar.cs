namespace MusicX.Core.Models.Genius;

public record TopScholar(
    string Type,
    double AttributionValue,
    object PinnedRole,
    User User
);