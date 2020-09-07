using System;
using System.Collections.Generic;
using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;

namespace AgileLab.Data.MySql
{
    class MySqlStoriesDataModel : MySqlDataModel, IStoriesDataModel
    {
        private static readonly string _STORIES_TABLE_NAME = "stories";

        private IStoryStatusesDataModel _storyStatusesDataModel = new MySqlStoryStatusesDataModel();

        public event EventHandler<Story> StoryUpdated;
        public event EventHandler<Story> StoryRemoved;

        public Story CreateNewStory(string name, uint importance, string notes, uint backlogId)
        {
            Story story = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    uint waitingForExecutorStatusId = _storyStatusesDataModel.GetIdOfStatus(StoryStatus.WaitingForExecutor);

                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO `{_STORIES_TABLE_NAME}` " +
                                            "(Name, Importance, Notes, Status, BacklogId) " +
                                            "VALUES (@Name, @Importance, @Notes, @StatusId, @BacklogId)";

                    command.Parameters.Add(new MySqlParameter("@Name", name));
                    command.Parameters.Add(new MySqlParameter("@Importance", importance));
                    command.Parameters.Add(new MySqlParameter("@Notes", notes));
                    command.Parameters.Add(new MySqlParameter("@StatusId", waitingForExecutorStatusId));
                    command.Parameters.Add(new MySqlParameter("@BacklogId", backlogId));

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);
                    string waitingForExecutorStatus = _storyStatusesDataModel.GetStatus(waitingForExecutorStatusId);

                    story = new Story(id, name, importance, 0, string.Empty, notes, waitingForExecutorStatus, default(uint), backlogId);
                }
            }
            catch
            {
                throw;
            }

            return story;
        }

        public Story CreateNewStory(string name, uint importance, uint initialEstimate, string howToDemo, string notes, uint backlogId)
        {
            Story story = null;

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    uint waitingForExecutorStatusId = _storyStatusesDataModel.GetIdOfStatus(StoryStatus.WaitingForExecutor);

                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO `{_STORIES_TABLE_NAME}` " +
                                            "(Name, Importance, InitialEstimate, HowToDemo, Notes, Status, BacklogId) " +
                                            "VALUES (@Name, @Importance, @InitialEstimate, @HowToDemo, @Notes, @StatusId, @BacklogId)";

                    command.Parameters.Add(new MySqlParameter("@Name", name));
                    command.Parameters.Add(new MySqlParameter("@Importance", importance));
                    command.Parameters.Add(new MySqlParameter("@InitialEstimate", initialEstimate));
                    command.Parameters.Add(new MySqlParameter("@HowToDemo", howToDemo));
                    command.Parameters.Add(new MySqlParameter("@Notes", notes));
                    command.Parameters.Add(new MySqlParameter("@StatusId", waitingForExecutorStatusId));
                    command.Parameters.Add(new MySqlParameter("@BacklogId", backlogId));

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);
                    string waitingForExecutorStatus = _storyStatusesDataModel.GetStatus(waitingForExecutorStatusId);

                    story = new Story(id, name, importance, initialEstimate, howToDemo, notes, waitingForExecutorStatus, default(uint), backlogId);
                }
            }
            catch
            {
                throw;
            }

            return story;
        }

        public IEnumerable<Story> GetStoriesByBacklogId(uint backlogId)
        {
            List<Story> stories = new List<Story>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand(_STORIES_TABLE_NAME, "BacklogId", "=", backlogId);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            uint id = SafeValueReader.GetUIntSafe(reader, "Id");
                            string name = SafeValueReader.GetStringSafe(reader, "Name");
                            uint importance = SafeValueReader.GetUIntSafe(reader, "Importance");
                            uint initialEstimate = SafeValueReader.GetUIntSafe(reader, "InitialEstimate");
                            string howToDemo = SafeValueReader.GetStringSafe(reader, "HowToDemo");
                            string notes = SafeValueReader.GetStringSafe(reader, "Notes");
                            string status = _storyStatusesDataModel.GetStatus(Convert.ToUInt32(reader["Status"]));
                            uint executorId = SafeValueReader.GetUIntSafe(reader, "ExecutorId");

                            stories.Add(new Story(id, name, importance, initialEstimate, howToDemo, notes, status, executorId, backlogId));
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

            return stories;
        }

        public void Remove(Story story)
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = CreateDeleteCommand(_STORIES_TABLE_NAME, "Id", "=", story.Id);
                    command.Connection = connection;
                    command.ExecuteNonQuery();

                    StoryRemoved?.Invoke(this, story);
                }
            }
            catch
            {
                throw;
            }
        }

        public void Update(Story story)
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = 
                        $"UPDATE `{_STORIES_TABLE_NAME}` SET " +
                        "Name = @Name, " +
                        "Importance = @Importance, " +
                        "InitialEstimate = @InitialEstimate, " +
                        "HowToDemo = @HowToDemo, " +
                        "Notes = @Notes, " +
                        "Status = @StatusId, " +
                        "ExecutorId = @ExecutorId " +
                        "WHERE Id = @Id;";
                    command.Connection = connection;

                    uint statusId = _storyStatusesDataModel.GetIdOfStatus(story.Status);

                    command.Parameters.Add(new MySqlParameter("@Name", story.Name));
                    command.Parameters.Add(new MySqlParameter("@Importance", story.Importance));
                    command.Parameters.Add(new MySqlParameter("@InitialEstimate", (story.InitialEstimate == default(uint)) ? (object)DBNull.Value : story.InitialEstimate));
                    command.Parameters.Add(new MySqlParameter("@HowToDemo", story.HowToDemo));
                    command.Parameters.Add(new MySqlParameter("@Notes", story.Notes));
                    command.Parameters.Add(new MySqlParameter("@StatusId", statusId));
                    command.Parameters.Add(new MySqlParameter("@ExecutorId", (story.ExecutorId == default(uint)) ? (object)DBNull.Value : story.ExecutorId));
                    command.Parameters.Add(new MySqlParameter("@Id", story.Id));

                    command.ExecuteNonQuery();

                    StoryUpdated?.Invoke(this, story);
                }
            }
            catch(Exception ex)
            {
                _logger?.Fatal(ex);
                throw;
            }
        }
    }
}
