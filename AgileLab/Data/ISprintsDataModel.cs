using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data
{
    interface ISprintsDataModel
    {
        Sprint CreateNewSprint(string mainGoal, DateTime startDate, DateTime finishDate, uint projectId);
        event EventHandler<Sprint> NewSprintCreated;

        IEnumerable<Sprint> GetAllProjectSprints(uint projectId);
        Sprint GetSprintByProjectId(uint projectId);
        Sprint GetSprintByBacklogId(uint backlogId);

        void Finish(Sprint sprint);
    }
}
