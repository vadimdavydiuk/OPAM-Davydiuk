using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

public static class Algorithms
{
    public static List<T> Filter<T>(List<T> src, Func<T, bool> keep)
    {
        var result = new List<T>();
        foreach (T item in src) if (keep(item)) result.Add(item);
        return result;
    }
    public static List<TOut> Map<TIn, TOut>(List<TIn> src, Func<TIn, TOut> convert)
    {
        var result = new List<TOut>();
        foreach (TIn item in src) result.Add(convert(item));
        return result;
    }
    public static TAcc Aggregate<T, TAcc>(List<T> src, TAcc start, Func<TAcc, T, TAcc> step)
    {
        TAcc state = start;
        foreach (T item in src) state = step(state, item);
        return state;
    }
    public static Dictionary<TKey, List<T>> GroupBy<T, TKey>(List<T> src, Func<T, TKey> keyOf) where TKey : notnull
    {
        var result = new Dictionary<TKey, List<T>>();
        foreach (T item in src)
        {
            TKey key = keyOf(item);
            if (!result.TryGetValue(key, out var group)) { group = new List<T>(); result[key] = group; }
            group.Add(item);
        }
        return result;
    }
}

public sealed class Student
{
    public string Name;
    public string Group;
    public int Score;
    public Student(string name, string group, int score) { Name = name; Group = group; Score = score; }
    public override string ToString() => $"{Name} ({Group}, {Score})";
}

public static class Data
{
    public static List<Student> Students() => new List<Student>
    {
        new Student("Коваленко", "КН-3/1", 88), new Student("Мельник", "КН-3/2", 64),
        new Student("Іщенко", "КН-3/1", 93), new Student("Бондар", "КН-3/1", 71),
        new Student("Савченко", "КН-3/2", 55), new Student("Ткачук", "КН-3/2", 90),
        new Student("Кравець", "КН-3/1", 76), new Student("Іванів", "КН-3/1", 48),
        new Student("Мороз", "КН-3/2", 82), new Student("Шевчук", "КН-3/1", 67),
        new Student("Бойко", "КН-3/2", 95), new Student("Кушнір", "КН-3/2", 73)
    };
    public static int[] Numbers(int n, int seed, int min, int max)
    {
        var r = new Random(seed); var a = new int[n];
        for (int i = 0; i < n; i++) a[i] = r.Next(min, max + 1);
        return a;
    }
    public static Student[] Records(int n)
    {
        var r = new Random(5); var source = Students(); var a = new Student[n];
        for (int i = 0; i < n; i++)
        { var s = source[i % source.Count]; a[i] = new Student(s.Name, s.Group, r.Next(101)); }
        return a;
    }
}

public static class Bench
{
    public static double Min(Action action)
    {
        action(); // Прогрів перед трьома вимірюваннями.
        double best = double.MaxValue;
        for (int i = 0; i < 3; i++)
        { var sw = Stopwatch.StartNew(); action(); sw.Stop(); best = Math.Min(best, sw.Elapsed.TotalMilliseconds); }
        return best;
    }
    public static string F(double x) => x.ToString("F4", CultureInfo.InvariantCulture);
    public static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

public sealed class Report
{
    private readonly StringBuilder text = new StringBuilder();
    public void Line(string s = "") { text.AppendLine(s); Console.WriteLine(s); }
    public void Save(string file)
    {
        File.WriteAllText(file, text.ToString(), new UTF8Encoding(false));
    }
    public void EnvironmentInfo()
    {
        Line($"Фактичний запуск: {DateTimeOffset.Now:O}; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; {System.Runtime.InteropServices.RuntimeInformation.OSDescription}; {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}.");
#if DEBUG
        throw new InvalidOperationException("Для вимірювань потрібна збірка Release.");
#else
        Line("Конфігурація Release; Stopwatch; прогрів; мінімум із трьох запусків. Час залежить від комп'ютера й навантаження.");
#endif
        Line();
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; var r = new Report(); r.Line("# Результати лабораторної 7"); r.EnvironmentInfo();
        var numbers = new List<int>(Data.Numbers(20, 5, -3, 3));
        var words = new List<string> { "алгоритм", "код", "а", "блок-схема", "ввід", "вивід", "онлайн-курс", "цикл", "тип", "тест-кейс", "мова", "дані", "пам'ять" };
        var students = Data.Students();
        r.Line("Числа: " + string.Join(", ", numbers)); r.Line("Слова: " + string.Join(", ", words)); r.Line();
        r.Line("## 1. Filter");
        Show(r, "Числа ≤ 0", Algorithms.Filter(numbers, x => x <= 0));
        Show(r, "Слова з дефісом", Algorithms.Filter(words, x => HasHyphen(x)));
        Show(r, "Бали 60–89", Algorithms.Filter(students, s => s.Score >= 60 && s.Score <= 89));
        r.Line("## 2. Map");
        Show(r, "Остача від ділення на 5", Algorithms.Map(numbers, x => x % 5));
        Show(r, "Верхній регістр", Algorithms.Map(words, x => x.ToUpperInvariant()));
        Show(r, "Прізвище (бал)", Algorithms.Map(students, s => $"{s.Name} ({s.Score})"));
        Show(r, "Композиція Filter → Map", Algorithms.Map(Algorithms.Filter(students, s => s.Score >= 60 && s.Score <= 89), s => $"{s.Name} ({s.Score})"));
        r.Line("## 3. Aggregate");
        int sum = Algorithms.Aggregate(numbers, 0, (s, x) => s + x);
        long product = Algorithms.Aggregate(numbers, 1L, (p, x) => p * x);
        int max = Maximum(numbers);
        int count = Algorithms.Aggregate(numbers, 0, (c, x) => c + (x <= 0 ? 1 : 0));
        r.Line($"Сума: {sum}; добуток: {product}; максимум: {max}; кількість ≤0: {count}."); r.Line();
        r.Line("## 4. GroupBy");
        ShowGroups(r, "За групою", Algorithms.GroupBy(students, s => s.Group));
        ShowGroups(r, "За першою літерою", Algorithms.GroupBy(students, s => s.Name[0]));
        ShowGroups(r, "За діапазоном балу", Algorithms.GroupBy(students, s => s.Score < 60 ? "0–59" : s.Score < 75 ? "60–74" : s.Score < 90 ? "75–89" : "90–100"));
        ShowGroups(r, "Слова за довжиною", Algorithms.GroupBy(words, x => x.Length));
        Captures(r); Edges(r); 
    }
    private static bool HasHyphen(string text)
    { foreach (char c in text) if (c == '-') return true; return false; }
    public static int Maximum(List<int> src)
    {
        if (src.Count == 0) throw new ArgumentException("Порожній список не має максимуму.", nameof(src));
        return Algorithms.Aggregate(src, src[0], (m, x) => Math.Max(m, x));
    }
    private static void Show<T>(Report r, string title, List<T> values)
    { r.Line($"{title} ({values.Count}): {string.Join(", ", values)}"); r.Line(); }
    private static void ShowGroups<T, TKey>(Report r, string title, Dictionary<TKey, List<T>> groups) where TKey : notnull
    { r.Line(title); foreach (var g in groups) r.Line($"- {g.Key}: {g.Value.Count}"); r.Line(); }
    private static void Captures(Report r)
    {
        r.Line("## 5. Захоплення змінних"); int threshold = 3;
        Func<int, bool> predicate = x => x > threshold;
        bool before = predicate(4); threshold = 1000; bool after = predicate(4);
        Bench.Check(before && !after, "Захоплення порога");
        r.Line($"predicate(4): до зміни {before}, після threshold=1000: {after}.");
        var functions = new List<Func<int>>(); for (int i = 0; i < 3; i++) functions.Add(() => i);
        var values = Algorithms.Map(functions, f => f()); r.Line("for: " + string.Join(", ", values));
        Bench.Check(values[0] == 3 && values[1] == 3 && values[2] == 3, "for");
        functions.Clear(); foreach (int i in new[] { 0, 1, 2 }) functions.Add(() => i);
        values = Algorithms.Map(functions, f => f()); r.Line("foreach: " + string.Join(", ", values));
        Bench.Check(values[0] == 0 && values[1] == 1 && values[2] == 2, "foreach");
        functions.Clear(); for (int i = 0; i < 3; i++) { int copy = i; functions.Add(() => copy); }
        r.Line("Виправлений for: " + string.Join(", ", Algorithms.Map(functions, f => f()))); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        var empty = new List<int>();
        Bench.Check(Algorithms.Filter(empty, x => true).Count == 0 && Algorithms.Map(empty, x => x.ToString()).Count == 0 && Algorithms.GroupBy(empty, x => x).Count == 0, "Порожні алгоритми");
        r.Line("| Порожній список: Filter, Map, GroupBy | порожні результати | 0, 0, 0, OK |");
        Bench.Check(Algorithms.Aggregate(empty, int.MinValue, (m, x) => Math.Max(m, x)) == int.MinValue, "Старт на порожньому");
        bool thrown = false; try { Maximum(empty); } catch (ArgumentException) { thrown = true; }
        Bench.Check(thrown, "Порожній максимум");
        r.Line("| Порожній максимум | Aggregate повертає старт; Maximum відхиляє вхід | int.MinValue; ArgumentException, OK |");
        Bench.Check(Algorithms.Filter(new List<int> { 1, 2 }, x => x <= 0).Count == 0, "Нічого не підходить");
        r.Line("| Нічого не підходить | порожній Filter | 0, OK |");
        var group = Algorithms.GroupBy(new List<int> { 1, 2, 3 }, x => "одна");
        Bench.Check(group.Count == 1 && group.TryGetValue("одна", out var items) && items.Count == 3, "Одна група");
        r.Line("| Усі елементи в одній групі | 1 група із 3 елементів | 1; 3, OK |");
        var one = new List<int> { -2 };
        Bench.Check(Algorithms.Aggregate(one, 0, (s, x) => s + x) == -2 && Algorithms.Aggregate(one, 1L, (p, x) => p * x) == -2 && Maximum(one) == -2 && Algorithms.Aggregate(one, 0, (c, x) => c + (x <= 0 ? 1 : 0)) == 1, "Один елемент");
        r.Line("| Один елемент −2 | сума −2, добуток −2, максимум −2, кількість 1 | −2; −2; −2; 1, OK |");
    }
}
