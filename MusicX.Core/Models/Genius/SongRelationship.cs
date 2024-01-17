namespace MusicX.Core.Models.Genius;

public record SongRelationship(
    string Type,
    string RelationshipType,
    IReadOnlyList<Song> Songs
);