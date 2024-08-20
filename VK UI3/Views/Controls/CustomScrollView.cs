using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Controls
{
    public sealed class CustomScrollView : ScrollView
    {

        public CustomScrollView() : base()
        {
          PointerWheelChanged += OnPointerWheelChanged;

            // —оздаем вектор скорости прокрутки
            Vector2 scrollVelocity = new Vector2(0, 100); // 100 пикселей в секунду по вертикали

            // ƒобавл€ем скорость прокрутки
            this.AddScrollVelocity(scrollVelocity, null);
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
         
        }

        protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
        {
            // ¬аша логика обработки прокрутки колесиком мыши
            base.OnPointerWheelChanged(e);
        }

    }



}
