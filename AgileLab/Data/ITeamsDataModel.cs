using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;

namespace AgileLab.Data
{
    interface ITeamsDataModel
    {
        void AddTeamMember(Team team, User newMember);
        void RemoveUserFromTeam(User user, Team team);

        IList<Team> GetTeamsOfUser(uint userId);
        IList<Team> GetAllTeams();
        IList<ExtendedTeam> GetAllExtendedTeams();
        ExtendedTeam GetExtendedTeam(Team team);
        Team GetTeamById(uint id);

        bool TeamExists(string teamName);
        bool IsTeamMember(Team team, User user);

        Team CreateNewTeam(string newTeamName, User creator);

        event EventHandler<Team> NewTeamCreated;
        event TeamMembersUpdatedEventHandler UserJoinedTeam;
        event TeamMembersUpdatedEventHandler UserLeavedTeam;
    }
}
