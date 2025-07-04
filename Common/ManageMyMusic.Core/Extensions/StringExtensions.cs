using System.Globalization;
using System.Text;

namespace ManageMyMusic.Core.Extensions
{
    public static class StringExtensions
    {

        private static readonly Dictionary<char, char> VietnameseCharMap = new Dictionary<char, char>
        {
            {'à', 'a'}, {'á', 'a'}, {'ả', 'a'}, {'ã', 'a'}, {'ạ', 'a'},
            {'ă', 'a'}, {'ằ', 'a'}, {'ắ', 'a'}, {'ẳ', 'a'}, {'ẵ', 'a'}, {'ặ', 'a'},
            {'â', 'a'}, {'ầ', 'a'}, {'ấ', 'a'}, {'ẩ', 'a'}, {'ẫ', 'a'}, {'ậ', 'a'},
            {'è', 'e'}, {'é', 'e'}, {'ẻ', 'e'}, {'ẽ', 'e'}, {'ẹ', 'e'},
            {'ê', 'e'}, {'ề', 'e'}, {'ế', 'e'}, {'ể', 'e'}, {'ễ', 'e'}, {'ệ', 'e'},
            {'ì', 'i'}, {'í', 'i'}, {'ỉ', 'i'}, {'ĩ', 'i'}, {'ị', 'i'},
            {'ò', 'o'}, {'ó', 'o'}, {'ỏ', 'o'}, {'õ', 'o'}, {'ọ', 'o'},
            {'ô', 'o'}, {'ồ', 'o'}, {'ố', 'o'}, {'ổ', 'o'}, {'ỗ', 'o'}, {'ộ', 'o'},
            {'ơ', 'o'}, {'ờ', 'o'}, {'ớ', 'o'}, {'ở', 'o'}, {'ỡ', 'o'}, {'ợ', 'o'},
            {'ù', 'u'}, {'ú', 'u'}, {'ủ', 'u'}, {'ũ', 'u'}, {'ụ', 'u'},
            {'ư', 'u'}, {'ừ', 'u'}, {'ứ', 'u'}, {'ử', 'u'}, {'ữ', 'u'}, {'ự', 'u'},
            {'ỳ', 'y'}, {'ý', 'y'}, {'ỷ', 'y'}, {'ỹ', 'y'}, {'ỵ', 'y'},
            {'À', 'A'}, {'Á', 'A'}, {'Ả', 'A'}, {'Ã', 'A'}, {'Ạ', 'A'},
            {'Ă', 'A'}, {'Ằ', 'A'}, {'Ắ', 'A'}, {'Ẳ', 'A'}, {'Ẵ', 'A'}, {'Ặ', 'A'},
            {'Â', 'A'}, {'Ầ', 'A'}, {'Ấ', 'A'}, {'Ẩ', 'A'}, {'Ẫ', 'A'}, {'Ậ', 'A'},
            {'È', 'E'}, {'É', 'E'}, {'Ẻ', 'E'}, {'Ẽ', 'E'}, {'Ẹ', 'E'},
            {'Ê', 'E'}, {'Ề', 'E'}, {'Ế', 'E'}, {'Ể', 'E'}, {'Ễ', 'E'}, {'Ệ', 'E'},
            {'Ì', 'I'}, {'Í', 'I'}, {'Ỉ', 'I'}, {'Ĩ', 'I'}, {'Ị', 'I'},
            {'Ò', 'O'}, {'Ó', 'O'}, {'Ỏ', 'O'}, {'Õ', 'O'}, {'Ọ', 'O'},
            {'Ô', 'O'}, {'Ồ', 'O'}, {'Ố', 'O'}, {'Ổ', 'O'}, {'Ỗ', 'O'}, {'Ộ', 'O'},
            {'Ơ', 'O'}, {'Ờ', 'O'}, {'Ớ', 'O'}, {'Ở', 'O'}, {'Ỡ', 'O'}, {'Ợ', 'O'},
            {'Ù', 'U'}, {'Ú', 'U'}, {'Ủ', 'U'}, {'Ũ', 'U'}, {'Ụ', 'U'},
            {'Ư', 'U'}, {'Ừ', 'U'}, {'Ứ', 'U'}, {'Ử', 'U'}, {'Ữ', 'U'}, {'Ự', 'U'},
            {'Ỳ', 'Y'}, {'Ý', 'Y'}, {'Ỷ', 'Y'}, {'Ỹ', 'Y'}, {'Ỵ', 'Y'},
            {'đ', 'd'}, {'Đ', 'D'}
        };

        public static string RemoveDiacriticsVietnamese(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Normalize the string to decompose characters (e.g., 'ạ' -> 'a' + combining mark)
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder(text.Length);

            foreach (char c in normalized)
            {
                // Skip non-spacing marks (diacritics)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    // Replace Vietnamese characters if they exist in the map, otherwise keep the character
                    result.Append(VietnameseCharMap.TryGetValue(c, out char value) ? value : c);
                }
            }

            // Recompose the string to ensure proper formatting
            return result.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
