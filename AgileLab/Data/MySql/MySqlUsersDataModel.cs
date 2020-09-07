using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;
using System;

namespace AgileLab.Data.MySql
{
    class MySqlUsersDataModel : MySqlDataModel, IUsersDataModel
    {
        public void CreateNewUser(string firstName, string lastName, string username, string passwordHash)
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = "INSERT INTO `users` " +
                                            "(FirstName, LastName, UserName, PasswordHash) " +
                                            "VALUES (@FirstName, @LastName, @UserName, @PasswordHash)";

                    command.Parameters.Add(new MySqlParameter("@FirstName", firstName));
                    command.Parameters.Add(new MySqlParameter("@LastName", lastName));
                    command.Parameters.Add(new MySqlParameter("@UserName", username));
                    command.Parameters.Add(new MySqlParameter("@PasswordHash", passwordHash));

                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetPasswordHashByUsername(string username)
        {
            string hash = string.Empty;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    string columnName = "PasswordHash";

                    MySqlCommand selectCommand = CreateSelectCommand(columnName, "users", "UserName", "=", username);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            hash = SafeValueReader.GetStringSafe(reader, columnName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                    }
                }
            }
            catch
            {
                throw;
            }

            return hash;
        }

        public User GetUser(string username)
        {
            User user = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand("users", "UserName", "=", username);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            uint id = Convert.ToUInt32(reader["Id"]);
                            string firstName = SafeValueReader.GetStringSafe(reader, "FirstName");
                            string lastName = SafeValueReader.GetStringSafe(reader, "LastName");

                            user = new User(id, firstName, lastName, username);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                    }
                }
            }
            catch
            {
                throw;
            }

            return user;
        }

        public User GetUser(uint id)
        {
            User user = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand("users", "Id", "=", id);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            string firstName = SafeValueReader.GetStringSafe(reader, "FirstName");
                            string lastName = SafeValueReader.GetStringSafe(reader, "LastName");
                            string username = SafeValueReader.GetStringSafe(reader, "UserName");

                            user = new User(id, firstName, lastName, username);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger?.Error(ex);
                throw;
            }

            return user;
        }

        public bool UserExists(string username)
        {
            bool exists = false;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectCommand("Id", "users", "UserName", "=", username);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            exists = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                    }
                }
            }
            catch
            {
                throw;
            }

            return exists;
        }
    }
}
