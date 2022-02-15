using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vergiBlueDesktop.Views
{
    internal static class Dialogs
    {
        public static string AskFenString(out bool playerWhite)
        {
            playerWhite = false;

            var initial = "";
            var clipBoard = Clipboard.GetText();
            if (clipBoard.Count(c => c == '/') == 7)
            {
                initial = clipBoard;
            }

            var dialog = new FenDialog(initial);
            if (dialog.ShowDialog() == true)
            {
                playerWhite = dialog.PlayerIsWhite;
                return dialog.FenText;
            }

            return "";
        }
    }
}
