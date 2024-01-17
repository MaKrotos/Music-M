namespace MusicX.Core.Models.Genius;

public record Stats(
    int AcceptedAnnotations,
    int Contributors,
    int IqEarners,
    int Transcribers,
    int UnreviewedAnnotations,
    int VerifiedAnnotations,
    bool Hot,
    int Pageviews
);