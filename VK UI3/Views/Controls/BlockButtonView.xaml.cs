using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using WinUI3.Common;
using Button = MusicX.Core.Models.Button;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Controls
{
    public sealed partial class BlockButtonView : Microsoft.UI.Xaml.Controls.Button
    {
        public BlockButtonView()
        {
            this.InitializeComponent();
            InvokeCommand = new RelayCommand(Invoke);
            changeIcon = new AnimationsChangeIcon(symbolIcon);
            changeText = new AnimationsChangeText(text, this.DispatcherQueue);
        }

        AnimationsChangeIcon changeIcon;
        AnimationsChangeText changeText;


        public Button Action
        {
            get => blockBTN.action; set
            {
                blockBTN.action = value;
                if (value is not null)
                    Refresh();
            }
        }

        public ICommand InvokeCommand { get; }

        public class BlockBTN
        {
            public Artist Artist { get; set; }
            public Block ParentBlock { get; set; }

            public Button action { get; set; }

            public BlockBTN(Button action, Artist artist = null, Block parentBlock = null)
            {
                Artist = artist;
                ParentBlock = parentBlock;
                this.action = action;
            }
        }

        public BlockBTN blockBTN;
        bool firstRefresh = false;
        public void Refresh()
        {
            string txt = "";
            Symbol icon = Symbol.Accept;

            if (Action.Action.Type == "toggle_artist_subscription" && blockBTN.Artist != null && blockBTN.ParentBlock != null)
            {
                txt = blockBTN.Artist.IsFollowed ? "Отписаться" : "Подписаться";
                icon = blockBTN.Artist.IsFollowed ? Symbol.UnFavorite : Symbol.Favorite;
            }
            else if (Action.Action.Type == "play_shuffled_audios_from_block")
            {
                txt = "Перемешать все";
                icon = Symbol.Play;
            }
            else if (Action.Action.Type == "create_playlist")
            {
                txt = "Создать плейлист";
                icon = Symbol.Add;
            }
            else if (Action.Action.Type == "play_audios_from_block")
            {
                txt = "Слушать всё";
                icon = Symbol.Play;
            }
            else if (Action.Action.Type == "open_section")
            {
                txt = Action.Title ?? "Открыть";
                icon = Symbol.OpenFile;
            }
            else if (Action.Action.Type == "music_follow_owner")
            {
                txt = Action.IsFollowing ? "Вы подписаны на музыку" : "Подписаться на музыку";
                icon = Action.IsFollowing ? Symbol.SolidStar : Symbol.Add;
            }
            else if (Action.Action.Type == "open_url")
            {
                txt = Action.Title;
            }
            if (!firstRefresh)
            {



                text.Text = txt;
                symbolIcon.Symbol = icon;

            }
            else
            {
                changeText.ChangeTextWithAnimation(txt);
                changeIcon.ChangeSymbolIconWithAnimation(icon);
            }
        }




        private async void Invoke()
        {
            try
            {
                switch (Action.Action.Type)
                {
                    case "toggle_artist_subscription" when blockBTN.Artist is not null && blockBTN.ParentBlock is not null:
                        {
                            var vkService = VK.vkService;

                            if (blockBTN.Artist.IsFollowed)
                                await vkService.UnfollowArtist(Action.ArtistId, blockBTN.ParentBlock.Id);
                            else
                                await vkService.FollowArtist(Action.ArtistId, blockBTN.ParentBlock.Id);

                            blockBTN.Artist.IsFollowed = !blockBTN.Artist.IsFollowed;
                            Refresh();
                            break;
                        }
                    case "play_shuffled_audios_from_block" or "play_audios_from_block":
                        {
                            Task task = new Task(async () =>
                            {
                                AudioPlayer.PlayList(new SectionAudio(Action.BlockId, this.DispatcherQueue));
                            });
                            task.Start();

                            break;
                        }
                    case "create_playlist":
                        {
                            /*
                            var vkService = VK.vkService;
                            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
                            var viewModel = StaticService.Container.GetRequiredService<CreatePlaylistModalViewModel>();
                            viewModel.IsEdit = false;

                            if (!string.IsNullOrEmpty(Action.BlockId))
                            {
                                viewModel.Tracks.AddRange(await vkService.LoadFullAudiosAsync(Action.BlockId).ToArrayAsync());
                                viewModel.CreateIsEnable = true;
                            }

                            navigationService.OpenModal<CreatePlaylistModal>(viewModel);
                             */
                            break;

                        }
                    case "open_section":
                        {
                            MainView.OpenSection(Action.SectionId);
                            break;
                        }
                    case "music_follow_owner":
                        {
                            var vkService = VK.vkService;

                            if (Action.IsFollowing)
                                await vkService.UnfollowOwner(Action.OwnerId);
                            else
                                await vkService.FollowOwner(Action.OwnerId);

                            Action.IsFollowing = !Action.IsFollowing;
                            Refresh();
                            break;
                        }
                    case "open_url":
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                UseShellExecute = true,
                                FileName = Action.Action.Url
                            });
                            break;
                        }

                }
                Refresh();
            }
            catch (Exception e)
            {
                AppCenterHelper.SendCrash(e);
                // StaticService.Container.GetRequiredService<Logger>().Error(e, "Failed to invoke action {Type}", Action.Action.Type);
            }
        }
    }
}
