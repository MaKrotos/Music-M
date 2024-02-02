using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeText
    {
        string textNow = null;
        Storyboard storyboard = null;
        TextBlock textBlockControl = null;
        Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue = null;

        public AnimationsChangeText(TextBlock textBlockControl, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            this.textBlockControl = textBlockControl;
            this.dispatcherQueue = dispatcherQueue;
        }

        public void ChangeTextWithAnimation(string newText)
        {
            if (textNow != null && textNow == newText)
                return;
            textNow = newText;

            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard == null) storyboard = new Storyboard();
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }

                // Create opacity animation
                var animation = new DoubleAnimation
                {
                    From = textBlockControl.Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(50),
                };

                // Create Storyboard object to control the animation
                storyboard = new Storyboard();
                Storyboard.SetTarget(animation, textBlockControl);
                Storyboard.SetTargetProperty(animation, "Opacity");

                // Add animation to Storyboard
                storyboard.Children.Add(animation);

                // Handle animation completion event
                storyboard.Completed += async (s, e) =>
                {
                    // Change the text after the animation completes
                    if (newText == null || newText == "null") return;
                    textBlockControl.Text = newText;

                    var animation = new DoubleAnimation
                    {
                        From = textBlockControl.Opacity,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                    };
                    // Create Storyboard object to control the animation
                    storyboard = new Storyboard();
                    Storyboard.SetTarget(animation, textBlockControl);
                    Storyboard.SetTargetProperty(animation, "Opacity");
                    // Add animation to Storyboard
                    storyboard.Children.Add(animation);
                    storyboard.Begin();
                };

                // Start the animation
                storyboard.Begin();
            });
        }
    }
}
