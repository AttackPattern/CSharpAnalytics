using System.Windows.Forms;

namespace CSharpAnalytics.Environment
{
    /// <summary>
    /// Implements the IEnvironment interface required by analytics to track details of the machine
    /// in a Windows Forms application.
    /// </summary>
    internal class WinFormsEnvironment : IEnvironment
    {
        public string CharacterSet { get { return "UTF-8"; } }

        public string LanguageCode
        {
            get { return "en-US"; }
        }

        public string FlashVersion { get { return null; } }
        public bool? JavaEnabled { get { return null; } }

        public uint ScreenColorDepth { get { return 32; } }

        public uint ScreenHeight
        {
            get { return (uint)Screen.PrimaryScreen.Bounds.Height; }
        }

        public uint ScreenWidth
        {
            get { return (uint)Screen.PrimaryScreen.Bounds.Width; }
        }

        public uint ViewportHeight
        {
            get { return (uint)Screen.PrimaryScreen.WorkingArea.Height; }
        }

        public uint ViewportWidth
        {
            get { return (uint)Screen.PrimaryScreen.WorkingArea.Width; }
        }
    }
}