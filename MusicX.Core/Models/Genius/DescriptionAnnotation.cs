namespace MusicX.Core.Models.Genius;

public record DescriptionAnnotation(
    string Type,
    int AnnotatorId,
    string AnnotatorLogin,
    string ApiPath,
    string Classification,
    string Fragment,
    int Id,
    string IosAppUrl,
    bool IsDescription,
    bool IsImage,
    string Path,
    Range Range,
    int SongId,
    string Url,
    IReadOnlyList<object> VerifiedAnnotatorIds,
    CurrentUserMetadata CurrentUserMetadata,
    TrackingPaths TrackingPaths,
    string TwitterShareMessage,
    Annotatable Annotatable,
    IReadOnlyList<Annotation> Annotations
);