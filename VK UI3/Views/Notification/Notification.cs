using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.Notification
{
    internal class Notification
    {
        public static ObservableCollection<Notification> Notifications = new ObservableCollection<Notification>();

        public string header { get; set; }
        public string Message { get; set; }

        public ButtonNotification button1;
        public ButtonNotification button2;
    }

    public class ButtonNotification
    {
        public string Text { get; set; }  
        public Action BtnAction { get; set; }  
    }
}
