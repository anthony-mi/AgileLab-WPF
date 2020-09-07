using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Services.Logging
{
    interface ILogger
    {
        void Debug(string message);
        void Debug(Exception exception);

        void Error(string message);
        void Error(Exception exception);

        void Fatal(string message);
        void Fatal(Exception exception);

        void Information(string message);
        void Information(Exception exception);

        void Warning(string message);
        void Warning(Exception exception);
    }
}
