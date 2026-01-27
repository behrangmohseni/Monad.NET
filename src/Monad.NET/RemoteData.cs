using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Represents the state of remotely-loaded data.
/// Perfect for tracking API calls, database queries, or any asynchronous data fetching.
/// Eliminates the need for separate loading/error boolean flags.
/// Inspired by Elm's RemoteData type.
/// </summary>
/// <typeparam name="T">The type of the data</typeparam>
/// <typeparam name="TErr">The type of the error</typeparam>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(RemoteDataDebugView<,>))]
public readonly struct RemoteData<T, TErr> : IEquatable<RemoteData<T, TErr>>, IComparable<RemoteData<T, TErr>>, IComparable
{
    private readonly T? _data;
    private readonly TErr? _error;
    private readonly RemoteDataState _state;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _state switch
    {
        RemoteDataState.NotAsked => "NotAsked",
        RemoteDataState.Loading => "Loading",
        RemoteDataState.Success => $"Success({_data})",
        RemoteDataState.Failure => $"Failure({_error})",
        _ => "Unknown"
    };

    private enum RemoteDataState
    {
        NotAsked,
        Loading,
        Success,
        Failure
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RemoteData(T data, TErr error, RemoteDataState state)
    {
        _data = data;
        _error = error;
        _state = state;
    }

    /// <summary>
    /// Returns true if the data has not been requested yet.
    /// </summary>
    public bool IsNotAsked
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _state == RemoteDataState.NotAsked;
    }

    /// <summary>
    /// Returns true if the data is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _state == RemoteDataState.Loading;
    }

    /// <summary>
    /// Returns true if the data was successfully loaded.
    /// </summary>
    public bool IsSuccess
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _state == RemoteDataState.Success;
    }

    /// <summary>
    /// Returns true if loading the data failed.
    /// </summary>
    public bool IsFailure
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _state == RemoteDataState.Failure;
    }

    /// <summary>
    /// Creates a RemoteData in the NotAsked state.
    /// Use this as the initial state before any data fetching begins.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> NotAsked() =>
        new(default!, default!, RemoteDataState.NotAsked);

    /// <summary>
    /// Creates a RemoteData in the Loading state.
    /// Use this when starting an async operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> Loading() =>
        new(default!, default!, RemoteDataState.Loading);

    /// <summary>
    /// Creates a RemoteData in the Success state with the loaded data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> Success(T data)
    {
        if (data is null)
            ThrowHelper.ThrowArgumentNull(nameof(data), "Cannot create Success with null data.");

        return new RemoteData<T, TErr>(data, default!, RemoteDataState.Success);
    }

    /// <summary>
    /// Creates a RemoteData in the Failure state with an error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> Failure(TErr error)
    {
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Cannot create Failure with null error.");

        return new RemoteData<T, TErr>(default!, error, RemoteDataState.Failure);
    }

    /// <summary>
    /// Returns the data if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Success state</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue()
    {
        if (_state == RemoteDataState.Success)
            return _data!;

        var stateMessage = _state switch
        {
            RemoteDataState.NotAsked => "RemoteData is NotAsked. Cannot get value.",
            RemoteDataState.Loading => "RemoteData is Loading. Cannot get value.",
            RemoteDataState.Failure => $"RemoteData is Failure. Cannot get value. Error: {_error}",
            _ => "RemoteData is in invalid state."
        };
        ThrowHelper.ThrowInvalidOperation(stateMessage);
        return default!; // Unreachable
    }

    /// <summary>
    /// Returns the error if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Failure state</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TErr GetError()
    {
        if (_state != RemoteDataState.Failure)
            ThrowHelper.ThrowInvalidOperation("Cannot get error on non-Failure state.");

        return _error!;
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        return _state == RemoteDataState.Success ? _data! : defaultValue;
    }

    /// <summary>
    /// Returns the data if successful, otherwise computes a default value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrElse(Func<T> defaultFunc)
    {
        ThrowHelper.ThrowIfNull(defaultFunc);

        return _state == RemoteDataState.Success ? _data! : defaultFunc();
    }

    /// <summary>
    /// Tries to get the contained data using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="data">When this method returns, contains the data if Success; otherwise, the default value.</param>
    /// <returns>True if the RemoteData is in Success state; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (remoteData.TryGet(out var data))
    /// {
    ///     Console.WriteLine($"Data: {data}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(out T? data)
    {
        data = _data;
        return _state == RemoteDataState.Success;
    }

    /// <summary>
    /// Tries to get the contained error using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="error">When this method returns, contains the error if Failure; otherwise, the default value.</param>
    /// <returns>True if the RemoteData is in Failure state; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// if (remoteData.TryGetError(out var error))
    /// {
    ///     Console.WriteLine($"Error: {error}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetError(out TErr? error)
    {
        error = _error;
        return _state == RemoteDataState.Failure;
    }

    /// <summary>
    /// Maps the data if successful.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, TErr> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        return _state switch
        {
            RemoteDataState.Success => RemoteData<U, TErr>.Success(mapper(_data!)),
            RemoteDataState.NotAsked => RemoteData<U, TErr>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, TErr>.Loading(),
            RemoteDataState.Failure => RemoteData<U, TErr>.Failure(_error!),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Maps the error if failed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, F> MapError<F>(Func<TErr, F> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        return _state switch
        {
            RemoteDataState.Success => RemoteData<T, F>.Success(_data!),
            RemoteDataState.NotAsked => RemoteData<T, F>.NotAsked(),
            RemoteDataState.Loading => RemoteData<T, F>.Loading(),
            RemoteDataState.Failure => RemoteData<T, F>.Failure(mapper(_error!)),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Chains a remote data operation.
    /// This is the monadic bind operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, TErr> Bind<U>(Func<T, RemoteData<U, TErr>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);

        return _state switch
        {
            RemoteDataState.Success => binder(_data!),
            RemoteDataState.NotAsked => RemoteData<U, TErr>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, TErr>.Loading(),
            RemoteDataState.Failure => RemoteData<U, TErr>.Failure(_error!),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Maps both the data and error values.
    /// </summary>
    /// <typeparam name="U">The new data type.</typeparam>
    /// <typeparam name="F">The new error type.</typeparam>
    /// <param name="dataMapper">Function to transform the data if successful.</param>
    /// <param name="errorMapper">Function to transform the error if failed.</param>
    /// <returns>A new RemoteData with transformed data or error.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, F> BiMap<U, F>(Func<T, U> dataMapper, Func<TErr, F> errorMapper)
    {
        ThrowHelper.ThrowIfNull(dataMapper);
        ThrowHelper.ThrowIfNull(errorMapper);

        return _state switch
        {
            RemoteDataState.Success => RemoteData<U, F>.Success(dataMapper(_data!)),
            RemoteDataState.NotAsked => RemoteData<U, F>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, F>.Loading(),
            RemoteDataState.Failure => RemoteData<U, F>.Failure(errorMapper(_error!)),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Returns this RemoteData if Success, otherwise returns the alternative.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, TErr> Or(RemoteData<T, TErr> alternative)
    {
        return _state == RemoteDataState.Success ? this : alternative;
    }

    /// <summary>
    /// Recovers from a Failure state by providing an alternative RemoteData.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, TErr> OrElse(Func<TErr, RemoteData<T, TErr>> recovery)
    {
        ThrowHelper.ThrowIfNull(recovery);

        return _state == RemoteDataState.Failure ? recovery(_error!) : this;
    }

    /// <summary>
    /// Pattern matches on all four states.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(
        Action notAskedAction,
        Action loadingAction,
        Action<T> successAction,
        Action<TErr> failureAction)
    {
        ThrowHelper.ThrowIfNull(notAskedAction);
        ThrowHelper.ThrowIfNull(loadingAction);
        ThrowHelper.ThrowIfNull(successAction);
        ThrowHelper.ThrowIfNull(failureAction);

        switch (_state)
        {
            case RemoteDataState.NotAsked:
                notAskedAction();
                break;
            case RemoteDataState.Loading:
                loadingAction();
                break;
            case RemoteDataState.Success:
                successAction(_data!);
                break;
            case RemoteDataState.Failure:
                failureAction(_error!);
                break;
        }
    }

    /// <summary>
    /// Pattern matches on all four states and returns a result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(
        Func<U> notAskedFunc,
        Func<U> loadingFunc,
        Func<T, U> successFunc,
        Func<TErr, U> failureFunc)
    {
        ThrowHelper.ThrowIfNull(notAskedFunc);
        ThrowHelper.ThrowIfNull(loadingFunc);
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);

        return _state switch
        {
            RemoteDataState.NotAsked => notAskedFunc(),
            RemoteDataState.Loading => loadingFunc(),
            RemoteDataState.Success => successFunc(_data!),
            RemoteDataState.Failure => failureFunc(_error!),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Converts this RemoteData to an Option.
    /// Returns Some if Success, None for all other states.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        return _state == RemoteDataState.Success
            ? Option<T>.Some(_data!)
            : Option<T>.None();
    }

    /// <summary>
    /// Converts this RemoteData to a Result.
    /// Returns Ok if Success, Err if Failure, throws for NotAsked/Loading.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> ToResult()
    {
        if (_state == RemoteDataState.Success)
            return Result<T, TErr>.Ok(_data!);
        if (_state == RemoteDataState.Failure)
            return Result<T, TErr>.Err(_error!);

        var message = _state == RemoteDataState.NotAsked
            ? "RemoteData.ToResult called on NotAsked. Use ToResult(notAskedError, loadingError) overload instead."
            : "RemoteData.ToResult called on Loading. Use ToResult(notAskedError, loadingError) overload instead.";
        ThrowHelper.ThrowInvalidOperation(message);
        return default!; // Unreachable
    }

    /// <summary>
    /// Converts this RemoteData to a Result with default errors for NotAsked/Loading states.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TErr> ToResult(TErr notAskedError, TErr loadingError)
    {
        return _state switch
        {
            RemoteDataState.Success => Result<T, TErr>.Ok(_data!),
            RemoteDataState.Failure => Result<T, TErr>.Err(_error!),
            RemoteDataState.NotAsked => Result<T, TErr>.Err(notAskedError),
            RemoteDataState.Loading => Result<T, TErr>.Err(loadingError),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(RemoteData<T, TErr> other)
    {
        if (_state != other._state)
            return false;

        return _state switch
        {
            RemoteDataState.Success => EqualityComparer<T>.Default.Equals(_data, other._data),
            RemoteDataState.Failure => EqualityComparer<TErr>.Default.Equals(_error, other._error),
            _ => true // NotAsked and Loading have no data to compare
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is RemoteData<T, TErr> other && Equals(other);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return _state switch
        {
            RemoteDataState.Success => HashCode.Combine(_state, _data),
            RemoteDataState.Failure => HashCode.Combine(_state, _error),
            _ => _state.GetHashCode()
        };
    }

    /// <summary>
    /// Compares this RemoteData to another RemoteData.
    /// States are ordered: NotAsked &lt; Loading &lt; Failure &lt; Success.
    /// When both are Success, the data values are compared.
    /// When both are Failure, the errors are compared.
    /// </summary>
    /// <param name="other">The other RemoteData to compare to.</param>
    /// <returns>A negative value if this is less than other, zero if equal, positive if greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(RemoteData<T, TErr> other)
    {
        var stateCompare = _state.CompareTo(other._state);
        if (stateCompare != 0)
            return stateCompare;

        return _state switch
        {
            RemoteDataState.Success => Comparer<T>.Default.Compare(_data, other._data),
            RemoteDataState.Failure => Comparer<TErr>.Default.Compare(_error, other._error),
            _ => 0
        };
    }

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is RemoteData<T, TErr> other)
            return CompareTo(other);
        ThrowHelper.ThrowArgument(nameof(obj), $"Object must be of type RemoteData<{typeof(T).Name}, {typeof(TErr).Name}>");
        return 0; // unreachable
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _state switch
        {
            RemoteDataState.NotAsked => "NotAsked",
            RemoteDataState.Loading => "Loading",
            RemoteDataState.Success => $"Success({_data})",
            RemoteDataState.Failure => $"Failure({_error})",
            _ => "Invalid"
        };
    }

    /// <summary>
    /// Determines whether two RemoteData instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RemoteData<T, TErr> left, RemoteData<T, TErr> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two RemoteData instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RemoteData<T, TErr> left, RemoteData<T, TErr> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the RemoteData into its components for pattern matching.
    /// </summary>
    /// <param name="data">The success data, or default if not Success.</param>
    /// <param name="isSuccess">True if the RemoteData is in Success state.</param>
    /// <example>
    /// <code>
    /// var (data, isSuccess) = remoteData;
    /// if (isSuccess)
    ///     Console.WriteLine($"Data: {data}");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T? data, out bool isSuccess)
    {
        data = _data;
        isSuccess = _state == RemoteDataState.Success;
    }

    /// <summary>
    /// Deconstructs the RemoteData into all its components for pattern matching.
    /// </summary>
    /// <param name="data">The success data, or default if not Success.</param>
    /// <param name="error">The error, or default if not Failure.</param>
    /// <param name="isNotAsked">True if in NotAsked state.</param>
    /// <param name="isLoading">True if in Loading state.</param>
    /// <param name="isSuccess">True if in Success state.</param>
    /// <param name="isFailure">True if in Failure state.</param>
    /// <example>
    /// <code>
    /// var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(
        out T? data,
        out TErr? error,
        out bool isNotAsked,
        out bool isLoading,
        out bool isSuccess,
        out bool isFailure)
    {
        data = _data;
        error = _error;
        isNotAsked = _state == RemoteDataState.NotAsked;
        isLoading = _state == RemoteDataState.Loading;
        isSuccess = _state == RemoteDataState.Success;
        isFailure = _state == RemoteDataState.Failure;
    }
}

/// <summary>
/// Extension methods for RemoteData&lt;T, E&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RemoteDataExtensions
{
    /// <summary>
    /// Executes an action if the data is in Success state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> Tap<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsSuccess)
            action(remoteData.GetValue());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Failure state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> TapFailure<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action<TErr> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsFailure)
            action(remoteData.GetError());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in NotAsked state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> TapNotAsked<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsNotAsked)
            action();

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Loading state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> TapLoading<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsLoading)
            action();

        return remoteData;
    }

    /// <summary>
    /// Converts a Result to RemoteData in Success or Failure state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TErr> ToRemoteData<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match(
            okFunc: static data => RemoteData<T, TErr>.Success(data),
            errFunc: static err => RemoteData<T, TErr>.Failure(err)
        );
    }

    /// <summary>
    /// Wraps an async operation in RemoteData, starting with Loading and ending with Success/Failure.
    /// </summary>
    public static async Task<RemoteData<T, Exception>> FromTaskAsync<T>(Func<Task<T>> taskFunc)
    {
        ThrowHelper.ThrowIfNull(taskFunc);

        try
        {
            var result = await taskFunc().ConfigureAwait(false);
            return RemoteData<T, Exception>.Success(result);
        }
        catch (Exception ex)
        {
            return RemoteData<T, Exception>.Failure(ex);
        }
    }

    /// <summary>
    /// Maps RemoteData with an async function.
    /// </summary>
    public static async Task<RemoteData<U, TErr>> MapAsync<T, TErr, U>(
        this RemoteData<T, TErr> remoteData,
        Func<T, Task<U>> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);

        if (!remoteData.IsSuccess)
            return remoteData.Map(static _ => default(U)!); // Preserves state

        var result = await mapper(remoteData.GetValue()).ConfigureAwait(false);
        return RemoteData<U, TErr>.Success(result);
    }

    /// <summary>
    /// Returns true if the data is loaded (either Success or Failure).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLoaded<T, TErr>(this RemoteData<T, TErr> remoteData)
    {
        return remoteData.IsSuccess || remoteData.IsFailure;
    }

    /// <summary>
    /// Returns true if the data is not loaded (either NotAsked or Loading).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotLoaded<T, TErr>(this RemoteData<T, TErr> remoteData)
    {
        return remoteData.IsNotAsked || remoteData.IsLoading;
    }
}

/// <summary>
/// Debug view proxy for <see cref="RemoteData{T, TErr}"/> to provide a better debugging experience.
/// </summary>
internal sealed class RemoteDataDebugView<T, TErr>
{
    private readonly RemoteData<T, TErr> _remoteData;

    public RemoteDataDebugView(RemoteData<T, TErr> remoteData)
    {
        _remoteData = remoteData;
    }

    public bool IsNotAsked => _remoteData.IsNotAsked;
    public bool IsLoading => _remoteData.IsLoading;
    public bool IsSuccess => _remoteData.IsSuccess;
    public bool IsFailure => _remoteData.IsFailure;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Data => _remoteData.IsSuccess ? _remoteData.GetValue() : null;

    public object? Error => _remoteData.IsFailure ? _remoteData.GetError() : null;
}
