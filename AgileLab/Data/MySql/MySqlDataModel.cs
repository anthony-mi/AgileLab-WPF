using AgileLab.Services.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace AgileLab.Data.MySql
{
    class MySqlDataModel
    {
        protected static ILogger _logger = null/*ComponentsContainer.Get<ILogger>()*/; // commented due to cross reference

        private static readonly string DISTINCT_KEY_WORD = "DISTINCT";

        public MySqlDataModel()
        {
            _logger = new FileLogger();
        }

        #region "Select Commands"
        protected MySqlCommand CreateSelectCommand(
            string columnName,
            string tableName,
            string whereConditionColumnName,
            string conditionOperator,
            object whereConditionValue,
            bool distinct = false)
        {
            string query = string.Empty;

            if (whereConditionValue is string)
            {
                query = string.Format("SELECT " + (distinct ? DISTINCT_KEY_WORD + " " : string.Empty) + "{0} FROM {1} WHERE {2} {3} '{4}';",
                    columnName, tableName, whereConditionColumnName, conditionOperator, ConvertToSafeString(whereConditionValue as string));
            }
            else
            {
                query = $"SELECT " + (distinct ? DISTINCT_KEY_WORD + " " : string.Empty) + 
                    $"{columnName} FROM {tableName} WHERE {whereConditionColumnName} {conditionOperator} {ConvertToSafeString(whereConditionValue)};";
            }

            return new MySqlCommand(query);
        }

        protected MySqlCommand CreateSelectAllCommand(
            string tableName,
            string whereConditionColumnName,
            string conditionOperator,
            object whereConditionValue,
            bool distinct = false)
        {
            return CreateSelectCommand("*", tableName, whereConditionColumnName, conditionOperator, whereConditionValue, distinct);
        }

        protected MySqlCommand CreateSelectAllCommand(
            string tableName,
            bool distinct = false)
        {
            string query = string.Format("SELECT " + (distinct ? DISTINCT_KEY_WORD + " " : string.Empty) + $" * FROM {tableName}");
            return new MySqlCommand(query);
        }

        protected MySqlCommand CreateSelectCommandWithJoin(
            IList<string> columnNames,
            string fromTableName,
            string joinTableName,
            string onColumnName1,
            string onColumnName2,
            JoinType joinType,
            bool distinct = false)
        {
            string join = ConvertToString(joinType);

            string query =
                "SELECT " + 
                (distinct ? DISTINCT_KEY_WORD + " " : string.Empty) + 
                string.Join(",", columnNames) + " " +
                "FROM " + fromTableName + " " +
                join + " JOIN " + joinTableName + " " +
                "ON " + onColumnName1 + " = " + onColumnName2;

            return new MySqlCommand(query);
        }

        protected MySqlCommand CreateSelectCommandWithJoin(
            IList<string> columnNames,
            string fromTableName,
            string joinTableName,
            string onColumnName1,
            string onColumnName2,
            string whereConditionColumnName,
            string conditionOperator,
            object whereConditionValue,
            JoinType joinType,
            bool distinct = false)
        {
            MySqlCommand command = CreateSelectCommandWithJoin(
                columnNames, 
                fromTableName,
                joinTableName,
                onColumnName1,
                onColumnName2,
                joinType,
                distinct);

            string query = command.CommandText;

            if (whereConditionValue is string)
            {
                query += $" WHERE {whereConditionColumnName} {conditionOperator} '{ConvertToSafeString(whereConditionValue as string)}'";
            }
            else
            {
                query += $" WHERE {whereConditionColumnName} {conditionOperator} {ConvertToSafeString(whereConditionValue)}";
            }

            return new MySqlCommand(query);
        }
        #endregion

        #region "Delete Commands"
        protected MySqlCommand CreateDeleteCommand(
            string fromTableName,
           string whereConditionColumnName,
            string conditionOperator,
            object whereConditionValue)
        {
            string query = string.Empty;

            if (whereConditionValue is string)
            {
                query = $"DELETE FROM {fromTableName} WHERE {whereConditionColumnName} {conditionOperator} '{ConvertToSafeString(whereConditionValue as string)}';";
            }
            else
            {
                query = $"DELETE FROM {fromTableName} WHERE {whereConditionColumnName} {conditionOperator} {ConvertToSafeString(whereConditionValue)};";
            }

            
            return new MySqlCommand(query);
        }
        #endregion

        #region Converters
        private string ConvertToString(JoinType joinType)
        {
            string str = string.Empty;

            switch (joinType)
            {
                case JoinType.FullOuter:
                    str = "FULL OUTER";
                    break;

                case JoinType.Inner:
                    str = "INNER";
                    break;

                case JoinType.Left:
                    str = "LEFT";
                    break;

                case JoinType.Right:
                    str = "RIGHT";
                    break;
            }

            return str;
        }

        private string ConvertToSafeString(string str)
        {
            return str.Replace("'", "''");
        }

        private string ConvertToSafeString(object obj)
        {
            return ConvertToSafeString(obj.ToString());
        }
        #endregion

        #region Connection
        protected MySqlConnection OpenNewConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["mySqlDbConnection"].ConnectionString;

            MySqlConnection newConnection = null;

            try
            {
                newConnection = new MySqlConnection(connectionString);
                newConnection.Open();
            }
            catch (Exception)
            {
                throw;
            }

            return newConnection;
        }
        #endregion

        protected bool ValueExists(
            string columnName,
            string tableName,
            object value,
            MySqlConnection connection)
        {
            bool exists = true;

            try
            {
                MySqlCommand command = CreateSelectCommand(columnName, tableName, columnName, "=", value);
                command.Connection = connection;

                MySqlDataReader reader = command.ExecuteReader();

                exists = reader.Read();

                reader.Close();

            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex);
                exists = false;
            }

            return exists;
        }
    }
}
