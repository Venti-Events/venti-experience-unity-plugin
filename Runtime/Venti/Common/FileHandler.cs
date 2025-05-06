using System.IO;
using System;
using UnityEngine;

namespace Venti
{
    public class FileHandler
    {
        public static string GetFolderPath(string folderName, bool isEditor = false)
        {
            string directoryPath;
            if (isEditor)
                directoryPath = Path.Combine(Application.dataPath, folderName);
            else
                directoryPath = Path.Combine(Application.persistentDataPath, folderName);

            return directoryPath;
        }

        public static string GetFilePath(string filename, string folderName, bool isEditor = false)
        {
            string directoryPath = GetFolderPath(folderName, isEditor);
            string validFileName = ConvertToValidFileName(filename);
            string filePath = Path.Combine(directoryPath, validFileName);

            return filePath;
        }

        public static bool FileExists(string filename, string folderName, bool isEditor = false)
        {
            string filePath = GetFilePath(filename, folderName, isEditor);
            return File.Exists(filePath);
        }

        public static void WriteString(string content, string filename, string folderName, bool isEditor = false)
        {
            string directoryPath = GetFolderPath(folderName, isEditor);
            string filePath = GetFilePath(filename, folderName, isEditor);

            try
            {
                FileStream fileStream = null;

                // Create folder if it does not exist
                Directory.CreateDirectory(directoryPath);

                // Open and use file
                using (fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    // Clear file contents
                    fileStream.SetLength(0);

                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Close();

                    Debug.Log("File Saved Successfully at " + filePath);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Exception encountered while generating schema : " + e.Message);

            }
        }

        public static void WriteBytes(byte[] bytes, string filename, string folderName, bool isEditor = false)
        {
            string directoryPath = GetFolderPath(folderName, isEditor);
            string filePath = GetFilePath(filename, folderName, isEditor);

            try
            {
                FileStream fileStream = null;

                // Create folder if it does not exist
                Directory.CreateDirectory(directoryPath);

                // Open and use file
                using (fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    // Clear file contents
                    fileStream.SetLength(0);

                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Close();

                    Debug.Log("File Saved Successfully at " + filePath);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Exception encountered while generating schema : " + e.Message);

            }
        }

        public static string ReadFile(string filename, string folderName, bool isEditor = false)
        {
            try
            {
                string filePath = GetFilePath(filename, folderName, isEditor);
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return null;
                }

                string contents = File.ReadAllText(filePath);
                return contents;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading file: {e.Message}");
                return null;
            }
        }

        public static byte[] ReadFileBytes(string filename, string folderName, bool isEditor = false)
        {
            try
            {
                string filePath = GetFilePath(filename, folderName, isEditor);
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);
                return bytes;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading file: {e.Message}");
                return null;
            }
        }

        public static void DeleteFile(string filename, string folderName, bool isEditor = false)
        {
            string filePath = GetFilePath(filename, folderName, isEditor);
            if (File.Exists(filePath))
                File.Delete(filePath);
            else
                Debug.LogWarning($"File not found: {filePath}");
        }

        public static string ConvertToValidFileName(string fileName)
        {
            string validFileName = fileName;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                validFileName = validFileName.Replace(c, '_');
            }

            return validFileName;
        }
    }
}
