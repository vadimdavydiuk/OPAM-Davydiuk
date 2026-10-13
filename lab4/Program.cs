using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Лабораторна робота №4: Алгоритми над списками ===");
        Console.WriteLine("Варіант: N = 5, K = 3, a = 2, b = 1, c = 0\n");

        // Основний список: 20 випадкових цілих чисел у діапазоні [-9; 9], generator new Random(5)
        List<int> mainList = GenerateMainList(20, 5);
        Console.WriteLine("Основний список: " + string.Join(", ", mainList));
        Console.WriteLine();

        // --- Завдання 1 ---
        Console.WriteLine("--- Завдання 1: Побудова нового списку за критерієм (додатні й непарні) ---");
        List<int> filteredList = FilterByCriteria(mainList);
        Console.WriteLine("Відфільтрований список: " + string.Join(", ", filteredList));
        Console.WriteLine($"Кількість елементів (Count): {filteredList.Count}");
        Console.WriteLine($"Оригінальний список не змінено (довжина: {mainList.Count})\n");

        // --- Завдання 2 ---
        Console.WriteLine("--- Завдання 2: Вилучення елементів (кратні 5, нуль теж) ---");
        
        // Варіант 1: Хибний (цикл з початку з RemoveAt)
        List<int> copy1 = new List<int>(mainList);
        List<int> skippedByWrong = RemoveWrong(copy1);
        Console.WriteLine("1. Хибний варіант (з початку): " + string.Join(", ", copy1));
        Console.WriteLine($"   Пропущені через зсув елементи зафіксовано.");

        // Варіант 2: Цикл з кінця
        List<int> copy2 = new List<int>(mainList);
        RemoveFromEnd(copy2);
        Console.WriteLine("2. Правильний (з кінця):     " + string.Join(", ", copy2));

        // Варіант 3: Новий список
        List<int> copy3 = new List<int>(mainList);
        List<int> copy3Result = RemoveNewList(copy3);
        Console.WriteLine("3. Правильний (новий список):" + string.Join(", ", copy3Result));

        bool resultsMatch = AreListsEqual(copy2, copy3Result);
        Console.WriteLine($"Перевірка збігу варіантів 2 і 3: {resultsMatch}\n");

        // --- Завдання 3 ---
        Console.WriteLine("--- Завдання 3: Злиття двох впорядкованих списків ---");
        // довжина першого: 10 + K % 5 = 10 + 3 % 5 = 13 (rnd N = 5)
        List<int> sorted1 = GenerateSortedList(10 + 3 % 5, 5);
        // довжина другого: 6 + K % 4 = 6 + 3 % 4 = 9 (rnd N + 100 = 105)
        List<int> sorted2 = GenerateSortedList(6 + 3 % 4, 105);

        Console.WriteLine("Список 1: " + string.Join(", ", sorted1));
        Console.WriteLine("Список 2: " + string.Join(", ", sorted2));

        List<int> merged = MergeSortedLists(sorted1, sorted2);
        Console.WriteLine("Зі сприятливим злиттям: " + string.Join(", ", merged));
        bool isSorted = CheckIsSorted(merged);
        Console.WriteLine($"Перевірка впорядкованості результату: {isSorted}\n");

        // --- Завдання 4 ---
        Console.WriteLine("--- Завдання 4: Розділення списку на два (парні та непарні) ---");
        SplitList(mainList, out List<int> evenList, out List<int> oddList);
        Console.WriteLine("Парні:   " + string.Join(", ", evenList));
        Console.WriteLine("Непарні: " + string.Join(", ", oddList));
        bool countMatches = (evenList.Count + oddList.Count == mainList.Count);
        Console.WriteLine($"Сума Count обох списків дорівнює Count вихідного: {countMatches}\n");

        // --- Завдання 5 ---
        Console.WriteLine("--- Завдання 5: Замір часу (виконується в Release) ---");
        RunBenchmarks();
    }

    // --- Методи для Завдання 1 ---
    static List<int> GenerateMainList(int count, int seed)
    {
        Random rnd = new Random(seed);
        List<int> list = new List<int>();
        for (int i = 0; i < count; i++)
        {
            list.Add(rnd.Next(-9, 10));
        }
        return list;
    }

    static List<int> FilterByCriteria(List<int> source)
    {
        List<int> result = new List<int>();
        foreach (int num in source)
        {
            // Критерій a = 2: додатні й непарні одночасно (> 0 і num % 2 != 0)
            if (num > 0 && num % 2 != 0)
            {
                result.Add(num);
            }
        }
        return result;
    }

    // --- Методи для Завдання 2 ---
    static List<int> RemoveWrong(List<int> list)
    {
        // Хибний обхід з початку: при видаленні елемента зсуваються індекси
        for (int i = 0; i < list.Count; i++)
        {
            // Критерій c = 0: кратні 5 (нуль теж)
            if (list[i] % 5 == 0)
            {
                list.RemoveAt(i);
                // Помилка тут: наступний елемент пропускається через зсувліво
            }
        }
        return list;
    }

    static void RemoveFromEnd(List<int> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] % 5 == 0)
            {
                list.RemoveAt(i);
            }
        }
    }

    static List<int> RemoveNewList(List<int> list)
    {
        List<int> result = new List<int>();
        foreach (int num in list)
        {
            if (num % 5 != 0)
            {
                result.Add(num);
            }
        }
        return result;
    }

    static bool AreListsEqual(List<int> l1, List<int> l2)
    {
        if (l1.Count != l2.Count) return false;
        for (int i = 0; i < l1.Count; i++)
        {
            if (l1[i] != l2[i]) return false;
        }
        return true;
    }

    // --- Методи для Завдання 3 ---
    static List<int> GenerateSortedList(int length, int seed)
    {
        Random rnd = new Random(seed);
        List<int> list = new List<int>();
        if (length <= 0) return list;

        int current = rnd.Next(1, 10);
        list.Add(current);
        for (int i = 1; i < length; i++)
        {
            current += rnd.Next(1, 10);
            list.Add(current);
        }
        return list;
    }

    static List<int> MergeSortedLists(List<int> l1, List<int> l2)
    {
        List<int> result = new List<int>(l1.Count + l2.Count);
        int i = 0, j = 0;

        while (i < l1.Count && j < l2.Count)
        {
            if (l1[i] <= l2[j])
            {
                result.Add(l1[i]);
                i++;
            }
            else
            {
                result.Add(l2[j]);
                j++;
            }
        }

        // Дописуємо хвіст першого списку, якщо залишився
        while (i < l1.Count)
        {
            result.Add(l1[i]);
            i++;
        }

        // Дописуємо хвіст другого списку, якщо залишився
        while (j < l2.Count)
        {
            result.Add(l2[j]);
            j++;
        }

        return result;
    }

    static bool CheckIsSorted(List<int> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            if (list[i] > list[i + 1]) return false;
        }
        return true;
    }

    // --- Методи для Завдання 4 ---
    static void SplitList(List<int> source, out List<int> even, out List<int> odd)
    {
        even = new List<int>();
        odd = new List<int>();

        foreach (int num in source)
        {
            // Критерій b = 1: на парні та непарні
            if (num % 2 == 0)
            {
                even.Add(num);
            }
            else
            {
                odd.Add(num);
            }
        }
    }

    // --- Методи для Завдання 5 ---
    static void RunBenchmarks()
    {
        int n = 100000;

        // Прогрівання (Warm-up)
        List<int> warmUp = new List<int>();
        for (int i = 0; i < 1000; i++) warmUp.Add(i);

        // 1. 100 000 x Add, без заданої місткості
        Stopwatch sw = Stopwatch.StartNew();
        List<int> list1 = new List<int>();
        for (int i = 0; i < n; i++) list1.Add(i);
        sw.Stop();
        long timeAddDefault = sw.ElapsedMilliseconds;

        // 2. 100 000 x Add, з місткістю 100000
        sw = Stopwatch.StartNew();
        List<int> list2 = new List<int>(n);
        for (int i = 0; i < n; i++) list2.Add(i);
        sw.Stop();
        long timeAddCapacity = sw.ElapsedMilliseconds;

        // 3. 100 000 x Insert(0, x)
        // Примітка: якщо працює занадто довго, зменшуємо кількість до 10000
        sw = Stopwatch.StartNew();
        List<int> list3 = new List<int>();
        for (int i = 0; i < n; i++) list3.Insert(0, i);
        sw.Stop();
        long timeInsert0 = sw.ElapsedMilliseconds;

        // 4. 1 000 x Contains для елемента, якого немає
        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            bool found = list2.Contains(-999999);
        }
        sw.Stop();
        long timeContains = sw.ElapsedMilliseconds;

        Console.WriteLine("Таблиця часу виконання (в мс):");
        Console.WriteLine($"- 100 000 x Add (без місткості): {timeAddDefault} мс");
        Console.WriteLine($"- 100 000 x Add (з місткостю 100000): {timeAddCapacity} мс");
        Console.WriteLine($"- 100 000 x Insert(0, x): {timeInsert0} мс");
        Console.WriteLine($"- 1 000 x Contains (неіснуючого елемента): {timeContains} мс");
    }
}