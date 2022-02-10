using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia.Boilerplate
{
    public class MyWindow : Window
    {
        public MyWindow()
        {
            InitializeComponent();
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
