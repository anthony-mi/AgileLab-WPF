using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AgileLab.Data.MySql
{
    class MySqlBacklogsDataModel : MySqlDataModel, IBacklogsDataModel
    {
        private static readonly string _BACKLOGS_TABLE_NAME = "backlogs";

        private IStoriesDataModel _storiesDataModel = new MySqlStoriesDataModel(); // Not 'ComponentsContainer.Get<IStoriesDataModel>()' for avoiding cross references.

        public Backlog CreateBacklog()
        {
            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand command = new MySqlCommand();

                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO `{_BACKLOGS_TABLE_NAME}` () VALUES();";

                    command.ExecuteNonQuery();

                    uint id = Convert.ToUInt32(command.LastInsertedId);

                    Backlog newBacklog = new Backlog(id, new List<Story>());

                    return newBacklog;
                }
            }
            catch
            {
                throw;
            }
        }

        public Backlog GetBacklogById(uint id)
        {
            Backlog backlog = null;

            try
            {
                IEnumerable<Story> backlogStories = _storiesDataModel.GetStoriesByBacklogId(id);
                backlog = new Backlog(id, (List<Story>) backlogStories);
            }
            catch
            {
                throw;
            }

            return backlog;
        }
    }
}
