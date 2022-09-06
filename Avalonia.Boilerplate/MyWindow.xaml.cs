using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia.Boilerplate
{
    public class MyWindow : Window
    {
        public MyWindow()
        {
            InitializeComponent();
            Width = 200;
            Height = 200;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
