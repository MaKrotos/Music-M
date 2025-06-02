using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.LoginWindow
{
    internal class ExtendFrame : Frame
    {
        public void GoBack(ArgSender argSender)
        {
            base.GoBack();

            if (this.Content is NavigateArgsSender sender)
            {
                sender.SendArgs(argSender);
            }
        }
    }
}
