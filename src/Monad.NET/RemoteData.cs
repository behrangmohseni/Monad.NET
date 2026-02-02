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
/// <typeparam name="TError">The type of the error</typeparam>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(RemoteDataDebugView<,>))]
public readonly struct RemoteData<T, TError> : IEquatable<RemoteData<T, TError>>, IComparable<RemoteData<T, TError>>
{
    private readonly T? _data;
    private readonly TError? _error;
    private readonly RemoteDataState _state;
    private readonly bool _isInitialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _isInitialized
        ? _state switch
        {
            RemoteDataState.NotAsked => "NotAsked",
            RemoteDataState.Loading => "Loading",
            RemoteDataState.Success => $"Success({_data})",
            RemoteDataState.Failure => $"Failure({_error})",
            _ => "Unknown"
        }
        : "Uninitialized";

    private enum RemoteDataState
    {
        NotAsked,
        Loading,
        Success,
        Failure
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RemoteData(T data, TError error, RemoteDataState state)
    {
        _data = data;
        _error = error;
        _state = state;
        _isInitialized = true;
    }

    /// <summary>
    /// Indicates whether the RemoteData was properly initialized via factory methods.
    /// A default-constructed RemoteData (e.g., default(RemoteData&lt;T,E&gt;)) is not initialized.
    /// Always create RemoteData instances via factory methods: <see cref="NotAsked"/>, <see cref="Loading"/>,
    /// <see cref="Ok(T)"/>, or <see cref="Error(TError)"/>.
    /// </summary>
    public bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isInitialized;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDefault()
    {
        if (!_isInitialized)
            ThrowHelper.ThrowRemoteDataIsDefault();
    }

    /// <summary>
    /// Returns true if the data has not been requested yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    public bool IsNotAsked
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _state == RemoteDataState.NotAsked;
        }
    }

    /// <summary>
    /// Returns true if the data is currently being loaded.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    public bool IsLoading
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _state == RemoteDataState.Loading;
        }
    }

    /// <summary>
    /// Returns true if the data was successfully loaded.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    public bool IsOk
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _state == RemoteDataState.Success;
        }
    }

    /// <summary>
    /// Returns true if loading the data failed.
    /// </summary>
    /// <remarks>
    /// This follows F# naming conventions for consistency across monadic types.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    public bool IsError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDefault();
            return _state == RemoteDataState.Failure;
        }
    }

    /// <summary>
    /// Creates a RemoteData in the NotAsked state.
    /// Use this as the initial state before any data fetching begins.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> NotAsked() =>
        new(default!, default!, RemoteDataState.NotAsked);

    /// <summary>
    /// Creates a RemoteData in the Loading state.
    /// Use this when starting an async operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> Loading() =>
        new(default!, default!, RemoteDataState.Loading);

    /// <summary>
    /// Creates a RemoteData in the Ok (Success) state with the loaded data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> Ok(T data)
    {
        if (data is null)
            ThrowHelper.ThrowArgumentNull(nameof(data), "Cannot create Ok with null data.");

        return new RemoteData<T, TError>(data, default!, RemoteDataState.Success);
    }

    /// <summary>
    /// Creates a RemoteData in the Error (Failure) state with an error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> Error(TError error)
    {
        if (error is null)
            ThrowHelper.ThrowArgumentNull(nameof(error), "Cannot create Error with null error.");

        return new RemoteData<T, TError>(default!, error, RemoteDataState.Failure);
    }

    /// <summary>
    /// Returns the data if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Success state or if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetValue()
    {
        ThrowIfDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if not in Failure state or if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TError GetError()
    {
        ThrowIfDefault();
        if (_state != RemoteDataState.Failure)
            ThrowHelper.ThrowInvalidOperation("Cannot get error on non-Failure state.");

        return _error!;
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns a default value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOr(T defaultValue)
    {
        ThrowIfDefault();
        return _state == RemoteDataState.Success ? _data! : defaultValue;
    }

    /// <summary>
    /// Tries to get the contained data using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="data">When this method returns, contains the data if Success; otherwise, the default value.</param>
    /// <returns>True if the RemoteData is in Success state; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
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
        ThrowIfDefault();
        data = _data;
        return _state == RemoteDataState.Success;
    }

    /// <summary>
    /// Tries to get the contained error using the familiar C# TryGet pattern.
    /// </summary>
    /// <param name="error">When this method returns, contains the error if Failure; otherwise, the default value.</param>
    /// <returns>True if the RemoteData is in Failure state; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// if (remoteData.TryGetError(out var error))
    /// {
    ///     Console.WriteLine($"Error: {error}");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetError(out TError? error)
    {
        ThrowIfDefault();
        error = _error;
        return _state == RemoteDataState.Failure;
    }

    /// <summary>
    /// Maps the data if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, TError> Map<U>(Func<T, U> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        ThrowIfDefault();

        return _state switch
        {
            RemoteDataState.Success => RemoteData<U, TError>.Ok(mapper(_data!)),
            RemoteDataState.NotAsked => RemoteData<U, TError>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, TError>.Loading(),
            RemoteDataState.Failure => RemoteData<U, TError>.Error(_error!),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Maps the error if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, F> MapError<F>(Func<TError, F> mapper)
    {
        ThrowHelper.ThrowIfNull(mapper);
        ThrowIfDefault();

        return _state switch
        {
            RemoteDataState.Success => RemoteData<T, F>.Ok(_data!),
            RemoteDataState.NotAsked => RemoteData<T, F>.NotAsked(),
            RemoteDataState.Loading => RemoteData<T, F>.Loading(),
            RemoteDataState.Failure => RemoteData<T, F>.Error(mapper(_error!)),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Chains a remote data operation.
    /// This is the monadic bind operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, TError> Bind<U>(Func<T, RemoteData<U, TError>> binder)
    {
        ThrowHelper.ThrowIfNull(binder);
        ThrowIfDefault();

        return _state switch
        {
            RemoteDataState.Success => binder(_data!),
            RemoteDataState.NotAsked => RemoteData<U, TError>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, TError>.Loading(),
            RemoteDataState.Failure => RemoteData<U, TError>.Error(_error!),
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
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<U, F> BiMap<U, F>(Func<T, U> dataMapper, Func<TError, F> errorMapper)
    {
        ThrowHelper.ThrowIfNull(dataMapper);
        ThrowHelper.ThrowIfNull(errorMapper);
        ThrowIfDefault();

        return _state switch
        {
            RemoteDataState.Success => RemoteData<U, F>.Ok(dataMapper(_data!)),
            RemoteDataState.NotAsked => RemoteData<U, F>.NotAsked(),
            RemoteDataState.Loading => RemoteData<U, F>.Loading(),
            RemoteDataState.Failure => RemoteData<U, F>.Error(errorMapper(_error!)),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Returns this RemoteData if Success, otherwise returns the alternative.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, TError> Or(RemoteData<T, TError> alternative)
    {
        ThrowIfDefault();
        return _state == RemoteDataState.Success ? this : alternative;
    }

    /// <summary>
    /// Recovers from a Failure state by providing an alternative RemoteData.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteData<T, TError> OrElse(Func<TError, RemoteData<T, TError>> recovery)
    {
        ThrowHelper.ThrowIfNull(recovery);
        ThrowIfDefault();

        return _state == RemoteDataState.Failure ? recovery(_error!) : this;
    }

    /// <summary>
    /// Pattern matches on all four states.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(
        Action notAskedAction,
        Action loadingAction,
        Action<T> successAction,
        Action<TError> failureAction)
    {
        ThrowHelper.ThrowIfNull(notAskedAction);
        ThrowHelper.ThrowIfNull(loadingAction);
        ThrowHelper.ThrowIfNull(successAction);
        ThrowHelper.ThrowIfNull(failureAction);
        ThrowIfDefault();

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
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public U Match<U>(
        Func<U> notAskedFunc,
        Func<U> loadingFunc,
        Func<T, U> successFunc,
        Func<TError, U> failureFunc)
    {
        ThrowHelper.ThrowIfNull(notAskedFunc);
        ThrowHelper.ThrowIfNull(loadingFunc);
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);
        ThrowIfDefault();

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
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> ToOption()
    {
        ThrowIfDefault();
        return _state == RemoteDataState.Success
            ? Option<T>.Some(_data!)
            : Option<T>.None();
    }

    /// <summary>
    /// Converts this RemoteData to a Result.
    /// Returns Ok if Success, Err if Failure, throws for NotAsked/Loading.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult()
    {
        ThrowIfDefault();
        if (_state == RemoteDataState.Success)
            return Result<T, TError>.Ok(_data!);
        if (_state == RemoteDataState.Failure)
            return Result<T, TError>.Error(_error!);

        var message = _state == RemoteDataState.NotAsked
            ? "RemoteData.ToResult called on NotAsked. Use ToResult(notAskedError, loadingError) overload instead."
            : "RemoteData.ToResult called on Loading. Use ToResult(notAskedError, loadingError) overload instead.";
        ThrowHelper.ThrowInvalidOperation(message);
        return default!; // Unreachable
    }

    /// <summary>
    /// Converts this RemoteData to a Result with default errors for NotAsked/Loading states.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> ToResult(TError notAskedError, TError loadingError)
    {
        ThrowIfDefault();
        return _state switch
        {
            RemoteDataState.Success => Result<T, TError>.Ok(_data!),
            RemoteDataState.Failure => Result<T, TError>.Error(_error!),
            RemoteDataState.NotAsked => Result<T, TError>.Error(notAskedError),
            RemoteDataState.Loading => Result<T, TError>.Error(loadingError),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(RemoteData<T, TError> other)
    {
        if (_state != other._state)
            return false;

        return _state switch
        {
            RemoteDataState.Success => EqualityComparer<T>.Default.Equals(_data, other._data),
            RemoteDataState.Failure => EqualityComparer<TError>.Default.Equals(_error, other._error),
            _ => true // NotAsked and Loading have no data to compare
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is RemoteData<T, TError> other && Equals(other);
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
    /// <exception cref="InvalidOperationException">Thrown if either RemoteData was not properly initialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(RemoteData<T, TError> other)
    {
        ThrowIfDefault();
        other.ThrowIfDefault();
        var stateCompare = _state.CompareTo(other._state);
        if (stateCompare != 0)
            return stateCompare;

        return _state switch
        {
            RemoteDataState.Success => Comparer<T>.Default.Compare(_data, other._data),
            RemoteDataState.Failure => Comparer<TError>.Default.Compare(_error, other._error),
            _ => 0
        };
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
    public static bool operator ==(RemoteData<T, TError> left, RemoteData<T, TError> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two RemoteData instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RemoteData<T, TError> left, RemoteData<T, TError> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Deconstructs the RemoteData into its components for pattern matching.
    /// </summary>
    /// <param name="data">The success data, or default if not Success.</param>
    /// <param name="isSuccess">True if the RemoteData is in Success state.</param>
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
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
        ThrowIfDefault();
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
    /// <exception cref="InvalidOperationException">Thrown if the RemoteData was not properly initialized.</exception>
    /// <example>
    /// <code>
    /// var (data, error, isNotAsked, isLoading, isSuccess, isFailure) = remoteData;
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(
        out T? data,
        out TError? error,
        out bool isNotAsked,
        out bool isLoading,
        out bool isSuccess,
        out bool isFailure)
    {
        ThrowIfDefault();
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
    public static RemoteData<T, TError> Tap<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action<T> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsOk)
            action(remoteData.GetValue());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Error state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> TapError<T, TError>(
        this RemoteData<T, TError> remoteData,
        Action<TError> action)
    {
        ThrowHelper.ThrowIfNull(action);

        if (remoteData.IsError)
            action(remoteData.GetError());

        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in NotAsked state, allowing method chaining.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RemoteData<T, TError> TapNotAsked<T, TError>(
        this RemoteData<T, TError> remoteData,
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
    public static RemoteData<T, TError> TapLoading<T, TError>(
        this RemoteData<T, TError> remoteData,
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
    public static RemoteData<T, TError> ToRemoteData<T, TError>(this Result<T, TError> result)
    {
        return result.Match(
            okFunc: static data => RemoteData<T, TError>.Ok(data),
            errFunc: static err => RemoteData<T, TError>.Error(err)
        );
    }

    /// <summary>
    /// Wraps an async operation in RemoteData, starting with Loading and ending with Success/Failure.
    /// </summary>
    /// <param name="taskFunc">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Success with the result, or Failure with the exception.</returns>
    public static async Task<RemoteData<T, Exception>> FromTaskAsync<T>(
        Func<Task<T>> taskFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(taskFunc);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = await taskFunc().ConfigureAwait(false);
            return RemoteData<T, Exception>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            return RemoteData<T, Exception>.Error(ex);
        }
    }

    /// <summary>
    /// Maps RemoteData with an async function.
    /// </summary>
    /// <param name="remoteData">The remote data to map.</param>
    /// <param name="mapper">An async function to apply to the value if Success.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>Success with the mapped value, or the original state preserved.</returns>
    public static async Task<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this RemoteData<T, TError> remoteData,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        if (!remoteData.IsOk)
            return remoteData.Map(static _ => default(U)!); // Preserves state

        var result = await mapper(remoteData.GetValue()).ConfigureAwait(false);
        return RemoteData<U, TError>.Ok(result);
    }

    /// <summary>
    /// Returns true if the data is loaded (either Success or Failure).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLoaded<T, TError>(this RemoteData<T, TError> remoteData)
    {
        return remoteData.IsOk || remoteData.IsError;
    }

    /// <summary>
    /// Returns true if the data is not loaded (either NotAsked or Loading).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotLoaded<T, TError>(this RemoteData<T, TError> remoteData)
    {
        return remoteData.IsNotAsked || remoteData.IsLoading;
    }

    #region ValueTask Overloads

    /// <summary>
    /// Wraps a RemoteData in a completed ValueTask. More efficient than Task.FromResult.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<RemoteData<T, TError>> AsValueTask<T, TError>(this RemoteData<T, TError> remoteData)
        => new(remoteData);

    /// <summary>
    /// Maps the value using a synchronous function. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        if (remoteDataTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(remoteDataTask.Result.Map(mapper));
        }
        return Core(remoteDataTask, mapper, cancellationToken);

        static async ValueTask<RemoteData<U, TError>> Core(ValueTask<RemoteData<T, TError>> t, Func<T, U> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var rd = await t.ConfigureAwait(false);
            return rd.Map(m);
        }
    }

    /// <summary>
    /// Maps the value using an async function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<RemoteData<U, TError>> MapAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<T, CancellationToken, ValueTask<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var remoteData = await remoteDataTask.ConfigureAwait(false);
        if (!remoteData.IsOk)
            return remoteData.Map(static _ => default(U)!);

        cancellationToken.ThrowIfCancellationRequested();
        var result = await mapper(remoteData.GetValue(), cancellationToken).ConfigureAwait(false);
        return RemoteData<U, TError>.Ok(result);
    }

    /// <summary>
    /// Pattern matches with synchronous handlers. Optimized for already-completed scenarios.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<U> MatchAsync<T, TError, U>(
        this ValueTask<RemoteData<T, TError>> remoteDataTask,
        Func<U> notAskedFunc,
        Func<U> loadingFunc,
        Func<T, U> successFunc,
        Func<TError, U> failureFunc,
        CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(notAskedFunc);
        ThrowHelper.ThrowIfNull(loadingFunc);
        ThrowHelper.ThrowIfNull(successFunc);
        ThrowHelper.ThrowIfNull(failureFunc);

        if (remoteDataTask.IsCompletedSuccessfully)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new(remoteDataTask.Result.Match(notAskedFunc, loadingFunc, successFunc, failureFunc));
        }
        return Core(remoteDataTask, notAskedFunc, loadingFunc, successFunc, failureFunc, cancellationToken);

        static async ValueTask<U> Core(
            ValueTask<RemoteData<T, TError>> t,
            Func<U> na, Func<U> l, Func<T, U> s, Func<TError, U> f,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var rd = await t.ConfigureAwait(false);
            return rd.Match(na, l, s, f);
        }
    }

    #endregion
}

/// <summary>
/// Debug view proxy for <see cref="RemoteData{T, TError}"/> to provide a better debugging experience.
/// </summary>
internal sealed class RemoteDataDebugView<T, TError>
{
    private readonly RemoteData<T, TError> _remoteData;

    public RemoteDataDebugView(RemoteData<T, TError> remoteData)
    {
        _remoteData = remoteData;
    }

    public bool IsNotAsked => _remoteData.IsNotAsked;
    public bool IsLoading => _remoteData.IsLoading;
    public bool IsSuccess => _remoteData.IsOk;
    public bool IsFailure => _remoteData.IsError;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object? Data => _remoteData.IsOk ? _remoteData.GetValue() : null;

    public object? Error => _remoteData.IsError ? _remoteData.GetError() : null;
}
