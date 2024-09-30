using MuteAppUI.KeyLoggers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuteAppUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr hookId;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartHookClick(object sender, RoutedEventArgs e)
        {
            hookId = InterceptKeys.SetHook(InterceptKeys._proc);
        }

        private void StopHookClick(object sender, RoutedEventArgs e)
        {
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }

        private void LogHook(string hookPhraze)
        {
            logText.Text += $"\r\n {hookPhraze}";
        }
    }
}