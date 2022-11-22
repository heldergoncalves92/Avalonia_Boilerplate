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
            WindowState = WindowState.FullScreen;
            tabs = this.FindControl<TabControl>("tabs");

            var tabInfo = new TabHeaderInfo() { Caption = "Tab 11" };
            AddTab(tabInfo, new CustomTabContent());

#if DEBUG
            this.AttachDevTools();
#endif
        }

        
        private IEnumerable<TabItem> TabItems => tabs.Items.Cast<TabItem>();

        private void OnWindowPositionChanged(object sender, PixelPointEventArgs e) {
            // TODO - RICT-3212 Remove this event when Avalonia fixes this rendering issue
            // https://github.com/AvaloniaUI/Avalonia/issues/4107
            this.InvalidateMeasure();
        }

        private void ApplyTitleBarMargin(Avalonia.Controls.WindowState state) { }

        //TODO HYBRID Finish
        private void OnSelectedTabChanged(object sender, SelectionChangedEventArgs e) {
            var tabItem = e.AddedItems.OfType<TabItem>().FirstOrDefault()?.Content;

             if (tabItem != null) {
                
                 //     contextualDispatcher.Post(() => selectedAggregatorChanged?.Invoke(tabItem), contextualDispatcherFrameId);
             }
        }
        
        private void OnKeyUp(object sender, KeyEventArgs e) {
        }

        private void SelectTab(TabItem tabItem) {
            if (tabItem.Content != null) {
                tabs.SelectedIndex = GetTabIndex(tabItem.Content);
            }
        }

        private void OnNewTabButtonClick(object sender, RoutedEventArgs e) { }
        

        private void OnTabCloseButtonClick(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            button.IsEnabled = false;

            var tabHeaderInfo = (TabHeaderInfo)button.DataContext;
            tabHeaderInfo?.TriggerClose().ContinueWith(t => button.IsEnabled = true, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnWindowClosing(object sender, CancelEventArgs e) {            
            e.Cancel = true;
            IsEnabled = false;
            // contextualDispatcher.Post(closing, contextualDispatcherFrameId).ContinueWith(t => {
            //     IsEnabled = true;
            //     if (!t.IsFaulted && t.Result) {
            //         Closing -= OnWindowClosing;
            //         PositionChanged -= OnWindowPositionChanged;
            //         Close();
            //     }
            // }, TaskScheduler.FromCurrentSynchronizationContext());
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

            var isMouseInside = false;

            void OnMouseLeft() {
                isMouseInside = false;
            }

            tab.PointerMoved += (sender, e) => {
                // in order to properly get the position we need to listen to pointer moved
                if (isMouseInside) {
                    return;
                }
                isMouseInside = true;
                Dispatcher.UIThread.InvokeAsync(() => {
                    var w = new MyWindow();
                    w.ConfigurePosition(this, new Point(53.23, 20), anchor: PopupAnchor.TopLeft, gravity: PopupGravity.BottomRight);
                    w.ShowPopup();

                    Task.Run(async () => {
                        await Task.Delay(1000);
                        Dispatcher.UIThread.InvokeAsync(w.Close);
                    });
                });
            };
            tab.PointerLeave += delegate { OnMouseLeft(); };
            tab.Tapped += delegate { OnMouseLeft(); };
            tab.LostFocus += delegate { OnMouseLeft(); };
            tab.PointerPressed += OnPointerPressed;
            AddDragDropHandlers(tab);

            var pos = ((IList)tabs.Items).Add(tab);
        }

        private void RemoveTab(object aggregatorView) {
            var index = GetTabIndex(aggregatorView);

            if (tabs.SelectedIndex == index) {
                tabs.SelectedIndex = index - 1;
                tabItemSelectedForDragDrop = null;
            }

            var itemsList = (IList<object>)tabs.Items;
            var currentTab = (TabItem)itemsList.ElementAt(index);
            currentTab.Content = null;
            currentTab.DataContext = null;
            currentTab.Header = null;
            itemsList.RemoveAt(index);
            
        }

        private void AddDragDropHandlers(TabItem tab) {
            Avalonia.Input.DragDrop.SetAllowDrop(tab, true);
            tab.AddHandler(Avalonia.Input.DragDrop.DragEnterEvent, OnDragEnter);
        }

        private void OnDragEnter(object sender, Avalonia.Input.DragEventArgs e) {
            tabs.SelectedIndex = GetTabIndex(((TabItem)sender).Content);
        }
    }
}
