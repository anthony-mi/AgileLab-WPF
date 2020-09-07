using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data
{
    interface IStoriesDataModel
    {
        Story CreateNewStory(string name, uint importance, string notes, uint backlogId);
        Story CreateNewStory(string name, uint importance, uint initialEstimate, string howToDemo, string notes, uint backlogId);

        IEnumerable<Story> GetStoriesByBacklogId(uint backlogId);

        void Remove(Story story);
        void Update(Story story);

        event EventHandler<Story> StoryUpdated;
        event EventHandler<Story> StoryRemoved;
    }
}
