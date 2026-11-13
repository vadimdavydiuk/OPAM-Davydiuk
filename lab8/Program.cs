using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

// Навмисні помилки лише для експерименту завдання 2.
public static class Broken
{
    public static int StrictLoop<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        int lo = 0, hi = a.Length - 1; comparisons = 0;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2; comparisons++; int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
    }
    public static int Stuck<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons, out int lastLo, out int lastHi)
    {
        int lo = 0, hi = a.Length - 1; comparisons = 0;
        while (lo <= hi && comparisons < 100)
        {
            int mid = lo + (hi - lo) / 2; comparisons++; int c = cmp(a[mid], target);
            if (c == 0) { lastLo = lo; lastHi = hi; return mid; }
            if (c < 0) lo = mid; else hi = mid - 1;
        }
        lastLo = lo; lastHi = hi; return -1;
    }
    public static int UnsafeMidpoint<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        int lo = 0, hi = a.Length - 1; comparisons = 0;
        while (lo <= hi)
        {
            int mid = unchecked((lo + hi) / 2); comparisons++; int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
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

public static class Measurements
{
    private static long sink;
    public static void Run(Report r)
    {
        r.Line("## 5. Середні порівняння: 100 запитів");
        r.Line("Запити — бали 0–100 із Random(105); масив відсортований. Лінійний пошук доходить приблизно до середини масиву, а бінарний часто потрапляє в широкий блок рівних балів раніше, ніж за log₂n кроків.");
        r.Line("| n | Лінійний | Бінарний | Відношення | n/2; ceil(log₂n) |"); r.Line("|---:|---:|---:|---:|---|");
        foreach (int n in new[] { 1000, 10000, 100000, 1000000 })
        {
            var a = Data.Records(n); Array.Sort(a, (x, y) => Program.Cmp(x, y)); var random = new Random(105);
            long linear = 0, binary = 0;
            for (int i = 0; i < 100; i++)
            {
                var target = new Student("", "", random.Next(101));
                int li = Search.LinearSearch(a, target, Program.Cmp, out int lc), bi = Search.BinarySearch(a, target, Program.Cmp, out int bc);
                Bench.Check((li < 0) == (bi < 0), "Збіг запитів"); linear += lc; binary += bc;
            }
            r.Line($"| {n} | {Bench.F(linear / 100.0)} | {Bench.F(binary / 100.0)} | {Bench.F((double)linear / binary)} | {n / 2}; {Math.Ceiling(Math.Log2(n))} |");
        }
        r.Line(); r.Line("## Точка окупності");
        r.Line("Лінійний пошук працює на початковому невідсортованому масиві; сортований шлях щоразу починається з його копії. Клонування перед таймером в обох шляхах; сортування всередині таймера. Обидва шляхи повертають наявність бала, а не однакові індекси.");
        var source = Data.Records(100000); var rng = new Random(105); var queries = new Student[100];
        for (int i = 0; i < queries.Length; i++) queries[i] = new Student("", "", rng.Next(101));
        r.Line("| q | Лінійно, мс | Сортування + бінарно, мс | Швидший |"); r.Line("|---:|---:|---:|---|");
        int breakEven = -1;
        foreach (int q in new[] { 1, 5, 10, 20, 30, 50, 100 })
        {
            double linear = Measure(source, queries, q, false), binary = Measure(source, queries, q, true);
            if (breakEven < 0 && binary < linear) breakEven = q;
            r.Line($"| {q} | {Bench.F(linear)} | {Bench.F(binary)} | {(linear < binary ? "лінійний" : "сортування + бінарний")} |");
        }
        r.Line(breakEven < 0 ? "У перевірених q ≤ 100 точку окупності не досягнуто." : $"Перше виграшне q серед перевірених: {breakEven}.");
        if (breakEven < 0) Extend(r, source);
        r.Line("У невідсортованому масиві бали рівномірні 0–100: перший збіг у середньому знаходиться приблизно за 101 перевірку. Тому очікування 34 запити (для інших припущень про дані) сюди не переноситься; повтори істотно змінюють картину.");
        r.Line($"Контрольна сума: {sink}.");
    }
    private static void Extend(Report r, Student[] source)
    {
        r.Line(); r.Line("### Додатковий пошук окупності за межами q=100");
        var rng = new Random(105); var queries = new Student[1000000];
        for (int i = 0; i < queries.Length; i++) queries[i] = new Student("", "", rng.Next(101));
        r.Line("| q | Лінійно, мс | Сортування + бінарно, мс |"); r.Line("|---:|---:|---:|");
        int low = 100, high = 1000;
        while (true)
        {
            double l = Measure(source, queries, high, false), b = Measure(source, queries, high, true);
            r.Line($"| {high} | {Bench.F(l)} | {Bench.F(b)} |");
            if (b < l) break;
            low = high;
            if (high == queries.Length) { r.Line("Точку окупності не досягнуто навіть за q=1000000."); return; }
            high *= 10;
        }
        // Уточнення на сітці з кроком 1000; секундомір має шум.
        while (high - low > 1000)
        {
            int q = ((low + high) / 2000) * 1000;
            if (q <= low) q = low + 1000;
            double l = Measure(source, queries, q, false), b = Measure(source, queries, q, true);
            r.Line($"| {q} | {Bench.F(l)} | {Bench.F(b)} |");
            if (b < l) high = q; else low = q;
        }
        r.Line($"Експериментальна точка окупності на сітці 1000: q≈{high}; останній програшний перевірений q={low}. Це значно більше лекційних 34 через ранній збіг повторюваних балів у невідсортованому вході; шум вимірювань впливає на межу.");
    }
    private static double Measure(Student[] source, Student[] queries, int q, bool sort)
    {
        double best = double.MaxValue;
        for (int run = -1; run < 3; run++)
        {
            var copy = (Student[])source.Clone(); var sw = System.Diagnostics.Stopwatch.StartNew(); long sum = 0;
            if (sort) Array.Sort(copy, (x, y) => Program.Cmp(x, y));
            for (int i = 0; i < q; i++)
            {
                int pos = sort ? Search.BinarySearch(copy, queries[i], Program.Cmp, out _) : Search.LinearSearch(copy, queries[i], Program.Cmp, out _);
                sum += pos >= 0 ? 1 : 0;
            }
            sw.Stop(); sink = sum;
            if (run >= 0) best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
        }
        return best;
    }
}

public static class Program
{
    public static readonly Func<Student, Student, int> Cmp = (x, y) => x.Score.CompareTo(y.Score);
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; var r = new Report(); r.Line("# Результати лабораторної 8"); r.EnvironmentInfo();
        var unsorted = Data.Records(100000); var a = (Student[])unsorted.Clone(); Array.Sort(a, (x, y) => Cmp(x, y));
        Queries(r, a); Failures(r, a, unsorted); Boundaries(r, a); Edges(r); Measurements.Run(r); 
    }
    private static void Queries(Report r, Student[] a)
    {
        r.Line("## 1, 2, 4. Пошуки");
        r.Line("| Запит | Лінійний індекс / порівняння | Бінарний | Рекурсивний | Глибина |"); r.Line("|---|---|---|---|---|");
        foreach (int score in new[] { 53, a[0].Score, 101 })
        {
            var x = new Student("", "", score);
            int l = Search.LinearSearch(a, x, Cmp, out int lc), b = Search.BinarySearch(a, x, Cmp, out int bc);
            int rec = Search.RecursiveSearch(a, x, Cmp, out int rc, out int depth);
            Bench.Check((l < 0) == (b < 0) && b == rec, "Пошуки не узгоджені");
            r.Line($"| Бал {score} | {l} / {lc} | {b} / {bc} | {rec} / {rc} | {depth} |");
        }
        int maxDepth = 0;
        for (int score = -1; score <= 101; score++)
        { Search.RecursiveSearch(a, new Student("", "", score), Cmp, out _, out int d); maxDepth = Math.Max(maxDepth, d); }
        r.Line($"Максимальна фактична глибина серед усіх можливих балів і двох відсутніх: {maxDepth} (разом із порожнім базовим викликом)."); r.Line();
    }
    private static void Failures(Report r, Student[] a, Student[] unsorted)
    {
        r.Line("## Чотири поломки");
        int bad = Broken.StrictLoop(new[] { 7 }, 7, (x, y) => x.CompareTo(y), out int comparisons);
        Bench.Check(bad == -1, "Очікувана поломка <="); r.Line($"1. [7], шукаємо 7: {bad}, порівнянь {comparisons}. Останній кандидат не перевірений.");
        bad = Broken.Stuck(new[] { 1, 2 }, 2, (x, y) => x.CompareTo(y), out comparisons, out int lo, out int hi);
        Bench.Check(comparisons == 100 && lo == 0 && hi == 1, "Очікуване зависання");
        r.Line($"2. [1,2], шукаємо 2: зупинено після {comparisons} кроків; lo={lo}, hi={hi}, результат {bad}. Без обмежувача — нескінченний цикл.");
        int index = Broken.UnsafeMidpoint(a, new Student("", "", 53), Cmp, out comparisons);
        int largeLo = 1073741824, largeHi = 1073741824;
        r.Line($"3. На 100000 записах індекс={index}, порівнянь={comparisons}. Без великого масиву: lo=hi={largeLo}, unchecked((lo+hi)/2)={unchecked((largeLo + largeHi) / 2)}.");
        Bench.Check(index >= 0 && a[index].Score == 53, "Мала unsafe версія");
        int misses = 0; var random = new Random(105);
        for (int i = 0; i < 100; i++)
        {
            Student target = unsorted[random.Next(unsorted.Length)];
            if (Search.BinarySearch(unsorted, target, Cmp, out _) < 0) misses++;
        }
        r.Line($"4. Невідсортовані записи: з 100 запитів гарантовано наявних балів не знайдено {misses}."); r.Line();
    }
    private static void Boundaries(Report r, Student[] a)
    {
        r.Line("## 3. Межі"); var target = new Student("", "", 53);
        int lower = Search.Boundary(a, x => Cmp(x, target) >= 0, out int lc);
        int upper = Search.Boundary(a, x => Cmp(x, target) > 0, out int uc);
        int first = -1, last = -1, count = 0;
        for (int i = 0; i < a.Length; i++) if (Cmp(a[i], target) == 0) { if (first < 0) first = i; last = i; count++; }
        Bench.Check(lower == first && upper - 1 == last && upper - lower == count, "Межі");
        r.Line($"LowerBound={lower} ({lc} порівнянь); UpperBound={upper} ({uc}); останній X={upper - 1}; кількість X={upper - lower}.");
        r.Line($"Разом для останнього індексу й кількості: {lc + uc} проти {a.Length} порівнянь лінійного проходу. Лінійно: перший={first}, останній={last}, кількість={count}.");
        int boundary = Search.Boundary(a, s => s.Score % 7 == 0, out int c);
        int actual = 0; while (actual < a.Length && a[actual].Score % 7 != 0) actual++;
        r.Line($"Немонотонна умова Score % 7 == 0: межа={boundary}, лінійна відповідь={actual}, збіг={boundary == actual}, перевірок={c}. Це не коректний критерій для пошуку межі."); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід / запит | Очікувано: Binary, Lower, Upper | Отримано |"); r.Line("|---|---|---|");
        Edge(r, Array.Empty<int>(), 1, -1, 0, 0);
        Edge(r, new[] { 5 }, 5, 0, 0, 1);
        Edge(r, new[] { 1, 2 }, 2, 1, 1, 2);
        Edge(r, new[] { 3, 4 }, 1, -1, 0, 0);
        Edge(r, new[] { 3, 4 }, 5, -1, 2, 2);
        Edge(r, new[] { 7, 7, 7 }, 7, 1, 0, 3); r.Line();
    }
    private static void Edge(Report r, int[] a, int x, int eb, int el, int eu)
    {
        int b = Search.BinarySearch(a, x, (p, q) => p.CompareTo(q), out _);
        int l = Search.Boundary(a, v => v >= x, out _), u = Search.Boundary(a, v => v > x, out _);
        int rec = Search.RecursiveSearch(a, x, (p, q) => p.CompareTo(q), out _);
        Bench.Check(b == eb && l == el && u == eu && rec == b, "Крайовий пошук");
        r.Line($"| [{string.Join(",", a)}], X={x} | {eb}, {el}, {eu} | {b}, {l}, {u}; рекурсивний {rec}, OK |");
    }
}

public static class Search
{
    public static int LinearSearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        comparisons = 0;
        for (int i = 0; i < a.Length; i++) { comparisons++; if (cmp(a[i], target) == 0) return i; }
        return -1;
    }
    public static int BinarySearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
    {
        int lo = 0, hi = a.Length - 1; comparisons = 0;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2; comparisons++; int c = cmp(a[mid], target);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
    }
    // Умова повинна мати вигляд false...false,true...true.
    public static int Boundary<T>(T[] a, Func<T, bool> condition, out int comparisons)
    {
        int lo = 0, hi = a.Length; comparisons = 0;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2; comparisons++;
            if (condition(a[mid])) hi = mid; else lo = mid + 1;
        }
        return lo;
    }
    public static int RecursiveSearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons)
        => RecursiveSearch(a, target, cmp, out comparisons, out _);
    public static int RecursiveSearch<T>(T[] a, T target, Func<T, T, int> cmp, out int comparisons, out int depth)
    { comparisons = 0; depth = 0; return Recurse(a, target, cmp, 0, a.Length - 1, 1, ref comparisons, ref depth); }
    private static int Recurse<T>(T[] a, T target, Func<T, T, int> cmp, int lo, int hi, int level, ref int comparisons, ref int depth)
    {
        depth = Math.Max(depth, level);
        if (lo > hi) return -1;
        int mid = lo + (hi - lo) / 2; comparisons++; int c = cmp(a[mid], target);
        if (c == 0) return mid;
        return c < 0 ? Recurse(a, target, cmp, mid + 1, hi, level + 1, ref comparisons, ref depth)
                     : Recurse(a, target, cmp, lo, mid - 1, level + 1, ref comparisons, ref depth);
    }
}
