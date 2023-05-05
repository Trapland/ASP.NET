using System.Text;

namespace ASP_201MVC.Services.Transliteration
{
    public class Transliterate : ITransliterate
    {
        private readonly Dictionary<char, string> _transliterationTable = new Dictionary<char, string>()
    {
        { 'а', "a" },
        { 'б', "b" },
        { 'в', "v" },
        { 'г', "h" },
        { 'ґ', "g" },
        { 'д', "d" },
        { 'е', "e" },
        { 'є', "ie" },
        { 'ж', "zh" },
        { 'з', "z" },
        { 'и', "y" },
        { 'і', "i" },
        { 'ї', "i" },
        { 'й', "i" },
        { 'к', "k" },
        { 'л', "l" },
        { 'м', "m" },
        { 'н', "n" },
        { 'о', "o" },
        { 'п', "p" },
        { 'р', "r" },
        { 'с', "s" },
        { 'т', "t" },
        { 'у', "u" },
        { 'ф', "f" },
        { 'х', "kh" },
        { 'ц', "ts" },
        { 'ч', "ch" },
        { 'ш', "sh" },
        { 'щ', "shch" },
        { 'ь', "" },
        { 'ю', "iu" },
        { 'я', "ia" },
        { 'А', "A" },
        { 'Б', "B" },
        { 'В', "V" },
        { 'Г', "H" },
        { 'Ґ', "G" },
        { 'Д', "D" },
        { 'Е', "E" },
        { 'Є', "Ye" },
        { 'Ж', "Zh" },
        { 'З', "Z" },
        { 'И', "Y" },
        { 'І', "I" },
        { 'Ї', "Yi" },
        { 'Й', "Y" },
        { 'К', "K" },
        { 'Л', "L" },
        { 'М', "M" },
        { 'Н', "N" },
        { 'О', "O" },
        { 'П', "P" },
        { 'Р', "R" },
        { 'С', "S" },
        { 'Т', "T" },
        { 'У', "U" },
        { 'Ф', "F" },
        { 'Х', "Kh" },
        { 'Ц', "Ts" },
        { 'Ч', "Ch" },
        { 'Ш', "Sh" },
        { 'Щ', "Shch" },
        { 'Ь', "" },
        { 'Ю', "Yu" },
        { 'Я', "Ya" }
    };
        public string transliterate(string input)
        {
            StringBuilder output = new StringBuilder();

            foreach (char ch in input)
            {
                if (_transliterationTable.ContainsKey(ch))
                {
                    output.Append(_transliterationTable[ch]);
                }
                else
                {
                    output.Append(ch);
                }
            }

            return output.ToString();
        }
    }
}
