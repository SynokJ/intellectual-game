namespace IntellectualGame.Core
{
    /// <summary>Picks the right Russian noun form for a number: 1 гость, 2 гостя, 5 гостей.</summary>
    public static class Plural
    {
        public static string Pick(int n, string one, string few, string many)
        {
            int lastTwo = n % 100, last = n % 10;
            if (lastTwo >= 11 && lastTwo <= 14) return many;
            if (last == 1) return one;
            if (last >= 2 && last <= 4) return few;
            return many;
        }
    }
}
