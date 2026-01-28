namespace Monad.NET.Examples.Examples;

/// <summary>
/// Examples demonstrating RemoteData&lt;T, E&gt; for async data state management.
/// RemoteData tracks four states: NotAsked, Loading, Success, Failure.
/// </summary>
public static class RemoteDataExamples
{
    public static void Run()
    {
        Console.WriteLine("RemoteData<T, E> tracks async data: NotAsked, Loading, Success, Failure.\n");

        // The four states
        Console.WriteLine("1. The Four States:");
        var notAsked = RemoteData<string, string>.NotAsked();
        var loading = RemoteData<string, string>.Loading();
        var success = RemoteData<string, string>.Success("Data loaded!");
        var failure = RemoteData<string, string>.Failure("Network error");

        Console.WriteLine($"   NotAsked: {notAsked}");
        Console.WriteLine($"   Loading:  {loading}");
        Console.WriteLine($"   Success:  {success}");
        Console.WriteLine($"   Failure:  {failure}");

        // State checking
        Console.WriteLine("\n2. State Checking:");
        Console.WriteLine($"   notAsked.IsNotAsked: {notAsked.IsNotAsked}");
        Console.WriteLine($"   loading.IsLoading:   {loading.IsLoading}");
        Console.WriteLine($"   success.IsOk:   {success.IsOk}");
        Console.WriteLine($"   failure.IsError:   {failure.IsError}");

        // Pattern matching for UI rendering
        Console.WriteLine("\n3. UI Rendering Pattern:");
        RenderData("   ", notAsked);
        RenderData("   ", loading);
        RenderData("   ", success);
        RenderData("   ", failure);

        // Map only transforms Success
        Console.WriteLine("\n4. Map (only transforms Success):");
        var mappedSuccess = success.Map(s => s.ToUpper());
        var mappedLoading = loading.Map(s => s.ToUpper());
        Console.WriteLine($"   Success.Map(ToUpper): {mappedSuccess}");
        Console.WriteLine($"   Loading.Map(ToUpper): {mappedLoading}");

        // Default values with UnwrapOr
        Console.WriteLine("\n5. UnwrapOr:");
        Console.WriteLine($"   Success.GetValueOr(\"default\"): {success.GetValueOr("default")}");
        Console.WriteLine($"   Loading.GetValueOr(\"default\"): {loading.GetValueOr("default")}");
        Console.WriteLine($"   NotAsked.GetValueOr(\"default\"): {notAsked.GetValueOr("default")}");

        // Simulated API call flow
        Console.WriteLine("\n6. Simulated API Call Flow:");
        SimulateApiFlow();

        // Chaining
        Console.WriteLine("\n7. Chaining with AndThen:");
        var chained = success
            .Bind(s => RemoteData<int, string>.Success(s.Length))
            .Map(len => $"Length: {len}");
        Console.WriteLine($"   Result: {chained}");

        // Combining data
        Console.WriteLine("\n8. Combining Data:");
        var user = RemoteData<string, string>.Success("John");
        var posts = RemoteData<int, string>.Success(42);
        var combined = user.Map(u => $"{u} has {posts.GetValueOr(0)} posts");
        Console.WriteLine($"   Combined: {combined}");

        // Convert to Result
        Console.WriteLine("\n9. Convert to Result:");
        var asResult = success.ToResult("Not requested", "Still loading");
        var loadingAsResult = loading.ToResult("Not requested", "Still loading");
        Console.WriteLine($"   Success.ToResult: {asResult}");
        Console.WriteLine($"   Loading.ToResult: {loadingAsResult}");

        // Real-world: Dashboard widget
        Console.WriteLine("\n10. Dashboard Widget Pattern:");
        var widgetData = RemoteData<DashboardData, string>.Success(
            new DashboardData(Users: 1500, Revenue: 50000m, Growth: 12.5));

        var widgetOutput = widgetData.Match(
            notAskedFunc: () => "[Click to load]",
            loadingFunc: () => "[Loading...]",
            successFunc: data => $"Users: {data.Users}, Revenue: ${data.Revenue:N0}, Growth: {data.Growth}%",
            failureFunc: err => $"[Error: {err}] [Retry]"
        );
        Console.WriteLine($"   {widgetOutput}");
    }

    private static void RenderData(string prefix, RemoteData<string, string> data)
    {
        var ui = data.Match(
            notAskedFunc: () => "[Button: Load Data]",
            loadingFunc: () => "[Spinner: Loading...]",
            successFunc: d => $"[Content: {d}]",
            failureFunc: e => $"[Error: {e}] [Button: Retry]"
        );
        Console.WriteLine(prefix + ui);
    }

    private static void SimulateApiFlow()
    {
        var state = RemoteData<string, string>.NotAsked();
        Console.WriteLine($"   Initial:     {GetStateName(state)}");

        state = RemoteData<string, string>.Loading();
        Console.WriteLine($"   After click: {GetStateName(state)}");

        state = RemoteData<string, string>.Success("API Response");
        Console.WriteLine($"   After load:  {GetStateName(state)}");
    }

    private static string GetStateName<T, E>(RemoteData<T, E> data) => data.Match(
        notAskedFunc: () => "NotAsked",
        loadingFunc: () => "Loading",
        successFunc: _ => "Success",
        failureFunc: _ => "Failure"
    );

    record DashboardData(int Users, decimal Revenue, double Growth);
}

