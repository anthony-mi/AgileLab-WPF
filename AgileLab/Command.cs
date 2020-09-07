using System;
using System.Windows.Input;

namespace AgileLab
{
    public class Command : ICommand
    {
        #region Fields
        /// <summary>
        /// Invokes when the command is enabled.
        /// </summary>
        protected Action _action = null;
        protected Action<object> _parameterizedAction = null;

        /// <summary>
        /// Responsible for the ability to execute a command.
        /// </summary>
        Predicate<object> _canExecute;
        #endregion

        #region Events
        /// <summary>
        ///  Вызывается, когда меняется возможность выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializing a new instance of a class without parameters <see cref="Command"/>.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="canExecute">If set to <c>true</c> [can execute] (execution is allowed).</param>
        public Command(Action action, Predicate<object> canExecute)
        {
            //  Set the action.
            this._action = action;
            this._canExecute = canExecute;
        }

        /// <summary>
        /// Initializing a new instance of a class with parameters <see cref="Command"/>.
        /// </summary>
        /// <param name="parameterizedAction">Parameterized action.</param>
        /// <param name="canExecute">If set to <c>true</c> [can execute] (execution is allowed).</param>
        public Command(Action<object> parameterizedAction, Predicate<object> canExecute)
        {
            //  Set the action.
            this._parameterizedAction = parameterizedAction;
            this._canExecute = canExecute;
        }
        #endregion

        #region ICommand implemetation
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        ///  If the command does not require data to be passed,
        ///  this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        ///  If the command does not require data to be passed,
        ///  this object can be set to null.</param>

        void ICommand.Execute(object parameter)
        {
            this.DoExecute(parameter);
        }
        #endregion

        #region Invoke Methods
        protected void InvokeAction(object param)
        {
            var action = _action;
            var parameterizedAction = _parameterizedAction;
            if (action != null)
                action();
            else
            {
#pragma warning disable IDE1005 // Delegate invocation can be simplified.
                parameterizedAction?.Invoke(param);
#pragma warning restore IDE1005
            }
        }
        #endregion

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="param">The param.</param>
        public virtual void DoExecute(object param)
        {
            //  Call the action or the parameterized action, whichever has been set.
            InvokeAction(param);
        }
    }
}
