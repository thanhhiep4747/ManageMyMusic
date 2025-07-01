using ManageMyMusic.Core;
using ManageMyMusic.Core.Extensions;
using ManageMyMusic.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ManageMyMusic
{
    public class Actions : IActions
    {
        #region Console Color
        string ConsoleColor_NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
        string ConsoleColor_RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
        string ConsoleColor_GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
        string ConsoleColor_YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
        string ConsoleColor_BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
        #endregion

        private readonly IMusicConfiguration m_MusicConfiguration;

        public Actions(IMusicConfiguration musicConfiguration)
        {
            m_MusicConfiguration = musicConfiguration;
        }

        public async Task DoActionsAsync()
        {
            await GetAndCreateStructureFolderAsync("MusicStructure.json");

            GetAllZipFilesPath();

            ExcuteManageMergeMusicFile(m_MusicConfiguration.AppSettings.SourceFolder);
        }

        #region Step 1: Verify Music Structure
        private async Task GetAndCreateStructureFolderAsync(string jsonFile)
        {
            if (string.IsNullOrWhiteSpace(m_MusicConfiguration.AppSettings.DestinationFolder))
            {
                Console.WriteLine($"{ConsoleColor_RED} Error: DestinationFolder is empty");
                return;
            }

            string jsonFilePath = Path.Combine(AppContext.BaseDirectory, $"Data/{jsonFile}");

            string fullOutputPath = m_MusicConfiguration.AppSettings.DestinationFolder;

            Console.WriteLine($"Searching for JSON file at: {jsonFilePath}");
            Console.WriteLine($"Output directory will be: {fullOutputPath}");

            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"{ConsoleColor_RED}Error: JSON file '{jsonFile}' not found at the specified path.");
                return;
            }

            try
            {
                // Step 1: Read JSON file content
                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                Console.WriteLine("JSON file content read successfully.");

                // Step 2: Parse JSON into a JObject for flexible traversal using Newtonsoft.Json
                JObject rootObject = JObject.Parse(jsonContent);

                if (rootObject == null || !rootObject.HasValues)
                {
                    Console.WriteLine("The JSON file does not contain a valid object structure or is empty.");
                    return;
                }

                // Step 3: Create the root output directory if it doesn't exist
                if (!Directory.Exists(fullOutputPath))
                {
                    Directory.CreateDirectory(fullOutputPath);
                    Console.WriteLine($"Root directory created: {fullOutputPath}");
                }
                else
                {
                    Console.WriteLine($"Root directory already exists: {fullOutputPath}");
                }

                // Step 4: Iterate through JSON structure and create folders
                Console.WriteLine("\nCreating folder structure...");
                CreateFoldersRecursive(rootObject, fullOutputPath, 0);

                Console.WriteLine("\nFolder structure creation completed.");
            }
            catch (JsonException ex) // Catches Newtonsoft.Json's JsonException
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                Console.WriteLine("Please ensure the JSON file has a valid format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
        private static void CreateFoldersRecursive(JToken token, string currentPath, int depth)
        {
            string indent = new string(' ', depth * 2); // For console output indentation

            if (token.Type != JTokenType.Object)
            {
                return;
            }

            JObject currentObject = (JObject)token;

            foreach (JProperty property in currentObject.Properties())
            {
                string folderName = property.Name;
                string newPath = Path.Combine(currentPath, folderName);

                try
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                        Console.WriteLine($"{indent}- Created: {folderName} at '{newPath}'");
                    }
                    else
                    {
                        Console.WriteLine($"{indent}- Exists: {folderName} at '{newPath}'");
                    }

                    // If the property's value is an object, recurse into it
                    if (property.Value.Type == JTokenType.Object)
                    {
                        CreateFoldersRecursive(property.Value, newPath, depth + 1);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"{indent}Error: Unauthorized access to create folder '{newPath}'. Please check permissions.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"{indent}I/O error while creating folder '{newPath}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{indent}Unknown error while creating folder '{newPath}': {ex.Message}");
                }
            }
        }
        #endregion


        #region Step 2: Verify and Excute Zip Files
        public void GetAllZipFilesPath()
        {
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

                        ExtractZipFile(filePath, folderPath);
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
        }

        private bool ExtractZipFile(string zipFilePath, string destinationExtracting)
        {
            if (!File.Exists(zipFilePath))
                return false;

            var zipFileName = Path.GetFileName(zipFilePath);

            try
            {
                Console.WriteLine($"Extracting '{zipFileName}' to '{destinationExtracting}'...");
                ZipFile.ExtractToDirectory(zipFilePath, destinationExtracting, true); // 'true' to overwrite existing files

                Console.WriteLine($"Successfully extracted '{zipFileName}'.");

                Console.WriteLine($"Deleting original zip file '{zipFileName}'...");
                File.Delete(zipFilePath);
                Console.WriteLine($"Successfully deleted '{zipFileName}'.");

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Unauthorized access during zip operation. Please check permissions for '{zipFilePath}' or '{destinationExtracting}'.");
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error during zip operation: {ex.Message}");
                return false;
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Error: The zip file '{zipFileName}' is corrupted or not a valid zip archive: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during zip operations: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Step 3: 

        public void ExcuteManageMergeMusicFile(string path)
        {
            GetAllMusicFileInPath(path);
            GetAllFolderInPath(path);
        }

        public void GetAllFolderInPath(string path)
        {
            try
            {
                string[] directories = Directory.GetDirectories(path);

                Console.WriteLine($"Folders in {path}:");
                foreach (string dir in directories)
                {
                    Console.WriteLine(dir);
                    ExcuteManageMergeMusicFile(dir);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Error: Directory '{path}' not found.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Access to '{path}' is denied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public void GetAllMusicFileInPath(string path)
        {
            try
            {
                string[] files = Directory.GetFiles(path);
                Console.WriteLine("\nFiles:");
                if (files.Length == 0)
                {
                    Console.WriteLine($"  No files found in {path}");
                    return;
                }
                foreach (string file in files)
                {
                    var extension = Path.GetExtension(file);
                    if (m_MusicConfiguration.AppSettings.ExtensionMusicFiles.Contains(extension))
                    {
                        Console.WriteLine($"  {Path.GetFileName(file)}"); // Just the file name
                        ReadAudioFileInfo(file);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Error: Directory '{path}' not found.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Access to '{path}' is denied.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public void ReadAudioFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at '{filePath}'");
                return;
            }

            try
            {
                // Create a TagLib.File object from the file path
                using (TagLib.File file = TagLib.File.Create(filePath))
                {
                    Console.WriteLine($"File: {Path.GetFileName(filePath)}");

                    // --- Common Audio Properties ---
                    Console.WriteLine($"  Duration: {file.Properties.Duration}");
                    Console.WriteLine($"  Sample Rate: {file.Properties.AudioSampleRate} Hz");
                    Console.WriteLine($"  Channels: {file.Properties.AudioChannels}");
                    Console.WriteLine($"  Bits Per Sample: {file.Properties.BitsPerSample}");
                    Console.WriteLine($"  Codecs: {string.Join(", ", file.Properties.Codecs)}");
                    Console.WriteLine($"  Media Types: {file.Properties.MediaTypes}");

                    // --- Tag (Metadata) Information (More relevant for FLAC, MP3, etc.) ---
                    if (file.Tag != null)
                    {
                        Console.WriteLine("\n  --- Metadata (Tags) ---");
                        Console.WriteLine($"    Title: {FixingErrorUtf8Format(file.Tag.Title)}");
                        Console.WriteLine($"    Artist(s): {FixingErrorUtf8Format(string.Join(", ", file.Tag.Performers))}");
                        Console.WriteLine($"    Album: {FixingErrorUtf8Format(file.Tag.Album)}");
                        Console.WriteLine($"    Year: {file.Tag.Year}");
                        Console.WriteLine($"    Genre(s): {FixingErrorUtf8Format(string.Join(", ", file.Tag.Genres))}");
                        Console.WriteLine($"    Track: {file.Tag.Track}");
                        Console.WriteLine($"    Disc: {file.Tag.Disc}");
                        Console.WriteLine($"    Comment: {FixingErrorUtf8Format(file.Tag.Comment)}");
                        Console.WriteLine($"    Lyrics: {FixingErrorUtf8Format(file.Tag.Lyrics)}");
                    }
                    else
                    {
                        Console.WriteLine("\n  No extensive metadata (tags) found for this file type.");
                    }
                }
            }
            catch (TagLib.UnsupportedFormatException ex)
            {
                Console.WriteLine($"Error: '{filePath}' is not a supported audio format. {ex.Message}");
            }
            catch (TagLib.CorruptFileException ex)
            {
                Console.WriteLine($"Error: '{filePath}' is a corrupt file. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while processing '{filePath}': {ex.Message}");
            }
        }

        private string FixingErrorUtf8Format(string source)
        {
            byte[] rawBytes = Encoding.Default.GetBytes(source); // Get bytes based on current system's default encoding (often similar to 1252)
                                                                   // Or specifically try: byte[] rawBytes = Encoding.GetEncoding(1252).GetBytes(rawTitle);

            string fixed_UTF8 = Encoding.UTF8.GetString(rawBytes);

            return fixed_UTF8;
        }

        #endregion
    }
}
