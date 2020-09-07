using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data.Entities
{
    class User
    {
        #region Constructors
        public User(uint id, string firstName, string lastName, string userName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
        }
        #endregion

        #region Properties
        public uint Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string UserName { get; private set; }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            bool equals = false;

            if(obj is User)
            {
                equals = (obj as User).Id == this.Id;
            }

            return equals;
        }
        #endregion
    }
}
