using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monad.NET;

/// <summary>
/// Extension methods for Option&lt;T&gt;.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class OptionExtensions
{
    #region When/Unless Guards

    /// <summary>
    /// Creates an Option based on a condition. Returns Some containing the factory result if the condition is true,
    /// otherwise returns None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="factory">The factory function to create the value if condition is true.</param>
    /// <returns>Some containing the factory result if condition is true; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = OptionExtensions.When(user.IsAdmin, () => new AdminPanel());
    /// // Some(AdminPanel) if user is admin, None otherwise
    ///
    /// var discount = OptionExtensions.When(order.Total > 100, () => 0.1m);
    /// // Some(0.1m) if order total > 100, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, Func<T> factory)
    {
        ThrowHelper.ThrowIfNull(factory);
        return condition ? Option<T>.Some(factory()) : Option<T>.None();
    }

    /// <summary>
    /// Creates an Option based on a condition. Returns Some containing the value if the condition is true,
    /// otherwise returns None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to wrap if condition is true.</param>
    /// <returns>Some containing the value if condition is true; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var result = OptionExtensions.When(isEnabled, defaultConfig);
    /// // Some(defaultConfig) if isEnabled, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> When<T>(bool condition, T value)
    {
        return condition ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Creates an Option based on a negated condition. Returns Some containing the factory result if the condition is false,
    /// otherwise returns None. This is the opposite of <see cref="When{T}(bool, Func{T})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="factory">The factory function to create the value if condition is false.</param>
    /// <returns>Some containing the factory result if condition is false; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var warning = OptionExtensions.Unless(user.HasVerifiedEmail, () => "Please verify your email");
    /// // Some("Please verify...") if email NOT verified, None otherwise
    ///
    /// var fallback = OptionExtensions.Unless(cache.HasValue, () => LoadFromDatabase());
    /// // Some(dbValue) if cache is empty, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, Func<T> factory)
    {
        return When(!condition, factory);
    }

    /// <summary>
    /// Creates an Option based on a negated condition. Returns Some containing the value if the condition is false,
    /// otherwise returns None. This is the opposite of <see cref="When{T}(bool, T)"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to evaluate (negated).</param>
    /// <param name="value">The value to wrap if condition is false.</param>
    /// <returns>Some containing the value if condition is false; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var defaultValue = OptionExtensions.Unless(hasCustomValue, standardDefault);
    /// // Some(standardDefault) if NOT hasCustomValue, None otherwise
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Unless<T>(bool condition, T value)
    {
        return When(!condition, value);
    }

    #endregion

    #region DefaultIfNone

    /// <summary>
    /// Returns the Option if it contains a value, otherwise returns an Option containing the default value.
    /// Unlike UnwrapOr which extracts the value, this returns an Option containing the default.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="defaultValue">The default value to use if None.</param>
    /// <returns>The original Option if Some; otherwise Some containing the default value.</returns>
    /// <example>
    /// <code>
    /// var some = Option&lt;int&gt;.Some(42);
    /// var result = some.DefaultIfNone(0); // Some(42)
    ///
    /// var none = Option&lt;int&gt;.None();
    /// var result2 = none.DefaultIfNone(0); // Some(0)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> DefaultIfNone<T>(this Option<T> option, T defaultValue)
    {
        return option.IsSome ? option : Option<T>.Some(defaultValue);
    }

    /// <summary>
    /// Returns the Option if it contains a value, otherwise returns an Option containing the result of the factory function.
    /// The factory is only called if the Option is None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="defaultFactory">The factory function to create the default value if None.</param>
    /// <returns>The original Option if Some; otherwise Some containing the factory result.</returns>
    /// <example>
    /// <code>
    /// var some = Option&lt;Config&gt;.Some(existingConfig);
    /// var result = some.DefaultIfNone(() => new Config()); // Some(existingConfig)
    ///
    /// var none = Option&lt;Config&gt;.None();
    /// var result2 = none.DefaultIfNone(() => new Config()); // Some(new Config())
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> DefaultIfNone<T>(this Option<T> option, Func<T> defaultFactory)
    {
        ThrowHelper.ThrowIfNull(defaultFactory);
        return option.IsSome ? option : Option<T>.Some(defaultFactory());
    }

    #endregion

    #region ThrowIfNone

    /// <summary>
    /// Returns the contained value if Some, otherwise throws the specified exception.
    /// This is an alternative to Expect that allows throwing specific exception types.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="exception">The exception to throw if None.</param>
    /// <returns>The contained value if Some.</returns>
    /// <exception cref="Exception">Throws the specified exception if None.</exception>
    /// <example>
    /// <code>
    /// var some = Option&lt;User&gt;.Some(user);
    /// var value = some.ThrowIfNone(new UserNotFoundException()); // returns user
    ///
    /// var none = Option&lt;User&gt;.None();
    /// none.ThrowIfNone(new UserNotFoundException()); // throws UserNotFoundException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNone<T>(this Option<T> option, Exception exception)
    {
        ThrowHelper.ThrowIfNull(exception);

        if (option.IsNone)
            throw exception;

        return option.GetValue();
    }

    /// <summary>
    /// Returns the contained value if Some, otherwise throws an exception created by the factory.
    /// The factory is only called if the Option is None.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <param name="exceptionFactory">The factory function to create the exception if None.</param>
    /// <returns>The contained value if Some.</returns>
    /// <exception cref="Exception">Throws the exception from the factory if None.</exception>
    /// <example>
    /// <code>
    /// var result = FindUser(id).ThrowIfNone(() => new UserNotFoundException(id));
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNone<T>(this Option<T> option, Func<Exception> exceptionFactory)
    {
        ThrowHelper.ThrowIfNull(exceptionFactory);

        if (option.IsNone)
            throw exceptionFactory();

        return option.GetValue();
    }

    #endregion

    /// <summary>
    /// Converts a nullable value to an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : struct
    {
        return value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None();
    }

    /// <summary>
    /// Converts a reference type to an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> ToOption<T>(this T? value) where T : class
    {
        return value is not null ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Flattens a nested Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
    {
        return option.Bind(static inner => inner);
    }

    /// <summary>
    /// Transposes an Option of a Result into a Result of an Option.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Option<T>, TError> Transpose<T, TError>(this Option<Result<T, TError>> option)
    {
        return option.Match(
            someFunc: static result => result.Match(
                okFunc: static value => Result<Option<T>, TError>.Ok(Option<T>.Some(value)),
                errFunc: static err => Result<Option<T>, TError>.Error(err)
            ),
            noneFunc: static () => Result<Option<T>, TError>.Ok(Option<T>.None())
        );
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified type.
    /// Returns Some if the Option is Some and the value is of type TTarget; otherwise None.
    /// This is the Option equivalent of LINQ's OfType for single values.
    /// </summary>
    /// <typeparam name="TSource">The source type of the Option.</typeparam>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some("hello");
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;object, string&gt;(); // Some("hello")
    /// Option&lt;int&gt; intOption = objOption.OfType&lt;object, int&gt;();       // None
    ///
    /// // With base/derived types
    /// Option&lt;Animal&gt; animal = Option&lt;Animal&gt;.Some(new Dog());
    /// Option&lt;Dog&gt; dog = animal.OfType&lt;Animal, Dog&gt;();               // Some(Dog)
    /// Option&lt;Cat&gt; cat = animal.OfType&lt;Animal, Cat&gt;();               // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfType<TSource, TTarget>(this Option<TSource> option)
        where TTarget : class
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified value type.
    /// Returns Some if the Option is Some and the value is of type TTarget; otherwise None.
    /// </summary>
    /// <typeparam name="TSource">The source type of the Option.</typeparam>
    /// <typeparam name="TTarget">The target value type to cast to.</typeparam>
    /// <param name="option">The source Option.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some(42);
    /// Option&lt;int&gt; intOption = objOption.OfTypeValue&lt;object, int&gt;(); // Some(42)
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;object, string&gt;(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfTypeValue<TSource, TTarget>(this Option<TSource> option)
        where TTarget : struct
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    /// <summary>
    /// Attempts to cast the contained value to the specified type using a type parameter.
    /// Returns Some if the Option is Some and the value can be cast to TTarget; otherwise None.
    /// Works with both reference types and value types.
    /// </summary>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="option">The source Option containing an object.</param>
    /// <returns>Some containing the cast value if successful; otherwise None.</returns>
    /// <example>
    /// <code>
    /// Option&lt;object&gt; objOption = Option&lt;object&gt;.Some("hello");
    /// Option&lt;string&gt; strOption = objOption.OfType&lt;string&gt;(); // Some("hello")
    ///
    /// Option&lt;object&gt; numOption = Option&lt;object&gt;.Some(42);
    /// Option&lt;int&gt; intOption = numOption.OfType&lt;int&gt;();        // Some(42)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TTarget> OfType<TTarget>(this Option<object> option)
    {
        return option.Bind(value => value is TTarget target
            ? Option<TTarget>.Some(target)
            : Option<TTarget>.None());
    }

    #region String Conversions

    /// <summary>
    /// Converts a string to an Option, returning None if the string is null or empty.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the string if not null or empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "hello".ToOptionNotEmpty();  // Some("hello")
    /// "".ToOptionNotEmpty();       // None
    /// ((string?)null).ToOptionNotEmpty(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionNotEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value)
            ? Option<string>.None()
            : Option<string>.Some(value);
    }

    /// <summary>
    /// Converts a string to an Option, returning None if the string is null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the string if not null, empty, or whitespace; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "hello".ToOptionNotWhiteSpace();  // Some("hello")
    /// "   ".ToOptionNotWhiteSpace();    // None
    /// "".ToOptionNotWhiteSpace();       // None
    /// ((string?)null).ToOptionNotWhiteSpace(); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionNotWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Option<string>.None()
            : Option<string>.Some(value);
    }

    /// <summary>
    /// Converts a string to an Option with the string trimmed, returning None if the result is empty.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>Some containing the trimmed string if not empty after trimming; otherwise None.</returns>
    /// <example>
    /// <code>
    /// "  hello  ".ToOptionTrimmed();  // Some("hello")
    /// "   ".ToOptionTrimmed();        // None
    /// "".ToOptionTrimmed();           // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<string> ToOptionTrimmed(this string? value)
    {
        if (value is null)
            return Option<string>.None();

        var trimmed = value.Trim();
        return trimmed.Length == 0
            ? Option<string>.None()
            : Option<string>.Some(trimmed);
    }

    #endregion

    #region Dictionary/Collection Lookups

    /// <summary>
    /// Attempts to get a value from a dictionary by key.
    /// Returns Some if the key exists; otherwise None.
    /// Works with Dictionary, ImmutableDictionary, and other dictionary types.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some containing the value if the key exists; otherwise None.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2 };
    /// dict.GetOption("a"); // Some(1)
    /// dict.GetOption("c"); // None
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<TValue> GetOption<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key)
    {
        ThrowHelper.ThrowIfNull(dictionary);

        return dictionary.TryGetValue(key, out var value)
            ? Option<TValue>.Some(value!)
            : Option<TValue>.None();
    }

    /// <summary>
    /// Returns the first element of a sequence, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the first element from.</param>
    /// <returns>Some containing the first element if the sequence is not empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.FirstOption();      // Some(1)
    /// Array.Empty&lt;int&gt;().FirstOption();    // None
    /// </code>
    /// </example>
    public static Option<T> FirstOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count > 0
                ? Option<T>.Some(list[0])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext()
            ? Option<T>.Some(enumerator.Current)
            : Option<T>.None();
    }

    /// <summary>
    /// Returns the first element of a sequence that matches the predicate, or None if no match is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to search.</param>
    /// <param name="predicate">The condition to match.</param>
    /// <returns>Some containing the first matching element; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.FirstOption(x => x > 1);  // Some(2)
    /// new[] { 1, 2, 3 }.FirstOption(x => x > 10); // None
    /// </code>
    /// </example>
    public static Option<T> FirstOption<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ThrowHelper.ThrowIfNull(source);
        ThrowHelper.ThrowIfNull(predicate);

        foreach (var item in source)
        {
            if (predicate(item))
                return Option<T>.Some(item);
        }

        return Option<T>.None();
    }

    /// <summary>
    /// Returns the last element of a sequence, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the last element from.</param>
    /// <returns>Some containing the last element if the sequence is not empty; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.LastOption();      // Some(3)
    /// Array.Empty&lt;int&gt;().LastOption();    // None
    /// </code>
    /// </example>
    public static Option<T> LastOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count > 0
                ? Option<T>.Some(list[list.Count - 1])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return Option<T>.None();

        var last = enumerator.Current;
        while (enumerator.MoveNext())
        {
            last = enumerator.Current;
        }

        return Option<T>.Some(last);
    }

    /// <summary>
    /// Returns the single element of a sequence, or None if the sequence is empty or has more than one element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to get the single element from.</param>
    /// <returns>Some containing the element if exactly one exists; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 42 }.SingleOption();         // Some(42)
    /// new[] { 1, 2 }.SingleOption();       // None (more than one)
    /// Array.Empty&lt;int&gt;().SingleOption();  // None (empty)
    /// </code>
    /// </example>
    public static Option<T> SingleOption<T>(this IEnumerable<T> source)
    {
        ThrowHelper.ThrowIfNull(source);

        if (source is IList<T> list)
        {
            return list.Count == 1
                ? Option<T>.Some(list[0])
                : Option<T>.None();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return Option<T>.None();

        var single = enumerator.Current;
        if (enumerator.MoveNext())
            return Option<T>.None(); // More than one element

        return Option<T>.Some(single);
    }

    /// <summary>
    /// Returns the element at the specified index, or None if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to index into.</param>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>Some containing the element at the index; otherwise None.</returns>
    /// <example>
    /// <code>
    /// new[] { 1, 2, 3 }.ElementAtOption(1);  // Some(2)
    /// new[] { 1, 2, 3 }.ElementAtOption(10); // None
    /// new[] { 1, 2, 3 }.ElementAtOption(-1); // None
    /// </code>
    /// </example>
    public static Option<T> ElementAtOption<T>(this IEnumerable<T> source, int index)
    {
        ThrowHelper.ThrowIfNull(source);

        if (index < 0)
            return Option<T>.None();

        if (source is IList<T> list)
        {
            return index < list.Count
                ? Option<T>.Some(list[index])
                : Option<T>.None();
        }

        var currentIndex = 0;
        foreach (var item in source)
        {
            if (currentIndex == index)
                return Option<T>.Some(item);
            currentIndex++;
        }

        return Option<T>.None();
    }

    #endregion
}
