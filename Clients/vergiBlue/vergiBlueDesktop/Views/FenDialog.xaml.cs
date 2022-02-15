using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vergiBlueDesktop.Views
{
    /// <summary>
    /// Interaction logic for FenDialog.xaml
    /// </summary>
    public partial class FenDialog : Window
    {
        public string FenText => FenTextBox.Text;
        public bool PlayerIsWhite => PlayerIsWhiteCheckBox.IsChecked == true;
        public FenDialog(string initialText = "")
        {
            InitializeComponent();
            FenTextBox.Text = initialText;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
