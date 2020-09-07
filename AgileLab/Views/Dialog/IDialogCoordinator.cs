using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Views.Dialog
{
    interface IDialogCoordinator
    {
        Task<DialogResult> ShowMessageAsync(object context, string title, string message, DialogStyle style = DialogStyle.Affirmative);
    }
}
