using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Helpers.Animations;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class MixControl : UserControl
    {

        // Массив цветов
        Color[] colors = new Color[]
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Yellow,
            Colors.Purple,
            Colors.Orange,
            Colors.Pink,
            Colors.Cyan,
            Colors.Magenta,
            Colors.Lime,
            Colors.Gold,
            Colors.Teal,
            Colors.Salmon,

        };
        public MixControl()
        {
            this.InitializeComponent();

            this.Loading += MixControl_Loading;
        }
        Random random = new Random();
        private void MixControl_Loading(FrameworkElement sender, object args)
        {
          
            foreach (var child in columns.Children)
            {
                if (child is Palka palka)
                {
                    // Установка цвета фона для каждого элемента palka
                    int colorIndex = random.Next(colors.Length);
                    palka.Background = new SolidColorBrush(colors[colorIndex]);
                    palka.StartAnimation();
                }
            }
        }
    }
}
