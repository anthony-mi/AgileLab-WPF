using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data
{
    interface IProjectsDataModel
    {
        Project CreateNewProject(string newProjectName, User creator, Team developmentTeam);
        IEnumerable<Project> GetProjectsOfTeam(Team team);
        Project GetProjectById(uint id);
        bool ProjectExists(string projectName, uint teamId);

        event EventHandler<Project> NewProjectCreated;
    }
}
