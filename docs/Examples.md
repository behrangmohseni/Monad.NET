# Real-World Examples

Practical examples showing how to use Monad.NET in production scenarios.

## Table of Contents

- [API Response Handling](#api-response-handling)
- [Form Validation Pipeline](#form-validation-pipeline)
- [Async Configuration with ReaderAsync](#async-configuration-with-readerasync)
- [Blazor Component with RemoteData](#blazor-component-with-remotedata)
- [Data Pipeline with Try](#data-pipeline-with-try)
- [NonEmptyList for Business Rules](#nonemptylist-for-business-rules)
- [Parallel Batch Processing](#parallel-batch-processing)

---

## API Response Handling

```csharp
public async Task<Result<UserDto, ApiError>> GetUserProfileAsync(int userId)
{
    // Chain multiple API calls with automatic error propagation
    return await _httpClient.GetUserAsync(userId)
        .AndThenAsync(user => _httpClient.GetUserPreferencesAsync(user.Id))
        .MapAsync(prefs => new UserDto(user, prefs))
        .TapAsync(dto => _cache.SetAsync($"user:{userId}", dto))
        .TapErrAsync(err => _logger.LogError("Failed to get user {Id}: {Error}", userId, err));
}

// Usage in controller
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    return await GetUserProfileAsync(id).Match(
        ok: user => Ok(user),
        err: error => error.Code switch
        {
            "NOT_FOUND" => NotFound(),
            "UNAUTHORIZED" => Unauthorized(),
            _ => StatusCode(500, error.Message)
        }
    );
}
```

---

## Form Validation Pipeline

```csharp
public record CreateUserRequest(string Name, string Email, int Age);

public Validation<User, ValidationError> ValidateCreateUser(CreateUserRequest request)
{
    return ValidateName(request.Name)
        .Apply(ValidateEmail(request.Email), (name, email) => (name, email))
        .Apply(ValidateAge(request.Age), (partial, age) => 
            new User(partial.name, partial.email, age));
}

Validation<string, ValidationError> ValidateName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Name", "Name is required"));
    
    if (name.Length < 2)
        return Validation<string, ValidationError>.Invalid(
            new ValidationError("Name", "Name must be at least 2 characters"));
    
    return Validation<string, ValidationError>.Valid(name);
}

// Returns ALL validation errors at once
var result = ValidateCreateUser(request);
result.Match(
    valid: user => SaveUser(user),
    invalid: errors => BadRequest(new { Errors = errors })
);
```

---

## Async Configuration with ReaderAsync

```csharp
public record AppConfig(string ConnectionString, string ApiKey, int MaxRetries);

// Build composable async configuration-dependent operations
var getUsers = ReaderAsync<AppConfig, List<User>>.From(async config =>
{
    using var conn = new SqlConnection(config.ConnectionString);
    return await conn.QueryAsync<User>("SELECT * FROM Users").ToListAsync();
});

var enrichWithApi = ReaderAsync<AppConfig, Func<User, Task<UserWithDetails>>>.From(config =>
    async user =>
    {
        var client = new ApiClient(config.ApiKey);
        var details = await client.GetDetailsAsync(user.Id);
        return new UserWithDetails(user, details);
    });

// Compose and run with parallel execution
var workflow = 
    from users in getUsers
    from enricher in enrichWithApi
    from enrichedUsers in ReaderAsync<AppConfig, List<UserWithDetails>>.From(async config =>
        await Task.WhenAll(users.Select(enricher)))
    select enrichedUsers.ToList();

var config = new AppConfig("Server=...", "api-key-123", 3);
var enrichedUsers = await workflow.RunAsync(config);
```

---

## Blazor Component with RemoteData

```csharp
@page "/users/{Id:int}"

<div class="user-profile">
    @_userData.Match(
        notAsked: () => @<button @onclick="LoadUser">Load Profile</button>,
        loading: () => @<div class="skeleton-loader">
            <div class="skeleton-avatar"></div>
            <div class="skeleton-text"></div>
        </div>,
        success: user => @<article class="profile-card">
            <img src="@user.AvatarUrl" alt="@user.Name" />
            <h2>@user.Name</h2>
            <p>@user.Email</p>
            <span class="badge">@user.Role</span>
        </article>,
        failure: error => @<div class="error-state">
            <p>@error.Message</p>
            <button @onclick="LoadUser">Retry</button>
        </div>
    )
</div>

@code {
    [Parameter] public int Id { get; set; }
    
    private RemoteData<User, ApiError> _userData = RemoteData<User, ApiError>.NotAsked();
    
    private async Task LoadUser()
    {
        _userData = RemoteData<User, ApiError>.Loading();
        StateHasChanged();
        
        try
        {
            var user = await _userService.GetUserAsync(Id);
            _userData = RemoteData<User, ApiError>.Success(user);
        }
        catch (ApiException ex)
        {
            _userData = RemoteData<User, ApiError>.Failure(ex.Error);
        }
        
        StateHasChanged();
    }
}
```

---

## Data Pipeline with Try

```csharp
public Try<ProcessedData> ProcessDataPipeline(string rawInput)
{
    return Try<string>.Of(() => ValidateInput(rawInput))
        .FlatMap(input => Try<ParsedData>.Of(() => JsonSerializer.Deserialize<ParsedData>(input)!))
        .FlatMap(parsed => Try<EnrichedData>.Of(() => EnrichWithExternalData(parsed)))
        .FlatMap(enriched => Try<ProcessedData>.Of(() => ApplyBusinessRules(enriched)))
        .Recover(ex => ex switch
        {
            JsonException => new ProcessedData { Error = "Invalid JSON format" },
            ValidationException ve => new ProcessedData { Error = ve.Message },
            _ => throw ex  // Re-throw unexpected exceptions
        });
}

// Usage
var result = ProcessDataPipeline(userInput);
result.Match(
    success: data => Console.WriteLine($"Processed: {data}"),
    failure: ex => Console.WriteLine($"Pipeline failed: {ex.Message}")
);
```

---

## NonEmptyList for Business Rules

```csharp
// Ensure at least one admin exists
public Result<NonEmptyList<User>, BusinessError> GetSystemAdmins()
{
    var admins = _userRepository.GetAll()
        .Where(u => u.Role == Role.Admin)
        .ToList();
    
    return NonEmptyList<User>.FromEnumerable(admins)
        .OkOr(BusinessError.NoAdminsConfigured);
}

// Safe aggregation without null checks
public decimal CalculateAverageOrderValue(NonEmptyList<Order> orders)
{
    // Reduce is always safe — list is guaranteed non-empty
    var total = orders.Reduce((acc, order) => 
        new Order { Total = acc.Total + order.Total }).Total;
    
    return total / orders.Count;
}
```

---

## Parallel Batch Processing

```csharp
// Process orders in parallel with controlled concurrency
public async Task<(List<Order> Successes, List<OrderError> Failures)> ProcessOrders(
    IEnumerable<OrderRequest> requests)
{
    var (successes, failures) = await requests.PartitionParallelAsync(
        async request => await ProcessOrderAsync(request),
        maxDegreeOfParallelism: 8
    );
    
    return (successes.ToList(), failures.ToList());
}

// Fetch all users in parallel, fail fast if any not found
public async Task<Option<IReadOnlyList<User>>> GetAllUsers(IEnumerable<int> userIds)
{
    return await userIds.TraverseParallelAsync(
        id => FindUserAsync(id),
        maxDegreeOfParallelism: 4
    );
}
```

---

[← Advanced Usage](AdvancedUsage.md) | [Integrations →](Integrations.md) | [Back to README](../README.md)

