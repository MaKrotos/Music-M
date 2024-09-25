using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeText
    {
        string textNow = null;
        Storyboard storyboard = new Storyboard();
        TextBlock textBlockControl = null;
        double opac;
        Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue = null;

        public AnimationsChangeText(TextBlock textBlockControl, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            this.textBlockControl = textBlockControl;
            this.dispatcherQueue = dispatcherQueue;
            opac = textBlockControl.Opacity;
        }

        public void ChangeTextWithAnimation(string newText)
        {
            if (textNow != null && textNow == newText)
                return;
            textNow = newText;

            dispatcherQueue.TryEnqueue(async () =>
            {
                if (storyboard.GetCurrentState() == ClockState.Active)
                {
                    storyboard.Pause();
                }

                // Create opacity animation
                var animation = new DoubleAnimation
                {
                    From = textBlockControl.Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(250),
                };

                // Set the target of the animation
                Storyboard.SetTarget(animation, textBlockControl);
                Storyboard.SetTargetProperty(animation, "Opacity");

                // Clear the storyboard and add the new animation
                storyboard.Stop();
                storyboard.Children.Clear();
                storyboard.Children.Add(animation);

                // Handle animation completion event
                storyboard.Completed -= AnimationCompleted;
                storyboard.Completed += AnimationCompleted;

                // Start the animation
                storyboard.Begin();

            });
        }

        private void AnimationCompleted(object sender, object e)
        {
            // Unsubscribe from the event
            storyboard.Completed -= AnimationCompleted;

            // Change the text after the animation completes
            if (textNow == null || textNow == "null") return;
            textBlockControl.Text = textNow;

            var animation = new DoubleAnimation
            {
                From = textBlockControl.Opacity,
                To = opac,
                Duration = TimeSpan.FromMilliseconds(250),
            };
            // Set the target of the animation
            Storyboard.SetTarget(animation, textBlockControl);
            Storyboard.SetTargetProperty(animation, "Opacity");
            // Clear the storyboard and add the new animation
            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

}
