using MusicX.Core.Models;
using MusicX.Core.Models.General;
using System.Diagnostics;

namespace MusicX.Core.Helpers
{
    public static class VkBlockHelper
    {

        public static ResponseData Proccess(this ResponseData response)
        {

            if (response.Playlists != null)
            {
                foreach (var playlist in response.Playlists)
                {
                    if (playlist.Original != null)
                    {
                        if (playlist.Original?.OwnerId < 0)
                        {
                            var id = (playlist.Original.OwnerId * -1);
                            if (response.Groups != null)
                            {
                                var value = response.Groups.SingleOrDefault(g => g.Id == id);
                                playlist.OwnerName = value?.Name;
                                playlist.groupOwner = value;
                            }

                        }
                        else
                        {
                            var value = response.Profiles.SingleOrDefault(p => p.Id == playlist.Original?.OwnerId);
                            playlist.userOwner = value;
                            playlist.OwnerName = value?.FirstName + " " + value?.LastName;
                        }
                    }
                    else
                    {
                        if (playlist.OwnerId < 0)
                        {
                            var id = (playlist.OwnerId * -1);
                            if (response.Groups != null)
                            {
                                var value = response.Groups.SingleOrDefault(g => g.Id == id);
                                playlist.groupOwner = value;
                                playlist.OwnerName = value?.Name;
                            }
                        }
                        else
                        {
                            var value = response.Profiles.SingleOrDefault(p => p.Id == playlist?.OwnerId);
                            playlist.userOwner = value;
                            playlist.OwnerName = value?.FirstName + " " + value?.LastName;
                        }
                    }
                }
            }

            if (response.Replacements != null)
            {
                foreach (var replaceModel in response.Replacements.ReplacementsModels)
                {
                    foreach (var block in replaceModel.ToBlocks)
                    {
                        ToBlocks(block, response);
                    }
                }
            }

            if (response.Block != null)
            {
                ToBlocks(response.Block, response);
            }

            if (response.Section == null) return response;

            foreach (var block in response.Section.Blocks)
            {
                ToBlocks(block, response);
            }

            var snippetsBannerIndex = response.Section.Blocks.FindIndex(b => b is { Layout.Name: "snippets_banner" });

            if (snippetsBannerIndex >= 0)
            {
                response.Section.Blocks.RemoveAt(snippetsBannerIndex);
                response.Section.Blocks.RemoveAt(snippetsBannerIndex); // excess separator
            }

            response.Section.Blocks.RemoveAll(block =>
                block is { DataType: "radiostations" } or { Layout.Title: "Радиостанции" } ||
                (
                    block is { Banners.Count: > 0 } &&
                    block.Banners.RemoveAll(banner => banner.ClickAction?.Action.Url.Contains("subscription") is true ||
                                                           banner.ClickAction?.Action.Url.Contains("combo") is true ||
                                                           banner.ClickAction?.Action.Url.Contains("https://vk.com/app") is true ||
                                                           banner.ClickAction?.Action.Url.Contains("https://vk.com/vk_music") is true) > 0 &&
                    block.Banners.Count == 0
                )
            );

            for (var i = 0; i < response.Section.Blocks.Count; i++)
            {
                var block = response.Section.Blocks[i];
                if (block.DataType == "action" && block.Layout?.Name == "horizontal_buttons")
                {
                    var refBlockIndex = response.Section.Blocks.FindIndex(b => b.DataType == block.Actions[0].RefDataType && b.Layout?.Name == block.Actions[0].RefLayoutName) - 1;

                    if (refBlockIndex >= 0 && block.Actions[0].Action.Type == "open_section")
                    {
                        response.Section.Blocks[refBlockIndex].Actions.AddRange(block.Actions);
                        response.Section.Blocks.RemoveAt(i);
                        i--;
                    }
                }
            }


            return response;
        }

        private static void ToBlocks(Block block, ResponseData response)
        {
            if (block.AudiosIds != null || block.AudiosIds.Count > 0)
            {
                foreach (var audioStringId in block.AudiosIds)
                {
                    var audioArray = audioStringId.Split('_');

                    var audioId = long.Parse(audioArray[1]);
                    var ownerId = long.Parse(audioArray[0]);

                    var audio = response.Audios.SingleOrDefault(a => a.Id == audioId && a.OwnerId == ownerId);

                    if (audio == null) continue;

                    audio.ParentBlockId = block.Id;

                    block.Audios.Add(audio);

                }
            }

            if (block.PlaylistsIds != null || block.PlaylistsIds.Count > 0)
            {

                if (block.DataType == "music_recommended_playlists")
                {
                    foreach (var lid in block.PlaylistsIds)
                    {
                        var p = response.RecommendedPlaylists.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);
                        if (p == null) continue;

                        var pp = response.Playlists.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);

                        p.Playlist = pp;
                        block.RecommendedPlaylists.Add(p);

                    }

                    foreach (var r in block.RecommendedPlaylists)
                    {
                        foreach (var aid in r.AudiosIds)
                        {
                            var a = response.Audios.SingleOrDefault(b => b.OwnerId + "_" + b.Id == aid);

                            if (a == null) continue;

                            r.Audios.Add(a);
                        }
                    }



                }
                else
                {
                    foreach (var playlistStringId in block.PlaylistsIds)
                    {
                        var playlistArray = playlistStringId.Split('_');

                        var playlistId = long.Parse(playlistArray[1]);
                        var ownerId = long.Parse(playlistArray[0]);

                        var playlist = response.Playlists.SingleOrDefault(p => p.Id == playlistId && p.OwnerId == ownerId);

                        if (playlist == null) continue;

                        block.Playlists.Add(playlist);

                    }
                }
            }

            if (block.CatalogBannerIds != null || block.CatalogBannerIds.Count > 0)
            {
                foreach (var bannerId in block.CatalogBannerIds)
                {

                    var banner = response.CatalogBanners.SingleOrDefault(b => b.Id == bannerId);

                    if (banner == null) continue;

                    block.Banners.Add(banner);

                }
            }

            if (block.LinksIds != null || block.LinksIds.Count > 0)
            {
                foreach (var linkId in block.LinksIds)
                {

                    var link = response.Links.SingleOrDefault(b => b.Id == linkId);

                    if (link == null) continue;

                    block.Links.Add(link);

                }
            }

            if (block.PlaceholdersIds != null || block.PlaceholdersIds.Count > 0)
            {
                foreach (var placeholderId in block.PlaceholdersIds)
                {

                    var placeholder = response.Placeholders.SingleOrDefault(b => b.Id == placeholderId);

                    if (placeholder == null) continue;

                    block.Placeholders.Add(placeholder);

                }
            }

            if (block.SuggestionsIds != null || block.SuggestionsIds.Count > 0)
            {
                foreach (var suggestionId in block.SuggestionsIds)
                {
                    // first instead of single because vk was giving two identical suggestions for some queries
                    var suggestion = response.Suggestions.FirstOrDefault(b => b.Id == suggestionId);

                    if (suggestion == null) continue;

                    block.Suggestions.Add(suggestion);

                }
            }

            if (block.ArtistsIds != null || block.ArtistsIds.Count > 0)
            {
                foreach (var artistId in block.ArtistsIds)
                {
                    var artist = response.Artists.SingleOrDefault(b => b.Id == artistId);

                    if (artist == null) continue;

                    block.Artists.Add(artist);

                }
            }

            if (block.TextIds != null || block.TextIds.Count > 0)
            {
                foreach (var textId in block.TextIds)
                {
                    var text = response.Texts.SingleOrDefault(b => b.Id == textId);

                    if (text == null) continue;

                    block.Texts.Add(text);

                }
            }

            if (block.GroupIds != null || block.GroupIds.Count > 0)
            {
                foreach (var groupId in block.GroupIds)
                {
                    var group = response.Groups.SingleOrDefault(b => b.Id == groupId);

                    if (group == null) continue;

                    block.Groups.Add(group);

                }
            }

            if (block.GroupsItemsIds != null || block.GroupsItemsIds.Count > 0)
            {
                foreach (var groupId in block.GroupsItemsIds)
                {
                    var group = response.Groups.SingleOrDefault(b => b.Id == groupId.Id);

                    if (group == null) continue;

                    block.Groups.Add(group);

                }
            }

            if (block.CuratorsIds != null || block.CuratorsIds.Count > 0)
            {
                foreach (var curatorId in block.CuratorsIds)
                {
                    var curator = response.Curators.SingleOrDefault(b => b.Id == curatorId);

                    if (curator == null) continue;

                    block.Curators.Add(curator);

                }
            }

            if (block.PodcastSliderItemsIds.Count > 0)
            {
                foreach (var psid in block.PodcastSliderItemsIds)
                {
                    try
                    {
                        var ps = response.PodcastSliderItems.SingleOrDefault(b => b.ItemId == psid);
                        if (ps == null) continue;

                        block.PodcastSliderItems.Add(ps);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in psid: {psid}");
                    }
                }
            }

            if (block.PodcastEpisodesIds.Count > 0)
            {
                foreach (var peid in block.PodcastEpisodesIds)
                {
                    var pe = response.PodcastEpisodes.SingleOrDefault(b => b.OwnerId + "_" + b.Id == peid);

                    if (pe == null) continue;

                    block.PodcastEpisodes.Add(pe);

                }
            }

            if (block.LongreadsIds.Count > 0)
            {
                foreach (var lid in block.LongreadsIds)
                {
                    var l = response.Longreads.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);

                    if (l == null) continue;

                    block.Longreads.Add(l);

                }
            }

            if (block.VideosIds.Count > 0)
            {
                foreach (var vid in block.VideosIds)
                {
                    var v = response.Videos.SingleOrDefault(b => b.OwnerId + "_" + b.Id == vid);

                    if (v == null) continue;

                    block.Videos.Add(v);

                }
            }

            if (block.ArtistVideosIds.Count > 0)
            {
                foreach (var vid in block.ArtistVideosIds)
                {
                    var v = response.ArtistVideos.SingleOrDefault(b => b.OwnerId + "_" + b.Id == vid);

                    if (v == null) continue;

                    block.ArtistVideos.Add(v);

                }
            }

            if (block.MusicOwnerIds.Count > 0)
            {
                foreach (var ownerId in block.MusicOwnerIds)
                {
                    var owner = response.MusicOwners.SingleOrDefault(b => b.Id == ownerId);

                    if (owner == null) continue;

                    block.MusicOwners.Add(owner);
                }
            }

            if (block.FollowingUpdateInfoIds.Count > 0)
            {
                foreach (var updateInfoId in block.FollowingUpdateInfoIds)
                {
                    var updateInfo = response.FollowingsUpdateInfos.SingleOrDefault(b => b.Id == updateInfoId);

                    if (updateInfo == null) continue;

                    block.FollowingsUpdateInfos.Add(updateInfo);
                }
            }
        }
    }
}
