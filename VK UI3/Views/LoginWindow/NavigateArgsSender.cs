using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.LoginWindow
{
    public class ArgSender
    { 
      public string ErrorText { get; set; }
    }

    internal interface NavigateArgsSender
    {
        public void SendArgs(ArgSender argSender);
    }
}
