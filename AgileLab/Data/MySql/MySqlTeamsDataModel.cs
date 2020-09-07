using System;
using System.Collections.Generic;
using System.Windows.Threading;
using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;

namespace AgileLab.Data.MySql
{
    class MySqlTeamsDataModel : MySqlDataModel, ITeamsDataModel
    {
        public event EventHandler<Team> NewTeamCreated;
        public event TeamMembersUpdatedEventHandler UserJoinedTeam;
        public event TeamMembersUpdatedEventHandler UserLeavedTeam;

        public Team CreateNewTeam(string newTeamName, User creator)
        {
            try
            {
                Team newTeam = CreateTeam(newTeamName);
                AddTeamMember(newTeam, creator);
                return newTeam;
            }
            catch
            {
                throw;
            }
        }

        public void AddTeamMember(Team team, User newMember)
        {
            try
            {
                if(IsTeamMember(team, newMember))
                {
                    throw new Exception($"User '{newMember.UserName}' is already a member of the team '{team.Name}'.");
                }

                // Add user to the team.
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand insertCommand = new MySqlCommand();

                    insertCommand.Connection = connection;
                    insertCommand.CommandText = "INSERT INTO `teammembers` " +
                                                "(TeamId, UserId) " +
                                                "VALUES (@teamId, @userId)";

                    insertCommand.Parameters.Add(new MySqlParameter("@teamId", team.Id));
                    insertCommand.Parameters.Add(new MySqlParameter("@userId", newMember.Id));

                    insertCommand.ExecuteNonQuery();

                    UserJoinedTeam?.Invoke(team, newMember);
                }
            }
            catch
            {
                throw;
            }
        }

        public bool IsTeamMember(Team team, User user)
        {
            using (MySqlConnection connection = OpenNewConnection())
            {
                MySqlCommand selectCommand = new MySqlCommand();

                selectCommand.Connection = connection;
                selectCommand.CommandText = $"SELECT * FROM `teammembers` WHERE `TeamId` = @teamId AND `UserId` = @userId";

                selectCommand.Parameters.Add(new MySqlParameter("@teamId", team.Id));
                selectCommand.Parameters.Add(new MySqlParameter("@userId", user.Id));

                selectCommand.ExecuteNonQuery();

                MySqlDataReader reader = selectCommand.ExecuteReader();

                return reader.Read();
            }
        }

        private Team CreateTeam(string newTeamName)
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = "INSERT INTO `teams` (Name) VALUES (@Name)";

                    command.Parameters.Add(new MySqlParameter("@Name", newTeamName));

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);

                    Team newTeam = new Team(id, newTeamName);

                    NewTeamCreated?.Invoke(this, newTeam);

                    return newTeam;
                }
            }
            catch
            {
                throw;
            }
        }

        public IList<Team> GetTeamsOfUser(uint userId)
        {
            List<Team> teams = new List<Team>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectCommandWithJoin(
                        new List<string> { "teams.Id", "teams.Name" }, "teams", "teammembers", "teams.Id", "teammembers.TeamId", "teammembers.UserId", "=", userId, JoinType.Inner);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            uint id = Convert.ToUInt32(reader["Id"]);
                            string name = SafeValueReader.GetStringSafe(reader, "Name");

                            teams.Add(new Team(id, name));
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

            }

            return teams;
        }

        public bool TeamExists(string name)
        {
            bool exists = false;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    exists = ValueExists("Name", "teams", name, connection);
                }
            }
            catch
            {
                throw;
            }

            return exists;
        }

        public IList<Team> GetAllTeams()
        {
            List<Team> teams = new List<Team>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand("teams");
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            uint id = Convert.ToUInt32(reader["Id"]);
                            string name = SafeValueReader.GetStringSafe(reader, "Name");

                            teams.Add(new Team(id, name));
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
                _logger?.Fatal($"{GetType().Name}.GetAllTeams() error.");
                _logger?.Fatal(ex);
            }

            return teams;
        }

        public IList<ExtendedTeam> GetAllExtendedTeams()
        {
            List<ExtendedTeam> extendedTeams = new List<ExtendedTeam>();

            try
            {
                List<Team> teams = (List<Team>)GetAllTeams();

                foreach (Team team in teams)
                {
                    extendedTeams.Add(GetExtendedTeam(team));  
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
            }

            return extendedTeams;
        }

        public ExtendedTeam GetExtendedTeam(Team team)
        {
            string countOfMembersAlias = "countOfMembers";
            string countOfProjectsAlias = "countOfProjects";

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();

                    command.CommandText = "SELECT" +
                        $"(SELECT COUNT(UserId) FROM `teammembers` WHERE TeamId = @teamId) as {countOfMembersAlias}," +
                        $"(SELECT COUNT(Id) FROM `projects` WHERE DevelopmentTeamId = @teamId) as {countOfProjectsAlias}";
                    command.Parameters.Add(new MySqlParameter("@teamId", team.Id));

                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        uint countOfMembers = Convert.ToUInt32(reader[countOfMembersAlias]);
                        uint countOfProjects = Convert.ToUInt32(reader[countOfProjectsAlias]);

                        return new ExtendedTeam(team, countOfMembers, countOfProjects);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger?.Error(ex);
            }

            return null;
        }

        public void RemoveUserFromTeam(User user, Team team)
        {
            try
            {
                if (!IsTeamMember(team, user))
                {
                    throw new Exception($"User '{user.UserName}' is not a member of the team '{team.Name}'.");
                }

                // Removing user from the team.
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand deleteCommand = new MySqlCommand();

                    deleteCommand.Connection = connection;
                    deleteCommand.CommandText = "DELETE FROM teammembers " +
                                                       $"WHERE TeamId = {team.Id} " +
                                                       $"AND UserId = {user.Id};";

                    deleteCommand.ExecuteNonQuery();

                    UserLeavedTeam?.Invoke(team, user);
                }
            }
            catch
            {
                throw;
            }
        }

        public Team GetTeamById(uint id)
        {
            Team team = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand("teams", "Id", "=", id);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if(reader.Read())
                        {
                            string name = SafeValueReader.GetStringSafe(reader, "Name");
                            team = new Team(id, name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Fatal($"{GetType().Name}.GetTeamById(uint id) error.");
                _logger?.Fatal(ex);
            }

            return team;
        }
    }
}
