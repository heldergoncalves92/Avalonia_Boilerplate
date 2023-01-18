using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace Avalonia.Boilerplate
{
    public class MyWindow : Window
    { 
        private Window parent;
        private ManagedPopupPositioner positioner;
        private PopupPositionerParameters positionerParameters;
        
        public MyWindow()
        {
            AvaloniaXamlLoader.Load(this);
            PseudoClasses.Set(":osx", RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

            Width = 650;
            Height = 650;
            CanResize = false;
            
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
            ExtendClientAreaToDecorationsHint = false;
            this.AttachDevTools();
        }

        private void UpdatePosition() {
            positioner?.Update(positionerParameters);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            positionerParameters.Size = finalSize;
            UpdatePosition();
            return base.ArrangeOverride(finalSize);
        }
    }
}
