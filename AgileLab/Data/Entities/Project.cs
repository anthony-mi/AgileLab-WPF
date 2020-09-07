using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data.Entities
{
    class Project
    {
        #region Constructors
        public Project(uint id, string name, uint managerId, uint developmentTeamId, uint productBacklogId)
        {
            Id = id;
            Name = name;
            ManagerId = managerId;
            DevelopmentTeamId = developmentTeamId;
            ProductBacklogId = productBacklogId;
        }
        #endregion

        #region Properties
        public uint Id { get; private set; }
        public string Name { get; private set; }
        public uint ManagerId { get; private set; }
        public uint DevelopmentTeamId { get; private set; }
        public uint ProductBacklogId { get; private set; }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            bool equals = false;

            if (obj is Project)
            {
                Project project = obj as Project;
                equals = project.Id == this.Id;
            }

            return equals;
        }
        #endregion
    }
}
