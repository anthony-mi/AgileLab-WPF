using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data.Entities
{
    class Sprint
    {
        #region Constructors
        public Sprint(uint id, string mainGoal, DateTime startDate, DateTime finishDate, uint projectId, uint backlogId, bool finishedBeforeTheApointedDate)
        {
            Id = id;
            MainGoal = mainGoal;
            StartDate = startDate;
            FinishDate = finishDate;
            ProjectId = projectId;
            BacklogId = backlogId;
            FinishedBeforeTheApointedDate = finishedBeforeTheApointedDate;
        }
        #endregion

        #region Properties
        public uint Id { get; private set; }
        public string MainGoal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public uint ProjectId { get; set; }
        public uint BacklogId { get; set; }
        public bool FinishedBeforeTheApointedDate { get; set; }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            bool equals = false;

            if (obj is Sprint)
            {
                Sprint sprint = obj as Sprint;
                equals = sprint.Id == this.Id;
            }

            return equals;
        }
        #endregion
    }
}
