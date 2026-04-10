using System.Windows.Input;

namespace RestAll.Desktop.App.ViewModels;

public interface IRelayCommand : ICommand
{
    void RaiseCanExecuteChanged();
}

public interface IRelayCommand<T> : ICommand
{
    void RaiseCanExecuteChanged();
}

public interface IRelayCommand<T1, T2> : ICommand
{
    void RaiseCanExecuteChanged();
}

public interface IAsyncRelayCommand : ICommand
{
    bool IsRunning { get; }
    Task ExecuteAsync();
    void NotifyCanExecuteChanged();
}

public interface IAsyncRelayCommand<T> : ICommand
{
    bool IsRunning { get; }
    Task ExecuteAsync(T parameter);
    void NotifyCanExecuteChanged();
}

public interface IAsyncRelayCommand<T1, T2> : ICommand
{
    bool IsRunning { get; }
    Task ExecuteAsync(T1 parameter1, T2 parameter2);
    void NotifyCanExecuteChanged();
}

public class RelayCommand : IRelayCommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute();

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : IRelayCommand<T>
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (_ => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => parameter is T t && _canExecute(t);

    public void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            _execute(t);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T1, T2> : IRelayCommand<T1, T2>
{
    private readonly Action<T1, T2> _execute;
    private readonly Func<T1, T2, bool> _canExecute;

    public RelayCommand(Action<T1, T2> execute, Func<T1, T2, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? ((_, _) => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (parameter is Tuple<T1, T2> tuple)
        {
            return _canExecute(tuple.Item1, tuple.Item2);
        }
        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is Tuple<T1, T2> tuple)
        {
            _execute(tuple.Item1, tuple.Item2);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand : IAsyncRelayCommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsRunning => _isRunning;

    public bool CanExecute(object? parameter) => !_isRunning && _canExecute();

    public async void Execute(object? parameter) => await ExecuteAsync();

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null))
        {
            return;
        }

        try
        {
            _isRunning = true;
            NotifyCanExecuteChanged();
            await _execute();
        }
        finally
        {
            _isRunning = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand<T> : IAsyncRelayCommand<T>
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, bool> _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (_ => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsRunning => _isRunning;

    public bool CanExecute(object? parameter) => parameter is T value && !_isRunning && _canExecute(value);

    public async void Execute(object? parameter)
    {
        if (parameter is T value)
        {
            await ExecuteAsync(value);
        }
    }

    public async Task ExecuteAsync(T parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            _isRunning = true;
            NotifyCanExecuteChanged();
            await _execute(parameter);
        }
        finally
        {
            _isRunning = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand<T1, T2> : IAsyncRelayCommand<T1, T2>
{
    private readonly Func<T1, T2, Task> _execute;
    private readonly Func<T1, T2, bool> _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<T1, T2, Task> execute, Func<T1, T2, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? ((_, _) => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsRunning => _isRunning;

    public bool CanExecute(object? parameter)
    {
        if (_isRunning)
        {
            return false;
        }

        return parameter is Tuple<T1, T2> tuple && _canExecute(tuple.Item1, tuple.Item2);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is Tuple<T1, T2> tuple)
        {
            await ExecuteAsync(tuple.Item1, tuple.Item2);
        }
    }

    public async Task ExecuteAsync(T1 parameter1, T2 parameter2)
    {
        if (!_canExecute(parameter1, parameter2) || _isRunning)
        {
            return;
        }

        try
        {
            _isRunning = true;
            NotifyCanExecuteChanged();
            await _execute(parameter1, parameter2);
        }
        finally
        {
            _isRunning = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

