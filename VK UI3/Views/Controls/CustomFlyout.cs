using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Views.Controls
{
    public class CustomFlyout : MenuFlyout
    {
        public CustomFlyout()
        {
            this.Opening += CustomFlyout_Opening;
        }

        private void CustomFlyout_Opening(object sender, object e)
        {
            bool hasVisibleItems = false;

            foreach (var item in this.Items)
            {
                if (item is MenuFlyoutItem menuItem && menuItem.Visibility == Microsoft.UI.Xaml.Visibility.Visible)
                {
                    hasVisibleItems = true;
                    break;
                }
            }

            if (!hasVisibleItems)
            {
                this.Hide();
            }
        }
    }


}
