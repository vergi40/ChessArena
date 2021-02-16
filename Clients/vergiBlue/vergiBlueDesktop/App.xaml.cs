﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace vergiBlueDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
            var view = new MainView();
            view.Title = "vergiBlue desktop environment";
            view.DataContext = viewModel;
            view.Show();
        }
    }
}
