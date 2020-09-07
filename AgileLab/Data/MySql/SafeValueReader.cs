using System;
using System.Data;

namespace AgileLab.Data.MySql
{
    static class SafeValueReader
    {
        public static string GetStringSafe(IDataReader reader, int colIndex)
        {
            return GetStringSafe(reader, colIndex, string.Empty);
        }

        public static string GetStringSafe(IDataReader reader, int colIndex, string defaultValue = "")
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return defaultValue;
        }

        public static string GetStringSafe(IDataReader reader, string indexName, string defaultValue = "")
        {
            return GetStringSafe(reader, reader.GetOrdinal(indexName));
        }

        public static int? GetNullableIntSafe(IDataReader reader, int colIndex, int? defaultValue = null)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToInt32(reader[colIndex]);
            else
                return defaultValue;
        }

        public static int? GetNullableIntSafe(IDataReader reader, string indexName)
        {
            return GetNullableIntSafe(reader, reader.GetOrdinal(indexName));
        }

        public static uint GetUIntSafe(IDataReader reader, int colIndex, uint defaultValue = default(uint))
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToUInt32(reader[colIndex]);
            else
                return defaultValue;
        }

        public static uint GetUIntSafe(IDataReader reader, string indexName, uint defaultValue = default(uint))
        {
            return GetUIntSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }

        public static int GetIntSafe(IDataReader reader, int colIndex, int defaultValue = default(int))
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToInt32(reader[colIndex]);
            else
                return defaultValue;
        }

        public static int GetIntSafe(IDataReader reader, string indexName, int defaultValue = default(int))
        {
            return GetIntSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }
    }
}
