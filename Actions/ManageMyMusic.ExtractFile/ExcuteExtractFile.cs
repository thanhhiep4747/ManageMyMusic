using ManageMyMusic.Core;
using ManageMyMusic.ExtractFile.Interfaces;

namespace ManageMyMusic.ExtractFile
{
    public class ExcuteExtractFile : IExcuteExtractFile
    {
        private readonly IMusicConfiguration m_MusicConfiguration;

        public ExcuteExtractFile(IMusicConfiguration musicConfiguration)
        {
            m_MusicConfiguration = musicConfiguration;
        }

        public IEnumerable<string> GetAllZipFilesPath()
        {
            var listZipFiles = new List<string>();
            var folderPath = m_MusicConfiguration.AppSettings.SourceFolder;

            Console.WriteLine($"Searching for .zip files in '{folderPath}'...");

            try
            {
                Console.WriteLine("\n--- Searching Top Directory Only ---");
                string[] zipFilesTopOnly = Directory.GetFiles(folderPath, "*.zip", SearchOption.TopDirectoryOnly);

                if (zipFilesTopOnly.Length > 0)
                {
                    Console.WriteLine($"Found {zipFilesTopOnly.Length} .zip files:");
                    foreach (string filePath in zipFilesTopOnly)
                    {
                        Console.WriteLine($"- {filePath}");

                        listZipFiles.Add(filePath);
                    }
                }
                else
                {
                    Console.WriteLine("No .zip files found in the top directory.");
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Error: The specified directory was not found: '{folderPath}'");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Access to the directory '{folderPath}' is denied. Check file permissions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();


            return listZipFiles;
        }
    }
}
