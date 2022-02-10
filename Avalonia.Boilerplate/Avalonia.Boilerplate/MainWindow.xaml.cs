using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Avalonia.Boilerplate {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
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
            this.WindowState = WindowState.Minimized;
            var appWindow = new MyWindow();
            appWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            appWindow.Show(this);
        }
    }
}
