using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeFontIcon
    {
        string fontIconNow = null;
        Storyboard storyboard = null;
        FontIcon iconControl = null;

        public AnimationsChangeFontIcon(FontIcon iconControl)
        {
            fontIconNow = iconControl.Glyph;
            this.iconControl = iconControl;
        }

        public async void ChangeFontIconWithAnimation(string newFontIcon)
        {
            if (fontIconNow != null && fontIconNow == newFontIcon)
                return;

            fontIconNow = newFontIcon;
            if (storyboard == null) storyboard = new Storyboard();
            if (storyboard.GetCurrentState() == ClockState.Active)
            {
                storyboard.Pause();

            }

            // РЎРѕР·РґР°РµРј Р°РЅРёРјР°С†РёСЋ РїСЂРѕР·СЂР°С‡РЅРѕСЃС‚Рё
            var animation = new DoubleAnimation
            {
                From = iconControl.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(50),
            };

            // РЎРѕР·РґР°РµРј РѕР±СЉРµРєС‚ Storyboard РґР»СЏ СѓРїСЂР°РІР»РµРЅРёСЏ Р°РЅРёРјР°С†РёРµР№
            storyboard = new Storyboard();
            Storyboard.SetTarget(animation, iconControl);
            Storyboard.SetTargetProperty(animation, "Opacity");

            // Р”РѕР±Р°РІР»СЏРµРј Р°РЅРёРјР°С†РёСЋ РІ Storyboard
            storyboard.Children.Add(animation);

            // РћР±СЂР°Р±Р°С‚С‹РІР°РµРј СЃРѕР±С‹С‚РёРµ Р·Р°РІРµСЂС€РµРЅРёСЏ Р°РЅРёРјР°С†РёРё
            storyboard.Completed += (s, e) =>
            {
                // РњРµРЅСЏРµРј РёРєРѕРЅРєСѓ РїРѕСЃР»Рµ Р·Р°РІРµСЂС€РµРЅРёСЏ Р°РЅРёРјР°С†РёРё
                iconControl.Glyph = newFontIcon;

                var animation = new DoubleAnimation
                {
                    From = iconControl.Opacity,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(50),

                };
                // РЎРѕР·РґР°РµРј РѕР±СЉРµРєС‚ Storyboard РґР»СЏ СѓРїСЂР°РІР»РµРЅРёСЏ Р°РЅРёРјР°С†РёРµР№
                storyboard = new Storyboard();
                Storyboard.SetTarget(animation, iconControl);
                Storyboard.SetTargetProperty(animation, "Opacity");
                // Р”РѕР±Р°РІР»СЏРµРј Р°РЅРёРјР°С†РёСЋ РІ Storyboard
                storyboard.Children.Add(animation);
                storyboard.Begin();
            };

            // Р—Р°РїСѓСЃРєР°РµРј Р°РЅРёРјР°С†РёСЋ
            storyboard.Begin();

        }
    }
}

