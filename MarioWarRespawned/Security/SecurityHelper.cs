using MarioWarRespawned.Input;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioWarRespawned.Security
{
    public static class SecurityHelper
    {
        private static readonly string[] AllowedFileExtensions = { ".png", ".jpg", ".wav", ".mp3", ".ogg", ".json", ".xml" };
        private static readonly Regex PlayerNameRegex = new Regex(@"^[a-zA-Z0-9_\-\s]{1,16}$", RegexOptions.Compiled);

        public static string SanitizePlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Player";

            var sanitized = name.Trim();
            if (!PlayerNameRegex.IsMatch(sanitized))
            {
                // Remove invalid characters
                sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_\-\s]", "");
                if (sanitized.Length > 16) sanitized = sanitized.Substring(0, 16);
                if (string.IsNullOrWhiteSpace(sanitized)) sanitized = "Player";
            }

            return sanitized;
        }

        public static bool ValidateInput(PlayerInput input)
        {
            // Basic input validation - could be expanded for anti-cheat
            return input != null;
        }

        public static bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                var fullPath = Path.GetFullPath(filePath);
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();

                return AllowedFileExtensions.Contains(extension) &&
                       fullPath.StartsWith(GetSecureContentDirectory());
            }
            catch
            {
                return false;
            }
        }

        private static string GetSecureContentDirectory()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"));
        }
    }
}
