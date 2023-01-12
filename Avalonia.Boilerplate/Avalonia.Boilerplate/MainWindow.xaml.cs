using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

namespace Avalonia.Boilerplate {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        
        private Window CreateSampleWindow() {
            var window = new Window
            {
                Background = Brushes.Black,
                Height = 200,
                Width = 200,
                Content = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = "Hello world!" }
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            return window;
        }
        
        Window GetWindow() => (Window)this.VisualRoot;

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            var button = this.GetControl<Button>("Dialog");
            button.Click += (sender, args) => CreateSampleWindow().Show(this);
        }
    }
}
