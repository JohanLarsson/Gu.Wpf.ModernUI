﻿namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    using Microsoft.Win32;
    using Gu.Wpf.ModernUI.Win32;

    /// <summary>
    /// A window instance that is capable of per-monitor DPI awareness when supported.
    /// </summary>
    public abstract class DpiAwareWindow
        : Window
    {
        private static readonly double ScaleTolerance = 0.01;

        private readonly bool isPerMonitorDpiAware;
        private HwndSource source;
        private DpiInformation dpiInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DpiAwareWindow"/> class.
        /// </summary>
        protected DpiAwareWindow()
        {
            this.SourceInitialized += this.OnSourceInitialized;

            // WM_DPICHANGED is not send when window is minimized, do listen to global display setting changes
            SystemEvents.DisplaySettingsChanged += this.OnSystemEventsDisplaySettingsChanged;

            // try to set per-monitor dpi awareness, before the window is displayed
            this.isPerMonitorDpiAware = ModernUIHelper.TrySetPerMonitorDpiAware();
        }

        /// <summary>
        /// Occurs when the system or monitor DPI for this window has changed.
        /// </summary>
        public event EventHandler DpiChanged;

        /// <summary>
        /// Gets the DPI information for this window instance.
        /// </summary>
        /// <remarks>
        /// DPI information is available after a window handle has been created.
        /// </remarks>
        public DpiInformation DpiInformation => this.dpiInfo ?? Gu.Wpf.ModernUI.DpiInformation.Identity;

        /// <summary>
        /// Raises the System.Windows.Window.Closed event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // detach global event handlers
            SystemEvents.DisplaySettingsChanged -= this.OnSystemEventsDisplaySettingsChanged;
        }

        /// <summary>
        /// Refreshes the current monitor DPI settings and update the window size and layout scale accordingly.
        /// </summary>
        protected void RefreshMonitorDpi()
        {
            if (!this.isPerMonitorDpiAware)
            {
                return;
            }

            // get the current DPI of the monitor of the window
            var monitor = NativeMethods.MonitorFromWindow(this.source.Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);

            uint xDpi = 96;
            uint yDpi = 96;
            if (NativeMethods.GetDpiForMonitor(monitor, (int)MonitorDpiType.EffectiveDpi, ref xDpi, ref yDpi) != NativeMethods.S_OK)
            {
                xDpi = 96;
                yDpi = 96;
            }

            // vector contains the change of the old to new DPI
            var dpiVector = this.dpiInfo.UpdateMonitorDpi(xDpi, yDpi);

            // update Width and Height based on the current DPI of the monitor
            this.UpdateWindowSize(this.Width * dpiVector.X, this.Height * dpiVector.Y);

            // update graphics and text based on the current DPI of the monitor
            this.UpdateLayoutTransform();
        }

        /// <summary>
        /// Raises the <see cref="E:DpiChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnDpiChanged(EventArgs e)
        {
            this.DpiChanged?.Invoke(this, e);
        }

        private void OnSystemEventsDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (this.source != null && this.WindowState == WindowState.Minimized)
            {
                this.RefreshMonitorDpi();
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.source = (HwndSource)PresentationSource.FromVisual(this);

            // calculate the DPI used by WPF; this is the same as the system DPI
            var matrix = this.source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;

            this.dpiInfo = new DpiInformation(96D * matrix.M11, 96D * matrix.M22);

            if (this.isPerMonitorDpiAware)
            {
                this.source?.AddHook(this.WndProc);

                this.RefreshMonitorDpi();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_DPICHANGED)
            {
                // Marshal the value in the lParam into a Rect.
                var newDisplayRect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                // Set the Window's position & size.
                var matrix = this.source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
                var ul = matrix.Transform(new Vector(newDisplayRect.left, newDisplayRect.top));
                var hw = matrix.Transform(new Vector(newDisplayRect.right - newDisplayRect.left, newDisplayRect.bottom - newDisplayRect.top));
                this.SetCurrentValue(LeftProperty, ul.X);
                this.SetCurrentValue(TopProperty, ul.Y);
                this.UpdateWindowSize(hw.X, hw.Y);

                // Remember the current DPI settings.
                var oldDpiX = this.dpiInfo.MonitorDpiX;
                var oldDpiY = this.dpiInfo.MonitorDpiY;

                // Get the new DPI settings from wParam
                var dpiX = (double)(wParam.ToInt32() >> 16);
                var dpiY = (double)(wParam.ToInt32() & 0x0000FFFF);

                if (oldDpiX != dpiX || oldDpiY != dpiY)
                {
                    this.dpiInfo.UpdateMonitorDpi(dpiX, dpiY);

                    // update layout scale
                    this.UpdateLayoutTransform();

                    // raise DpiChanged event
                    this.OnDpiChanged(EventArgs.Empty);
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        private void UpdateLayoutTransform()
        {
            if (this.isPerMonitorDpiAware)
            {
                var root = (FrameworkElement)this.GetVisualChild(0);
                if (root != null)
                {
                    if (Math.Abs(this.DpiInformation.ScaleX - 1) > ScaleTolerance || Math.Abs(this.DpiInformation.ScaleY - 1) > ScaleTolerance)
                    {
                        root.SetCurrentValue(LayoutTransformProperty, new ScaleTransform(this.dpiInfo.ScaleX, this.dpiInfo.ScaleY));
                    }
                    else
                    {
                        root.SetCurrentValue(LayoutTransformProperty, null);
                    }
                }
            }
        }

        private void UpdateWindowSize(double width, double height)
        {
            // determine relative scalex and scaley
            var relScaleX = width / this.Width;
            var relScaleY = height / this.Height;

            if (Math.Abs(relScaleX - 1) > ScaleTolerance || Math.Abs(relScaleY - 1) > ScaleTolerance)
            {
                // adjust window size constraints as well
                this.MinWidth *= relScaleX;
                this.MaxWidth *= relScaleX;
                this.MinHeight *= relScaleY;
                this.MaxHeight *= relScaleY;

                this.SetCurrentValue(WidthProperty, width);
                this.SetCurrentValue(HeightProperty, height);
            }
        }
    }
}
