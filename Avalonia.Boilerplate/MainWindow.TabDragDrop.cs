using System;
using System.Collections;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Avalonia.Layout;
using Avalonia.Threading;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Avalonia.Boilerplate { 
    
    partial class MainWindow {
        private const int Epsilon = 11;
        const string ImagesNamespace = "Avalonia.Boilerplate.";
        public const string GhostSSFacsimileLightImageUri = "resm:" + ImagesNamespace + "GhostTabWindow-Light@2x.png";
        public const string GhostSSFacsimileDarkImageUri = "resm:" + ImagesNamespace + "GhostTabWindow-Dark@2x.png";

        // This is a work around for not being able to change the rounded corners on a Mac window.
        // If someone figures out how to create a right angle cornered Mac window,
        // then remove code associated with this constant.
        private const int AdjustForCurvedCorners = 10;
        
         private Object lockMove = new Object();
        private Object lockAdjust = new Object();

        private Window ghostWindow;
        private Control ghostTab;
        private Control ghostSSFacsimile;

        private DockPanel savedDockPanel;
        private TabItem tabItemSelectedForDragDrop;
        private TabDragInfo tabBeingMoved;

        private bool isFirstMove;

        private PixelPoint clickPoint;
        private PixelPoint clickOffset;

        private int tabOffset;

        private int tabStripLeft;
        private int tabStripRight;
        private int tabStripTop;
        private int tabStripBottom;
        private int tabStripHalfHeight;

        private bool hasRail;
        private bool isMoveInProgress;

        private IPointer currentPointer;

        private void OnPointerPressed(object sender, PointerPressedEventArgs e) {
                var tabItemTest = sender as TabItem;

                    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed) {
                        HandleTabRightMouseClick(sender);
                        e.Handled = true;
                        return;
                    }
                    if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) {
                        HandleTabMiddleMouseClick(sender);
                        e.Handled = true;
                        return;
                    }


                // Check for module tab (defined by the fact it is the only tab type that allows the user to close)
                if ((TabHeaderInfo)tabItemTest?.Header is null ||
                    !((TabHeaderInfo)tabItemTest.Header).AllowClose ||
                    TabItems.Count(t => (TabHeaderInfo)(t.Header) is null || ((TabHeaderInfo)(t.Header)).AllowClose) == 0
                ) {
                    return;
                }

                // Check that the left button has been pressed
                if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                    return;
                }

                // Check that eSpace has loaded
                if (IsLoading(tabItemTest)) {
                    return;
                }

                var caption = ((TabHeaderInfo)tabItemTest.Header).Caption;
                if (caption is null) {
                    return;
                }

                tabItemSelectedForDragDrop = tabItemTest;

                var tempTabItems = this.tabs.Items.Cast<TabItem>().ToArray();
                var count = tempTabItems.Length;

                // Get first module tab item
                int tabIndex;
                TabItem tabItem = null;
                PixelPoint originPoint = default;
                for (tabIndex = 0; tabIndex < count; tabIndex++) {
                    tabItem = tempTabItems[tabIndex];
                    if ((TabHeaderInfo)tabItem.Header != null && ((TabHeaderInfo)tabItem.Header).AllowClose) {
                        originPoint = tabItem.PointToScreen(new Point(0, 0));
                        break;
                    }
                }
                // The first module tab's origin point represents the top-left corner of the rail
                tabStripLeft = originPoint.X;
                tabStripTop = originPoint.Y;

                // Has a rail when there are one or more module tabs
                hasRail = (count - tabIndex) >= 1;

                // Build list of tab drag infos from module tab items (skipping last tab module)
                TabDragInfo tabInfoBefore = null;
                for (; tabIndex < count - 1; tabIndex++) {
                    // The next tab item is needed to determine width of the current tab item
                    var nextTabItem = tempTabItems[tabIndex + 1];
                    var nextOriginPoint = nextTabItem.PointToScreen(new Point(0, 0));

                    // Create tab drag info for tab at <tabIndex>
                    var tabInfo = new TabDragInfo(tabItem, tabIndex, originPoint, (nextOriginPoint.X - originPoint.X), tabInfoBefore);
                    if (tabItem == tabItemSelectedForDragDrop) {
                        tabBeingMoved = tabInfo;
                    }

                    // The next tab item becomes the <tabIndex> tab item
                    tabInfoBefore = tabInfo;
                    tabItem = nextTabItem;
                    originPoint = nextOriginPoint;
                }

                if(tabItem is null) { // if we don't have a last tab, bail out
                    return;
                }
                
                // Width of the last tab is determined by the last tab's bounds
                var lastEndPoint = tabItem.PointToScreen(new Point(tabItem.Bounds.Width, tabItem.Bounds.Height));

                // The last tab's end point represents the end of the rail
                tabStripRight = lastEndPoint.X;
                tabStripBottom = lastEndPoint.Y;
                // Vertical center of the tabstrip (rail)
                tabStripHalfHeight = (tabStripBottom - tabStripTop) / 2;

                // Add last tab module which width is calculated from its bounds
                var lastTabInfo = new TabDragInfo(tabItem, tabIndex, originPoint, (lastEndPoint.X - originPoint.X), tabInfoBefore);
                if (tabItem == tabItemSelectedForDragDrop) {
                    tabBeingMoved = lastTabInfo;
                }

                // Determine initial pointer's relative position to the tab's origin
                var tabOriginPoint = tabBeingMoved.originPoint;
                clickPoint = tabBeingMoved.tabItem.PointToScreen(e.GetPosition(tabBeingMoved.tabItem));
                clickOffset = clickPoint - tabOriginPoint;

                tabOffset = (tabBeingMoved.GetMinWidth() / 2) - 1;

                DeleteGhostWindow();

                var ghostWidth = tabItemSelectedForDragDrop.Bounds.Width;
                var ghostHeight = tabItemSelectedForDragDrop.Bounds.Height;

                ghostTab = CreateGhostTab(caption, ghostWidth, ghostHeight, tabItemSelectedForDragDrop.Background, tabItemSelectedForDragDrop.Foreground);
                ghostSSFacsimile = CreateGhostSSFacsimile(caption, this.Width, this.Height, tabItemSelectedForDragDrop.Background, tabItemSelectedForDragDrop.Foreground);

                var ghostPosition = tabOriginPoint - new PixelPoint(AdjustForCurvedCorners, 0);
                ghostWindow = CreateGhostWindow(caption, ghostWidth, ghostHeight, ghostPosition);
                ghostWindow.KeyDown += OnKeyDownWhileDragging;
                SetPointerCapture(e.Pointer);

                // Trick to make current tab look like it is moving by making the existing tab foreground invisible.
                if (hasRail) {
                    // tabItemSelectedForDragDrop can be null at this phase because CreateGhostWindow allows the jobs that were in the queue to be run.
                    // It's probably a bug on avalonia because it seems it is allowing to run operations that were pushed to be run async to run at the CreateWindow phase
                    // That allows the RemoveTab that was pushed to run async to run at the CreateWindow phase and will set tabItemSelectedForDragDrop as null
                    // Didn't fix changing the order because that could lead to breaking this in the future if we forgot the order matters and the savedDockPanel won't be needed as the tab was closed
                    // To be followed by RDOIAT-602
                    savedDockPanel = (DockPanel)((IStyledElement)tabItemSelectedForDragDrop)?.LogicalChildren.FirstOrDefault(c => c is DockPanel);
                }

                lock (lockMove) {
                    isFirstMove = true;
                    this.PointerLeave += OnPointerLeave;
                    this.PointerCaptureLost += OnPointerCaptureLost;
                    this.PointerMoved += OnPointerMove;
                    this.PointerReleased += OnPointerReleased;
                }
        }

        private void HandleTabRightMouseClick(object sender) {
            var tabItem = sender as TabItem ?? (sender as TextBlock)?.FindAncestorOfType<TabItem>();
            var textBlock = sender as TextBlock ?? (sender as TabItem)?.FindDescendantOfType<TextBlock>();

            SelectTab(tabItem);
           
        }

        private void HandleTabMiddleMouseClick(object sender) {
            var tabItem = sender as TabItem ?? (sender as IVisual)?.FindAncestorOfType<TabItem>();
            var tabHeaderInfo = tabItem?.DataContext as TabHeaderInfo;
            tabHeaderInfo?.TriggerClose();
        }

        private void OnKeyDownWhileDragging(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
            {
                CancelTabMove(currentPointer);
            }
        }

        private void OnPointerLeave(object sender, PointerEventArgs e) {
            SetPointerCapture(e.Pointer);
        }

        private void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e) {
            CancelTabMove(e.Pointer);
        }

        private void OnPointerMove(object sender, PointerEventArgs e) {
            // Cursor in screen coordinates
            var currentPoint = this.PointToScreen(e.GetPosition(this));
            if (isFirstMove) {
                // Move a bit before showing the ghost window
                var pointDiff = currentPoint - clickPoint;
                if (Math.Abs(pointDiff.X) <= Epsilon && Math.Abs(pointDiff.Y) <= Epsilon) {
                    SetPointerCapture(e.Pointer);
                    return;
                }
                // Show ghost window
                lock (lockMove) {
                    if (isFirstMove) {
                        isFirstMove = false;
                        isMoveInProgress = true;
                        ghostWindow.Show();
                        SetPointerCapture(e.Pointer);
                    }
                }
            } else {
                if (IsPointOnRail(currentPoint, out PixelPoint originPoint)) {
                    // Move ghost window along rail
                    if (!Object.ReferenceEquals(ghostWindow.Content, ghostTab)) {
                        ghostWindow.SystemDecorations = SystemDecorations.None;
                        ghostWindow.Content = ghostTab;
                        ghostWindow.InvalidateMeasure();
                        HideTabItem();
                    }
                    ghostWindow.Position = new PixelPoint(originPoint.X - AdjustForCurvedCorners, tabStripTop);
                    AdjustTabPositions(originPoint.X);
                } else {
                    // Move ghost window off of rail
                    if (!Object.ReferenceEquals(ghostWindow.Content, ghostSSFacsimile)) {
                        ghostWindow.SystemDecorations = SystemDecorations.BorderOnly;
                        ghostWindow.Content = ghostSSFacsimile;
                        ghostWindow.InvalidateMeasure();
                        ShowTabItem();
                    }
                    ghostWindow.Position = originPoint;
                }
            }
        }

        private void OnPointerReleased(object sender, PointerEventArgs e) {

            EndTabMove(e.Pointer);

            var currentPoint = this.PointToScreen(e.GetPosition(this));
            if (IsPointOnRail(currentPoint, out PixelPoint originPoint)) {
                AdjustTabPositions(originPoint.X);
            } else {
                SpawnTabToInstance(currentPoint);
            }

            tabBeingMoved = null;
        }

        private void EndTabMove(IPointer pointer) {

            // No longer subscribe to pointer events.
            this.PointerLeave -= OnPointerLeave;
            this.PointerCaptureLost -= OnPointerCaptureLost;
            this.PointerMoved -= OnPointerMove;
            this.PointerReleased -= OnPointerReleased;

            DeleteGhostWindow();
            pointer?.Capture(null);
            ShowTabItem();

            isMoveInProgress = false;
        }

        private void CancelTabMove(IPointer pointer) {
            EndTabMove(pointer);
            tabBeingMoved = null;
        }

        private void SetPointerCapture(IPointer pointer) {
            pointer.Capture(this);
            currentPointer = pointer;
        }

        private bool IsPointOnRail(PixelPoint currentPoint, out PixelPoint originPoint) {
            // Compute the adjusted origin and enter points
            originPoint = currentPoint - clickOffset;
            var centerPoint = originPoint + new PixelPoint(tabBeingMoved.halfWidth, tabStripHalfHeight);

            return (
                // more than one module tab
                hasRail
                // between top and bottom of strip - between top and bottom of rail
                && (tabStripTop - Epsilon < centerPoint.Y && centerPoint.Y < tabStripBottom + Epsilon)
                // between left and right of modules part of tab strip - between left and right of rail
                && (tabStripLeft < centerPoint.X && centerPoint.X < tabStripRight) 
            );
        }

        private void AdjustTabPositions(int xOriginAdjusted) {
            lock (lockAdjust) {
                // AdjustPosition returns true when the tab being moved switches position with the tab before or after
                if (tabBeingMoved.AdjustPosition(xOriginAdjusted, tabOffset, out int originalIndex, out int moveToIndex)) {
                    // Swap the tab positions (do this on the UIThread at a priority before input)
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
                        () => {
                            var list = (IList)tabs.Items;
                            var tab = list[moveToIndex];
                            list.RemoveAt(moveToIndex);
                            list.Insert(originalIndex, tab);
                            tabs.SelectedIndex = moveToIndex;
                        },
                        Avalonia.Threading.DispatcherPriority.Loaded
                    );
                }
            }
        }

        Rect rect;
        private void HideTabItem() {
            if (hasRail && isMoveInProgress && tabItemSelectedForDragDrop != null) {
               if (!(savedDockPanel is null)) {
                    rect = tabItemSelectedForDragDrop.Bounds;
                    savedDockPanel.IsVisible = false;
                    tabItemSelectedForDragDrop.Width = rect.Width;
               }

               ((IPseudoClasses)(tabItemSelectedForDragDrop).Classes).Set(":hide", true);
            }
        }

        private void ShowTabItem() {
            if (hasRail && isMoveInProgress && tabItemSelectedForDragDrop != null) {
                if (!(savedDockPanel is null)) {
                    tabItemSelectedForDragDrop.Width = double.NaN;
                    savedDockPanel.IsVisible = true;
                }
                
                ((IPseudoClasses)(tabItemSelectedForDragDrop).Classes).Set(":hide", false);
            }
        }

        private static Control CreateGhostTab(string caption, double width, double height, IBrush backgroundBrush, IBrush foregroundBrush) {

            var textBlock = new TextBlock() {
                Text = caption,
                Width = width - 50,
                FontSize = 12,
                Margin = new Thickness(16, 0, 6, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };

            var canvas = new Canvas() {
                Width = 20,
                Height = 10,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            canvas.Children.Add(new Line() { StartPoint = new Point(0, 0), EndPoint = new Point(8, 8), Stroke = foregroundBrush, StrokeThickness = 1 });
            canvas.Children.Add(new Line() { StartPoint = new Point(8, 0), EndPoint = new Point(0, 8), Stroke = foregroundBrush, StrokeThickness = 1 });

            var dockPanel = new DockPanel() {
                Height = height,
                Background = Brush.Parse("#202327"),
                VerticalAlignment = VerticalAlignment.Top,
            };
            dockPanel.Children.Add(textBlock);
            dockPanel.Children.Add(canvas);

            var borderLineBrushColor ="#ff3b3d41";
            Border border = new Border {
                Background = Brushes.Transparent,
                BorderBrush = Brush.Parse(borderLineBrushColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderThickness = new Thickness(1, 1, 1, 0),
                Child = dockPanel
            };

            var panel = new Panel() {
                Width = width + 2 + (2 * AdjustForCurvedCorners),
                Margin = new Thickness(AdjustForCurvedCorners, 0, AdjustForCurvedCorners, 0),
                Background = Brushes.Transparent,
            };
            panel.Children.Add(border);

            return panel;
        }

        private static Control CreateGhostSSFacsimile(string caption, double currentSSWindowWidth, double currentSSWindowHeight, IBrush backgroundBrush, IBrush foregroundBrush) {
            var scaledWidth = (int)(currentSSWindowWidth / 3);
            var scaledHeight = (int)(currentSSWindowHeight / 3);
            // load bitmap image
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var ghostWindowThemeUri = GhostSSFacsimileLightImageUri;

            var bitmapOrig = new Bitmap(assets.Open(new Uri(ghostWindowThemeUri)));
            var scaledBitmap = bitmapOrig.CreateScaledBitmap(new PixelSize(scaledWidth, scaledHeight));
            var ghostTabImage = new Image { Source = scaledBitmap };

            var ghostContainer = new Canvas {
                Background = Brushes.Transparent,
                Width = scaledWidth,
                Height = scaledHeight
            };
            ghostContainer.Children.Add(ghostTabImage);
            ghostTabImage.SetValue(Canvas.TopProperty, 0);
            ghostTabImage.SetValue(Canvas.LeftProperty, 0);

            return ghostContainer;
        }

        private static Window CreateGhostWindow(string caption, double width, double height, PixelPoint windowPosition) {

            return new Window() {
                SystemDecorations = SystemDecorations.BorderOnly,
                CanResize = false,
                Focusable = false,
                IsHitTestVisible = false,
                Background = Brushes.Transparent,
                TransparencyBackgroundFallback = Brushes.Transparent,
                TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                CornerRadius = new CornerRadius(0),
                ShowInTaskbar = false,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Position = windowPosition,
                SizeToContent = SizeToContent.WidthAndHeight, 
            };
        }

        private void DeleteGhostWindow() {
            if (!(ghostWindow is null)) {
                ghostWindow.KeyDown -= OnKeyDownWhileDragging;
                ghostWindow.Close();
                ghostWindow = null;
            }
        }

        private void SpawnTabToInstance(PixelPoint dropPoint) {
           
        }

        private static bool IsLoading(TabItem tabItemTest) {
            return false;
        }


        public static MainWindow CreateAggregatorWindow(int x, int y) {
            var window = new MainWindow();
            
            return window;
        }

    }
}
