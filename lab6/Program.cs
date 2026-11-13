using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

public static class Algorithms
{
    public static List<string> Words(string text)
    {
        var result = new List<string>(); var word = new StringBuilder();
        foreach (char ch in text)
        {
            if (char.IsLetter(ch)) word.Append(char.ToLowerInvariant(ch));
            else if (word.Length > 0) { result.Add(word.ToString()); word.Clear(); }
        }
        if (word.Length > 0) result.Add(word.ToString());
        return result;
    }
    public static Dictionary<T, int> Frequencies<T>(List<T> values) where T : notnull
    {
        var result = new Dictionary<T, int>();
        foreach (T x in values) { result.TryGetValue(x, out int count); result[x] = count + 1; }
        return result;
    }
    // Два паралельні списки: словника й множини тут немає.
    public static (List<T> keys, List<int> counts) FrequenciesList<T>(List<T> values)
    {
        var keys = new List<T>(); var counts = new List<int>();
        foreach (T x in values)
        {
            int i = Position(keys, x);
            if (i < 0) { keys.Add(x); counts.Add(1); } else counts[i]++;
        }
        return (keys, counts);
    }
    public static int Position<T>(List<T> values, T target)
    {
        for (int i = 0; i < values.Count; i++)
            if (EqualityComparer<T>.Default.Equals(values[i], target)) return i;
        return -1;
    }
    public static Dictionary<string, List<Student>> Group(List<Student> students)
    {
        var result = new Dictionary<string, List<Student>>();
        foreach (var s in students)
        {
            if (!result.TryGetValue(s.Group, out var group)) { group = new List<Student>(); result[s.Group] = group; }
            group.Add(s);
        }
        return result;
    }
    public static (List<string> keys, List<List<Student>> groups) GroupList(List<Student> students)
    {
        var keys = new List<string>(); var groups = new List<List<Student>>();
        foreach (var s in students)
        {
            int i = Position(keys, s.Group);
            if (i < 0) { i = keys.Count; keys.Add(s.Group); groups.Add(new List<Student>()); }
            groups[i].Add(s);
        }
        return (keys, groups);
    }
    public static double Average(List<Student> students)
    { int sum = 0; foreach (var s in students) sum += s.Score; return students.Count == 0 ? 0 : (double)sum / students.Count; }
    public static HashSet<int> UniqueUnordered(List<int> values) => new HashSet<int>(values);
    public static List<int> UniqueOrdered(List<int> values)
    {
        var seen = new HashSet<int>(); var result = new List<int>();
        foreach (int x in values) if (seen.Add(x)) result.Add(x);
        return result;
    }
    public static List<int> UniqueList(List<int> values)
    {
        var result = new List<int>(); foreach (int x in values) if (Position(result, x) < 0) result.Add(x);
        return result;
    }
    public static HashSet<int> Intersection(List<int> a, List<int> b)
    { var result = new HashSet<int>(a); result.IntersectWith(b); return result; }
    public static List<int> IntersectionList(List<int> a, List<int> b)
    {
        var result = new List<int>();
        foreach (int x in a) if (Position(b, x) >= 0 && Position(result, x) < 0) result.Add(x);
        return result;
    }
    public static bool Same(List<int> a, List<int> b)
    {
        if (a.Count != b.Count) return false;
        foreach (int x in a) if (Position(b, x) < 0) return false;
        return true;
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
    private static int sink;
    public static void Run(Report r)
    {
        r.Line("## 5. Час");
        int scale = 1;
        // Пробний повний прогін; якщо будь-яка спискова операція >60 с,
        // усі кількості зменшуються вдесятеро, й заміри повторюються.
        while (true)
        {
            var times = Measure(scale);
            bool tooSlow = false; foreach (var row in times) if (row.list > 60000) tooSlow = true;
            if (tooSlow) { scale *= 10; continue; }
            r.Line($"Масштаб: усі кількості поділено на {scale}. Побудова початкових даних поза таймером; побудова результату входить у час алгоритму.");
            r.Line("| Операція | Список, мс | Хеш-структура, мс | Виграш |"); r.Line("|---|---:|---:|---:|");
            foreach (var row in times) r.Line($"| {row.name} | {Bench.F(row.list)} | {Bench.F(row.hash)} | {Bench.F(row.list / row.hash)} |");
            r.Line($"Контрольна сума: {sink}."); break;
        }
    }
    private static List<(string name, double list, double hash)> Measure(int scale)
    {
        int n = 100000 / scale, q = 10000 / scale;
        var values = new List<int>(); for (int i = 0; i < n; i++) values.Add(i);
        var set = new HashSet<int>(values); var queries = Data.Numbers(q, 5, 0, n * 2);
        Action linear = () => { int found = 0; foreach (int x in queries) if (Algorithms.Position(values, x) >= 0) found++; sink = found; };
        Action hash = () => { int found = 0; foreach (int x in queries) if (set.Contains(x)) found++; sink = found; };
        var result = new List<(string, double, double)> { ($"{q} перевірок / {n} елементів", Bench.Min(linear), Bench.Min(hash)) };
        var words = new List<string>(); var random = new Random(5);
        for (int i = 0; i < 50000 / scale; i++) words.Add("слово" + random.Next(5000 / scale));
        double l = Bench.Min(() => sink = Algorithms.FrequenciesList(words).keys.Count);
        double h = Bench.Min(() => sink = Algorithms.Frequencies(words).Count);
        result.Add(($"Частоти {words.Count} слів", l, h));
        var numbers = new List<int>(Data.Numbers(20000 / scale, 5, 0, 10000 / scale));
        l = Bench.Min(() => sink = Algorithms.UniqueList(numbers).Count); h = Bench.Min(() => sink = Algorithms.UniqueOrdered(numbers).Count);
        result.Add(($"Дублікати {numbers.Count} чисел", l, h));
        var a = new List<int>(Data.Numbers(5000 / scale, 5, 0, 10000 / scale)); var b = new List<int>(Data.Numbers(5000 / scale, 105, 0, 10000 / scale));
        l = Bench.Min(() => sink = Algorithms.IntersectionList(a, b).Count); h = Bench.Min(() => sink = Algorithms.Intersection(a, b).Count);
        result.Add(($"Перетин {a.Count} + {b.Count}", l, h)); return result;
    }
}

public static class Program
{
    public const string Text = "Алгоритм описує послідовність дій. Алгоритм можна записати словами, а можна записати кодом. Кодом алгоритм стає тоді, коли його виконує машина, а не людина. Одна задача — багато алгоритмів.";
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; var r = new Report(); r.Line("# Результати лабораторної 6"); r.EnvironmentInfo();
        Frequencies(r); Groups(r); Sets(r); Edges(r); Measurements.Run(r);
        
    }
    private static void Frequencies(Report r)
    {
        r.Line("## 1. Частоти довжин слів"); r.Line(Text); r.Line();
        var lengths = new List<int>(); foreach (string word in Algorithms.Words(Text)) lengths.Add(word.Length);
        var d = Algorithms.Frequencies(lengths); var l = Algorithms.FrequenciesList(lengths);
        Bench.Check(d.Count == l.keys.Count, "Кількість довжин");
        foreach (var item in d)
        {
            int i = Algorithms.Position(l.keys, item.Key);
            Bench.Check(i >= 0 && l.counts[i] == item.Value, "Частоти різні");
            if (item.Value >= 2) r.Line($"Довжина {item.Key}: {item.Value}");
        }
        r.Line($"Різних довжин: {d.Count}; слів: {lengths.Count}. Обидві реалізації збігаються."); r.Line();
    }
    private static void Groups(Report r)
    {
        r.Line("## 2. Групування"); var students = Data.Students();
        var d = Algorithms.Group(students); var l = Algorithms.GroupList(students);
        Bench.Check(d.Count == l.keys.Count, "Кількість груп");
        foreach (var item in d)
        {
            int i = Algorithms.Position(l.keys, item.Key);
            Bench.Check(i >= 0 && item.Value.Count == l.groups[i].Count, "Розміри груп");
            for (int j = 0; j < item.Value.Count; j++) Bench.Check(ReferenceEquals(item.Value[j], l.groups[i][j]), "Склад груп");
            Bench.Check(Algorithms.Average(item.Value) == Algorithms.Average(l.groups[i]), "Середній бал");
            r.Line($"{item.Key}: {item.Value.Count} студентів, середній бал {Bench.F(Algorithms.Average(item.Value))}");
        }
        r.Line("Спискова й словникова реалізації збігаються."); r.Line();
    }
    private static void Sets(Report r)
    {
        r.Line("## 3. Дублікати"); var values = new List<int>(Data.Numbers(25, 5, 1, 9));
        var unordered = Algorithms.UniqueUnordered(values); var ordered = Algorithms.UniqueOrdered(values); var list = Algorithms.UniqueList(values);
        Bench.Check(Algorithms.Same(new List<int>(unordered), ordered) && Algorithms.Same(list, ordered), "Унікальні елементи");
        for (int i = 0; i < list.Count; i++) Bench.Check(list[i] == ordered[i], "Порядок");
        r.Line("Вхід: " + string.Join(", ", values)); r.Line("HashSet: " + string.Join(", ", unordered));
        r.Line("Із порядком: " + string.Join(", ", ordered)); r.Line();
        r.Line("## 4. Перетин"); var a = new List<int>(Data.Numbers(30, 5, 1, 23)); var b = new List<int>(Data.Numbers(30, 105, 1, 23));
        var hs = Algorithms.Intersection(a, b); var ls = Algorithms.IntersectionList(a, b);
        Bench.Check(Algorithms.Same(new List<int>(hs), ls), "Перетини різні");
        r.Line("A: " + string.Join(", ", a)); r.Line("B: " + string.Join(", ", b));
        r.Line("Спільні: " + string.Join(", ", hs)); r.Line($"Кількість: {hs.Count}; результати збігаються."); r.Line();
    }
    private static void Edges(Report r)
    {
        r.Line("## Крайові випадки"); r.Line("| Вхід | Очікувано | Отримано |"); r.Line("|---|---|---|");
        Bench.Check(Algorithms.Frequencies(new List<int>()).Count == 0 && Algorithms.FrequenciesList(new List<int>()).keys.Count == 0, "Порожній текст");
        r.Line("| Порожній текст | 0 різних довжин | 0; обидва способи, OK |");
        var words = Algorithms.Words("а ми три"); var lengths = new List<int>(); foreach (var w in words) lengths.Add(w.Length);
        var f = Algorithms.Frequencies(lengths); int shown = 0; foreach (var p in f) if (p.Value >= 2) shown++;
        Bench.Check(shown == 0 && f.Count == 3, "Різні слова");
        r.Line("| Усі слова різні: а ми три | 3 довжини, нічого з частотою ≥2 | 3; 0, OK |");
        var equalLengths = Algorithms.Frequencies(new List<int> { 3, 3 }); Bench.Check(equalLengths.TryGetValue(3, out int count) && count == 2, "Різні слова однакової довжини");
        r.Line("| Різні слова однакової довжини: кіт дім | довжина 3 має частоту 2 | 2, OK |");
        var s = new List<Student> { new Student("А", "Одна", 60), new Student("Б", "Одна", 80) };
        Bench.Check(Algorithms.Group(s).Count == 1 && Algorithms.GroupList(s).groups.Count == 1 && Algorithms.Average(s) == 70, "Одна група");
        r.Line("| Усі студенти в одній групі | 1 група, середній 70 | 1; 70, OK |");
        var unique = new List<int> { 1, 2, 3 }; Bench.Check(Algorithms.UniqueUnordered(unique).Count == 3 && Algorithms.Same(Algorithms.UniqueOrdered(unique), unique) && Algorithms.Same(Algorithms.UniqueList(unique), unique), "Без дублікатів");
        r.Line("| Без дублікатів | 1, 2, 3 | усі способи: 1, 2, 3, OK |");
        var other = new List<int> { 4, 5 }; Bench.Check(Algorithms.Intersection(unique, other).Count == 0 && Algorithms.IntersectionList(unique, other).Count == 0, "Немає перетину");
        r.Line("| Набори не перетинаються | порожній перетин | обидва способи: 0, OK |");
        Bench.Check(Algorithms.Intersection(unique, unique).Count == 3 && Algorithms.IntersectionList(unique, unique).Count == 3, "Однакові набори");
        r.Line("| Однакові набори | 1, 2, 3 | обидва способи: 3 елементи, OK |"); r.Line();
    }
}
