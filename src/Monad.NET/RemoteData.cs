namespace Monad.NET;

/// <summary>
/// Represents the state of remotely-loaded data.
/// Perfect for tracking API calls, database queries, or any asynchronous data fetching.
/// Eliminates the need for separate loading/error boolean flags.
/// Inspired by Elm's RemoteData type.
/// </summary>
/// <typeparam name="T">The type of the data</typeparam>
/// <typeparam name="TErr">The type of the error</typeparam>
public readonly struct RemoteData<T, TErr> : IEquatable<RemoteData<T, TErr>>
{
    private readonly T? _data;
    private readonly TErr? _error;
    private readonly RemoteDataState _state;

    private enum RemoteDataState
    {
        NotAsked,
        Loading,
        Success,
        Failure
    }

    private RemoteData(T data, TErr error, RemoteDataState state)
    {
        _data = data;
        _error = error;
        _state = state;
    }

    /// <summary>
    /// Returns true if the data has not been requested yet.
    /// </summary>
    public bool IsNotAsked => _state == RemoteDataState.NotAsked;

    /// <summary>
    /// Returns true if the data is currently being loaded.
    /// </summary>
    public bool IsLoading => _state == RemoteDataState.Loading;

    /// <summary>
    /// Returns true if the data was successfully loaded.
    /// </summary>
    public bool IsSuccess => _state == RemoteDataState.Success;

    /// <summary>
    /// Returns true if loading the data failed.
    /// </summary>
    public bool IsFailure => _state == RemoteDataState.Failure;

    /// <summary>
    /// Creates a RemoteData in the NotAsked state.
    /// Use this as the initial state before any data fetching begins.
    /// </summary>
    public static RemoteData<T, TErr> NotAsked() =>
        new(default!, default!, RemoteDataState.NotAsked);

    /// <summary>
    /// Creates a RemoteData in the Loading state.
    /// Use this when starting an async operation.
    /// </summary>
    public static RemoteData<T, TErr> Loading() =>
        new(default!, default!, RemoteDataState.Loading);

    /// <summary>
    /// Creates a RemoteData in the Success state with the loaded data.
    /// </summary>
    public static RemoteData<T, TErr> Success(T data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data), "Cannot create Success with null data.");
        
        return new RemoteData<T, TErr>(data, default!, RemoteDataState.Success);
    }

    /// <summary>
    /// Creates a RemoteData in the Failure state with an error.
    /// </summary>
    public static RemoteData<T, TErr> Failure(TErr error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error), "Cannot create Failure with null error.");
        
        return new RemoteData<T, TErr>(default!, error, RemoteDataState.Failure);
    }

    /// <summary>
    /// Returns the data if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Success state</exception>
    public T Unwrap()
    {
        return _state switch
        {
            RemoteDataState.Success => _data!,
            RemoteDataState.NotAsked => throw new InvalidOperationException("Called Unwrap on NotAsked"),
            RemoteDataState.Loading => throw new InvalidOperationException("Called Unwrap on Loading"),
            RemoteDataState.Failure => throw new InvalidOperationException($"Called Unwrap on Failure: {_error}"),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Returns the error if failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if not in Failure state</exception>
    public TErr UnwrapError()
    {
        if (_state != RemoteDataState.Failure)
            throw new InvalidOperationException("Called UnwrapError on non-Failure state");
        
        return _error!;
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns a default value.
    /// </summary>
    public T UnwrapOr(T defaultValue)
    {
        return _state == RemoteDataState.Success ? _data! : defaultValue;
    }

    /// <summary>
    /// Returns the data if successful, otherwise computes a default value.
    /// </summary>
    public T UnwrapOrElse(Func<T> defaultFunc)
    {
        if (defaultFunc is null)
            throw new ArgumentNullException(nameof(defaultFunc));
        
        return _state == RemoteDataState.Success ? _data! : defaultFunc();
    }

    /// <summary>
    /// Maps the data if successful.
    /// </summary>
    public RemoteData<U, TErr> Map<U>(Func<T, U> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
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
    public RemoteData<T, F> MapError<F>(Func<TErr, F> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));
        
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
    /// </summary>
    public RemoteData<U, TErr> AndThen<U>(Func<T, RemoteData<U, TErr>> binder)
    {
        if (binder is null)
            throw new ArgumentNullException(nameof(binder));
        
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
    /// Returns this RemoteData if Success, otherwise returns the alternative.
    /// </summary>
    public RemoteData<T, TErr> Or(RemoteData<T, TErr> alternative)
    {
        return _state == RemoteDataState.Success ? this : alternative;
    }

    /// <summary>
    /// Recovers from a Failure state by providing an alternative RemoteData.
    /// </summary>
    public RemoteData<T, TErr> OrElse(Func<TErr, RemoteData<T, TErr>> recovery)
    {
        if (recovery is null)
            throw new ArgumentNullException(nameof(recovery));
        
        return _state == RemoteDataState.Failure ? recovery(_error!) : this;
    }

    /// <summary>
    /// Pattern matches on all four states.
    /// </summary>
    public void Match(
        Action notAskedAction,
        Action loadingAction,
        Action<T> successAction,
        Action<TErr> failureAction)
    {
        if (notAskedAction is null)
            throw new ArgumentNullException(nameof(notAskedAction));
        if (loadingAction is null)
            throw new ArgumentNullException(nameof(loadingAction));
        if (successAction is null)
            throw new ArgumentNullException(nameof(successAction));
        if (failureAction is null)
            throw new ArgumentNullException(nameof(failureAction));

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
    public U Match<U>(
        Func<U> notAskedFunc,
        Func<U> loadingFunc,
        Func<T, U> successFunc,
        Func<TErr, U> failureFunc)
    {
        if (notAskedFunc is null)
            throw new ArgumentNullException(nameof(notAskedFunc));
        if (loadingFunc is null)
            throw new ArgumentNullException(nameof(loadingFunc));
        if (successFunc is null)
            throw new ArgumentNullException(nameof(successFunc));
        if (failureFunc is null)
            throw new ArgumentNullException(nameof(failureFunc));

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
    public Result<T, TErr> ToResult()
    {
        return _state switch
        {
            RemoteDataState.Success => Result<T, TErr>.Ok(_data!),
            RemoteDataState.Failure => Result<T, TErr>.Err(_error!),
            RemoteDataState.NotAsked => throw new InvalidOperationException("Cannot convert NotAsked to Result"),
            RemoteDataState.Loading => throw new InvalidOperationException("Cannot convert Loading to Result"),
            _ => throw new InvalidOperationException("Invalid state")
        };
    }

    /// <summary>
    /// Converts this RemoteData to a Result with default errors for NotAsked/Loading states.
    /// </summary>
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
    public override bool Equals(object? obj)
    {
        return obj is RemoteData<T, TErr> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _state switch
        {
            RemoteDataState.Success => HashCode.Combine(_state, _data),
            RemoteDataState.Failure => HashCode.Combine(_state, _error),
            _ => _state.GetHashCode()
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
    public static bool operator ==(RemoteData<T, TErr> left, RemoteData<T, TErr> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two RemoteData instances are not equal.
    /// </summary>
    public static bool operator !=(RemoteData<T, TErr> left, RemoteData<T, TErr> right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Extension methods for RemoteData&lt;T, E&gt;.
/// </summary>
public static class RemoteDataExtensions
{
    /// <summary>
    /// Executes an action if the data is in Success state, allowing method chaining.
    /// </summary>
    public static RemoteData<T, TErr> Tap<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (remoteData.IsSuccess)
            action(remoteData.Unwrap());
        
        return remoteData;
    }

    /// <summary>
    /// Executes an action if the data is in Failure state, allowing method chaining.
    /// </summary>
    public static RemoteData<T, TErr> TapError<T, TErr>(
        this RemoteData<T, TErr> remoteData,
        Action<TErr> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (remoteData.IsFailure)
            action(remoteData.UnwrapError());
        
        return remoteData;
    }

    /// <summary>
    /// Converts a Result to RemoteData in Success or Failure state.
    /// </summary>
    public static RemoteData<T, TErr> ToRemoteData<T, TErr>(this Result<T, TErr> result)
    {
        return result.Match(
            okFunc: data => RemoteData<T, TErr>.Success(data),
            errFunc: err => RemoteData<T, TErr>.Failure(err)
        );
    }

    /// <summary>
    /// Wraps an async operation in RemoteData, starting with Loading and ending with Success/Failure.
    /// </summary>
    public static async Task<RemoteData<T, Exception>> FromTaskAsync<T>(Func<Task<T>> taskFunc)
    {
        if (taskFunc is null)
            throw new ArgumentNullException(nameof(taskFunc));

        try
        {
            var result = await taskFunc();
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
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        if (!remoteData.IsSuccess)
            return remoteData.Map(_ => default(U)!); // Preserves state

        var result = await mapper(remoteData.Unwrap());
        return RemoteData<U, TErr>.Success(result);
    }

    /// <summary>
    /// Returns true if the data is loaded (either Success or Failure).
    /// </summary>
    public static bool IsLoaded<T, TErr>(this RemoteData<T, TErr> remoteData)
    {
        return remoteData.IsSuccess || remoteData.IsFailure;
    }

    /// <summary>
    /// Returns true if the data is not loaded (either NotAsked or Loading).
    /// </summary>
    public static bool IsNotLoaded<T, TErr>(this RemoteData<T, TErr> remoteData)
    {
        return remoteData.IsNotAsked || remoteData.IsLoading;
    }
}

