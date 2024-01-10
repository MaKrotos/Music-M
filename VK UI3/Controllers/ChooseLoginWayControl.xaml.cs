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
                        MainTxT.Text = "Генератор кода";
                        secondTXT.Text = "Воспользуйтесь кодом из приложения генерации кодов авторизации";
                    } 
                    else if (loginWay.Name == LoginWay.Push)
                    {
                        fontIcon.Glyph = "\uE90A";
                        MainTxT.Text = "PUSH уведомление";
                        secondTXT.Text = "Вам будет отправлено PUSH уведомление для авторизации";
                    }
                    else if (loginWay.Name == LoginWay.Passkey)
                    {
                        fontIcon.Glyph = "\uE928";
                        MainTxT.Text = "OnePass";
                        secondTXT.Text = "Воспользуйтесь OnePass для авторизации";
                    }
                    else if (loginWay.Name == LoginWay.CallReset)
                    {
                        fontIcon.Glyph = "\uE717";
                        MainTxT.Text = "Звонок";
                        secondTXT.Text = "Вам позвонят и продиктуют код.";
                    }
                    else if (loginWay.Name == LoginWay.Sms)
                    {
                        fontIcon.Glyph = "\uE8BD";
                        MainTxT.Text = "Sms уведомление";
                        secondTXT.Text = "Вам будет отправлено СМС сообщение для авторизации";
                    }
                    else if (loginWay.Name == LoginWay.ReserveCode)
                    {
                        fontIcon.Glyph = "\uE821";
                        MainTxT.Text = "Резервный код";
                        secondTXT.Text = "Воспользуйтесь резервным кодом для авторизации";
                    }
                    else if (loginWay.Name == LoginWay.Email)
                    {
                        fontIcon.Glyph = "\uE715";
                        MainTxT.Text = "Электронная почта";
                        secondTXT.Text = "Вам будет отправлен код на электронную почту";
                    }
                    else if (loginWay.Name == LoginWay.Password)
                    {
                        fontIcon.Glyph = "\uE8AC";
                        MainTxT.Text = "Пароль";
                        secondTXT.Text = "Введите пароль для входа в свой аккаунт.";
                    }
                    else
                    {
                        fontIcon.Glyph = "?";
                        MainTxT.Text = "Нереализованный метод ("+loginWay.Name.ToString()+")";
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
