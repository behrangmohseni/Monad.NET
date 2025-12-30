namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating NonEmptyList&lt;T&gt; for lists guaranteed to have at least one element.
/// Head access is always safe - no more empty collection checks!
/// </summary>
public static class NonEmptyListExamples
{
    public static void Run()
    {
        Console.WriteLine("NonEmptyList<T> guarantees at least one element - no more empty checks!\n");

        // Creating NonEmptyList
        Console.WriteLine("1. Creating NonEmptyList:");
        var single = NonEmptyList<int>.Of(42);
        var multiple = NonEmptyList<int>.Of(1, 2, 3, 4, 5);
        Console.WriteLine($"   Single:   {single}");
        Console.WriteLine($"   Multiple: {multiple}");

        // Safe head access
        Console.WriteLine("\n2. Safe Head Access (always works!):");
        Console.WriteLine($"   single.Head:   {single.Head}");
        Console.WriteLine($"   multiple.Head: {multiple.Head}");
        Console.WriteLine($"   multiple.Tail: [{string.Join(", ", multiple.Tail)}]");
        Console.WriteLine($"   multiple.Last(): {multiple.Last()}");

        // FromEnumerable returns Option
        Console.WriteLine("\n3. FromEnumerable (returns Option):");
        var fromList = NonEmptyList<int>.FromEnumerable(new[] { 10, 20, 30 });
        var fromEmpty = NonEmptyList<int>.FromEnumerable(Array.Empty<int>());
        Console.WriteLine($"   From [10,20,30]: {fromList}");
        Console.WriteLine($"   From []:         {fromEmpty}");

        // Properties
        Console.WriteLine("\n4. Properties:");
        Console.WriteLine($"   multiple.Count: {multiple.Count}");
        Console.WriteLine($"   multiple[2]:    {multiple[2]}");

        // Map
        Console.WriteLine("\n5. Map:");
        var doubled = multiple.Map(x => x * 2);
        Console.WriteLine($"   Doubled: {doubled}");

        // FlatMap
        Console.WriteLine("\n6. FlatMap:");
        var expanded = NonEmptyList<int>.Of(1, 2)
            .FlatMap(x => NonEmptyList<int>.Of(x, x * 10));
        Console.WriteLine($"   Expanded: {expanded}");

        // Reduce (no initial value needed!)
        Console.WriteLine("\n7. Reduce (safe - always has elements!):");
        var sum = multiple.Reduce((a, b) => a + b);
        var product = multiple.Reduce((a, b) => a * b);
        var max = multiple.Reduce((a, b) => Math.Max(a, b));
        Console.WriteLine($"   Sum:     {sum}");
        Console.WriteLine($"   Product: {product}");
        Console.WriteLine($"   Max:     {max}");

        // Append and Prepend
        Console.WriteLine("\n8. Append and Prepend:");
        var appended = NonEmptyList<int>.Of(1, 2).Append(3);
        var prepended = NonEmptyList<int>.Of(2, 3).Prepend(1);
        Console.WriteLine($"   Appended:  {appended}");
        Console.WriteLine($"   Prepended: {prepended}");

        // Concat
        Console.WriteLine("\n9. Concat:");
        var list1 = NonEmptyList<int>.Of(1, 2);
        var list2 = NonEmptyList<int>.Of(3, 4);
        var concatenated = list1.Concat(list2);
        Console.WriteLine($"   [{string.Join(", ", list1)}] ++ [{string.Join(", ", list2)}] = {concatenated}");

        // Filter returns Option<NonEmptyList>
        Console.WriteLine("\n10. Filter (returns Option - might be empty!):");
        var filtered = multiple.Filter(x => x > 2);
        var filteredAll = multiple.Filter(x => x > 100);
        Console.WriteLine($"   Filter(>2):   {filtered}");
        Console.WriteLine($"   Filter(>100): {filteredAll}");

        // Reverse
        Console.WriteLine("\n11. Reverse:");
        var reversed = multiple.Reverse();
        Console.WriteLine($"   Original: {multiple}");
        Console.WriteLine($"   Reversed: {reversed}");

        // Distinct
        Console.WriteLine("\n12. Distinct:");
        var withDupes = NonEmptyList<int>.Of(1, 2, 2, 3, 3, 3);
        var distinct = withDupes.Distinct();
        Console.WriteLine($"   Original: {withDupes}");
        Console.WriteLine($"   Distinct: {distinct}");

        // Real-world: Email recipients
        Console.WriteLine("\n13. Real-World: Email Recipients:");
        var recipients = NonEmptyList<string>.Of("admin@example.com", "user@example.com");
        SendEmail(recipients, "Important notification");

        // Real-world: Error aggregation
        Console.WriteLine("\n14. Real-World: Validation Errors:");
        var errors = NonEmptyList<string>.Of(
            "Name is required",
            "Email is invalid",
            "Age must be positive"
        );
        DisplayValidationErrors(errors);
    }

    private static void SendEmail(NonEmptyList<string> recipients, string message)
    {
        // No need to check if empty - type guarantees at least one!
        Console.WriteLine($"   Sending: \"{message}\"");
        Console.WriteLine($"   To: {recipients.Head}");
        if (recipients.Tail.Count > 0)
            Console.WriteLine($"   CC: {string.Join(", ", recipients.Tail)}");
    }

    private static void DisplayValidationErrors(NonEmptyList<string> errors)
    {
        Console.WriteLine($"   Found {errors.Count} validation error(s):");
        int i = 1;
        foreach (var error in errors)
        {
            Console.WriteLine($"     {i}. {error}");
            i++;
        }
    }
}

