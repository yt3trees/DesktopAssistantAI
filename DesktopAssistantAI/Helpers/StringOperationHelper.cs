using System.Text;
namespace DesktopAssistantAI.Helpers;

public class StringOperationHelper
{
    public static string FormatDateTime(int targetDatetime)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)targetDatetime).UtcDateTime;
        var localDateTime = dateTime.ToLocalTime();
        return localDateTime.ToString("yyyy/MM/dd HH:mm");
    }

    public static string FormatDateTime(int targetDatetime, string format)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)targetDatetime).UtcDateTime;
        var localDateTime = dateTime.ToLocalTime();
        return localDateTime.ToString(format);
    }

    public static string TruncateString(string input, int maxBytes)
    {
        input = input.Replace("\r\n", "").Replace("\n", "");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding enc = Encoding.GetEncoding("Shift_JIS");
        byte[] bytes = enc.GetBytes(input);

        if (bytes.Length <= maxBytes)
        {
            return input;
        }

        int byteCount = 0;
        int charCount = 0;
        while (byteCount <= maxBytes && charCount < input.Length)
        {
            char c = input[charCount];
            byteCount += enc.GetByteCount(new char[] { c });
            if (byteCount > maxBytes) break;
            charCount++;
        }

        return input.Substring(0, charCount) + "...";
    }
}
