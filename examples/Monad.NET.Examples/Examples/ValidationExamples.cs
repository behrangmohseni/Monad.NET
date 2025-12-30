using Monad.NET.Examples.Models;

namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating Validation&lt;T, E&gt; for accumulating errors.
/// Unlike Result which short-circuits on the first error, Validation collects ALL errors.
/// </summary>
public static class ValidationExamples
{
    public static void Run()
    {
        Console.WriteLine("Validation<T, E> accumulates ALL errors instead of stopping at the first.\n");
        
        // Creating Validations
        Console.WriteLine("1. Creating Validations:");
        var valid = Validation<int, string>.Valid(42);
        var invalid = Validation<int, string>.Invalid("Value is required");
        var multiError = Validation<int, string>.Invalid(new[] { "Error 1", "Error 2" });
        Console.WriteLine($"   Valid(42):       {valid}");
        Console.WriteLine($"   Invalid(single): {invalid}");
        Console.WriteLine($"   Invalid(multi):  {multiError}");

        // Individual field validations
        Console.WriteLine("\n2. Field Validations:");
        var nameResult = ValidateName(""); // Invalid
        var emailResult = ValidateEmail("bad-email"); // Invalid
        var ageResult = ValidateAge(-5); // Invalid
        Console.WriteLine($"   Name:  {nameResult}");
        Console.WriteLine($"   Email: {emailResult}");
        Console.WriteLine($"   Age:   {ageResult}");

        // Combining with Apply - accumulates ALL errors
        Console.WriteLine("\n3. Combining with Apply (accumulates errors):");
        var userValidation = ValidateName("")
            .Apply(ValidateEmail("invalid"), (n, e) => (Name: n, Email: e))
            .Apply(ValidateAge(-5), (partial, age) => new UserDto(partial.Name, partial.Email, age));
        
        userValidation.Match(
            validAction: user => Console.WriteLine($"   Valid: {user}"),
            invalidAction: errors => Console.WriteLine($"   Errors: {string.Join("; ", errors)}")
        );

        // Valid case
        Console.WriteLine("\n4. All Valid Case:");
        var validUser = ValidateName("John")
            .Apply(ValidateEmail("john@example.com"), (n, e) => (Name: n, Email: e))
            .Apply(ValidateAge(25), (partial, age) => new UserDto(partial.Name, partial.Email, age));
        Console.WriteLine($"   Result: {validUser}");

        // Using Zip for combining
        Console.WriteLine("\n5. Using Zip:");
        var zipped = ValidateName("Jane").Zip(ValidateEmail("jane@test.com"));
        Console.WriteLine($"   Zipped: {zipped}");

        // Map transforms valid value
        Console.WriteLine("\n6. Map (transforms Valid):");
        var mapped = valid.Map(x => x * 2);
        Console.WriteLine($"   Valid(42).Map(x * 2): {mapped}");

        // Converting to Result
        Console.WriteLine("\n7. Convert to Result:");
        var asResult = valid.ToResult();
        var invalidAsResult = invalid.ToResult();
        Console.WriteLine($"   Valid.ToResult():   {asResult}");
        Console.WriteLine($"   Invalid.ToResult(): {invalidAsResult}");

        // LINQ warning: short-circuits!
        Console.WriteLine("\n8. LINQ Query (Warning: Short-Circuits!):");
        Console.WriteLine("   Note: LINQ stops at FIRST error, use Apply/Zip instead!");
        var linqResult = from name in ValidateName("")
                         from email in ValidateEmail("invalid")
                         from age in ValidateAge(-1)
                         select new UserDto(name, email, age);
        Console.WriteLine($"   LINQ result: {linqResult}"); // Only shows first error

        // Real-world: Form validation
        Console.WriteLine("\n9. Real-World: Form Validation:");
        ValidateForm("", "invalid-email", -5);
        ValidateForm("John Doe", "john@example.com", 25);
    }

    private static Validation<string, string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Validation<string, string>.Invalid("Name is required");
        if (name.Length < 2)
            return Validation<string, string>.Invalid("Name must be at least 2 characters");
        return Validation<string, string>.Valid(name);
    }

    private static Validation<string, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Validation<string, string>.Invalid("Email is required");
        if (!email.Contains('@'))
            return Validation<string, string>.Invalid("Email must contain @");
        return Validation<string, string>.Valid(email);
    }

    private static Validation<int, string> ValidateAge(int age)
    {
        if (age < 0)
            return Validation<int, string>.Invalid("Age must be non-negative");
        if (age < 18)
            return Validation<int, string>.Invalid("Must be 18 or older");
        return Validation<int, string>.Valid(age);
    }

    private static void ValidateForm(string name, string email, int age)
    {
        var result = ValidateName(name)
            .Apply(ValidateEmail(email), (n, e) => (n, e))
            .Apply(ValidateAge(age), (x, a) => new UserDto(x.n, x.e, a));
        
        result.Match(
            validAction: user => Console.WriteLine($"   Form valid: {user.Name}, {user.Email}, Age {user.Age}"),
            invalidAction: errors => Console.WriteLine($"   Form errors: [{string.Join(", ", errors)}]")
        );
    }
}

