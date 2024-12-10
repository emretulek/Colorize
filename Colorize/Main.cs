using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Wox.Plugin;

namespace Colorize
{
    public class Main : IPlugin
    {
        public void Init(PluginInitContext context) {
           
        }
        
        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>
            {
                new Result()
                {
                    Title = "Colorize",
                    SubTitle = "Color Picker",
                    IcoPath = "Images\\colorize.png",
                    Action = e =>
                    {
                        try{
                            var window = new ColorPicker();
                            window.ShowDialog();
                            return true;
                        }catch(Exception){
                            return false;
                        }
                    }
                }
            };

            var stringColors = StringColorList(query.Search);
            foreach (var color in stringColors)
            {
                results.Add(new Result()
                {
                    Title = color.Key,
                    SubTitle = color.Value,
                    IcoPath = ColorImage(color.Key, color.Value),
                    Action = e =>
                    {
                        Clipboard.SetText(color.Value);
                        return true;
                    }
                });
            }

            return results;
        }


        private Dictionary<string,string> StringColorList(string search)
        {
            var matchingColors = typeof(Colors)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                p => p.Name,
                p => "#" + p.GetValue(null).ToString().Substring(3)
            );

            return matchingColors;
        }

        private string ColorImage(string name, string color)
        {
            int width = 64;
            int height = 64;
            System.Drawing.Color backgroundColor = ColorTranslator.FromHtml(color);
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"colorize_{name}.png");

            if (File.Exists(tempFilePath))
            {
                return tempFilePath;
            }

            try
            {
                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(backgroundColor);
                    }

                    bitmap.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
                }

                return tempFilePath;
            }
            catch (Exception)
            {
                return "Images\\app.png";
            }
        }
    }
}
