using System;
using System.Collections.Generic;
using System.Linq;
using AgileLab.Data.Entities;
using MySql.Data.MySqlClient;

namespace AgileLab.Data.MySql
{
    class MySqlStoryStatusesDataModel : MySqlDataModel, IStoryStatusesDataModel
    {
        private struct Status
        {
            public uint Id;
            public string Name;
            public StoryStatus EnumValue;
        }

        private static readonly string _STORY_STATUSES_TABLE_NAME = "storystatuses";

        private List<Status> _statuses = null;

        public MySqlStoryStatusesDataModel()
        {
            InitializeStatuses();
        }

        private void InitializeStatuses()
        {
            _statuses = new List<Status>();

            try
            {
                using (MySqlConnection connection = OpenNewConnection())
                {
                    MySqlCommand selectCommand = CreateSelectAllCommand(_STORY_STATUSES_TABLE_NAME);
                    selectCommand.Connection = connection;

                    MySqlDataReader reader = selectCommand.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            uint id = SafeValueReader.GetUIntSafe(reader, "Id");
                            string name = SafeValueReader.GetStringSafe(reader, "Name");
                            StoryStatus enumValue = GetStatusEnumValue(name);

                            _statuses.Add(new Status
                            {
                                Id = id,
                                Name = name,
                                EnumValue = enumValue
                            });

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
        }

        //public IEnumerable<string> GetAllStatuses()
        //{
        //    List<string> statuses = new List<string>();

        //    try
        //    {
        //        using (MySqlConnection connection = OpenNewConnection())
        //        {
        //            MySqlCommand selectCommand = CreateSelectAllCommand(_STORY_STATUSES_TABLE_NAME);
        //            selectCommand.Connection = connection;

        //            MySqlDataReader reader = selectCommand.ExecuteReader();

        //            try
        //            {
        //                while (reader.Read())
        //                {
        //                    string name = SafeValueReader.GetStringSafe(reader, "Name");
        //                    statuses.Add(name);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger?.Error(ex);
        //                throw;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return statuses;
        //}

        public IEnumerable<string> GetAllStatuses()
        {
            return _statuses.Select(item => item.Name).ToList();
        }

        //public uint GetIdOfStatus(StoryStatus status)
        //{
        //    uint id = 0;

        //    string searchingStatusName = GetStatusText(status);

        //    try
        //    {
        //        id = GetIdOfStatus(searchingStatusName);
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return id;
        //}

        public uint GetIdOfStatus(StoryStatus status)
        {
            return _statuses.Where(item => item.EnumValue == status).First().Id;
        }

        //public uint GetIdOfStatus(string status)
        //{
        //    uint id = 0;

        //    try
        //    {
        //        using (MySqlConnection connection = OpenNewConnection())
        //        {
        //            MySqlCommand selectCommand = CreateSelectCommand("Id", _STORY_STATUSES_TABLE_NAME, "Name", "=", status);
        //            selectCommand.Connection = connection;

        //            MySqlDataReader reader = selectCommand.ExecuteReader();

        //            try
        //            {
        //                if (reader.Read())
        //                {
        //                    id = Convert.ToUInt32(reader["Id"]);
        //                }
        //                else
        //                {
        //                    throw new Exception($"Story status with name `{status}` not found.");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger?.Error(ex);
        //                throw;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return id;
        //}

        public uint GetIdOfStatus(string status)
        {
            return _statuses.Where(item => item.Name == status).First().Id;
        }

        public string GetStatusText(StoryStatus statusValue)
        {
            string statusText = string.Empty;

            switch (statusValue)
            {
                case StoryStatus.WaitingForExecutor:
                    statusText = "Waiting for executor";
                    break;

                case StoryStatus.InProgress:
                    statusText = "In progress";
                    break;

                case StoryStatus.Completed:
                    statusText = "Completed";
                    break;
            }

            return statusText;
        }

        private static StoryStatus GetStatusEnumValue(string statusName)
        {
            StoryStatus enumValue = StoryStatus.WaitingForExecutor;

            switch (statusName)
            {
                case "Waiting for executor":
                    enumValue = StoryStatus.WaitingForExecutor;
                    break;

                case "In progress":
                    enumValue = StoryStatus.InProgress;
                    break;

                case "Completed":
                    enumValue = StoryStatus.Completed;
                    break;
            }

            return enumValue;
        }

        //public string GetStatus(uint statusId)
        //{
        //    string status = string.Empty;

        //    try
        //    {
        //        using (MySqlConnection connection = OpenNewConnection())
        //        {
        //            MySqlCommand selectCommand = CreateSelectCommand("Name", _STORY_STATUSES_TABLE_NAME, "Id", "=", statusId);
        //            selectCommand.Connection = connection;

        //            MySqlDataReader reader = selectCommand.ExecuteReader();

        //            try
        //            {
        //                if (reader.Read())
        //                {
        //                    status = SafeValueReader.GetStringSafe(reader, "Name");
        //                }
        //                else
        //                {
        //                    throw new Exception($"Story status with id `{statusId}` not found.");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger?.Error(ex);
        //                throw;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return status;
        //}

        public string GetStatus(uint statusId)
        {
            return _statuses.Where(item => item.Id == statusId).First().Name;
        }
    }
}
