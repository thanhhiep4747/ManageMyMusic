using ManageMyMusic.Core;
using ManageMyMusic.Core.Extensions;
using ManageMyMusic.Enums;
using ManageMyMusic.Interfaces;
using ManageMyMusic.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

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
        private readonly IMusicDataExcute m_musicDataExcute;
        private MusicDataModel m_MusicData = new MusicDataModel();

        public Actions(IMusicConfiguration musicConfiguration, IMusicDataExcute musicDataExcute)
        {
            m_MusicConfiguration = musicConfiguration;
            m_musicDataExcute = musicDataExcute;
        }

        public async Task DoActionsAsync()
        {
            await GetAndCreateStructureFolderAsync("MusicStructure.json");

            GetAllZipFilesPath();

            m_MusicData = await m_musicDataExcute.GetMusicDataModel();

            ExcuteManageMergeMusicFile(m_MusicConfiguration.AppSettings.SourceFolder);

            await m_musicDataExcute.SaveJsonData(JsonConvert.SerializeObject(m_MusicData.Albums), FileDataType.Album);
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

        #region Step 3: Read Information from source folder

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

                    //DisplayFileInformation(file);

                    if (file.Tag != null)
                    {
                        if (file.Tag.Performers != null && file.Tag.Performers.Any())
                        {
                            for (int i = 0; i < file.Tag.Performers.Count(); i++)
                            {
                                string artistName = file.Tag.Performers[i];
                                bool isLast = (i == file.Tag.Performers.Count() - 1);

                                bool deletesourcefile = false;
                                if (isLast)
                                {
                                    deletesourcefile = true;
                                }

                                string albumName = file.Tag.Album;

                                var musicStructureResponse = m_musicDataExcute.MergeAlbumIntoStructure(artistName, albumName, m_MusicData);
                                if (musicStructureResponse != null && !string.IsNullOrWhiteSpace(musicStructureResponse.RelativePath))
                                {
                                    var destinationDirectory = $"{m_MusicConfiguration.AppSettings.DestinationFolder}//{musicStructureResponse.RelativePath}";
                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }

                                    CopyRenameAndDeleteOriginal(filePath, destinationDirectory, deletesourcefile, false);
                                }
                            }
                        }
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

        #endregion

        #region Step 4: Merge Music Files 
        public void CopyRenameAndDeleteOriginal(string sourceFilePath, string destinationDirectory, bool deleteSourceFile, bool replaceIfExists = false)
        {
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"Error: Source file not found at '{sourceFilePath}'. Aborting operation.");
                return;
            }

            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Console.WriteLine("Error: Destination directory cannot be empty. Aborting operation.");
                return;
            }

            if (!Directory.Exists(destinationDirectory))
            {
                try
                {
                    Directory.CreateDirectory(destinationDirectory);
                    Console.WriteLine($"Created destination directory: '{destinationDirectory}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating destination directory '{destinationDirectory}': {ex.Message}");
                    return;
                }
            }

            string newFileName = Path.GetFileName(sourceFilePath).RemoveDiacriticsVietnamese();
            string destinationFilePath = Path.Combine(destinationDirectory, newFileName);

            Console.WriteLine($"Attempting to copy '{sourceFilePath}' to '{destinationFilePath}'...");

            try
            {
                File.Copy(sourceFilePath, destinationFilePath, replaceIfExists);
                Console.WriteLine($"Successfully copied file to: '{destinationFilePath}'");

                if (deleteSourceFile)
                {
                    Console.WriteLine($"Attempting to delete original file: '{sourceFilePath}'...");
                    File.Delete(sourceFilePath);
                    Console.WriteLine($"Successfully deleted original file: '{sourceFilePath}'");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: File not found during copy/delete. {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: Access denied during file operation. Check permissions. {ex.Message}");
            }
            catch (IOException ex) when (ex.Message.Contains("already exists") && ex.HResult == -2147024816)
            {
                // Verify new file high quality from existing file or not, if yes then replace

                TagLib.File TagLib_New = null;
                TagLib.File TabLib_Old = null;

                TagLib_New = TagLib.File.Create(sourceFilePath);
                TabLib_Old = TagLib.File.Create(destinationFilePath);

                var resultCompare = CompareFlacFiles(TagLib_New, TabLib_Old);
                if (resultCompare == ComparisonResult.NewFileBetter)
                    CopyRenameAndDeleteOriginal(sourceFilePath, destinationDirectory, deleteSourceFile, true);
                else
                    File.Delete(sourceFilePath);

            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error during file operation (IO related): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public ComparisonResult CompareFlacFiles(TagLib.File newFile, TagLib.File oldFile)
        {
            if (oldFile?.Properties == null || newFile?.Properties == null)
            {
                return ComparisonResult.CannotCompare;
            }

            var old = oldFile.Properties; // Old file properties
            var _new = newFile.Properties; // New file properties

            // Priority 1: Sample Rate
            if (_new.AudioSampleRate > old.AudioSampleRate)
            {
                Console.WriteLine($"New file has higher Sample Rate ({_new.AudioSampleRate} Hz > {old.AudioSampleRate} Hz).");
                return ComparisonResult.NewFileBetter;
            }
            if (old.AudioSampleRate > _new.AudioSampleRate)
            {
                Console.WriteLine($"Old file has higher Sample Rate ({old.AudioSampleRate} Hz > {_new.AudioSampleRate} Hz).");
                return ComparisonResult.OldFileBetter;
            }
            Console.WriteLine($"Sample Rate is the same ({old.AudioSampleRate} Hz).");

            // Priority 2: Bits Per Sample (Bit Depth)
            if (_new.BitsPerSample > old.BitsPerSample)
            {
                Console.WriteLine($"New file has higher Bits Per Sample ({_new.BitsPerSample} bits > {old.BitsPerSample} bits).");
                return ComparisonResult.NewFileBetter;
            }
            if (old.BitsPerSample > _new.BitsPerSample)
            {
                Console.WriteLine($"Old file has higher Bits Per Sample ({old.BitsPerSample} bits > {_new.BitsPerSample} bits).");
                return ComparisonResult.OldFileBetter;
            }
            Console.WriteLine($"Bits Per Sample is the same ({old.BitsPerSample} bits).");

            // Priority 3: Bitrate (for lossless, this often correlates with above, but can be a tie-breaker)
            if (_new.AudioBitrate > old.AudioBitrate)
            {
                Console.WriteLine($"New file has higher Bitrate ({_new.AudioBitrate} kbps > {old.AudioBitrate} kbps).");
                return ComparisonResult.NewFileBetter;
            }
            if (old.AudioBitrate > _new.AudioBitrate)
            {
                Console.WriteLine($"Old file has higher Bitrate ({old.AudioBitrate} kbps > {_new.AudioBitrate} kbps).");
                return ComparisonResult.OldFileBetter;
            }
            Console.WriteLine($"Bitrate is the same ({old.AudioBitrate} kbps).");

            // If all properties are the same
            return ComparisonResult.Equal;
        }

        private void DisplayFileInformation(TagLib.File file)
        {
            // --- Common Audio Properties ---
            Console.WriteLine($"  Duration: {file.Properties.Duration.TotalMilliseconds}");
            Console.WriteLine($"  Sample Rate: {file.Properties.AudioSampleRate} Hz");
            Console.WriteLine($"  Channels: {file.Properties.AudioChannels}");
            Console.WriteLine($"  Bits Per Sample: {file.Properties.BitsPerSample}");
            Console.WriteLine($"  Codecs: {string.Join(", ", file.Properties.Codecs)}");
            Console.WriteLine($"  Media Types: {file.Properties.MediaTypes}");

            // --- Tag (Metadata) Information (More relevant for FLAC, MP3, etc.) ---
            if (file.Tag != null)
            {
                Console.WriteLine("\n  --- Metadata (Tags) ---");
                Console.WriteLine($"    Title: {file.Tag.Title}");
                Console.WriteLine($"    Artist(s): {string.Join(", ", file.Tag.Performers)}");
                Console.WriteLine($"    Album: {file.Tag.Album}");
                Console.WriteLine($"    Year: {file.Tag.Year}");
                Console.WriteLine($"    Genre(s): {string.Join(", ", file.Tag.Genres)}");
                Console.WriteLine($"    Track: {file.Tag.Track}");
                Console.WriteLine($"    Disc: {file.Tag.Disc}");
                Console.WriteLine($"    Comment: {file.Tag.Comment}");
                Console.WriteLine($"    Lyrics: {file.Tag.Lyrics}");
            }
            else
            {
                Console.WriteLine("\n  No extensive metadata (tags) found for this file type.");
            }
        }
        #endregion
    }
}
