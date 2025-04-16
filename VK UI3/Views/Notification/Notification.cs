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

        public Notification(string header = null, string message = null, ButtonNotification button1 = null, ButtonNotification button2 = null)
        {
            this.header = header;
            Message = message;
            this.button1 = button1;
            this.button2 = button2;
            Notifications.Add(this);
        }

        internal void Delete()
        {
            Notifications.Remove(this);
        }
    }

    public class ButtonNotification
    {
        public ButtonNotification(string text, Action btnAction, bool closeNotification = false)
        {
            Text = text;
            BtnAction = btnAction;
            this.closeNotification = closeNotification;
        }

        public bool closeNotification { get; set; }
        public string Text { get; set; }  
        public Action BtnAction { get; set; }  
    }
}
