using System.Text;

namespace task01
{
    public static class StringExtensions
    {
        public static bool IsPalindrome(this string input)
        {
            string input_tolower = input.ToLower();
            StringBuilder sb = new StringBuilder();

            foreach (char c in input_tolower)
            {
                if (!(char.IsPunctuation(c) || char.IsWhiteSpace(c))) sb.Append(c); 
            }

            string clear_input = sb.ToString();

            if (clear_input.Length == 0) return false;

            for (int i = 0; i < clear_input.Length; i++)
            {
                if (clear_input[i] != clear_input[^(i + 1)]) return false;
            }
            return true;
        }
    }
}
