using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Category;
using VkNet.Abstractions.Core;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.AntiCaptcha;

namespace VkNet.Extensions.DependencyInjection;

public class VkApi : IVkApi
{
    private readonly IVkApiAuthAsync _auth;
    private readonly IVkApiInvoke _invoke;
    private readonly ILanguageService _languageService;
    private readonly IVkApiCategories _categories;
    private readonly IVkTokenStore _tokenStore;
    private readonly ICaptchaHandler _captchaHandler;
    private readonly IAuthorizationFlow _authorizationFlow;
    private readonly INeedValidationHandler _needValidationHandler;
    private readonly IVkApiVersionManager _vkApiVersion;

    public VkApi(IVkApiAuthAsync auth, IVkApiInvoke invoke, ILanguageService languageService,
                 IVkApiCategories categories,
                 IAuthorizationFlow authorizationFlow,
                 INeedValidationHandler needValidationHandler, IVkApiVersionManager vkApiVersion,
                 IVkTokenStore tokenStore, ICaptchaHandler captchaHandler,
                 ICaptchaSolver? captchaSolver = null)
    {
        _auth = auth;
        _invoke = invoke;
        _languageService = languageService;
        _categories = categories;
        CaptchaSolver = captchaSolver;
        _authorizationFlow = authorizationFlow;
        _needValidationHandler = needValidationHandler;
        _vkApiVersion = vkApiVersion;
        _tokenStore = tokenStore;
        _captchaHandler = captchaHandler;
    }

    public void Dispose()
    {
    }

    public void Authorize(IApiAuthParams @params) =>
        _auth.Authorize(@params);

    public void Authorize(ApiAuthParams @params) =>
        _auth.Authorize(@params);

    public void RefreshToken(Func<string>? code = null) =>
        _auth.RefreshToken(code);

    public void LogOut() => _auth.LogOut();

    public bool IsAuthorized => _auth.IsAuthorized;

    public Task AuthorizeAsync(IApiAuthParams @params) =>
        _auth.AuthorizeAsync(@params);

    public Task RefreshTokenAsync(Func<string>? code = null) =>
        _auth.RefreshTokenAsync(code);

    public Task LogOutAsync() => _auth.LogOutAsync();

    public IUsersCategory Users => _categories.Users;
    public IFriendsCategory Friends => _categories.Friends;
    public IStatusCategory Status => _categories.Status;
    public IMessagesCategory Messages => _categories.Messages;
    public IGroupsCategory Groups => _categories.Groups;
    public IAudioCategory Audio =>  _categories.Audio;
    public IDatabaseCategory Database => _categories.Database;
    public IUtilsCategory Utils => _categories.Utils;
    public IWallCategory Wall => _categories.Wall;
    public IBoardCategory Board => _categories.Board;
    public IFaveCategory Fave => _categories.Fave;
    public IVideoCategory Video => _categories.Video;
    public IAccountCategory Account => _categories.Account;
    public IPhotoCategory Photo => _categories.Photo;
    public IDocsCategory Docs => _categories.Docs;
    public ILikesCategory Likes => _categories.Likes;
    public IPagesCategory Pages => _categories.Pages;
    public IAppsCategory Apps => _categories.Apps;
    public INewsFeedCategory NewsFeed => _categories.NewsFeed;
    public IStatsCategory Stats => _categories.Stats;
    public IGiftsCategory Gifts => _categories.Gifts;
    public IMarketsCategory Markets => _categories.Markets;
    public IAuthCategory Auth => _categories.Auth;
    public IExecuteCategory Execute => _categories.Execute;
    public IPollsCategory PollsCategory => _categories.PollsCategory;
    public ISearchCategory Search => _categories.Search;
    public IStorageCategory Storage => _categories.Storage;
    public IAdsCategory Ads => _categories.Ads;
    public INotificationsCategory Notifications => _categories.Notifications;
    public IWidgetsCategory Widgets => _categories.Widgets;
    public ILeadsCategory Leads => _categories.Leads;
    public IStreamingCategory Streaming => _categories.Streaming;
    public IPlacesCategory Places => _categories.Places;
    public INotesCategory Notes { get => _categories.Notes; set => throw new NotSupportedException(); }
    public IAppWidgetsCategory AppWidgets { get => _categories.AppWidgets; set => throw new NotSupportedException(); }
    public IOrdersCategory Orders { get => _categories.Orders; set => throw new NotSupportedException(); }
    public ISecureCategory Secure { get => _categories.Secure; set => throw new NotSupportedException(); }
    public IStoriesCategory Stories { get => _categories.Stories; set => throw new NotSupportedException(); }
    public ILeadFormsCategory LeadForms { get => _categories.LeadForms; set => throw new NotSupportedException(); }
    public IPrettyCardsCategory PrettyCards { get => _categories.PrettyCards; set => throw new NotSupportedException(); }
    public IPodcastsCategory Podcasts { get => _categories.Podcasts; set => throw new NotSupportedException(); }
    public IDonutCategory Donut => _categories.Donut;
    public IDownloadedGamesCategory DownloadedGames => _categories.DownloadedGames;
    public ICaptchaSolver? CaptchaSolver { get; }

    public int MaxCaptchaRecognitionCount
    {
        get => _captchaHandler.MaxCaptchaRecognitionCount;
        set => _captchaHandler.MaxCaptchaRecognitionCount = value;
    }

    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.Call(methodName, parameters, skipAuthorization);

    public T Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                     params JsonConverter[] jsonConverters) =>
        _invoke.Call<T>(methodName, parameters, skipAuthorization, jsonConverters);

    public Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.CallAsync(methodName, parameters, skipAuthorization);

    public Task<T> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.CallAsync<T>(methodName, parameters, skipAuthorization);

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false) =>
        _invoke.Invoke(methodName, parameters, skipAuthorization);

    public Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters,
                                    bool skipAuthorization = false) =>
        _invoke.InvokeAsync(methodName, parameters, skipAuthorization);

    public DateTimeOffset? LastInvokeTime => _invoke.LastInvokeTime;
    public TimeSpan? LastInvokeTimeSpan => _invoke.LastInvokeTimeSpan;
    
    public VkResponse CallLongPoll(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<VkResponse> CallLongPollAsync(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public string InvokeLongPoll(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public JObject InvokeLongPollExtended(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<string> InvokeLongPollAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<JObject> InvokeLongPollExtendedAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public void SetLanguage(Language language) =>
        _languageService.SetLanguage(language);

    public Language? GetLanguage() =>
        _languageService.GetLanguage();

    public void Validate(string validateUrl) =>
        _needValidationHandler.ValidateAsync(validateUrl).GetAwaiter().GetResult();

    public int RequestsPerSecond { get; set; }

    [Obsolete]
    public IBrowser Browser
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public IAuthorizationFlow AuthorizationFlow
    {
        get => _authorizationFlow;
        set => throw new NotSupportedException("Container is immutable");
    }

    public INeedValidationHandler NeedValidationHandler
    {
        get => _needValidationHandler;
        set => throw new NotSupportedException("Container is immutable");
    }

    public IVkApiVersionManager VkApiVersion
    {
        get => _vkApiVersion;
        set => throw new NotSupportedException("Container is immutable");
    }

    public string Token => _tokenStore.Token;
    public long? UserId { get; set; }
    public event VkApiDelegate? OnTokenExpires
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }

    public event VkApiDelegate? OnTokenUpdatedAutomatically
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }
}