using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Data
{
    interface IBacklogsDataModel
    {
        Backlog CreateBacklog();
        Backlog GetBacklogById(uint id);
    }
}
