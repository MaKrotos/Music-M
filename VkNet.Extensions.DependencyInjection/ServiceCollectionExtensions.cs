using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions;
using VkNet.Abstractions.Category;
using VkNet.Abstractions.Core;
using VkNet.Categories;
using VkNet.Utils;

namespace VkNet.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddVkNet(this IServiceCollection collection)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
        
        collection.TryAddSingleton<IVkApiInvoke, VkApiInvoke>();
        collection.TryAddSingleton<ICaptchaHandler, AsyncCaptchaHandler>();
        collection.TryAddSingleton<IVkApiAuthAsync, VkApiAuth>();
        collection.TryAddSingleton<IVkTokenStore, DefaultVkTokenStore>();
        collection.TryAddSingleton<IVkApi, VkApi>();
        collection.TryAddSingleton<IAsyncCaptchaSolver>(_ => null!);

        collection.TryAddTransient<IVkApiAuth>(s => s.GetRequiredService<IVkApiAuthAsync>());
        collection.TryAddTransient<IVkInvoke>(s => s.GetRequiredService<IVkApi>());
        collection.TryAddSingleton<IAsyncRateLimiter>(_ => new AsyncRateLimiter(TimeSpan.FromSeconds(1), 3));
        
        collection.RegisterDefaultDependencies();
        
        AddCategories(collection);
    }

    private static void AddCategories(IServiceCollection collection)
    {
        collection.TryAddSingleton<IVkApiCategories, VkApiCategories>();
        
        collection.TryAddSingleton<IUsersCategory, UsersCategory>();
        collection.TryAddSingleton<IFriendsCategory, FriendsCategory>();
        collection.TryAddSingleton<IStatsCategory, StatsCategory>();
        collection.TryAddSingleton<IMessagesCategory, MessagesCategory>();
        collection.TryAddSingleton<IGroupsCategory, GroupsCategory>();
        collection.TryAddSingleton<IAudioCategory, AudioCategory>();
        collection.TryAddSingleton<IDatabaseCategory, DatabaseCategory>();
        collection.TryAddSingleton<IUtilsCategory, UtilsCategory>();
        collection.TryAddSingleton<IWallCategory, WallCategory>();
        collection.TryAddSingleton<IBoardCategory, BoardCategory>();
        collection.TryAddSingleton<IFaveCategory, FaveCategory>();
        collection.TryAddSingleton<IVideoCategory, VideoCategory>();
        collection.TryAddSingleton<IAccountCategory, AccountCategory>();
        collection.TryAddSingleton<IPhotoCategory, PhotoCategory>();
        collection.TryAddSingleton<IDocsCategory, DocsCategory>();
        collection.TryAddSingleton<ILikesCategory, LikesCategory>();
        collection.TryAddSingleton<IPagesCategory, PagesCategory>();
        collection.TryAddSingleton<IAppsCategory, AppsCategory>();
        collection.TryAddSingleton<INewsFeedCategory, NewsFeedCategory>();
        collection.TryAddSingleton<IStatsCategory, StatsCategory>();
        collection.TryAddSingleton<IGiftsCategory, GiftsCategory>();
        collection.TryAddSingleton<IMarketsCategory, MarketsCategory>();
        collection.TryAddSingleton<IAuthCategory, AuthCategory>();
        collection.TryAddSingleton<IExecuteCategory, ExecuteCategory>();
        collection.TryAddSingleton<IPollsCategory, PollsCategory>();
        collection.TryAddSingleton<ISearchCategory, SearchCategory>();
        collection.TryAddSingleton<IStorageCategory, StorageCategory>();
        collection.TryAddSingleton<IAdsCategory, AdsCategory>();
        collection.TryAddSingleton<INotificationsCategory, NotificationsCategory>();
        collection.TryAddSingleton<IWidgetsCategory, WidgetsCategory>();
        collection.TryAddSingleton<ILeadsCategory, LeadsCategory>();
        collection.TryAddSingleton<IStreamingCategory, StreamingCategory>();
        collection.TryAddSingleton<IPlacesCategory, PlacesCategory>();
        collection.TryAddSingleton<INotesCategory, NotesCategory>();
        collection.TryAddSingleton<IAppWidgetsCategory, AppWidgetsCategory>();
        collection.TryAddSingleton<IOrdersCategory, OrdersCategory>();
        collection.TryAddSingleton<ISecureCategory, SecureCategory>();
        collection.TryAddSingleton<IStoriesCategory, StoriesCategory>();
        collection.TryAddSingleton<ILeadFormsCategory, LeadFormsCategory>();
        collection.TryAddSingleton<IPrettyCardsCategory, PrettyCardsCategory>();
        collection.TryAddSingleton<IPodcastsCategory, PodcastsCategory>();
        collection.TryAddSingleton<IDonutCategory, DonutCategory>();
        collection.TryAddSingleton<IDownloadedGamesCategory, DownloadedGamesCategory>();
        collection.TryAddSingleton<IStatusCategory, StatusCategory>();
    }
}