namespace Tune.Helpers
{
    internal class StringUtils
    {
        public static string RemoveEmptyLines(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 1. 按所有常见换行符拆分
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // 2. 过滤掉 null、空字符串 或 只包含空白字符的行
            var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            // 3. 使用换行符重新拼接
            return string.Join("\n", nonEmptyLines);
        }

        /// <summary>
        /// 给无歌词内容的 LRC 时间轴填充空格
        /// 例如:
        /// [00:00.00]
        /// 转换为:
        /// [00:00.00] 
        /// </summary>
        public static string FillEmptyLyricText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // 找到时间标签结束位置
                int index = line.IndexOf(']');

                if (index >= 0)
                {
                    // ] 后没有任何内容
                    if (index == line.Length - 1)
                    {
                        lines[i] = line + "‎";  // This is a U+200E Left-to-Right Mark (invisible character) to ensure the line is not empty
                    }
                }
            }

            return string.Join("\n", lines);
        }
    }
}