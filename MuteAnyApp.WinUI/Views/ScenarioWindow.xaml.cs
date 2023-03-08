using MuteAnyApp.WinUI.ViewModels;
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

namespace MuteAnyApp.WinUI.Views
{
    /// <summary>
    /// Логика взаимодействия для ScenaroWindow.xaml
    /// </summary>
    public partial class ScenarioWindow : Window
    {
        public ScenarioWindow(ScenarioViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += ScenarioWindow_Loaded;
        }

        private async void ScenarioWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var context = (DataContext as ScenarioViewModel);
            if (context != null)
            {
                await context.GetSoundProcess();
            }
        }
    }
}
