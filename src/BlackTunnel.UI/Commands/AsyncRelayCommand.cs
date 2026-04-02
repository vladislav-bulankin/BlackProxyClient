using System.Windows.Input;

namespace BlackTunnel.UI.Commands; 
public class AsyncRelayCommand : ICommand {
    private readonly Func<Task> execute;
    private readonly Func<bool>? canExecute;
    private bool isExecuting;

    public AsyncRelayCommand (Func<Task> execute, Func<bool>? canExecute = null) {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute (object? parameter)
        => !isExecuting && (canExecute?.Invoke() ?? true);

    public async void Execute (object? parameter) {
        if (!CanExecute(parameter))
            return;

        try {
            isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            await execute();
        } finally {
            isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
