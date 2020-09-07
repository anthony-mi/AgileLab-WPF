namespace AgileLab.Data.Entities
{
    enum StoryStatus { WaitingForExecutor, InProgress, Completed };

    class Story
    {
        #region Constructors
        public Story(uint id, string name, uint importance, uint initialEstimate, string howToDemo, string notes, string status, uint executorId, uint backlogId)
        {
            Id = id;
            Name = name;
            Importance = importance;
            InitialEstimate = initialEstimate;
            HowToDemo = howToDemo;
            Notes = notes;
            Status = status;
            ExecutorId = executorId;
            BacklogId = backlogId;
        }
        #endregion

        #region Properties
        public uint Id { get; private set; }
        public string Name { get; set; }
        public uint Importance { get; set; }
        public uint InitialEstimate { get; set; }
        public string HowToDemo { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public uint ExecutorId { get; set; }
        public uint BacklogId { get; set; }
        #endregion

        #region Methods
        public string GetExecutorName()
        {
            string name = string.Empty;

            if(ExecutorId != default(uint))
            {
                User executor = ComponentsContainer.Get<IUsersDataModel>().GetUser(ExecutorId);
                name = $"{executor.FirstName} {executor.LastName}";
            }

            return name;
        }
        #endregion
    }
}
