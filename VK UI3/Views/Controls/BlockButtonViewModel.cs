using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using MusicX.Core.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using VK_UI3.Controllers;
using VK_UI3.Views.LoginWindow;
using VK_UI3.VKs;
using WinUI3.Common;
using Button = MusicX.Core.Models.Button;

namespace VK_UI3.Views.Controls
{
    public class BlockButtonViewModel : Microsoft.UI.Xaml.Controls.Button, INotifyPropertyChanged
    {
        private Button _action;

        public Button Action
        {
            get => _action; set
            {
                _action = value;
                if (value is not null)
                    Refresh();
            }
        }

        public Artist Artist { get; set; }
        public Block ParentBlock { get; set; }

        public BlockButtonViewModel()
        {
            InvokeCommand = new RelayCommand(Invoke);
        }

        public BlockButtonViewModel(Button action, Artist artist = null, Block parentBlock = null) : this()
        {
            Artist = artist;
            ParentBlock = parentBlock;
            Action = action;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Refresh()
        {
            switch (Action.Action.Type)
            {
                case "toggle_artist_subscription" when Artist is not null && ParentBlock is not null:
                    Icon = new SymbolIcon(Artist.IsFollowed ? Symbol.UnFavorite : Symbol.Favorite);
                    Text = Artist.IsFollowed ? "Отписаться" : "Подписаться";
                    break;
                case "play_shuffled_audios_from_block":
                    Icon = new SymbolIcon(Symbol.Audio);
                    Text = "Перемешать все";
                    break;
                case "create_playlist":
                    Icon = new SymbolIcon(Symbol.Add);
                    Text = "Создать плейлист";
                    break;
                case "play_audios_from_block":
                    Icon = new SymbolIcon(Symbol.Play);
                    Text = "Слушать всё";
                    break;
                case "open_section":
                    Icon = new SymbolIcon(Symbol.OpenFile);
                    Text = Action.Title ?? "Открыть";
                    break;
                case "music_follow_owner":
                    Icon = new SymbolIcon(Action.IsFollowing ? Symbol.SolidStar : Symbol.Add);
                    Text = Action.IsFollowing ? "Вы подписаны на музыку" : "Подписаться на музыку";
                    break;
                case "open_url":
                    Icon = new SymbolIcon(Symbol.Link);
                    Text = Action.Title;
                    break;
                default:
                    Icon = new SymbolIcon(Symbol.Accept);
                    Text = "content";
                    break;
            }
            OnPropertyChanged();
        }

        public SymbolIcon Icon { get; set; }

        public string Text { get; set; } = string.Empty;

        public ICommand InvokeCommand { get; }

        private async void Invoke()
        {
            try
            {
                switch (Action.Action.Type)
                {
                    case "toggle_artist_subscription" when Artist is not null && ParentBlock is not null:
                        {
                            var vkService = VK.vkService;

                            if (Artist.IsFollowed)
                                await vkService.UnfollowArtist(Action.ArtistId, ParentBlock.Id);
                            else
                                await vkService.FollowArtist(Action.ArtistId, ParentBlock.Id);

                            Artist.IsFollowed = !Artist.IsFollowed;
                            Refresh();
                            break;
                        }
                    case "play_shuffled_audios_from_block" or "play_audios_from_block":
                        {

                            Task task = new Task(async() =>
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
                // StaticService.Container.GetRequiredService<Logger>().Error(e, "Failed to invoke action {Type}", Action.Action.Type);
            }
        }
    }

}
