using System.Linq;
using Windows.Foundation;
using Windows.Globalization;
using Windows.UI.Xaml;

namespace CSharpAnalytics.WindowsStore
{
    /// <summary>
    /// Implements the IEnvironment interface required by analytics to track details of the machine
    /// in a WindowsStore application.
    /// </summary>
    internal class WindowsStoreEnvironment : IEnvironment
    {
        public string CharacterSet { get { return "UTF-8"; } }
        public string LanguageCode { get { return ApplicationLanguages.Languages.First(); } }

        public string FlashVersion { get { return null; } }
        public bool? JavaEnabled { get { return null; } }
        public string IpAddress { get { return null; } }

        public uint ScreenColorDepth { get { return 32; } }

        public uint ScreenHeight
        {
            get { return (uint)Screen.Height; }
        }

        public uint ScreenWidth
        {
            get { return (uint)Screen.Width; }
        }

        public uint ViewportHeight
        {
            get { return (uint)Screen.Height; }
        }

        public uint ViewportWidth
        {
            get { return (uint)Screen.Width; }
        }

        private static Rect Screen { get { return Window.Current.CoreWindow.Bounds; } }
    }
}