using ManageMyMusic.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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


        #region Verify and Excute Zip Files
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
        #endregion
    }
}
