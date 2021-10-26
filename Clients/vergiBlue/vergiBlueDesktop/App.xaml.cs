using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using vergiBlueDesktop.Views;

namespace vergiBlueDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private GameModel _gameModel;

        /// <summary>
        /// Startup in code-behind.
        /// https://www.wpf-tutorial.com/wpf-application/working-with-app-xaml/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Link in MVVM manner
            var viewModel = new MainViewModel();
            
            // Control interactions between AI and player
            _gameModel = new GameModel(viewModel);
            
            var view = new MainView();
            view.Title = "vergiBlue desktop environment";
            view.DataContext = viewModel;
            view.Show();
        }
    }
}
