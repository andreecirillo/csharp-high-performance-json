# C# High-Performance JSON Processing

🚀 **High-performance data cleansing and transformation in .NET 8.** This project explores the evolution of a JSON processing pipeline, moving from standard declarative LINQ to a high-performance, low-allocation streaming approach using `ReadOnlySpan<char>` and `yield return`.

## 📌 The Challenge

The goal is to process a "dirty" JSON array from a legacy system. The process involves:
* **Data Cleansing:** Trimming whitespace from names.
* **Data Validation:** Ensuring scores are valid integers between *0* and *100*.
* **Resilience:** Gracefully ignoring invalid records (nulls, alphanumeric scores, etc.) without throwing exceptions.
* **Performance:** Minimizing memory allocations and CPU cycles when processing **10 million+ records**.

## 🛠 Features & Modern C# Implementation

This laboratory showcases several modern C# features:
* **C# 12:** Collection Expressions (`[]`).
* **C# 11:** Raw String Literals (`"""`).
* **C# 9:** Record types (Positional Records) and Target-typed `new()`.
* **C# 7:** Digit Separators (`10_000_000`).
* **Memory Management:** `ReadOnlySpan<char>` for zero-allocation string manipulation.
* **Streaming:** `yield return` for deferred execution (State Machine).

## 📊 Benchmark Results (10M Records)

The following results were obtained by processing 10,000,000 generated records. The goal was to measure the impact of "String Churn" and Garbage Collector (GC) pressure.

| Strategy | Execution Time | Allocated Memory | Key Insight |
| :--- | :--- | :--- | :--- |
| **Standard (LINQ)** | *~6,942.88 ms* | **~1,049,033 KB** | High GC pressure due to `.Trim()` allocations. |
| **Optimized (Local Function)** | *~6,165.17 ms* | **~1,436,654 KB** | Balanced batch processing using `Span`. |
| **High Performance (Yield + Span)** | **~5,808.51 ms** | **~1,438,492 KB** | **Winner:** Atomic parsing & streaming. |

> **Note:** The "Standard" method appears to use less memory only because the Garbage Collector is forced to run aggressively to clean up millions of temporary strings, which significantly slows down execution time.

## 💻 Code Evolution

### 1. The Standard Approach (LINQ)
Highly readable but inefficient for large datasets due to repeated string allocations.
```csharp
return users
    .Where(u => !string.IsNullOrEmpty(u.name?.Trim()))
    .Select(u => new { Name = u.name.Trim(), Score = int.Parse(u.score) }) // Potential allocations here
```

### 2. The Optimized Approach (Span)
Uses `ReadOnlySpan<char>` to validate and trim data on the **Stack** instead of the **Heap**.
```csharp
static bool IsUserValid(User u) {
    var nameSpan = u.name.AsSpan().Trim();
    return int.TryParse(u.score.AsSpan().Trim(), out int val); // No strings created here!
}
```

### 3. The Elite Solution (Streaming)
Combines `Span` with `yield return` to provide a single-pass, atomic transformation that avoids "double parsing" and unnecessary materialization of lists.

## 🚀 How to Run

1. Clone the repository.
2. Ensure you have **.NET 8 SDK** installed.
3. Run the project in `Release` mode for accurate benchmark results:
   ```bash
   dotnet run -c Release
   ```

## 📝 Blog Post
I’ve written a detailed article explaining the "Why" and "How" of these optimizations on my blog:  
🔗 **[Challenges - JSON Processing - C#](https://andreecirillo.hashnode.dev/challeges-jsonprocessing-csharp)**

---
**André Cirillo** *Architect of Chaos | Software Engineer | Tech Lead*
