using System;
using System.Collections.Generic;
using AgileLab.Views.AllUserTasks;

namespace AgileLab.Data.Entities
{
    class Backlog
    {
        #region Constructors
        public Backlog(uint id, List<Story> stories)
        {
            Id = id;
            Stories = stories;
        }
        #endregion

        #region Properties
        public uint Id { get; protected set; }
        public List<Story> Stories { get; protected set; }
        #endregion
    }
}
