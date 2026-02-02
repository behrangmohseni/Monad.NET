using FsCheck;
using FsCheck.Xunit;
using Monad.NET;

namespace Monad.NET.Tests.PropertyBased;

/// <summary>
/// Property-based tests to verify that Monad.NET types satisfy the monad laws:
/// 1. Left Identity: return a >>= f ≡ f a
/// 2. Right Identity: m >>= return ≡ m
/// 3. Associativity: (m >>= f) >>= g ≡ m >>= (λx → f x >>= g)
/// 
/// Additionally tests functor laws and applicative laws where applicable.
/// </summary>
public class MonadLawsTests
{
    #region Option<T> Monad Laws

    [Property]
    public Property Option_LeftIdentity_Law()
    {
        // return a >>= f ≡ f a
        // Some(a).Bind(f) should equal f(a)
        Func<int, Option<string>> f = x => x >= 0
            ? Option<string>.Some(x.ToString())
            : Option<string>.None();

        return Prop.ForAll<int>(a =>
        {
            var left = Option<int>.Some(a).Bind(f);
            var right = f(a);
            return left.Equals(right);
        });
    }

    [Property]
    public Property Option_RightIdentity_Law()
    {
        // m >>= return ≡ m
        // option.Bind(Some) should equal option
        return Prop.ForAll<int>(a =>
        {
            var option = a >= 0 ? Option<int>.Some(a) : Option<int>.None();
            var result = option.Bind(x => Option<int>.Some(x));
            return result.Equals(option);
        });
    }

    [Property]
    public Property Option_Associativity_Law()
    {
        // (m >>= f) >>= g ≡ m >>= (λx → f x >>= g)
        Func<int, Option<string>> f = x => x >= 0
            ? Option<string>.Some(x.ToString())
            : Option<string>.None();
        Func<string, Option<int>> g = s => s.Length > 0
            ? Option<int>.Some(s.Length)
            : Option<int>.None();

        return Prop.ForAll<int>(a =>
        {
            var option = a >= 0 ? Option<int>.Some(a) : Option<int>.None();
            var left = option.Bind(f).Bind(g);
            var right = option.Bind(x => f(x).Bind(g));
            return left.Equals(right);
        });
    }

    #endregion

    #region Option<T> Functor Laws

    [Property]
    public Property Option_Functor_Identity()
    {
        // map id ≡ id
        // option.Map(x => x) should equal option
        return Prop.ForAll<int>(a =>
        {
            var option = a >= 0 ? Option<int>.Some(a) : Option<int>.None();
            var result = option.Map(x => x);
            return result.Equals(option);
        });
    }

    [Property]
    public Property Option_Functor_Composition()
    {
        // map (f . g) ≡ map f . map g
        Func<int, int> f = x => x * 2;
        Func<int, int> g = x => x + 1;

        return Prop.ForAll<int>(a =>
        {
            var option = a >= 0 ? Option<int>.Some(a) : Option<int>.None();
            var left = option.Map(x => f(g(x)));
            var right = option.Map(g).Map(f);
            return left.Equals(right);
        });
    }

    #endregion

    #region Result<T, E> Monad Laws

    [Property]
    public Property Result_LeftIdentity_Law()
    {
        Func<int, Result<string, string>> f = x => x >= 0
            ? Result<string, string>.Ok(x.ToString())
            : Result<string, string>.Error("negative");

        return Prop.ForAll<int>(a =>
        {
            var left = Result<int, string>.Ok(a).Bind(f);
            var right = f(a);
            return left.Equals(right);
        });
    }

    [Property]
    public Property Result_RightIdentity_Law()
    {
        return Prop.ForAll<int>(a =>
        {
            var result = a >= 0
                ? Result<int, string>.Ok(a)
                : Result<int, string>.Error("negative");
            var computed = result.Bind(x => Result<int, string>.Ok(x));
            return computed.Equals(result);
        });
    }

    [Property]
    public Property Result_Associativity_Law()
    {
        Func<int, Result<string, string>> f = x => x >= 0
            ? Result<string, string>.Ok(x.ToString())
            : Result<string, string>.Error("f failed");
        Func<string, Result<int, string>> g = s => s.Length > 0
            ? Result<int, string>.Ok(s.Length)
            : Result<int, string>.Error("g failed");

        return Prop.ForAll<int>(a =>
        {
            var result = a >= 0
                ? Result<int, string>.Ok(a)
                : Result<int, string>.Error("initial error");
            var left = result.Bind(f).Bind(g);
            var right = result.Bind(x => f(x).Bind(g));
            return left.Equals(right);
        });
    }

    #endregion

    #region Result<T, E> Functor Laws

    [Property]
    public Property Result_Functor_Identity()
    {
        return Prop.ForAll<int>(a =>
        {
            var result = a >= 0
                ? Result<int, string>.Ok(a)
                : Result<int, string>.Error("error");
            var computed = result.Map(x => x);
            return computed.Equals(result);
        });
    }

    [Property]
    public Property Result_Functor_Composition()
    {
        Func<int, int> f = x => x * 2;
        Func<int, int> g = x => x + 1;

        return Prop.ForAll<int>(a =>
        {
            var result = a >= 0
                ? Result<int, string>.Ok(a)
                : Result<int, string>.Error("error");
            var left = result.Map(x => f(g(x)));
            var right = result.Map(g).Map(f);
            return left.Equals(right);
        });
    }

    #endregion

    #region Try<T> Monad Laws

    [Property]
    public Property Try_LeftIdentity_Law()
    {
        Func<int, Try<string>> f = x => x >= 0
            ? Try<string>.Ok(x.ToString())
            : Try<string>.Error(new InvalidOperationException("negative"));

        return Prop.ForAll<int>(a =>
        {
            var left = Try<int>.Ok(a).Bind(f);
            var right = f(a);
            return left.IsOk == right.IsOk &&
                   (!left.IsOk || left.GetValue() == right.GetValue());
        });
    }

    [Property]
    public Property Try_RightIdentity_Law()
    {
        return Prop.ForAll<int>(a =>
        {
            var @try = a >= 0
                ? Try<int>.Ok(a)
                : Try<int>.Error(new InvalidOperationException("negative"));
            var result = @try.Bind(x => Try<int>.Ok(x));
            return result.IsOk == @try.IsOk &&
                   (!result.IsOk || result.GetValue() == @try.GetValue());
        });
    }

    #endregion

    #region Validation<T, E> (Applicative, not Monad - accumulates errors)

    [Property]
    public Property Validation_Functor_Identity()
    {
        return Prop.ForAll<int>(a =>
        {
            var validation = a >= 0
                ? Validation<int, string>.Ok(a)
                : Validation<int, string>.Error("error");
            var result = validation.Map(x => x);
            return result.Equals(validation);
        });
    }

    [Property]
    public Property Validation_Functor_Composition()
    {
        Func<int, int> f = x => x * 2;
        Func<int, int> g = x => x + 1;

        return Prop.ForAll<int>(a =>
        {
            var validation = a >= 0
                ? Validation<int, string>.Ok(a)
                : Validation<int, string>.Error("error");
            var left = validation.Map(x => f(g(x)));
            var right = validation.Map(g).Map(f);
            return left.Equals(right);
        });
    }

    [Property]
    public Property Validation_Apply_AccumulatesErrors()
    {
        // When both validations are invalid, errors should accumulate
        return Prop.ForAll<NonEmptyString, NonEmptyString>((err1, err2) =>
        {
            var v1 = Validation<int, string>.Error(err1.Get);
            var v2 = Validation<int, string>.Error(err2.Get);

            var result = v1.Apply(v2, (a, b) => a + b);

            return result.IsError &&
                   result.GetErrors().Length == 2 &&
                   result.GetErrors().Contains(err1.Get) &&
                   result.GetErrors().Contains(err2.Get);
        });
    }

    #endregion

    #region Option Additional Properties

    [Property]
    public Property Option_NoneIsAbsorbingElement()
    {
        // None.Bind(f) should always be None
        Func<int, Option<string>> f = x => Option<string>.Some(x.ToString());

        return Prop.ForAll<int>(_ =>
        {
            var result = Option<int>.None().Bind(f);
            return result.IsNone;
        });
    }

    [Property]
    public Property Option_MapPreservesNone()
    {
        // None.Map(f) should always be None
        return Prop.ForAll<int>(_ =>
        {
            var result = Option<int>.None().Map(x => x * 2);
            return result.IsNone;
        });
    }

    [Property]
    public Property Option_FilterWithTruePreservesValue()
    {
        return Prop.ForAll<int>(a =>
        {
            if (a < 0)
                return true; // Skip invalid inputs

            var option = Option<int>.Some(a);
            var result = option.Filter(_ => true);
            return result.Equals(option);
        });
    }

    [Property]
    public Property Option_FilterWithFalseReturnsNone()
    {
        return Prop.ForAll<int>(a =>
        {
            if (a < 0)
                return true; // Skip invalid inputs

            var option = Option<int>.Some(a);
            var result = option.Filter(_ => false);
            return result.IsNone;
        });
    }

    [Property]
    public Property Option_UnwrapOrReturnsValueWhenSome()
    {
        return Prop.ForAll<int, int>((a, defaultVal) =>
        {
            if (a < 0)
                return true; // Skip invalid inputs

            var option = Option<int>.Some(a);
            return option.GetValueOr(defaultVal) == a;
        });
    }

    [Property]
    public Property Option_UnwrapOrReturnsDefaultWhenNone()
    {
        return Prop.ForAll<int>(defaultVal =>
        {
            var option = Option<int>.None();
            return option.GetValueOr(defaultVal) == defaultVal;
        });
    }

    #endregion

    #region Result Additional Properties

    [Property]
    public Property Result_ErrIsAbsorbingElement()
    {
        // Err.Bind(f) should always be Err
        Func<int, Result<string, string>> f = x => Result<string, string>.Ok(x.ToString());

        return Prop.ForAll<NonEmptyString>(err =>
        {
            var result = Result<int, string>.Error(err.Get).Bind(f);
            return result.IsError && result.GetError() == err.Get;
        });
    }

    [Property]
    public Property Result_MapErrPreservesOk()
    {
        return Prop.ForAll<int>(a =>
        {
            if (a < 0)
                return true; // Skip invalid inputs

            var result = Result<int, string>.Ok(a);
            var mapped = result.MapError(e => e.ToUpper());
            return mapped.IsOk && mapped.GetValue() == a;
        });
    }

    #endregion

    #region Zip Properties

    [Property]
    public Property Option_Zip_BothSome()
    {
        return Prop.ForAll<int, int>((a, b) =>
        {
            if (a < 0 || b < 0)
                return true; // Skip invalid inputs

            var opt1 = Option<int>.Some(a);
            var opt2 = Option<int>.Some(b);
            var result = opt1.Zip(opt2);

            return result.IsSome && result.GetValue() == (a, b);
        });
    }

    [Property]
    public Property Option_Zip_AnyNoneReturnsNone()
    {
        return Prop.ForAll<int>(a =>
        {
            if (a < 0)
                return true; // Skip invalid inputs

            var some = Option<int>.Some(a);
            var none = Option<int>.None();

            return some.Zip(none).IsNone && none.Zip(some).IsNone;
        });
    }

    #endregion
}

