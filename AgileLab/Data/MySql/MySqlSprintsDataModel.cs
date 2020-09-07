using System;
using System.Collections.Generic;
using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;

namespace AgileLab.Data.MySql
{
    class MySqlSprintsDataModel : MySqlDataModel, ISprintsDataModel
    {
        private static readonly string _SPRINT_TABLE_NAME = "sprint";

        public event EventHandler<Sprint> NewSprintCreated;

        public Sprint CreateNewSprint(string mainGoal, DateTime startDate, DateTime finishDate, uint projectId)
        {
            Sprint sprint = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    uint backlogId = ComponentsContainer.Get<IBacklogsDataModel>().CreateBacklog().Id;

                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO `{_SPRINT_TABLE_NAME}` " +
                                            "(MainGoal, StartDate, FinishDate, ProjectId, BacklogId) " +
                                            "VALUES (@MainGoal, @StartDate, @FinishDate, @ProjectId, @BacklogId)";

                    command.Parameters.Add(new MySqlParameter("@MainGoal", mainGoal));
                    command.Parameters.Add(new MySqlParameter("@StartDate", startDate));
                    command.Parameters.Add(new MySqlParameter("@FinishDate", finishDate));
                    command.Parameters.Add(new MySqlParameter("@ProjectId", projectId));
                    command.Parameters.Add(new MySqlParameter("@BacklogId", backlogId));

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);

                    sprint = new Sprint(id, mainGoal, startDate, finishDate, projectId, backlogId, false);

                    NewSprintCreated?.Invoke(this, sprint);
                }
            }
            catch(Exception ex)
            {
                _logger.Fatal("Sprint creation exception.");
                _logger.Fatal(ex);
                throw;
            }

            return sprint;
        }

        public void Finish(Sprint sprint)
        {
            if(sprint == null)
            {
                return;
            }

            if (IsAlreadyFinished(sprint))
            {
                throw new Exception($"Sprint with id '{sprint.Id}' already finished.");
            }

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText =
                        $"UPDATE `{_SPRINT_TABLE_NAME}` SET FinishedBeforeTheApointedDate = TRUE WHERE Id = @Id;";
                    command.Connection = connection;

                    command.Parameters.Add(new MySqlParameter("@Id", sprint.Id));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger?.Fatal($"{GetType().Name}.Finish(Sprint sprint) error.");
                _logger?.Fatal(ex);
            }
        }

        private bool IsAlreadyFinished(Sprint sprint)
        {
            bool finished = false;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = $"SELECT * FROM `{_SPRINT_TABLE_NAME}` WHERE id = {sprint.Id} AND (NOW() > FinishDate OR FinishedBeforeTheApointedDate = TRUE);";
                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        finished = reader.Read();
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

            return finished;
        }

        public Sprint GetSprintByBacklogId(uint backlogId)
        {
            Sprint sprint = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = CreateSelectAllCommand(_SPRINT_TABLE_NAME, "BacklogId", "=", backlogId);
                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            uint id = SafeValueReader.GetUIntSafe(reader, "Id");
                            string mainGoal = SafeValueReader.GetStringSafe(reader, "MainGoal");
                            DateTime startDate = reader.GetDateTime("StartDate");
                            DateTime finishDate = reader.GetDateTime("FinishDate");
                            uint projectId = SafeValueReader.GetUIntSafe(reader, "ProjectId");
                            bool finishedBeforeTheApointedDate = Convert.ToBoolean(reader["FinishedBeforeTheApointedDate"]);

                            sprint = new Sprint(id, mainGoal, startDate, finishDate, projectId, backlogId, finishedBeforeTheApointedDate);
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

            return sprint;
        }

        public Sprint GetSprintByProjectId(uint projectId)
        {
            Sprint sprint = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = $"SELECT * FROM `{_SPRINT_TABLE_NAME}` WHERE NOW() >= StartDate AND NOW() <= FinishDate AND ProjectId = {projectId} AND FinishedBeforeTheApointedDate = FALSE;";
                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            uint id = SafeValueReader.GetUIntSafe(reader, "Id");
                            string mainGoal = SafeValueReader.GetStringSafe(reader, "MainGoal");
                            DateTime startDate = reader.GetDateTime("StartDate");
                            DateTime finishDate = reader.GetDateTime("FinishDate");
                            uint backlogId = SafeValueReader.GetUIntSafe(reader, "BacklogId");
                            bool finishedBeforeTheApointedDate = Convert.ToBoolean(reader["FinishedBeforeTheApointedDate"]);

                            sprint = new Sprint(id, mainGoal, startDate, finishDate, projectId, backlogId, finishedBeforeTheApointedDate);
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

            return sprint;
        }

        public IEnumerable<Sprint> GetAllProjectSprints(uint projectId)
        {
            List<Sprint> sprints = new List<Sprint>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = CreateSelectAllCommand(_SPRINT_TABLE_NAME, "ProjectId", "=", projectId);
                    command.Connection = connection;

                    MySqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            uint id = SafeValueReader.GetUIntSafe(reader, "Id");
                            string mainGoal = SafeValueReader.GetStringSafe(reader, "MainGoal");
                            DateTime startDate = reader.GetDateTime("StartDate");
                            DateTime finishDate = reader.GetDateTime("FinishDate");
                            uint backlogId = SafeValueReader.GetUIntSafe(reader, "BacklogId");
                            bool finishedBeforeTheApointedDate = Convert.ToBoolean(reader["FinishedBeforeTheApointedDate"]);

                            sprints.Add(new Sprint(id, mainGoal, startDate, finishDate, projectId, backlogId, finishedBeforeTheApointedDate));
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
                _logger?.Fatal($"{GetType().Name}.GetAllProjectSprints(uint projectId) error.");
                _logger?.Fatal(ex);
            }

            return sprints;
        }
    }
}
