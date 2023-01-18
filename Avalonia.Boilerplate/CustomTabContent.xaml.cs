using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Avalonia.Boilerplate;

public partial class CustomTabContent : UserControl {
    public CustomTabContent() {
        AvaloniaXamlLoader.Load(this);
        var button = this.FindControl<Button>("btn");

        button.Click += (sender, args) => {
            var w = new MyWindow();
            w.ShowPopup();
        };
    }
}
