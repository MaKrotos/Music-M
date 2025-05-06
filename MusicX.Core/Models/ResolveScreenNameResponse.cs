namespace MusicX.Core.Models;

public record AdConfig(
    int Id,
    bool MobWebEnabled
);

public record EmbeddedUri(
    string OriginalUrl,
    string ViewUrl,
    string ScreenTitle,
    string Type
);

public record ResolvedObject(
    string Type,
    int Id,
    string Title,
    int IsInCatalog,
    int AuthorOwnerId,
    string TrackCode,
    string WebviewUrl,
    int HideTabbar,
    string Icon139,
    string Icon150,
    string Icon278,
    string Icon576,
    string Icon75,
    bool IsVkuiInternal,
    bool HasVkConnect,
    AdConfig AdConfig,
    int MobileControlsType,
    bool NeedShowUnverifiedScreen,
    object PlaceholderInfo
);

public record ResolveScreenNameResponse(
    string Type,
    int ObjectId,
    ResolvedObject Object,
    EmbeddedUri EmbeddedUri
);