using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;

namespace Colorize
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        private byte _red = 255;
        private byte _green = 255;
        private byte _blue = 255;
        private string _hex;
        private string _hsl;
        private string _rgb;
        private Brush _colorPreview;
        private bool isUpdating;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte Red
        {
            get => _red;
            set
            {
                if (_red != value)
                {
                    _red = value;
                    OnPropertyChanged(nameof(Red));
                    UpdateColorsFromRGB();
                }
            }
        }

        public byte Green
        {
            get => _green;
            set
            {
                if (_green != value)
                {
                    _green = value;
                    OnPropertyChanged(nameof(Green));
                    UpdateColorsFromRGB();
                }
            }
        }

        public byte Blue
        {
            get => _blue;
            set
            {
                if (_blue != value)
                {
                    _blue = value;
                    OnPropertyChanged(nameof(Blue));
                    UpdateColorsFromRGB();
                }
            }
        }

        public string Hex
        {
            get => _hex;
            set
            {
                if (_hex != value)
                {
                    _hex = value;
                    OnPropertyChanged(nameof(Hex));
                    UpdateColorsFromHex();
                }
            }
        }

        public string HSL
        {
            get => _hsl;
            set
            {
                if (_hsl != value)
                {
                    _hsl = value;
                    OnPropertyChanged(nameof(HSL));
                    UpdateColorsFromHSL();
                }
            }
        }

        public string RGB
        {
            get => _rgb;
            set
            {
                if (_rgb != value)
                {
                    _rgb = value;
                    OnPropertyChanged(nameof(RGB));
                    UpdateColorsFromRGBString();
                }
            }
        }

        public Brush ColorPreview
        {
            get => _colorPreview;
            private set
            {
                if (_colorPreview != value)
                {
                    _colorPreview = value;
                    OnPropertyChanged(nameof(ColorPreview));
                }
            }
        }

        private void UpdateColorsFromRGB()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                Hex = $"#{Red:X2}{Green:X2}{Blue:X2}";
                HSL = $"{ RGBToHSL(Red, Green, Blue)}";
                RGB = $"rgb({Red}, {Green}, {Blue})";

                ColorPreview = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
                OnPropertyChanged(nameof(ColorPreview));
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void UpdateColorsFromHex()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                if (ColorConverter.ConvertFromString(Hex) is Color color)
                {
                    Red = color.R;
                    Green = color.G;
                    Blue = color.B;

                    HSL = $"{RGBToHSL(Red, Green, Blue)}";
                    OnPropertyChanged(nameof(HSL));
                    RGB = $"rgb({Red}, {Green}, {Blue})";
                    OnPropertyChanged(nameof(RGB));
                    ColorPreview = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
                    OnPropertyChanged(nameof(ColorPreview));
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void UpdateColorsFromHSL()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                var (r, g, b) = HSLToRGB(HSL);
                Red = r;
                Green = g;
                Blue = b;

                Hex = $"#{Red:X2}{Green:X2}{Blue:X2}";
                OnPropertyChanged(nameof(Hex));
                RGB = $"rgb({Red}, {Green}, {Blue})";
                OnPropertyChanged(nameof(RGB));
                ColorPreview = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
                OnPropertyChanged(nameof(ColorPreview));
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void UpdateColorsFromRGBString()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                var parts = RGB.Replace("rgb(", "").Replace(")", "").Split(',');
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0].Trim(), out byte r) &&
                    byte.TryParse(parts[1].Trim(), out byte g) &&
                    byte.TryParse(parts[2].Trim(), out byte b))
                {
                    Red = r;
                    Green = g;
                    Blue = b;

                    Hex = $"#{Red:X2}{Green:X2}{Blue:X2}";
                    OnPropertyChanged(nameof(Hex));
                    HSL = $"{RGBToHSL(Red, Green, Blue)}";
                    OnPropertyChanged(nameof(HSL));
                    ColorPreview = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
                    OnPropertyChanged(nameof(ColorPreview));
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private string RGBToHSL(byte r, byte g, byte b)
        {
            double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double h = 0, s, l = (max + min) / 2;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (max == rd)
                    h = (gd - bd) / d + (gd < bd ? 6 : 0);
                else if (max == gd)
                    h = (bd - rd) / d + 2;
                else
                    h = (rd - gd) / d + 4;

                h /= 6;
            }
            else
            {
                s = 0;
                h = 0;
            }

            return $"hsl({Math.Round(h * 360)}, {Math.Round(s * 100)}%, {Math.Round(l * 100)}%)";
        }

        private (byte r, byte g, byte b) HSLToRGB(string hsl)
        {
            var parts = hsl.Replace("hsl(", "").Replace(")", "").Split(',');
            if (parts.Length == 3 &&
                double.TryParse(parts[0], out double h) &&
                double.TryParse(parts[1].Replace("%", ""), out double s) &&
                double.TryParse(parts[2].Replace("%", ""), out double l))
            {
                h /= 360;
                s /= 100;
                l /= 100;

                double r, g, b;

                if (s == 0)
                {
                    r = g = b = l;
                }
                else
                {
                    double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                    double p = 2 * l - q;
                    r = HueToRGB(p, q, h + 1 / 3.0);
                    g = HueToRGB(p, q, h);
                    b = HueToRGB(p, q, h - 1 / 3.0);
                }

                return ((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
            }

            return (0, 0, 0);
        }

        private double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1 / 2.0) return q;
            if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
            return p;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
