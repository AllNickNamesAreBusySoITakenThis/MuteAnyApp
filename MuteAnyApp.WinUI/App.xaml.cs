using MuteAnyApp.WinUI.ViewModels;
using MuteAnyApp.WinUI.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MuteAnyApp.WinUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.MainWindow = new MainWindow(new MainWindowViewModel());
            Current.MainWindow.Show();
        }
    }
}
