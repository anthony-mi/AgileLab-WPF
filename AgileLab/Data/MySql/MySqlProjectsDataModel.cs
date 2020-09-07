using System;
using System.Collections.Generic;
using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;

namespace AgileLab.Data.MySql
{
    class MySqlProjectsDataModel : MySqlDataModel, IProjectsDataModel
    {
        private IBacklogsDataModel _backlogsDataModel = new MySqlBacklogsDataModel(); // Not 'ComponentsContainer.Get<IBacklogsDataModel>()' for avoiding cross references.

        private static readonly string _PROJECTS_TABLE_NAME = "projects";

        public event EventHandler<Project> NewProjectCreated;

        public Project CreateNewProject(string newProjectName, User creator, Team developmentTeam)
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    Backlog productBacklog = _backlogsDataModel.CreateBacklog();

                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO `{_PROJECTS_TABLE_NAME}` " +
                                            "(Name, Manager, DevelopmentTeamId, ProductBacklogId) " +
                                            "VALUES (@Name, @Manager, @DevelopmentTeamId, @ProductBacklogId)";

                    command.Parameters.Add(new MySqlParameter("@Name", newProjectName));
                    command.Parameters.Add(new MySqlParameter("@Manager", creator.Id));
                    command.Parameters.Add(new MySqlParameter("@DevelopmentTeamId", developmentTeam.Id));
                    command.Parameters.Add(new MySqlParameter("@ProductBacklogId", productBacklog.Id));

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);

                    Project newProject = new Project(id, newProjectName, creator.Id, developmentTeam.Id, productBacklog.Id);

                    NewProjectCreated?.Invoke(this, newProject);

                    return newProject;
                }
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Project> GetProjectsOfTeam(Team team)
        {
            List<Project> projects = new List<Project>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand(_PROJECTS_TABLE_NAME, "DevelopmentTeamId", "=", team.Id);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        while(reader.Read())
                        {
                            uint id = Convert.ToUInt32(reader["Id"]);
                            string name = SafeValueReader.GetStringSafe(reader, "Name");
                            uint manager = Convert.ToUInt32(reader["Manager"]);
                            uint developmentTeamId = Convert.ToUInt32(reader["DevelopmentTeamId"]);
                            uint productBacklogId = Convert.ToUInt32(reader["ProductBacklogId"]);

                            projects.Add(new Project(id, name, manager, developmentTeamId, productBacklogId));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex);
                        throw;
                    }
                }
            }
            catch
            {
                throw;
            }

            return projects;
        }

        public bool ProjectExists(string projectName, uint teamId)
        {
            bool exists = false;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = $"SELECT Id FROM `{_PROJECTS_TABLE_NAME}` WHERE Name = '{projectName}' AND DevelopmentTeamId = {teamId};";
                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();
                    exists = reader.Read();
                }
            }
            catch
            {
                throw;
            }

            return exists;
        }

        public Project GetProjectById(uint id)
        {
            Project project = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand(_PROJECTS_TABLE_NAME, "Id", "=", id);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            string name = SafeValueReader.GetStringSafe(reader, "Name");
                            uint manager = Convert.ToUInt32(reader["Manager"]);
                            uint developmentTeamId = Convert.ToUInt32(reader["DevelopmentTeamId"]);
                            uint productBacklogId = Convert.ToUInt32(reader["ProductBacklogId"]);

                            project = new Project(id, name, manager, developmentTeamId, productBacklogId);
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
                _logger?.Fatal($"{GetType().Name}.GetProjectById(uint id) error.");
                _logger?.Fatal(ex);
            }

            return project;
        }
    }
}
