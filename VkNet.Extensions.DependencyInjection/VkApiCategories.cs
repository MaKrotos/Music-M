using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.Abstractions.Category;

namespace VkNet.Extensions.DependencyInjection;

public class VkApiCategories : IVkApiCategories
{
    private readonly IServiceProvider _provider;

    public VkApiCategories(IUsersCategory users, IFriendsCategory friends, IStatusCategory status, IAudioCategory audio,
                           IDatabaseCategory database, IUtilsCategory utils, IWallCategory wall, IBoardCategory board,
                           IFaveCategory fave, IVideoCategory video, IAccountCategory account, IPhotoCategory photo,
                           IDocsCategory docs, ILikesCategory likes, IPagesCategory pages, IAppsCategory apps,
                           INewsFeedCategory newsFeed, IStatsCategory stats, IGiftsCategory gifts,
                           IMarketsCategory markets, IAuthCategory auth,
                           IPollsCategory pollsCategory, ISearchCategory search, IStorageCategory storage,
                           IAdsCategory ads, INotificationsCategory notifications,
                           ILeadsCategory leads, IAppWidgetsCategory appWidgets, IOrdersCategory orders,
                           ISecureCategory secure, IStoriesCategory stories, ILeadFormsCategory leadForms,
                           IPrettyCardsCategory prettyCards, IPodcastsCategory podcasts, IDonutCategory donut,
                           IDownloadedGamesCategory downloadedGames, IServiceProvider provider)
    {
        _provider = provider;
        Users = users;
        Friends = friends;
        Status = status;
        Audio = audio;
        Database = database;
        Utils = utils;
        Wall = wall;
        Board = board;
        Fave = fave;
        Video = video;
        Account = account;
        Photo = photo;
        Docs = docs;
        Likes = likes;
        Pages = pages;
        Apps = apps;
        NewsFeed = newsFeed;
        Stats = stats;
        Gifts = gifts;
        Markets = markets;
        Auth = auth;
        PollsCategory = pollsCategory;
        Search = search;
        Storage = storage;
        Ads = ads;
        Notifications = notifications;
        Widgets = null!; // maybe later
        Leads = leads;
        Streaming = null!; // maybe later
        Places = null!; // maybe later
        Notes = null!; // maybe later
        AppWidgets = appWidgets;
        Orders = orders;
        Secure = secure;
        Stories = stories;
        LeadForms = leadForms;
        PrettyCards = prettyCards;
        Podcasts = podcasts;
        Donut = donut;
        DownloadedGames = downloadedGames;
    }

    public IUsersCategory Users { get; }
    public IFriendsCategory Friends { get; }
    public IStatusCategory Status { get; }
    public IMessagesCategory Messages => _provider.GetRequiredService<IMessagesCategory>(); // i hate this world
    public IGroupsCategory Groups => _provider.GetRequiredService<IGroupsCategory>(); // i hate this world much 
    public IAudioCategory Audio { get; }
    public IDatabaseCategory Database { get; }
    public IUtilsCategory Utils { get; }
    public IWallCategory Wall { get; }
    public IBoardCategory Board { get; }
    public IFaveCategory Fave { get; }
    public IVideoCategory Video { get; }
    public IAccountCategory Account { get; }
    public IPhotoCategory Photo { get; }
    public IDocsCategory Docs { get; }
    public ILikesCategory Likes { get; }
    public IPagesCategory Pages { get; }
    public IAppsCategory Apps { get; }
    public INewsFeedCategory NewsFeed { get; }
    public IStatsCategory Stats { get; }
    public IGiftsCategory Gifts { get; }
    public IMarketsCategory Markets { get; }
    public IAuthCategory Auth { get; }
    public IExecuteCategory Execute => _provider.GetRequiredService<IExecuteCategory>(); // i hate this world even more
    public IPollsCategory PollsCategory { get; }
    public ISearchCategory Search { get; }
    public IStorageCategory Storage { get; }
    public IAdsCategory Ads { get; }
    public INotificationsCategory Notifications { get; }
    public IWidgetsCategory Widgets { get; }
    public ILeadsCategory Leads { get; }
    public IStreamingCategory Streaming { get; }
    public IPlacesCategory Places { get; }
    public INotesCategory Notes { get; set; }
    public IAppWidgetsCategory AppWidgets { get; set; }
    public IOrdersCategory Orders { get; set; }
    public ISecureCategory Secure { get; set; }
    public IStoriesCategory Stories { get; set; }
    public ILeadFormsCategory LeadForms { get; set; }
    public IPrettyCardsCategory PrettyCards { get; set; }
    public IPodcastsCategory Podcasts { get; set; }
    public IDonutCategory Donut { get; }
    public IDownloadedGamesCategory DownloadedGames { get; }
}