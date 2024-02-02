using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    public class AnimationsChangeIcon
    {
        Symbol? symbolNow = null;
        Storyboard storyboard = null;
        SymbolIcon iconControl = null;

        public AnimationsChangeIcon(SymbolIcon iconControl)
        {
            symbolNow = iconControl.Symbol;
            this.iconControl = iconControl;
        }

        public async void ChangeSymbolIconWithAnimation(Symbol newSymbol)
        {

            if (storyboard == null) storyboard = new Storyboard();

            if (symbolNow != null && (symbolNow == newSymbol && iconControl.Symbol == newSymbol)) return;

            symbolNow = newSymbol;

            if (storyboard.GetCurrentState() == ClockState.Active)
            {
                storyboard.Pause();

            }

            // РЎРѕР·РґР°РµРј Р°РЅРёРјР°С†РёСЋ РїСЂРѕР·СЂР°С‡РЅРѕСЃС‚Рё
            var animation = new DoubleAnimation
            {
                From = iconControl.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(100),
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
                iconControl.Symbol = newSymbol;

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

