using System.IO;

namespace Tune.Helpers
{
    internal class PathConverters
    {
        public static string GetLrcFilePath(string audioFilePath)
        {
            if (string.IsNullOrWhiteSpace(audioFilePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(audioFilePath));

            // 获取目录部分

            string? directory = Path.GetDirectoryName(audioFilePath);
            
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(audioFilePath);

            // 如果 directory 为 null 或空字符串，说明传入的是纯文件名（不含路径）
            // 此时按当前目录处理（也可以选择抛出异常，取决于你的需求）
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory; // 或 AppDomain.CurrentDomain.BaseDirectory
            }

            string lrcFilePath = Path.Combine(directory, fileNameWithoutExt + ".lrc");

            // 可选：检查文件是否存在
            if (!File.Exists(lrcFilePath))
            {
                var existing = Directory.GetFiles(directory, fileNameWithoutExt + ".*")
                                        .FirstOrDefault(f => Path.GetExtension(f).Equals(".lrc", StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                    return existing;
                else
                    return String.Empty; // 或者抛出异常
            }

            return lrcFilePath;
        }
    }
}
