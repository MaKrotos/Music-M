namespace MusicX.Core.Models.Genius;

public record CurrentUserMetadata(
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> ExcludedPermissions,
    Interactions Interactions,
    Relationships Relationships,
    IqByAction IqByAction
);