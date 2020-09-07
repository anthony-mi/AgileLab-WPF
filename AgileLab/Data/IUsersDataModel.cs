using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data
{
    interface IUsersDataModel
    {
        string GetPasswordHashByUsername(string username);
        User GetUser(string username);
        User GetUser(uint id);
        bool UserExists(string username);
        void CreateNewUser(string firstName, string lastName, string username, string passwordHash);
    }
}
