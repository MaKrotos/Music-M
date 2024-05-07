using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Resource
{
    class ChatColors
    {
        private Dictionary<int, Windows.UI.Color> colors = new Dictionary<int, Windows.UI.Color>
{
    { 0, Windows.UI.Color.FromArgb(255, 235, 164, 164) }, // Soft Red
    { 1, Windows.UI.Color.FromArgb(255, 255, 224, 164) }, // Soft Orange
    { 2, Windows.UI.Color.FromArgb(255, 255, 255, 164) }, // Soft Yellow
    { 3, Windows.UI.Color.FromArgb(255, 164, 255, 164) }, // Soft Green
    { 4, Windows.UI.Color.FromArgb(255, 214, 235, 255) }, // Soft Light Blue
    { 5, Windows.UI.Color.FromArgb(255, 255, 204, 255) } // Soft Violet
};



        public Windows.UI.Color GetColorByNumber(int number)
        {
            if (colors.ContainsKey(number))
            {
                return colors[number];
            }
            else
            {
                throw new ArgumentException("Invalid color number");
            }
        }
    }

}
