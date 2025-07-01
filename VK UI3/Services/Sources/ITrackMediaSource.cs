using MusicX.Services.Player.Sources;
using MusicX.Shared.Player;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model.Attachments;
using Windows.Media.Playback;
using VK_UI3.Controllers;

namespace MusicX.Services.Player;

public interface ITrackMediaSource
{
    Task<bool> OpenWithMediaPlayerAsync(CustomMediaPlayer player, Audio track, CancellationToken cancellationToken = default, AudioEqualizer equalizer = null);
}