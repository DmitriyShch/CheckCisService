using System.Text;

namespace CheckCisService.Helpers;

/// <summary>
/// Вспомогательный класс для работы с КИЗ, SGTIN и крипто-блоками.
/// </summary>
public class MdlpCashRegHelper(ILogger<MdlpCashRegHelper> logger)
{
    /// <summary>
    /// Префикс GTIN.
    /// </summary>
    public const string GTIN_PREFIX = "01";
    /// <summary>
    /// Размер GTIN.
    /// </summary>
    public const int GTIN_SIZE = 14;
    /// <summary>
    /// Префикс SGTIN.
    /// </summary>
    public const string SGTIN_PREFIX = "21";
    /// <summary>
    /// Размер SGTIN.
    /// </summary>
    public const int SGTIN_SIZE = 13;
    /// <summary>
    /// Префикс типа крипто-блока.
    /// </summary>
    public const string CRYPTO_TYPE_PREFIX = "91";
    /// <summary>
    /// Размер типа крипто-блока.
    /// </summary>
    public const int CRYPTO_TYPE_SIZE = 4;
    /// <summary>
    /// Префикс крипто-блока.
    /// </summary>
    public const string CRYPTO_BLOCK_PREFIX = "92";
    /// <summary>
    /// Размер крипто-блока.
    /// </summary>
    public const int CRYPTO_BLOCK_SIZE = 44;
    /// <summary>
    /// Префикс крипто-блока ГИСМТ.
    /// </summary>
    public const string CRYPTO_GISMT_PREFIX = "93";
    /// <summary>
    /// Размер крипто-блока ГИСМТ.
    /// </summary>
    public const int CRYPTO_GISMT_SIZE = 4;
    /// <summary>
    /// Символ-разделитель группы.
    /// </summary>
    public const char GROUP_SEPARATOR = '\x1d';
    /// <summary>
    /// Минимальная длина КИЗ.
    /// </summary>
    public const int MIN_CIS_LENGTH = 31;
    private const int CHAR_A_UP = 65;
    private const int CHAR_Z_UP = 90;
    private const int CHAR_A_LOW = 97;
    private const int CHAR_Z_LOW = 122;
    /// <summary>
    /// Словарь для исправления символов при ошибочной кодировке.
    /// </summary>
    public static readonly Lazy<Dictionary<char, char>> FixDict = new(CreateFixDict());

    /// <summary>
    /// Исправляет полный SGTIN, возвращая корректную строку.
    /// </summary>
    public string FixFullSgtin(string initialSgtin)
    {
        logger.LogDebug("Begin FixFullSgtin initialSgtin: {initialSgtin}", initialSgtin);
        
        string fixedSgtin;
        string restString = initialSgtin;

        CheckPrefix(GTIN_PREFIX, "КМ", true, ref restString);
        ExtractToken(GTIN_SIZE, "GTIN", ref restString, out string gtin);

        CheckPrefix(SGTIN_PREFIX, "КиЗ", true, ref restString);
        ExtractToken(SGTIN_SIZE, "SGTIN", ref restString, out string sgtin);

        if (CheckPrefix(CRYPTO_TYPE_PREFIX, "КриптоТип", false, ref restString))
        {
            ExtractToken(CRYPTO_TYPE_SIZE, "CRYPTO_TYPE", ref restString, out string cryptoType);

            CheckPrefix(CRYPTO_BLOCK_PREFIX, "КриптоБлок", true, ref restString);
            ExtractToken(CRYPTO_BLOCK_SIZE, "CRYPTO_BLOCK", ref restString, out string cryptoBlock);

            fixedSgtin = $"{GTIN_PREFIX}{gtin}" +
                $"{SGTIN_PREFIX}{sgtin}{GROUP_SEPARATOR}" +
                $"{CRYPTO_TYPE_PREFIX}{cryptoType}{GROUP_SEPARATOR}" +
                $"{CRYPTO_BLOCK_PREFIX}{cryptoBlock}";
        }
        else
        {
            CheckPrefix(CRYPTO_GISMT_PREFIX, "КриптоГИСМТ", true, ref restString);
            ExtractToken(CRYPTO_GISMT_SIZE, "CRYPTO_GISMT", ref restString, out string cryptoGismt);

            fixedSgtin = $"{GTIN_PREFIX}{gtin}" +
                $"{SGTIN_PREFIX}{sgtin}{GROUP_SEPARATOR}" +
                $"{CRYPTO_GISMT_PREFIX}{cryptoGismt}";
        }

        logger.LogDebug("End FixFullSgtin with success fixedSgtin: {fixedSgtin}", fixedSgtin);

        return fixedSgtin;
    }

    /// <summary>
    /// Проверяет, может ли код быть КИЗ.
    /// </summary>
    public bool IsCodeCanBeCis(string code) =>
        code.Length >= MIN_CIS_LENGTH &&
        code.StartsWith(GTIN_PREFIX);

    /// <summary>
    /// Проверяет наличие префикса в начале строки.
    /// </summary>
    public bool CheckPrefix(string prefix, string prefixName, bool showError, ref string text)
    {
        logger.LogDebug("Begin CheckPrefix prefix: {prefix},  prefixName: {prefixName}, " +
            "text: {text}.", prefix, prefixName, text);
        if (!text.StartsWith(prefix))
        {
            if (showError)
            {
                logger.LogError("Некорректный префикс {prefixName}: {text}. " +
                    "Ожидается: {prefix}.", prefixName, text, prefix);
                throw new MdlpHelperException($"Некорректный префикс " +
                    $"{prefixName}: {text}. Ожидается: {prefix}.");
            }
            return false;
        }

        text = text[prefix.Length..].Trim();
        logger.LogDebug("End CheckPrefix with success text: {text}", text);
        return true;
    }

    /// <summary>
    /// Извлекает токен заданной длины из строки.
    /// </summary>
    public bool ExtractToken(int tokenSize, string tokenName,
        ref string text, out string tokenText)
    {
        logger.LogDebug("Begin ExtractToken tokenSize: {tokenSize}, " +
            " tokenName: {tokenName}, text: {text}.", tokenSize, tokenName, text);
        tokenText = string.Empty;
        if (text.Length < tokenSize)
        {
            logger.LogError("Некорректная длина {tokenName}: {text}. " +
                "Ожидается: {tokenSize}, фактически: {text.Length}.",
                tokenName, text, tokenSize, text.Length);
            throw new MdlpHelperException($"Некорректная длина {tokenName}: {text}. " +
                $"Ожидается: {tokenSize}, фактически: {text.Length}.");
        }

        tokenText = text[..tokenSize];
        text = text[tokenSize..].Trim();
        logger.LogDebug("End ExtractToken with success tokenText: " +
            "{tokenText}, text: {text}", tokenText, text);
        return true;
    }

    /// <summary>
    /// Получает КИЗ из полного кода.
    /// </summary>
    public string GetCisFromCode(string fullCode)
    {
        logger.LogDebug("Begin FixSgtinFromCode fullCode: {fullCode}", fullCode);

        string restString = fullCode;

        CheckPrefix(GTIN_PREFIX, "КМ", true, ref restString);
        ExtractToken(GTIN_SIZE, "GTIN", ref restString, out string gtin);

        CheckPrefix(SGTIN_PREFIX, "КиЗ", true, ref restString);
        ExtractToken(SGTIN_SIZE, "SGTIN", ref restString, out string sgtin);
        
        return $"{GTIN_PREFIX}{gtin}{SGTIN_PREFIX}{sgtin}";
    }

    /// <summary>
    /// Автоматически исправляет ошибочную кодировку в строке.
    /// </summary>
    public string AutoCorrectWrongCodepage(string source)
    {
        if (!NeedToFixWrongCodepage(source))
            return source;
        logger.LogInformation("AutoCorrectWrongCodepage code: '{source}', нужна перекодировка",
            source);
        string res = "";
        foreach (char c in source)
        {
            res += FixDict.Value.TryGetValue(c, out char value) ? value : c;
        }
        logger.LogInformation("AutoCorrectWrongCodepage result: '{res}'", res);
        return res;
    }

    /// <summary>
    /// Проверяет, требуется ли исправление кодировки.
    /// </summary>
    public bool NeedToFixWrongCodepage(string source) =>
        !Encoding.ASCII
            .GetBytes(source)
            .Where(b => b is (>= CHAR_A_UP and <= CHAR_Z_UP) or
                        (>= CHAR_A_LOW and <= CHAR_Z_LOW))
            .Any();

    /// <summary>
    /// Создаёт словарь для исправления символов.
    /// </summary>
    private static Dictionary<char, char> CreateFixDict() =>
        new()
        {
                { 'Й', 'Q' },
                { 'Ц', 'W' },
                { 'У', 'E' },
                { 'К', 'R' },
                { 'Е', 'T' },
                { 'Н', 'Y' },
                { 'Г', 'U' },
                { 'Ш', 'I' },
                { 'Щ', 'O' },
                { 'З', 'P' },
                { 'Ф', 'A' },
                { 'Ы', 'S' },
                { 'В', 'D' },
                { 'А', 'F' },
                { 'П', 'G' },
                { 'Р', 'H' },
                { 'О', 'J' },
                { 'Л', 'K' },
                { 'Д', 'L' },
                { 'Я', 'Z' },
                { 'Ч', 'X' },
                { 'С', 'C' },
                { 'М', 'V' },
                { 'И', 'B' },
                { 'Т', 'N' },
                { 'Ь', 'M' },
                { 'й', 'q' },
                { 'ц', 'w' },
                { 'у', 'e' },
                { 'к', 'r' },
                { 'е', 't' },
                { 'н', 'y' },
                { 'г', 'u' },
                { 'ш', 'i' },
                { 'щ', 'o' },
                { 'з', 'p' },
                { 'ф', 'a' },
                { 'ы', 's' },
                { 'в', 'd' },
                { 'а', 'f' },
                { 'п', 'g' },
                { 'р', 'h' },
                { 'о', 'j' },
                { 'л', 'k' },
                { 'д', 'l' },
                { 'я', 'z' },
                { 'ч', 'x' },
                { 'с', 'c' },
                { 'м', 'v' },
                { 'и', 'b' },
                { 'т', 'n' },
                { 'ь', 'm' },
                { '?', '&' },
                { 'Б', '<' },
                { 'Ю', '>' },
                { ',', '?' },
                { 'б', ',' },
                { 'ю', '.' },
                { '.', '/' },
                { 'ж', ';' },
                { 'э', '\'' },
                { 'Ж', ':' },
                { 'Э', '"' },
        };
}

/// <summary>
/// Исключение, возникающее при ошибках в MdlpCashRegHelper.
/// </summary>
public class MdlpHelperException(string message) : Exception(message);
