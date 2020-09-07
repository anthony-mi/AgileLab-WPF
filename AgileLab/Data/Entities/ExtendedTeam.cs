namespace AgileLab.Data.Entities
{
    class ExtendedTeam : Team
    {
        #region Constructors
        public ExtendedTeam(Team team, uint countOfMembers, uint countOfProjects) : base(team.Id, team.Name)
        {
            CountOfMembers = countOfMembers;
            CountOfProjects = countOfProjects;
        }
        #endregion

        #region Properties
        public uint CountOfMembers { get; set; }
        public uint CountOfProjects { get; set; }
        #endregion
    }
}
