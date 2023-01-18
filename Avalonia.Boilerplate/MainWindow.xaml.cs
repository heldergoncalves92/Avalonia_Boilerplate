using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Avalonia.Boilerplate {
    public partial class MainWindow : Window {
        private readonly TabControl tabs;
        
        public MainWindow() {
            AvaloniaXamlLoader.Load(this);
            tabs = this.FindControl<TabControl>("tabs");

            var tabInfo = new TabHeaderInfo() { Caption = "Tab 11" };
            AddTab(tabInfo, new CustomTabContent());

#if DEBUG
            this.AttachDevTools();
#endif
        }

        
        private IEnumerable<TabItem> TabItems => tabs.Items.Cast<TabItem>();




        private void SelectTab(TabItem tabItem) {
            if (tabItem.Content != null) {
                tabs.SelectedIndex = GetTabIndex(tabItem.Content);
            }
        }

 
   

        private int GetTabIndex(object topLevelView) => TabItems.IndexOf(t => t.Content == topLevelView);

        private void AddTab(TabHeaderInfo header, Control view, bool isVisible = true, bool isEnabled = true) {
            var tab = new TabItem() {
                Content = view,
                DataContext = header,
                Header = header,
                IsVisible = isVisible,
                IsEnabled = isEnabled
            };

         
            var pos = ((IList)tabs.Items).Add(tab);
        }
        
    }
}
