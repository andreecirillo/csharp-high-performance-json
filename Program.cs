using System.Diagnostics;
using System.Text;
using System.Text.Json;

/* JSON */

// C# 11, Raw String Literals
string json = """
[
    {"name": "  Alice  ", "score": "295"}, 
    {"name": "Bob", "score": "58"},
    {"name": "Charlie", "score": "72"},
    {"name": "Daisy", "score": "88   "},
    {"name": "Eve", "score": "null"},
    {"name": "Frank", "score": "30"},
    {"name": "Grace", "score": "-81"},
    {"name": "Hank", "score": "a90"},
    {"name": "Jack", "score": "0"},
    {"name": "", "score": "1"}
]
""";

/* StandardProcess */
Console.WriteLine("StandardProcess");
var users_StandardProcessed = UserProcessor.StandardProcess(json).ToList();
users_StandardProcessed.ForEach(u =>
    Console.WriteLine($"User: {u.Name}, Score: {u.Score}")
);
int averageScore_standardProcessed = users_StandardProcessed.Sum(u => u.Score) / users_StandardProcessed.Count();
Console.WriteLine($"Average Score: {averageScore_standardProcessed}");
Console.WriteLine();

/* OptmizedProcess */
Console.WriteLine("OptmizedProcess");
var users_optmizedProcessed = UserProcessor.OptmizedProcess(json).ToList();
users_optmizedProcessed.ForEach(u =>
    Console.WriteLine($"User: {u.Name}, Score: {u.Score}")
);
int averageScore_optmizedProcessed = users_optmizedProcessed.Sum(u => u.Score) / users_optmizedProcessed.Count();
Console.WriteLine($"Average Score: {averageScore_optmizedProcessed}");
Console.WriteLine();

/* HighPerformanceProcess */
Console.WriteLine("HighPerformanceProcess");
var users_highPerformanceProcessed = UserProcessor.HighPerformanceProcess(json).ToList();
users_highPerformanceProcessed.ForEach(u =>
    Console.WriteLine($"User: {u.Name}, Score: {u.Score}")
);
int averageScore_highPerformanceProcessed = users_highPerformanceProcessed.Sum(u => u.Score) / users_highPerformanceProcessed.Count();
Console.WriteLine($"Average Score: {averageScore_highPerformanceProcessed}");
Console.WriteLine();

/* Benchmark */

// C# 7, Digit Separator
const int ITEMS = 10_000_000;

Console.WriteLine("Benchmark");
Console.WriteLine($"Generating JSON with {ITEMS} items...");
Console.WriteLine();

string big_json_input = GenerateBigJson(ITEMS);

BenchmarkProcess("Standard Process (LINQ)", () => UserProcessor.StandardProcess(big_json_input));
BenchmarkProcess("Optimized Process (Local Function)", () => UserProcessor.OptmizedProcess(big_json_input));
BenchmarkProcess("High Performance Process (Yield + Span)", () => UserProcessor.HighPerformanceProcess(big_json_input));

#region Definitions

static void BenchmarkProcess(string label, Func<IEnumerable<UserRecord>> processMethod)
{    
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    long memoryBefore = GC.GetTotalMemory(true);
    Stopwatch sw = Stopwatch.StartNew();
        
    var records = processMethod().Count();

    sw.Stop();
    long memoryAfter = GC.GetTotalMemory(false);
        
    long memoryUsed = memoryAfter - memoryBefore;
    Console.WriteLine($"--- {label} ---");
    Console.WriteLine($"Execution Time: {sw.Elapsed.TotalMilliseconds:F4} ms");
    Console.WriteLine($"Allocated memory: {memoryUsed / 1024.0:F2} KB");
    Console.WriteLine($"Processed records: {records}");
    Console.WriteLine(new string('-', 30));
}

static string GenerateBigJson(int count)
{
    var sb = new StringBuilder();
    sb.Append("[");
        
    string[] names = { "  Alice  ", "Bob", "Charlie", "Daisy  ", "", "Eve", "Frank", "Grace", "Hank", "Jack" };
    string[] scores = { "295", "58", "72", "88   ", "null", "30", "-81", "a90", "0", "1" };

    var rand = new Random();

    for (int i = 0; i < count; i++)
    {        
        string name = names[rand.Next(names.Length)];
        string score = scores[rand.Next(scores.Length)];

        sb.Append($$"""{"name": "{{name}}", "score": "{{score}}"}""");

        if (i < count - 1) sb.Append(",");
    }

    sb.Append("]");
    return sb.ToString();
}

// C# 9, Record type (Immutable: Read-only, Value-based Equality, Heap), init (private set), Positional Records (One line init)
public record UserRecord(string Name, int Score);

public class User
{
    public string name { get; set; } = string.Empty;
    public string score { get; set; } = string.Empty;
}

public static class UserProcessor
{
    public static IEnumerable<UserRecord> StandardProcess(string json)
    {
        // C# 2, Null-Coalescing, "??"
        // C# 9, Target-typed new, "new()"
        // C# 12, Collection Expressions, "[]"
        var users = JsonSerializer.Deserialize<List<User>>(json) ?? [];

        return users.
            Where(u => !string.IsNullOrEmpty(u.name?.Trim()) && !string.IsNullOrEmpty(u.score?.Trim()))
            .Select(r => new
            {
                Name = r.name.Trim(),
                IsValid = int.TryParse(r.score.Trim(), out int val),
                Score = val
            })
            .Where(x => x.IsValid && x.Score >= 0 && x.Score <= 100)
            .Select(x => new UserRecord(x.Name, x.Score));
    }

    public static IEnumerable<UserRecord> OptmizedProcess(string json)
    {
        var users = JsonSerializer.Deserialize<List<User>>(json) ?? [];

        static bool IsUserValid(User u)
        {
            var nameSpan = u.name.AsSpan().Trim();
            var scoreSpan = u.score.AsSpan().Trim();

            if (nameSpan.IsEmpty || scoreSpan.IsEmpty) return false;

            return int.TryParse(scoreSpan, out int val) && (val >= 0 && val <= 100);
        }

        return users.
            Where(IsUserValid)
            .Select(ru => new UserRecord(ru.name, int.Parse(ru.score)));        
    }

    public static IEnumerable<UserRecord> HighPerformanceProcess(string json)
    {
        var users = JsonSerializer.Deserialize<List<User>>(json) ?? [];

        foreach (var u in users)
        {
            var nameSpan = u.name.AsSpan().Trim();
            var scoreSpan = u.score.AsSpan().Trim();

            if (nameSpan.IsEmpty || scoreSpan.IsEmpty) continue;

            if(int.TryParse(scoreSpan, out int score))
            {
                if(score >= 0 && score <= 100)
                {
                    yield return new UserRecord(nameSpan.ToString(), score);
                }
            }
        }
    }
}

#endregion


