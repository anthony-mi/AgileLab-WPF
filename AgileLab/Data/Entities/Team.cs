using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data.Entities
{
    class Team
    {
        #region Constructors
        public Team(uint id, string name)
        {
            Id = id;
            Name = name;
        }
        #endregion

        #region Properties
        public uint Id { get; protected set; }
        public string Name { get; protected set; }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            bool equals = false;

            if(obj is Team)
            {
                Team team = obj as Team;
                equals = team.Id == this.Id;
            }

            return equals;
        }
        #endregion
    }
}
