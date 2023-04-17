using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SvgToXaml.Command;

/// <summary>
/// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.Execute(object)"/> and <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.CanExecute(object)"/>.
/// </summary>
public abstract class DelegateCommandBase : ICommand, IActiveAware
{
    private bool _isActive;
    private List<WeakReference> _canExecuteChangedHandlers;
    protected readonly Func<object, Task> ExecuteMethod;
    protected readonly Func<object, bool> CanExecuteMethod;

    /// <summary>
    /// Gets or sets a value indicating whether the object is active.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// <see langword="true"/> if the object is active; otherwise <see langword="false"/>.
    /// </value>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
                return;
            _isActive = value;
            OnIsActiveChanged( );
        }
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute. You must keep a hard
    ///             reference to the handler to avoid garbage collection and unexpected results. See remarks for more information.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// When subscribing to the <see cref="ICommand.CanExecuteChanged"/> event using
    ///             code (not when binding using XAML) will need to keep a hard reference to the event handler. This is to prevent
    ///             garbage collection of the event handler because the command implements the Weak Event pattern so it does not have
    ///             a hard reference to this handler. An example implementation can be seen in the CompositeCommand and CommandBehaviorBase
    ///             classes. In most scenarios, there is no reason to sign up to the CanExecuteChanged event directly, but if you do, you
    ///             are responsible for maintaining the reference.
    /// 
    /// </remarks>
    /// 
    /// <example>
    /// The following code holds a reference to the event handler. The myEventHandlerReference value should be stored
    ///             in an instance member to avoid it from being garbage collected.
    /// 
    /// <code>
    /// EventHandler myEventHandlerReference = new EventHandler(this.OnCanExecuteChanged);
    ///             command.CanExecuteChanged += myEventHandlerReference;
    /// 
    /// </code>
    /// 
    /// </example>
    public virtual event EventHandler CanExecuteChanged
    {
        add => WeakEventHandlerManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
        remove => WeakEventHandlerManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
    }

    /// <summary>
    /// Fired if the <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.IsActive"/> property changes.
    /// 
    /// </summary>
    public virtual event EventHandler IsActiveChanged;

    /// <summary>
    /// Creates a new instance of a <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase"/>, specifying both the execute action and the can execute function.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="Action"/> to execute when <see cref="ICommand.Execute(object)"/> is invoked.</param><param name="canExecuteMethod">The <see cref="System.Func"/> to invoked when <see cref="ICommand.CanExecute(object)"/> is invoked.</param>
    protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
    {
        if (executeMethod == null || canExecuteMethod == null)
            throw new ArgumentNullException(nameof(executeMethod), "DelegateCommand Delegates CannotBeNull");
        ExecuteMethod = arg =>
        {
            executeMethod(arg);
            return Task.Delay(0);
        };
        CanExecuteMethod = canExecuteMethod;
    }

    /// <summary>
    /// Creates a new instance of a <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase"/>, specifying both the Execute action as an awaitable Task and the CanExecute function.
    /// 
    /// </summary>
    /// <param name="executeMethod">The <see cref="System.Func"/> to execute when <see cref="ICommand.Execute(object)"/> is invoked.</param><param name="canExecuteMethod">The <see cref="System.Func"/> to invoked when <see cref="ICommand.CanExecute(object)"/> is invoked.</param>
    protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
    {
        if (executeMethod == null || canExecuteMethod == null)
            throw new ArgumentNullException(nameof(executeMethod), "DelegateCommand Delegates CannotBeNull");
        ExecuteMethod = executeMethod;
        CanExecuteMethod = canExecuteMethod;
    }

    /// <summary>
    /// Raises <see cref="ICommand.CanExecuteChanged"/> on the UI thread so every
    ///             command invoker can requery <see cref="ICommand.CanExecute(object)"/>.
    /// 
    /// </summary>
    protected virtual void OnCanExecuteChanged( )
    {
        WeakEventHandlerManager.CallWeakReferenceHandlers(this, _canExecuteChangedHandlers);
    }

    /// <summary>
    /// Raises <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.CanExecuteChanged"/> on the UI thread so every command invoker
    ///             can requery to check if the command can execute.
    /// 
    /// <remarks>
    /// Note that this will trigger the execution of <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.CanExecute(object)"/> once for each invoker.
    /// </remarks>
    /// 
    /// </summary>
    public void RaiseCanExecuteChanged( )
    {
        OnCanExecuteChanged( );
    }

    async void ICommand.Execute(object parameter)
    {
        await Execute(parameter).ConfigureAwait(false);
    }

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute(parameter);
    }

    /// <summary>
    /// Executes the command with the provided parameter by invoking the <see cref="Action"/> supplied during construction.
    /// </summary>
    /// <param name="parameter"/>
    protected async Task Execute(object parameter)
    {
        await ExecuteMethod(parameter).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines if the command can execute with the provided parameter by invoking the <see cref="System.Func"/> supplied during construction.
    /// </summary>
    /// <param name="parameter">The parameter to use when determining if this command can execute.</param>
    /// <returns>
    /// Returns <see langword="true"/> if the command can execute.  <see langword="False"/> otherwise.
    /// </returns>
    protected bool CanExecute(object parameter)
    {
        return CanExecuteMethod == null || CanExecuteMethod(parameter);
    }

    /// <summary>
    /// This raises the <see cref="Microsoft.Practices.Prism.Commands.DelegateCommandBase.IsActiveChanged"/> event.
    /// </summary>
    protected virtual void OnIsActiveChanged( )
    {
        EventHandler eventHandler = IsActiveChanged;
        eventHandler?.Invoke(this, EventArgs.Empty);
    }
}

