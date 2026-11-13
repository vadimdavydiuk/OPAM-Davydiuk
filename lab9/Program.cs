using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

public static class Checks
{
    public static bool IsSorted<T>(T[] a, Func<T, T, int> cmp)
    { for (int i = 1; i < a.Length; i++) if (cmp(a[i - 1], a[i]) > 0) return false; return true; }
    public static bool IsPermutationOf<T>(T[] original, T[] sorted) where T : notnull
    {
        if (original.Length != sorted.Length) return false;
        var counts = new Dictionary<T, int>();
        foreach (T x in original) { counts.TryGetValue(x, out int n); counts[x] = n + 1; }
        foreach (T x in sorted)
        {
            if (!counts.TryGetValue(x, out int n) || n == 0) return false;
            counts[x] = n - 1;
        }
        foreach (var p in counts) if (p.Value != 0) return false;
        return true;
    }
    public static void Verify<T>(T[] original, T[] sorted, Func<T, T, int> cmp) where T : notnull
    { Bench.Check(IsSorted(sorted, cmp) && IsPermutationOf(original, sorted), "Порушено порядок або перестановку"); }
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
    public static void Run(Report r)
    {
        r.Line("## 5. Час на випадковому вході, мс");
        r.Line("Копіювання вхідного масиву й обидві перевірки поза таймером; виділення робочого буфера сортування входить у час. Кожний результат перевірено, включно з прогрівом.");
        r.Line("| n | Бульбашка | Злиття | Підрахунок | Array.Sort |"); r.Line("|---:|---:|---:|---:|---:|");
        foreach (int n in new[] { 1000, 10000, 100000, 1000000 })
        {
            Console.WriteLine($"Вимірювання n={n}...");
            var source = Program.Input(n, 0);
            double bubble = n <= 100000 ? Time(source, Program.Algorithms[0]) : 0;
            double merge = Time(source, Program.Algorithms[1]); double count = Time(source, Program.Algorithms[2]);
            double standard = Time(source, Standard);
            r.Line($"| {n} | {(n <= 100000 ? Bench.F(bubble) : "—")} | {Bench.F(merge)} | {Bench.F(count)} | {Bench.F(standard)} |");
            r.Line($"<!-- n={n}: злиття / підрахунок = {Bench.F(merge / count)} -->");
        }
        r.Line(); r.Line("## Межа однієї секунди");
        r.Line("Межа визначається для випадкового входу Random(5) з кроком 1000: перший розмір на цій сітці з мінімальним часом трьох запусків >1000 мс. Це експериментальна межа на цьому комп'ютері, а не властивість для кожного можливого масиву.");
        int low = 1000, high = 2000;
        double highTime = Time(Program.Input(high, 0), Program.Algorithms[0]);
        while (highTime <= 1000) { low = high; high *= 2; highTime = Time(Program.Input(high, 0), Program.Algorithms[0]); }
        while (high - low > 1000)
        {
            int middle = ((low + high) / 2000) * 1000;
            if (middle <= low) middle = low + 1000;
            double t = Time(Program.Input(middle, 0), Program.Algorithms[0]);
            if (t > 1000) { high = middle; highTime = t; } else low = middle;
        }
        double lowTime = Time(Program.Input(low, 0), Program.Algorithms[0]);
        highTime = Time(Program.Input(high, 0), Program.Algorithms[0]);
        r.Line($"Останній перевірений нижче межі: n={low}, {Bench.F(lowTime)} мс; перший вище: n={high}, {Bench.F(highTime)} мс. Пошук межі передбачає загальне зростання часу; шум секундоміра може змінити сусідні точки.");
    }
    private static void Standard(int[] a, Func<int, int, int> cmp, out long comparisons, out long moves)
    { Array.Sort(a); comparisons = moves = 0; }
    private static double Time(int[] original, SortAlgorithm<int> sort)
    {
        double best = double.MaxValue;
        for (int run = -1; run < 3; run++)
        {
            var a = (int[])original.Clone(); var sw = Stopwatch.StartNew();
            sort(a, Program.Cmp, out _, out _); sw.Stop(); Checks.Verify(original, a, Program.Cmp);
            if (run >= 0) best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
        }
        return best;
    }
}

public static class Program
{
    public static readonly Func<int, int, int> Cmp = (x, y) => x.CompareTo(y);
    public static readonly string[] Names = { "Бульбашка", "Злиття знизу вгору", "Підрахунок" };
    public static readonly SortAlgorithm<int>[] Algorithms = { Sorts.Bubble, Sorts.MergeBottomUp, CountInts };
    public static void CountInts(int[] a, Func<int, int, int> cmp, out long comparisons, out long moves)
        => Sorts.Counting(a, x => x, 0, 60, out comparisons, out moves);
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; var r = new Report(); r.Line("# Результати лабораторної 9"); r.EnvironmentInfo();
        if (args.Length > 0 && args[0] == "--check") { Edges(r); return; } Trace(r); Edges(r); Counters(r); Stability.Run(r); Measurements.Run(r); 
    }
    public static int[] Input(int n, int kind)
    {
        var a = Data.Numbers(n, 5, 0, 60);
        if (kind > 0) Array.Sort(a); // Підготовка входу поза власними сортуваннями.
        if (kind == 2)
            for (int i = 0; i < a.Length / 2; i++) { int t = a[i]; a[i] = a[a.Length - 1 - i]; a[a.Length - 1 - i] = t; }
        if (kind == 3 && a.Length > 0)
        {
            var rng = new Random(5);
            for (int i = 0; i < 3; i++) { int x = rng.Next(n), y = rng.Next(n); int t = a[x]; a[x] = a[y]; a[y] = t; }
        }
        return a;
    }
    private static void Trace(Report r)
    {
        r.Line("## 1. Трасування 9 елементів, перевірене проходами");
        var a = Data.Numbers(9, 5, 0, 60); r.Line("| Прохід | Стан | Порівняння за прохід | Записи за прохід |"); r.Line("|---|---|---:|---:|");
        r.Line($"| Вхід | {string.Join(", ", a)} | — | — |");
        for (int end = a.Length - 1, pass = 1; end > 0; end--, pass++)
        {
            bool swapped = false; int moves = 0;
            for (int i = 0; i < end; i++) if (a[i] > a[i + 1])
            { int t = a[i]; a[i] = a[i + 1]; a[i + 1] = t; moves += 2; swapped = true; }
            r.Line($"| {pass} | {string.Join(", ", a)} | {end} | {moves} |");
            if (!swapped) break;
        }
        r.Line();
    }
    private static void Counters(Report r)
    {
        r.Line("## 4. Порівняння / записи елементів у масиви");
        r.Line("Рахуються записи як в основний масив, так і в буфер T[]. Присвоєння локальній змінній та службові частоти не враховуються. Лічильники long, бо на n=100000 int переповнюється.");
        for (int alg = 0; alg < Algorithms.Length; alg++)
        {
            r.Line(); r.Line("### " + Names[alg]); r.Line("| n | Випадковий | Відсортований | Навпаки | Майже |"); r.Line("|---:|---|---|---|---|");
            foreach (int n in new[] { 100, 1000, 10000 })
            {
                string row = $"| {n}";
                for (int kind = 0; kind < 4; kind++)
                {
                    var original = Input(n, kind); var a = (int[])original.Clone(); Algorithms[alg](a, Cmp, out long c, out long m); Checks.Verify(original, a, Cmp);
                    if (alg == 0 && kind == 1) Bench.Check(c == n - 1 && m == 0, "Бульбашка на впорядкованому");
                    row += $" | {c} / {m}";
                }
                r.Line(row + " |");
            }
        }
        var randomInput = Input(10000, 0); var b = (int[])randomInput.Clone(); var merge = (int[])randomInput.Clone();
        Sorts.Bubble(b, Cmp, out long bc, out _); Sorts.MergeBottomUp(merge, Cmp, out long mc, out _);
        r.Line(); r.Line($"Відношення порівнянь бульбашки до злиття на випадкових n=10000: {Bench.F((double)bc / mc)} (лекційне 376 — інша модель і вхід)."); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        int[][] inputs = { Array.Empty<int>(), new[] { 5 }, new[] { 1, 2 }, new[] { 2, 1 }, new[] { 7, 7, 7, 7 }, new[] { 2, 2, 2, 1, 1, 1 } };
        foreach (var input in inputs)
        {
            foreach (var sort in Algorithms)
            { var a = (int[])input.Clone(); sort(a, Cmp, out _, out _); Checks.Verify(input, a, Cmp); }
            r.Line($"| [{string.Join(",", input)}] | IsSorted і IsPermutationOf = true | усі три алгоритми: true / true, OK |");
        }
        // Додаткові розміри перевіряють неповні хвости злиття.
        for (int n = 0; n <= 257; n++)
        {
            var original = Input(n, 0);
            foreach (var sort in Algorithms) { var a = (int[])original.Clone(); sort(a, Cmp, out _, out _); Checks.Verify(original, a, Cmp); }
        }
        r.Line("Додатково пройдено всі довжини 0–257, включно з неповними хвостами злиття."); r.Line();
    }
}

public delegate void SortAlgorithm<T>(T[] a, Func<T, T, int> cmp, out long comparisons, out long moves);

public static class Sorts
{
    // long потрібен: на 100000 елементів лічильник записів бульбашки перевищує int.MaxValue.
    public static void Bubble<T>(T[] a, Func<T, T, int> cmp, out long comparisons, out long moves)
    {
        comparisons = 0; moves = 0;
        for (int end = a.Length - 1; end > 0; end--)
        {
            bool swapped = false;
            for (int i = 0; i < end; i++)
            {
                comparisons++;
                if (cmp(a[i], a[i + 1]) > 0)
                {
                    T temp = a[i]; a[i] = a[i + 1]; a[i + 1] = temp;
                    moves += 2; // Лише два записи в масив; локальна змінна не є записом у масив.
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }
    public static void MergeBottomUp<T>(T[] a, Func<T, T, int> cmp, out long comparisons, out long moves)
    {
        comparisons = 0; moves = 0; var buffer = new T[a.Length];
        for (int width = 1; width < a.Length; width = width > a.Length / 2 ? a.Length : width * 2)
        {
            for (int start = 0; start < a.Length; start += 2 * width)
            {
                int middle = Math.Min(start + width, a.Length), end = Math.Min(start + 2 * width, a.Length);
                int left = start, right = middle, pos = start;
                while (left < middle && right < end)
                {
                    comparisons++;
                    if (cmp(a[left], a[right]) <= 0) buffer[pos++] = a[left++];
                    else buffer[pos++] = a[right++];
                    moves++;
                }
                while (left < middle) { buffer[pos++] = a[left++]; moves++; }
                while (right < end) { buffer[pos++] = a[right++]; moves++; }
                for (int i = start; i < end; i++) { a[i] = buffer[i]; moves++; }
            }
        }
    }
    // Підрахунок не є порівняльним сортуванням. Узагальнення — за елементом,
    // а цілочисельний ключ і його діапазон передаються параметрами.
    public static void Counting<T>(T[] a, Func<T, int> keyOf, int min, int max, out long comparisons, out long moves)
    {
        if (max < min) throw new ArgumentException("Некоректний діапазон");
        comparisons = 0; moves = 0; var counts = new int[checked(max - min + 1)];
        foreach (T item in a)
        {
            int key = keyOf(item);
            if (key < min || key > max) throw new ArgumentOutOfRangeException(nameof(a));
            counts[key - min]++;
        }
        for (int i = 1; i < counts.Length; i++) counts[i] += counts[i - 1];
        var buffer = new T[a.Length];
        for (int i = a.Length - 1; i >= 0; i--) { buffer[--counts[keyOf(a[i]) - min]] = a[i]; moves++; }
        for (int i = 0; i < a.Length; i++) { a[i] = buffer[i]; moves++; }
        // counts — службові частоти, не масив елементів T; їх зміни не є moves.
    }
}

public static class Stability
{
    // Очікування зафіксовані до запуску експерименту.
    private static readonly string[] Expected = { "стабільне", "стабільне", "стабільне", "не гарантує", "стабільне" };
    public static void Run(Report r)
    {
        r.Line("## 5. Стабільність");
        r.Line("Початкові 12 студентів мають різні бали, тому самі не виявляють нестабільність. Додатково створено 96 записів із повторюваними прізвищами та балами 60, 70, 80, 90; перед кожним дослідом — упорядкування прізвищ за StringComparer.Ordinal.");
        var original = Data.Students().ToArray();
        Array.Sort(original, (x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));
        r.Line("| Алгоритм | Очікувано до запуску | Початкові 12 | 96 із рівними балами | Перший контрприклад |"); r.Line("|---|---|---|---|---|");
        var source = Data.Students(); var extra = new Student[96];
        for (int i = 0; i < extra.Length; i++) { var s = source[i % source.Count]; extra[i] = new Student(s.Name + "_" + i.ToString("D2"), s.Group, 60 + (i % 4) * 10); }
        Array.Sort(extra, (x, y) => StringComparer.Ordinal.Compare(x.Name, y.Name));
        string[] names = { "Бульбашка", "Злиття", "Підрахунок", "Array.Sort", "OrderBy (лише еталон)" };
        for (int algorithm = 0; algorithm < names.Length; algorithm++)
        {
            var basic = Sort(original, algorithm); var sorted = Sort(extra, algorithm);
            Checks.Verify(original, basic, (x, y) => x.Score.CompareTo(y.Score)); Checks.Verify(extra, sorted, (x, y) => x.Score.CompareTo(y.Score));
            string counter = Counterexample(sorted);
            r.Line($"| {names[algorithm]} | {Expected[algorithm]} | {Counterexample(basic)} | {(counter == "немає" ? "порядок збережено" : "порядок порушено")} | {counter} |");
            if (algorithm != 3) Bench.Check(counter == "немає", "Стабільність");
        }
        r.Line("Відсутність контрприкладу в окремому досліді не доводить стабільність Array.Sort. OrderBy викликано лише в еталоні цієї таблиці: це прямо вимагає завдання 5; у власних алгоритмах LINQ немає."); r.Line();
    }
    private static Student[] Sort(Student[] source, int algorithm)
    {
        var a = (Student[])source.Clone(); Func<Student, Student, int> cmp = (x, y) => x.Score.CompareTo(y.Score);
        switch (algorithm)
        {
            case 0: Sorts.Bubble(a, cmp, out _, out _); break;
            case 1: Sorts.MergeBottomUp(a, cmp, out _, out _); break;
            case 2: Sorts.Counting(a, s => s.Score, 0, 100, out _, out _); break;
            case 3: Array.Sort(a, (x, y) => cmp(x, y)); break;
            case 4:
                // Виняток з правила заради прямо зазначеного еталона OrderBy.
                a = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OrderBy(a, s => s.Score)); break;
        }
        return a;
    }
    private static string Counterexample(Student[] a)
    {
        for (int i = 1; i < a.Length; i++)
            if (a[i - 1].Score == a[i].Score && StringComparer.Ordinal.Compare(a[i - 1].Name, a[i].Name) > 0)
                return $"{a[i - 1].Name} перед {a[i].Name}, бал {a[i].Score}";
        return "немає";
    }
}
