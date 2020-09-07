using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace AgileLab.Views.Dialog
{
    class MahAppsDialogCoordinator : IDialogCoordinator
    {
        private object _context = null;

        //public MahAppsDialogCoordinator(object context)
        //{
        //    _context = context;
        //}

        private MahApps.Metro.Controls.Dialogs.IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;

        public async Task<DialogResult> ShowMessageAsync(object context, string title, string message, DialogStyle style = DialogStyle.Affirmative)
        {
            MessageDialogStyle mahAppsDialogStyle = ConvertToMahAppsDialogStyle(style);

            //Task<MessageDialogResult> task = _dialogCoordinator.ShowMessageAsync(context, title, message, mahAppsDialogStyle);
            MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(context, title, message, mahAppsDialogStyle);

            //MessageDialogResult mahAppsMessageDialogResult = awaiter.GetResult();/*task.Result;*/

            return ConvertToBaseDialogResult(result);
            //return DialogResult.Affirmative;
        }

        private DialogResult ConvertToBaseDialogResult(MessageDialogResult mahAppsMessageDialogResult)
        {
            DialogResult result = DialogResult.Affirmative;

            switch (mahAppsMessageDialogResult)
            {
                case MessageDialogResult.Affirmative:
                    result = DialogResult.Affirmative;
                    break;

                case MessageDialogResult.Canceled:
                    result = DialogResult.Canceled;
                    break;

                case MessageDialogResult.FirstAuxiliary:
                    result = DialogResult.FirstAuxiliary;
                    break;

                case MessageDialogResult.Negative:
                    result = DialogResult.Negative;
                    break;

                case MessageDialogResult.SecondAuxiliary:
                    result = DialogResult.SecondAuxiliary;
                    break;
            }

            return result;
        }

        private MessageDialogStyle ConvertToMahAppsDialogStyle(DialogStyle style)
        {
            MessageDialogStyle mahAppsDialogStyle = MessageDialogStyle.Affirmative;

            switch (style)
            {
                case DialogStyle.Affirmative:
                    mahAppsDialogStyle = MessageDialogStyle.Affirmative;
                    break;

                case DialogStyle.AffirmativeAndNegative:
                    mahAppsDialogStyle = MessageDialogStyle.AffirmativeAndNegative;
                    break;

                case DialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
                    mahAppsDialogStyle = MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary;
                    break;

                case DialogStyle.AffirmativeAndNegativeAndSingleAuxiliary:
                    mahAppsDialogStyle = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                    break;
            }

            return mahAppsDialogStyle;
        }
    }
}
