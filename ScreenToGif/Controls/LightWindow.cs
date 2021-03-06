﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace ScreenToGif.Controls
{
    /// <summary>
    /// Light Window used by the Recorder.
    /// </summary>
    public class LightWindow : Window
    {
        #region Native

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Variables

        public readonly static DependencyProperty ChildProperty;
        public readonly static DependencyProperty MaxSizeProperty;

        private bool _isRecording;

        #endregion

        #region Properties

        private string _caption;

        /// <summary>
        /// The caption/title of the window.
        /// </summary>
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;

                var text = GetTemplateChild("CaptionText") as TextBlock;
                if (text != null) text.Text = _caption;
            }
        }

        /// <summary>
        /// The Image of the caption bar.
        /// </summary>
        [Description("The Image of the caption bar.")]
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetCurrentValue(ChildProperty, value); }
        }

        /// <summary>
        /// The maximum size of the image.
        /// </summary>
        [Description("The maximum size of the image.")]
        public double MaxSize
        {
            get { return (double)GetValue(MaxSizeProperty); }
            set { SetCurrentValue(MaxSizeProperty, value); }
        }

        #endregion

        #region Constructors

        static LightWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LightWindow), new FrameworkPropertyMetadata(typeof(LightWindow)));

            ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(LightWindow), new FrameworkPropertyMetadata());
            MaxSizeProperty = DependencyProperty.Register("MaxSize", typeof(double), typeof(LightWindow), new FrameworkPropertyMetadata(26.0));
        }

        /// <summary>
        /// Default constructor. Registers the PreviewMouseMove event.
        /// </summary>
        public LightWindow()
        {
            PreviewMouseMove += OnPreviewMouseMove;
        }

        #endregion

        #region Click Events

        private void BackClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = false;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;

                var button = sender as Button;
                if (button != null) button.Content = Resources["Vector.Restore"];
            }
            else
            {
                WindowState = WindowState.Normal;

                var button = sender as Button;
                if (button != null) button.Content = Resources["Vector.Maximize"];
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion

        #region Initializers

        public override void OnApplyTemplate()
        {
            var backButton = GetTemplateChild("BackButton") as ImageButton;
            if (backButton != null)
                backButton.Click += BackClick;

            var minimizeButton = GetTemplateChild("MinimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.Click += MinimizeClick;

            var restoreButton = GetTemplateChild("RestoreButton") as Button;
            if (restoreButton != null)
                restoreButton.Click += RestoreClick;

            var closeButton = GetTemplateChild("CloseButton") as Button;
            if (closeButton != null)
                closeButton.Click += CloseClick;

            var moveRectangle = GetTemplateChild("MoveRectangle") as Grid;
            if (moveRectangle != null)
                moveRectangle.PreviewMouseDown += MoveRectangle_PreviewMouseDown;

            var resizeGrid = GetTemplateChild("ResizeGrid") as Grid;
            if (resizeGrid != null)
            {
                foreach (UIElement element in resizeGrid.Children)
                {
                    var resizeRectangle = element as Rectangle;
                    if (resizeRectangle != null)
                    {
                        resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                        resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                    }
                }
            }

            base.OnApplyTemplate();
        }

        private HwndSource _hwndSource;

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;

            base.OnInitialized(e);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        #endregion

        #region Drag

        private void MoveRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        #endregion

        #region Resize

        private void ResizeRectangle_MouseMove(Object sender, MouseEventArgs e)
        {
            var rectangle = sender as Rectangle;

            if (rectangle != null && !_isRecording)
                switch (rectangle.Name)
                {
                    case "top":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "bottom":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "left":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "right":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "topLeft":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case "topRight":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "bottomLeft":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "bottomRight":
                        Cursor = Cursors.SizeNWSE;
                        break;
                }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }

        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || _isRecording) return;

            var rectangle = sender as Rectangle;

            if (rectangle != null)
                switch (rectangle.Name)
                {
                    case "top":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Top);
                        break;
                    case "bottom":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Bottom);
                        break;
                    case "left":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Left);
                        break;
                    case "right":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Right);
                        break;
                    case "topLeft":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.TopLeft);
                        break;
                    case "topRight":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.TopRight);
                        break;
                    case "bottomLeft":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.BottomLeft);
                        break;
                    case "bottomRight":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.BottomRight);
                        break;
                }
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8
        }

        #endregion

        #region Special Methods

        /// <summary>
        /// If recording is active, the minimize and maximize buttons should be disabled.
        /// </summary>
        /// <param name="status">True if recording is active.</param>
        public void IsRecording(bool status)
        {
            var minimizeButton = GetTemplateChild("MinimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.IsEnabled = !status;

            var restoreButton = GetTemplateChild("RestoreButton") as Button;
            if (restoreButton != null)
                restoreButton.IsEnabled = !status;

            var backButton = GetTemplateChild("BackButton") as ImageButton;
            if (backButton != null)
                backButton.IsEnabled = !status;

            _isRecording = status;
        }

        /// <summary>
        /// Hides the Back button.
        /// </summary>
        public void HideBackButton()
        {
            var backButton = GetTemplateChild("BackButton") as ImageButton;
            if (backButton != null)
                backButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Hides the Minimize and Maximize buttons.
        /// </summary>
        /// <param name="hide">True if should hide the buttons.</param>
        public void HideMinimizeAndMaximize(bool hide)
        {
            var minimizeButton = GetTemplateChild("MinimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;

            var restoreButton = GetTemplateChild("RestoreButton") as Button;
            if (restoreButton != null)
                restoreButton.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
