using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Colorize
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private double _currentX = 0;
        private double _currentY = 0;
        private const double SmoothFactor = 0.8;
        private const int UpdateThreshold = 2; 
        private Color _currentColor = Colors.Transparent;

        private ColorPickerViewModel _viewModel;

        public ColorPicker()
        {
            InitializeComponent();
            _viewModel = new ColorPickerViewModel();
            DataContext = _viewModel;
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (GetCursorPos(out POINT point))
            {
                UpdatePopupOffset(point);
                GetMousePixelColor(point);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CompositionTarget.Rendering -= OnRendering;
        }


        private void GetMousePixelColor(POINT point)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, point.X, point.Y);
            ReleaseDC(IntPtr.Zero, hdc);

            byte r = (byte)(pixel & 0x000000FF);
            byte g = (byte)((pixel & 0x0000FF00) >> 8);
            byte b = (byte)((pixel & 0x00FF0000) >> 16);

            _currentColor = Color.FromRgb(r, g, b);

            this.Dispatcher.Invoke(() =>
            {
                ((Border)MouseColorPopup.Child).Background = new SolidColorBrush(_currentColor);
            });
        }

        private void UpdatePopupOffset(POINT point)
        {
            if (Math.Abs(point.X - _currentX) < UpdateThreshold && Math.Abs(point.Y - _currentY) < UpdateThreshold)
                return;

            _currentX += (point.X - _currentX) * SmoothFactor;
            _currentY += (point.Y - _currentY) * SmoothFactor;

            this.Dispatcher.Invoke(() => 
            {
                if (!MouseColorPopup.IsOpen || !MouseClickPopup.IsOpen)
                {
                    MouseColorPopup.IsOpen = true;
                    MouseClickPopup.IsOpen = true;
                }

                MouseColorPopup.HorizontalOffset = _currentX + 10;
                MouseColorPopup.VerticalOffset = _currentY + 10;

                MouseClickPopup.HorizontalOffset = _currentX - 7.5;
                MouseClickPopup.VerticalOffset = _currentY - 7.5;
            });
        }

        private void PickColor(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                OnRendering(null, new EventArgs());
                MouseColorPopup.IsOpen = false;
                MouseClickPopup.IsOpen = false;
                await Task.Delay(5);
                _viewModel.Red = _currentColor.R;
                _viewModel.Green = _currentColor.G;
                _viewModel.Blue = _currentColor.B;
                Clipboard.SetText($"{_viewModel.Hex}");
                CompositionTarget.Rendering -= OnRendering;
            }));
        }

        private void ButtonPicker_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += OnRendering;
        }

        private void ButtonCopyHex_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{TextHex.Text}");
        }

        private void ButtonCopyHsl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{TextHsl.Text}");
        }

        private void ButtonCopyRgb_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{TextRgb.Text}");
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                BindingExpression binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();
            }
        }
    }
}
