using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;


using NLog;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using Button = MusicX.Core.Models.Button;
using MusicX.Core.Services;

namespace VK_UI3.Services;


public class CustomSectionsService : ICustomSectionsService
{
    public const string CustomLinkRegex = @"^[c-]?\d*$";
    
    private readonly IVkApiCategories _vkCategories;
    private readonly IVkApiInvoke _apiInvoke;

    private readonly Logger _logger;

    public CustomSectionsService(IVkApiCategories vkCategories, IVkApiInvoke apiInvoke, Logger logger)
    {
        _vkCategories = vkCategories;
        _apiInvoke = apiInvoke;
   
        _logger = logger;
    }

    public async IAsyncEnumerable<Section> GetSectionsAsync()
    {
        /*yield return new()
        {
            Title = "Каталоги",
            Id = "profiles",
            Url = "https://vk.com/profiles"
        };*/
        yield return new()
        {
            Title = "Поиск",
            Id = "search"
        };
    }

   
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
                                Title = "Ничего не найдено"
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
                        Title = "Треки из вложений"
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

    public ValueTask<ResponseData> HandleSectionRequest(string id, string nextFrom = null)
    {
        throw new NotImplementedException();
    }
}

public record MessagesGetAttachmentsResponse(MessagesGetAttachments[] Items, string? NextFrom);
public record MessagesGetAttachments(MessagesGetAttachmentsAttachment Attachment);
public record MessagesGetAttachmentsAttachment(Audio Audio);