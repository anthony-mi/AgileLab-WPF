using AgileLab.Data.Entities;
using System.Collections.Generic;

namespace AgileLab.Data
{
    interface IStoryStatusesDataModel
    {
        uint GetIdOfStatus(StoryStatus waitingForExecutor);
        uint GetIdOfStatus(string status);
        string GetStatus(uint statusId);
        string GetStatusText(StoryStatus statusValue);
        IEnumerable<string> GetAllStatuses();
    }
}
