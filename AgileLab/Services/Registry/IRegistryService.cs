namespace AgileLab.Services.Registry
{
    interface IRegistryService
    {
        string GetPasswordHash();
        string GetUsername();
        uint GetCurrentTeamId();
        uint GetCurrentProjectId();

        void RemoveAllData();

        void SetPasswordHash(string address);
        void SetUsername(string username);
        void SetCurrentTeamId(uint id);
        void SetCurrentProjectId(uint id);
    }
}
