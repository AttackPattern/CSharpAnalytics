using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace CSharpAnalytics.Serializers
{
    /// <summary>
    /// Provides an easy way to serialize and deserialize simple classes to a user AppData folder in
    /// Windows Forms applications.
    /// </summary>
    internal static class AppDataContractSerializer
    {
        private static string folderPath;

        /// <summary>
        /// Gets or sets the file system path where the data should be serialized to and from.
        /// </summary>
        public static string FolderPath
        {
            get { return folderPath ?? (folderPath = GetDefaultFolderPath()); }
            set { folderPath = value; }
        }

        /// <summary>
        /// Determine a default file system path for the serialization of the data.
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultFolderPath()
        {
            var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            var customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            var folderName = customAttributes.Length > 0
                ? ((AssemblyCompanyAttribute)customAttributes[0]).Company
                : "CSharpAnalytics";

            return Path.Combine(appDataPath, folderName);
        }

        /// <summary>
        /// Restore an object from local folder storage.
        /// </summary>
        /// <param name="filename">Optional filename to use, name of the class if not provided.</param>
        /// <param name="deleteBadData">Optional boolean on whether delete the existing file if deserialization fails, defaults to false.</param>
        /// <returns>Task that holds the deserialized object once complete.</returns>
        public static async Task<T> Restore<T>(string filename = null, bool deleteBadData = false)
        {
            var serializer = new DataContractSerializer(typeof(T), new[] { typeof(DateTimeOffset) });
            var settings = new XmlReaderSettings { Async = true };

            try
            {
                var file = GetFilePath<T>(filename);
                try
                {
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    using (var xmlReader = XmlReader.Create(fileStream, settings))
                    {
                        await xmlReader.ReadAsync();
                        return (T)serializer.ReadObject(xmlReader);
                    }
                }
                catch (SerializationException)
                {
                    if (deleteBadData)
                        File.Delete(file);
                }
                catch (XmlException)
                {
                    if (deleteBadData)
                        File.Delete(file);
                }

                return default(T);
            }
            catch (FileNotFoundException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Save an object to local folder storage asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/> to save.</typeparam>
        /// <param name="value">Object to save to local storage.</param>
        /// <param name="filename">Optional filename to save to, defaults to the name of the class.</param>
        /// <returns>Task that completes once the object is saved.</returns>
        public static async Task Save<T>(T value, string filename = null)
        {
            var serializer = new DataContractSerializer(typeof(T), new[] { typeof(DateTimeOffset) });
            var settings = new XmlWriterSettings { Indent = true, Async = true };
            var file = GetFilePath<T>(filename);

            try
            {
                using (var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var xmlWriter = XmlWriter.Create(fileStream, settings))
                {
                    serializer.WriteObject(xmlWriter, value);
                    await xmlWriter.FlushAsync();
                }
            }
            catch (UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Failed to save to {0}. You may have insufficient rights or a synchronization may be occuring.", file);
            }
        }

        /// <summary>
        /// Gets the file path of the given type and file name.
        /// </summary>
        /// <typeparam name="T">The type to get path.</typeparam>
        /// <param name="filename">The file name to get path.</param>
        /// <returns>The full path to the file.</returns>
        private static string GetFilePath<T>(string filename)
        {
            // Ensure directory exists
            Directory.CreateDirectory(FolderPath);

            return Path.Combine(FolderPath, filename ?? typeof(T).Name);
        }
    }
}
