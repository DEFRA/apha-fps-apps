using System.Reflection;

namespace Apha.FPSApps.Web.Enums
{
    public enum MasterLookupTable
    {
        [ValueMaxLength(15)]
        Directorate,

        [ValueMaxLength(50)]
        Disease,

        [ValueMaxLength(50)]
        Customer
    }

    /// <summary>
    /// Declares the UI-only maximum value length for a <see cref="MasterLookupTable"/>
    /// member, matching the underlying DB column size.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ValueMaxLengthAttribute(int maxLength) : Attribute
    {
        public int MaxLength { get; } = maxLength;
    }

    public static class MasterLookupTableExtensions
    {
        /// <summary>Default max length used when a table is not recognised.</summary>
        public const int DefaultValueMaxLength = 50;

        /// <summary>
        /// Resolves the UI max length for the given table name (case-insensitive),
        /// falling back to <see cref="DefaultValueMaxLength"/> for unknown tables.
        /// </summary>
        public static int GetValueMaxLength(string tableName)
        {
            return Enum.TryParse<MasterLookupTable>(tableName?.Trim(), ignoreCase: true, out var table)
                ? table.GetValueMaxLength()
                : DefaultValueMaxLength;
        }

        /// <summary>
        /// Reads the <see cref="ValueMaxLengthAttribute"/> for the enum member.
        /// </summary>
        public static int GetValueMaxLength(this MasterLookupTable table)
        {
            var attribute = typeof(MasterLookupTable)
                .GetField(table.ToString())?
                .GetCustomAttribute<ValueMaxLengthAttribute>();

            return attribute?.MaxLength ?? DefaultValueMaxLength;
        }
    }
}
