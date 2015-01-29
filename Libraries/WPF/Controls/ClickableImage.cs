using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SBrickey.Libraries.WPF.Controls
{
    public class ClickableImage : Image
    {
        private bool isClicked = false;
        private bool isEntered = false;

        // ctor
        public ClickableImage()
        {
            this.MouseDown += this_MouseDown;
            this.MouseEnter += this_MouseEnter;
            this.MouseLeave += this_MouseLeave;
            this.MouseUp += this_MouseUp;

            // register mouse.QueryCursorEvent to handle changing cursors (on mouseover, etc)
            EventManager.RegisterClassHandler(typeof(ClickableImage), Mouse.QueryCursorEvent, new QueryCursorEventHandler(ClickableImage.QueryCursorEvent));
        }

        #region Click routed event
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClickableImage));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        #endregion

        new private static void QueryCursorEvent(object sender, QueryCursorEventArgs e)
        {
            var obj = (ClickableImage)sender;

            if (!obj.IsEnabled)
                return;

            e.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        #region Click event emulation

        private void this_MouseDown(object sender, MouseButtonEventArgs e) { this.isClicked = true; e.Handled = true; }
        private void this_MouseEnter(object sender, MouseEventArgs e) { this.isEntered = true; e.Handled = true; }
        private void this_MouseLeave(object sender, MouseEventArgs e) { this.isEntered = false; e.Handled = true; }
        private void this_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isClicked && this.isEntered)
            {
                RaiseEvent(new RoutedEventArgs(ClickableImage.ClickEvent));
            }
            this.isClicked = false;
            e.Handled = true;
        }

        #endregion

    } // class
} // namespace