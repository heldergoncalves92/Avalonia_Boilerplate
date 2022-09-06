using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Avalonia.Boilerplate {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            var button = this.FindControl<Button>("MyButton");
            button.Click += Button_Click;  
        }

        private async void Button_Click(object? sender, Interactivity.RoutedEventArgs e)
        {
            //this.WindowState = WindowState.Minimized;
            //await Task.Delay(3000);
            var appWindow = new MyWindow();
            appWindow.WindowState = WindowState.Maximized;
            appWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            appWindow.Show(this);
        }
    }
}
