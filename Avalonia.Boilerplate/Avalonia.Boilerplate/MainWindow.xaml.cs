using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Avalonia.Boilerplate {
    public class MainWindow : Window {
        public MainWindow() {
            AvaloniaXamlLoader.Load(this);

            // By setting the Height property we lost the Maximized state. Is it expected?
            Height = 718;
            WindowState = WindowState.Maximized;
        }
    }
}
