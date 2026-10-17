using System.Text.RegularExpressions;

namespace Charity.Application.Services.Implementations;

public static class AddressNormalizationService
{
    private static readonly Regex DiacriticsRegex = new(@"[\u064B-\u0652\u0656-\u065F\u0670]", RegexOptions.Compiled);
    private static readonly Regex TatweelRegex = new(@"\u0640", RegexOptions.Compiled);
    private static readonly Regex MultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new(@"[,\.\-_/\\#\(\)\[\]\{\}\:\*\!\?\""'«»]", RegexOptions.Compiled);

    /// <summary>
    /// يقوم بتوحيد وتنقية الحروف العربية والإنجليزية للبحث والمقارنة المتطابقة
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var text = input.Trim().ToLowerInvariant();

        // 1. إزالة التشكيل والحركات العربية
        text = DiacriticsRegex.Replace(text, "");

        // 2. إزالة الكشيدة والتطويل
        text = TatweelRegex.Replace(text, "");

        // 3. توحيد الألفات (أ، إ، آ -> ا)
        text = text.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا');

        // 4. توحيد الألف المقصورة والياء (ى -> ي)
        text = text.Replace('ى', 'ي');

        // 5. توحيد التاء المربوطة والهاء (ة -> ه)
        text = text.Replace('ة', 'ه');

        // 6. استبدال علامات الترقيم بمسافات
        text = PunctuationRegex.Replace(text, " ");

        // 7. دمج وتوحيد المسافات
        text = MultipleSpacesRegex.Replace(text, " ").Trim();

        return text;
    }

    /// <summary>
    /// تجريد ال التعريف من الكلمات لمطابقة متقدمة (مثال: الجزيرة -> جزيرة)
    /// </summary>
    public static string StripDefiniteArticle(string normalizedWord)
    {
        if (normalizedWord.StartsWith("ال") && normalizedWord.Length > 3)
        {
            return normalizedWord.Substring(2);
        }
        return normalizedWord;
    }

    /// <summary>
    /// حساب وزن التشابه بين النص المدخل وعنوان مقترح (من 0 إلى 100)
    /// </summary>
    public static int CalculateMatchScore(string candidateText, string candidateNormalized, string queryNormalized, int usageCount = 1)
    {
        if (string.IsNullOrWhiteSpace(candidateNormalized) || string.IsNullOrWhiteSpace(queryNormalized))
            return 0;

        int score = 0;

        // 1. تطابق تام
        if (candidateNormalized == queryNormalized)
        {
            score = 100;
        }
        // 2. يبدأ بالاستعلام
        else if (candidateNormalized.StartsWith(queryNormalized, StringComparison.OrdinalIgnoreCase))
        {
            score = 85;
        }
        // 3. يحتوي على الاستعلام كنص فرعي
        else if (candidateNormalized.Contains(queryNormalized, StringComparison.OrdinalIgnoreCase))
        {
            score = 70;
        }
        else
        {
            // 4. فحص تطابق الكلمات مع ال التعريف وتسامح الحروف
            var queryTokens = queryNormalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var candidateTokens = candidateNormalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int matchedTokens = 0;
            foreach (var qToken in queryTokens)
            {
                var strippedQ = StripDefiniteArticle(qToken);
                bool tokenMatched = false;

                foreach (var cToken in candidateTokens)
                {
                    var strippedC = StripDefiniteArticle(cToken);

                    if (strippedC == strippedQ || cToken == qToken)
                    {
                        tokenMatched = true;
                        break;
                    }

                    if (strippedC.StartsWith(strippedQ) && strippedQ.Length >= 2)
                    {
                        tokenMatched = true;
                        break;
                    }

                    // حساب المسافة القريبة للأخطاء الإملائية
                    int distance = LevenshteinDistance(strippedC, strippedQ);
                    int maxLen = Math.Max(strippedC.Length, strippedQ.Length);
                    if (maxLen > 3 && distance <= 1)
                    {
                        tokenMatched = true;
                        break;
                    }
                    if (maxLen > 6 && distance <= 2)
                    {
                        tokenMatched = true;
                        break;
                    }
                }

                if (tokenMatched)
                {
                    matchedTokens++;
                }
            }

            if (queryTokens.Length > 0 && matchedTokens > 0)
            {
                double tokenRatio = (double)matchedTokens / queryTokens.Length;
                score = (int)(tokenRatio * 60);
            }
        }

        // إضافة وزن إضافي للعناوين الأكثر استخداماً (Usage Boost) لتظهر في المقدمة
        if (score > 0)
        {
            int usageBonus = Math.Min(15, usageCount * 2);
            score = Math.Min(100, score + usageBonus);
        }

        return score;
    }

    /// <summary>
    /// حساب مسافة التعديل (Levenshtein Distance) لاكتشاف التقارب الإملائي
    /// </summary>
    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}
