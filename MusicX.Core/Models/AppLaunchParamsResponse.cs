namespace MusicX.Core.Models;

public record AppLaunchParamsResponse(
    long VkUserId,
    long VkAppId,
    bool VkIsAppUser,
    bool VkAreNotificationsEnabled,
    string VkLanguage,
    string VkRef,
    string VkAccessTokenSettings,
    long? VkGroupId,
    string? VkViewerGroupRole,
    string VkPlatform,
    bool VkIsFavorite,
    long VkTs,
    string Sign);