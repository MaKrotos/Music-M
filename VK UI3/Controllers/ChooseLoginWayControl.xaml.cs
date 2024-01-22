using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;
using static VK_UI3.DB.AccountsDB;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controllers
{
    public sealed partial class ChooseLoginWayControl : UserControl
    {
        private readonly DependencyProperty loginWayD = DependencyProperty.Register(
            "loginWay", typeof(EcosystemVerificationMethod), typeof(ChooseLoginWayControl), new PropertyMetadata(default(EcosystemVerificationMethod), onLoiginWayProrertyChanged));

        public DependencyProperty LoginWayD => loginWayD;
        public EcosystemVerificationMethod loginWay
        {
            get { return (EcosystemVerificationMethod)GetValue(loginWayD); }
            set { SetValue(loginWayD, value); }
        }

        string hello = "";
        private static void onLoiginWayProrertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
          //  throw new NotImplementedException();
        }

        public ChooseLoginWayControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) => {
               // Bindings.Update();

                if (loginWay != null)
                {
                  

                    if (loginWay.Name == LoginWay.Codegen)
                    {
                        fontIcon.Glyph = "\uECAD";
                        MainTxT.Text = "Р“РµРЅРµСЂР°С‚РѕСЂ РєРѕРґР°";
                        secondTXT.Text = "Р’РѕСЃРїРѕР»СЊР·СѓР№С‚РµСЃСЊ РєРѕРґРѕРј РёР· РїСЂРёР»РѕР¶РµРЅРёСЏ РіРµРЅРµСЂР°С†РёРё РєРѕРґРѕРІ Р°РІС‚РѕСЂРёР·Р°С†РёРё";
                    } 
                    else if (loginWay.Name == LoginWay.Push)
                    {
                        fontIcon.Glyph = "\uE90A";
                        MainTxT.Text = "PUSH СѓРІРµРґРѕРјР»РµРЅРёРµ";
                        secondTXT.Text = "Р’Р°Рј Р±СѓРґРµС‚ РѕС‚РїСЂР°РІР»РµРЅРѕ PUSH СѓРІРµРґРѕРјР»РµРЅРёРµ РґР»СЏ Р°РІС‚РѕСЂРёР·Р°С†РёРё";
                    }
                    else if (loginWay.Name == LoginWay.Passkey)
                    {
                        fontIcon.Glyph = "\uE928";
                        MainTxT.Text = "OnePass";
                        secondTXT.Text = "Р’РѕСЃРїРѕР»СЊР·СѓР№С‚РµСЃСЊ OnePass РґР»СЏ Р°РІС‚РѕСЂРёР·Р°С†РёРё";
                    }
                    else if (loginWay.Name == LoginWay.CallReset)
                    {
                        fontIcon.Glyph = "\uE717";
                        MainTxT.Text = "Р—РІРѕРЅРѕРє";
                        secondTXT.Text = "Р’Р°Рј РїРѕР·РІРѕРЅСЏС‚ Рё РїСЂРѕРґРёРєС‚СѓСЋС‚ РєРѕРґ.";
                    }
                    else if (loginWay.Name == LoginWay.Sms)
                    {
                        fontIcon.Glyph = "\uE8BD";
                        MainTxT.Text = "Sms СѓРІРµРґРѕРјР»РµРЅРёРµ";
                        secondTXT.Text = "Р’Р°Рј Р±СѓРґРµС‚ РѕС‚РїСЂР°РІР»РµРЅРѕ РЎРњРЎ СЃРѕРѕР±С‰РµРЅРёРµ РґР»СЏ Р°РІС‚РѕСЂРёР·Р°С†РёРё";
                    }
                    else if (loginWay.Name == LoginWay.ReserveCode)
                    {
                        fontIcon.Glyph = "\uE821";
                        MainTxT.Text = "Р РµР·РµСЂРІРЅС‹Р№ РєРѕРґ";
                        secondTXT.Text = "Р’РѕСЃРїРѕР»СЊР·СѓР№С‚РµСЃСЊ СЂРµР·РµСЂРІРЅС‹Рј РєРѕРґРѕРј РґР»СЏ Р°РІС‚РѕСЂРёР·Р°С†РёРё";
                    }
                    else if (loginWay.Name == LoginWay.Email)
                    {
                        fontIcon.Glyph = "\uE715";
                        MainTxT.Text = "Р­Р»РµРєС‚СЂРѕРЅРЅР°СЏ РїРѕС‡С‚Р°";
                        secondTXT.Text = "Р’Р°Рј Р±СѓРґРµС‚ РѕС‚РїСЂР°РІР»РµРЅ РєРѕРґ РЅР° СЌР»РµРєС‚СЂРѕРЅРЅСѓСЋ РїРѕС‡С‚Сѓ";
                    }
                    else if (loginWay.Name == LoginWay.Password)
                    {
                        fontIcon.Glyph = "\uE8AC";
                        MainTxT.Text = "РџР°СЂРѕР»СЊ";
                        secondTXT.Text = "Р’РІРµРґРёС‚Рµ РїР°СЂРѕР»СЊ РґР»СЏ РІС…РѕРґР° РІ СЃРІРѕР№ Р°РєРєР°СѓРЅС‚.";
                    }
                    else
                    {
                        fontIcon.Glyph = "?";
                        MainTxT.Text = "РќРµСЂРµР°Р»РёР·РѕРІР°РЅРЅС‹Р№ РјРµС‚РѕРґ ("+loginWay.Name.ToString()+")";
                        secondTXT.Text = "???";
                    }
                }

            };

            Loaded += ChooseLoginWayControl_Loaded;

           
        }

        private void ChooseLoginWayControl_Loaded(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
