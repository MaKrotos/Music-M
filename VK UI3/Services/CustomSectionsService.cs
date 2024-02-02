using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
using VK_UI3.Helpers;
using MusicX.Shared.ListenTogether.Radio;
using NLog;
using VK_UI3.Services;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using Button = MusicX.Core.Models.Button;

namespace MusicX.Services;

public class CustomSectionsService : ICustomSectionsService
{
    public const string CustomLinkRegex = @"^[c-]?\d*$";

    private readonly IVkApiCategories _vkCategories;
    private readonly IVkApiInvoke _apiInvoke;
    private readonly UserRadioService _userRadioService;
    private readonly Logger _logger;

    public CustomSectionsService(IVkApiCategories vkCategories, IVkApiInvoke apiInvoke, UserRadioService userRadioService, Logger logger)
    {
        _vkCategories = vkCategories;
        _apiInvoke = apiInvoke;
        _userRadioService = userRadioService;
        _logger = logger;
    }

    public async IAsyncEnumerable<Section> GetSectionsAsync()
    {
        /*yield return new()
        {
            Title = "РљР°С‚Р°Р»РѕРіРё",
            Id = "profiles",
            Url = "https://vk.com/profiles"
        };*/
        yield return new()
        {
            Title = "РџРѕРёСЃРє",
            Id = "search"
        };
    }

    public async ValueTask<ResponseData?> HandleSectionRequest(string id, string? nextFrom) =>
        id switch
        {
            "profiles" => new()
            {
                Section = await GetCatalogsSectionAsync()
            },
            "attachments_full" => new()
            {
                Section = await GetAttachmentConvsSectionAsync(nextFrom)
            },
            "search" => new()
            {
                Section = await GetSearchSectionAsync()
            },
            _ when Regex.IsMatch(id, CustomLinkRegex) => await GetAttachmentsSectionAsync(id, nextFrom),
            _ => null
        };

    private async Task<Section> GetSearchSectionAsync()
    {
        var vkService = StaticService.Container.GetRequiredService<VkService>();
        var response = await vkService.GetAudioSearchAsync();

        response.Catalog.Sections[0].Blocks[1].Suggestions = response.Suggestions;

        return response.Catalog.Sections[0];
    }

    private async Task<ResponseData> GetAttachmentsSectionAsync(string id, string? startFrom)
    {
        var peerId = long.Parse(id);

        var (attachments, nextFrom) = await _apiInvoke.CallAsync<MessagesGetAttachmentsResponse>("messages.getHistoryAttachments", new()
        {
            ["peer_id"] = peerId.ToString(),
            ["media_type"] = "audio",
            ["start_from"] = startFrom
        });

        if (attachments.Length == 0 && startFrom is null)
            return new()
            {
                Section = new()
                {
                    Id = id,
                    Blocks = new()
                    {
                        new()
                        {
                            Id = Random.Shared.Next().ToString(),
                            DataType = "none",
                            Layout = new()
                            {
                                Name = "header_extended",
                                Title = "РќРёС‡РµРіРѕ РЅРµ РЅР°Р№РґРµРЅРѕ"
                            }
                        },
                    }
                }
            };

        if (attachments.Length == 0)
            return new()
            {
                Section = new()
                {
                    Id = id
                }
            };

        var audios = attachments.Select(b =>
        {
            b.Attachment.Audio.ParentBlockId = id;
            return b.Attachment.Audio;
        }).ToList();

        var response = new ResponseData
        {
            Section = new()
            {
                Id = id,
                Blocks = new()
                {
                    new()
                    {
                        DataType = "music_audios",
                        Layout = new()
                        {
                            Name = "list"
                        },
                        Audios = audios
                    }
                },
                NextFrom = nextFrom!
            },
            Audios = audios
        };

        if (startFrom is null)
            response.Section.Blocks.InsertRange(0, new Block[]
            {
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header",
                        Title = "РўСЂРµРєРё РёР· РІР»РѕР¶РµРЅРёР№"
                    }
                },
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "action",
                    Layout = new()
                    {
                        Name = "horizontal"
                    },
                    Actions = new()
                    {
                        new()
                        {
                            BlockId = id,
                            Action = new()
                            {
                                Type = "create_playlist"
                            }
                        }
                    }
                }
            });

        return response;
    }

    private async Task<Section> GetAttachmentConvsSectionAsync(string? startFrom)
    {
        ulong? offset = startFrom is null ? null : ulong.Parse(startFrom);

        var convs = await _vkCategories.Messages.GetConversationsAsync(new()
        {
            Extended = true,
            Offset = offset
        });

        return new()
        {
            Id = "attachments_full",
            NextFrom = offset.GetValueOrDefault() + (ulong)convs.Items.Count < (ulong)convs.Count
                ? (offset.GetValueOrDefault() + (ulong)convs.Items.Count).ToString()
                : null!,
            Blocks = new()
            {
                MapLinksBlock(convs, true)
            }
        };
    }

    private async Task<Section> GetCatalogsSectionAsync()
    {
        var convs = await _vkCategories.Messages.GetConversationsAsync(new()
        {
            Extended = true,
            Count = 10
        });

        List<Station> stations = null;
        try
        {
            stations = await _userRadioService.GetStationsList();

        }
        catch (Exception ex)
        {
            _logger.Info($"РћС€РёР±РєР° РїРѕР»СѓС‡РµРЅРёСЏ СЃРїРёСЃРєР° СЂР°РґРёРѕСЃС‚Р°РЅС†РёР№: {ex}");
            _logger.Error(ex);
        }

        var buttons = convs.Count > 10
            ? new()
            {
                new()
                {
                    Title = "РџРѕРєР°Р·Р°С‚СЊ РІСЃРµ",
                    SectionId = "attachments_full"
                }
            }
            : new List<Button>();

        return new()
        {
            Title = "РџСЂРѕС„РёР»Рё",
            Id = "profiles",
            Blocks = new()
            {
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header_extended",
                        Title = "Р’Р»РѕР¶РµРЅРёСЏ"
                    },
                    Buttons = buttons
                },
                MapLinksBlock(convs),

                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "separator",
                    },
                },


                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header_extended",
                        Title = "Р Р°РґРёРѕСЃС‚Р°РЅС†РёРё РїРѕР»СЊР·РѕРІР°С‚РµР»РµР№"
                    }
                },

                MapStationsBlock(stations),

                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "separator",
                    },
                },

                GetPlaceholderBlock(),
            }
        };
    }

    private Block GetPlaceholderBlock()
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "placeholder",
            Placeholders = new List<Placeholder>()
                    {
                        new Placeholder()
                        {
                            Text = "Р­С‚РѕС‚ СЂР°Р·РґРµР» Р±СѓРґРµС‚ РїРѕРїРѕР»РЅСЏС‚СЃСЏ СЃРѕ РІСЂРµРјРµРЅРµРј :) РЎР»РµРґРёС‚Рµ Р·Р° РЅРѕРІРѕСЃС‚СЏРјРё!",
                            Id = "jksdfksdkf",
                            Title = "Р­С‚Рѕ РµС‰С‘ РЅРµ РІСЃРµ!",
                            Icons = new List<Core.Models.Image>()
                            {
                                new Core.Models.Image()
                                {
                                    Url = "https://sun2-17.userapi.com/O1eJDSj3KbMqaJMxBP46CTWtWTLytlS-4JSrEA/X8a7Q4les5o.png"
                                }
                            },
                            Buttons = new List<Button>
                            {
                                new Button()
                                {
                                    Title = "РўРµР»РµРіСЂР°Рј РєР°РЅР°Р»",
                                    Action = new ActionButton()
                                    {
                                        Url = "https://t.me/MusicXPlayer",
                                        Type = "custom_open_browser"
                                    }
                                }
                            }
                        }
                    },
        };
    }


    private Block MapStationsBlock(List<Station> stations)
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),

            DataType = "stations",

            Layout = new()
            {
                Name = "large_slider"
            },

            Stations = stations
        };
    }

    private static Block MapLinksBlock(GetConversationsResult convs, bool full = false)
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "links",
            Layout = new()
            {
                Name = full ? "list" : "large_slider"
            },
            Links = convs.Items.Where(b => b.Conversation.CanWrite.Allowed).Select(b =>
            {
                var type = b.Conversation.Peer.Type;
                var id = b.Conversation.Peer.Id.ToString();

                var name = true switch
                {
                    _ when type == ConversationPeerType.Chat => b.Conversation.ChatSettings.Title,
                    _ when type == ConversationPeerType.Group => convs.Groups.Single(g =>
                        g.Id == b.Conversation.Peer.LocalId).Name,
                    _ when type == ConversationPeerType.User => convs.Profiles.Single(g =>
                        g.Id == b.Conversation.Peer.Id).GetFullName(),
                    _ => "<unnamed>"
                };

                return new Link
                {
                    Id = id,
                    Url = $"https://vk.com/history{id}_audio",
                    Title = name,
                    Meta = new()
                    {
                        ContentType = true switch
                        {
                            _ when type == ConversationPeerType.Chat => "chat",
                            _ when type == ConversationPeerType.Group => "group",
                            _ when type == ConversationPeerType.User => "user",
                            _ => string.Empty
                        }
                    },
                    Image = true switch
                    {
                        _ when type == ConversationPeerType.Chat => b.Conversation.ChatSettings
                            .ToImageList(convs.Profiles),
                        _ when type == ConversationPeerType.Group => convs.Groups.Single(g =>
                            g.Id == b.Conversation.Peer.LocalId).ToImageList(),
                        _ when type == ConversationPeerType.User => convs.Profiles.Single(g =>
                            g.Id == b.Conversation.Peer.Id).ToImageList(),
                        _ => new()
                    }
                };
            }).ToList()
        };
    }
}

public record MessagesGetAttachmentsResponse(MessagesGetAttachments[] Items, string? NextFrom);
public record MessagesGetAttachments(MessagesGetAttachmentsAttachment Attachment);
public record MessagesGetAttachmentsAttachment(Audio Audio);

