using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace CheckCisService.Helpers
{
    /// <summary>
    /// Общие расширения для работы со строками, числами и объектами.
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Преобразует объект в JSON-строку.
        /// </summary>
        /// <param name="obj">Объект для сериализации.</param>
        /// <param name="formatting">Форматирование JSON.</param>
        /// <returns>JSON-строка или пустая строка, если объект null.</returns>
        public static string Json(this object? obj,
            Formatting formatting = Formatting.Indented) => obj == null ? "" :
             JsonConvert.SerializeObject(obj, formatting);

        /// <summary>
        /// Проверяет, что строка пуста или равна null.
        /// </summary>
        /// <param name="text">Проверяемая строка.</param>
        /// <returns>True, если строка пуста или null.</returns>
        public static bool IsEmpty([NotNullWhen(false)] this string? text) =>
             string.IsNullOrEmpty(text);

        /// <summary>
        /// Проверяет, что строка не пуста и не равна null.
        /// </summary>
        /// <param name="text">Проверяемая строка.</param>
        /// <returns>True, если строка не пуста и не null.</returns>
        public static bool IsNotEmpty([NotNullWhen(true)] this string? text) =>
             !string.IsNullOrEmpty(text);

        /// <summary>
        /// Возвращает левые символы строки длиной не более length.
        /// </summary>
        /// <param name="text">Исходная строка.</param>
        /// <param name="length">Максимальная длина результата.</param>
        /// <returns>Левая часть строки или null.</returns>
        public static string? Left(this string? text, int length) =>
            text?[..Math.Min(text.Length, length)];

        /// <summary>
        /// Преобразует double в int.
        /// </summary>
        /// <param name="value">Значение double.</param>
        /// <returns>Значение int.</returns>
        public static int ToInt(this double value) => Convert.ToInt32(value);

        /// <summary>
        /// Преобразует double? в int?.
        /// </summary>
        /// <param name="value">Nullable double.</param>
        /// <returns>Nullable int.</returns>
        public static int? ToInt(this double? value) =>
            value == null ? null : Convert.ToInt32(value);

        /// <summary>
        /// Преобразует double в long.
        /// </summary>
        /// <param name="value">Значение double.</param>
        /// <returns>Значение long.</returns>
        public static long ToLong(this double value) => Convert.ToInt64(value);

        /// <summary>
        /// Преобразует double? в long?.
        /// </summary>
        /// <param name="value">Nullable double.</param>
        /// <returns>Nullable long.</returns>
        public static long? ToLong(this double? value) =>
            value == null ? null : Convert.ToInt64(value);

        /// <summary>
        /// Получает значение свойства объекта по имени.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>Значение свойства в виде строки или null.</returns>
        public static string? GetPropValue(this object? obj, string propertyName) =>
            obj?.GetType().GetProperties()
            .FirstOrDefault(p => p.Name == propertyName)
            ?.GetValue(obj)
            ?.ToString();
    }
}
