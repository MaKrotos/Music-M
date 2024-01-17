namespace MusicX.Core.Models.Genius;

public record GeniusResponse<TResponse>(
    Meta Meta,
    TResponse Response
) where TResponse : class;