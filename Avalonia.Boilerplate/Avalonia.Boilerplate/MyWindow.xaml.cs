using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.Boilerplate
{
    internal class MyWindow : Window {

        public MyWindow()
        {
            AvaloniaXamlLoader.Load(this);
            WindowState = WindowState.Maximized;
            CanResize = true;
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            Height = 718;
            //Width = 1424;

        }

        protected override void HandleWindowStateChanged(WindowState state)
        {
            base.HandleWindowStateChanged(state);
            Console.Out.WriteLine($"State Changed: {state}");
        }
    }
}
