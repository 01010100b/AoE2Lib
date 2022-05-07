using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UI
{
    internal class UISettings
    {
        public static UISettings Load()
        {
            var file = Path.Combine(Program.Folder, "uisettings.json");

            if (File.Exists(file))
            {
                return Program.Deserialize<UISettings>(file);
            }
            else
            {
                return new UISettings();
            }
        }

        public string ExePath { get; set; } = "path-to-exe";

        public void Save()
        {
            var file = Path.Combine(Program.Folder, "uisettings.json");

            Program.Serialize(this, file);
        }
    }
}
