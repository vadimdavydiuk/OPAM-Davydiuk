using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Лабораторна робота №3: Рекурсивні алгоритми ===");
        Console.WriteLine("Варіант: N = 5, K = 3, a = 2, b = 1, c = 0\n");

        // --- Завдання 1 ---
        Console.WriteLine("--- Завдання 1: Максимальна глибина вкладеності папок ---");
        Console.Write("Введіть шлях до папки: ");
        string path = Console.ReadLine();

        if (Directory.Exists(path))
        {
            int maxDepth = GetMaxFolderDepth(path, 0);
            Console.WriteLine($"Максимальна глибина вкладеності: {maxDepth}\n");
        }
        else
        {
            Console.WriteLine("Повідомлення: Вказана папка не існує.\n");
        }

        // --- Завдання 2 ---
        Console.WriteLine("--- Завдання 2: Перевірка, чи всі елементи однакові (поділ навпіл) ---");
        int[] array = GenerateRandomArray(16, 5, -20, 20);
        Console.WriteLine("Масив: " + string.Join(", ", array));

        int maxRecDepth = 0;
        bool allEqual = CheckAllEqual(array, 0, array.Length - 1, ref maxRecDepth, 0);
        Console.WriteLine($"Чи всі елементи однакові: {allEqual}");
        Console.WriteLine($"Максимальна глибина рекурсії: {maxRecDepth} (log2(16) = 4)\n");

        // --- Завдання 3 ---
        Console.WriteLine("--- Завдання 3: Перебір усіх перестановок 3 літер («ВАД») ---");
        char[] letters = { 'В', 'А', 'Д' };
        int permCount = 0;
        GeneratePermutations(letters, 0, ref permCount);
        Console.WriteLine($"Загальна кількість перестановок: {permCount}\n");

        // --- Завдання 4 ---
        Console.WriteLine("--- Завдання 4: Трасування рекурсії на 4 елементах ---");
        int[] smallArray = { 5, 5, 5, 5 };
        TraceCheckAllEqual(smallArray, 0, smallArray.Length - 1, 0);
        Console.WriteLine();

        // --- Завдання 5 ---
        Console.WriteLine("--- Завдання 5: Ітеративна версія ---");
        bool iterativeResult = CheckAllEqualIterative(array);
        Console.WriteLine($"Результат ітеративної перевірки: {iterativeResult} (Збігається: {iterativeResult == allEqual})");
    }

    // --- Завдання 1: Рекурсивний обхід папок ---
    static int GetMaxFolderDepth(string path, int currentDepth)
    {
        int maxDepth = currentDepth;
        try
        {
            string[] subDirs = Directory.GetDirectories(path);
            foreach (string dir in subDirs)
            {
                int depth = GetMaxFolderDepth(dir, currentDepth + 1);
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Пропускаємо папки без доступу
        }
        return maxDepth;
    }

    // --- Завдання 2: Поділ навпіл ---
    static int[] GenerateRandomArray(int size, int seed, int minVal, int maxVal)
    {
        Random rand = new Random(seed);
        int[] arr = new int[size];
        for (int i = 0; i < size; i++)
        {
            arr[i] = rand.Next(minVal, maxVal + 1);
        }
        return arr;
    }

    static bool CheckAllEqual(int[] arr, int lo, int hi, ref int maxDepth, int depth)
    {
        if (depth > maxDepth) maxDepth = depth;
        if (lo == hi) return true;

        int mid = lo + (hi - lo) / 2;
        bool left = CheckAllEqual(arr, lo, mid, ref maxDepth, depth + 1);
        bool right = CheckAllEqual(arr, mid + 1, hi, ref maxDepth, depth + 1);

        return left && right && (arr[lo] == arr[mid + 1]);
    }

    // --- Завдання 3: Перестановки літер ---
    static void GeneratePermutations(char[] chars, int index, ref int count)
    {
        if (index == chars.Length - 1)
        {
            Console.WriteLine(new string(chars));
            count++;
            return;
        }

        for (int i = index; i < chars.Length; i++)
        {
            Swap(ref chars[index], ref chars[i]);
            GeneratePermutations(chars, index + 1, ref count);
            Swap(ref chars[index], ref chars[i]); // Скасування вибору (backtrack)
        }
    }

    static void Swap(ref char a, ref char b)
    {
        char temp = a;
        a = b;
        b = temp;
    }

    // --- Завдання 4: Трасування ---
    static bool TraceCheckAllEqual(int[] arr, int lo, int hi, int depth)
    {
        string indent = new string(' ', depth * 2);
        Console.WriteLine($"{indent}enter Check({lo}..{hi})");

        if (lo == hi)
        {
            bool res = true;
            Console.WriteLine($"{indent}exit  Check({lo}..{hi}) -> {res}");
            return res;
        }

        int mid = lo + (hi - lo) / 2;
        bool left = TraceCheckAllEqual(arr, lo, mid, depth + 1);
        bool right = TraceCheckAllEqual(arr, mid + 1, hi, depth + 1);
        bool result = left && right && (arr[lo] == arr[mid + 1]);

        Console.WriteLine($"{indent}exit  Check({lo}..{hi}) -> {result}");
        return result;
    }

    // --- Завдання 5: Ітеративна версія ---
    static bool CheckAllEqualIterative(int[] arr)
    {
        if (arr.Length <= 1) return true;
        int first = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] != first) return false;
        }
        return true;
    }
}