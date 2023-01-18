using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Layout;
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
            Width = 650;
            Height = 650;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
            ExtendClientAreaToDecorationsHint = true;
        }
        
        

     

        private void UpdatePosition() {
            positioner?.Update(positionerParameters);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            positionerParameters.Size = finalSize;
            UpdatePosition();
            return base.ArrangeOverride(finalSize);
        }

        public void ShowPopup() {
            if (parent != null) {
                Show(parent);
            } else {
                Show();
            }
        }
    }
}
