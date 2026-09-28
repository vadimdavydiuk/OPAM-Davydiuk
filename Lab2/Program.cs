using System;

class Program
{
    static void Main()
    {
        int n = 7;  // Ваш номер у списку
        int k = 19; // Номер першої літери імені в абетці
        
        Console.WriteLine("=== Лабораторна робота 2: Алгоритми обробки масивів ===");
        Console.WriteLine($"Параметри: N = {n}, K = {k}, a = 1, b = 3, c = 2\n");

        // Генерація масиву за варіантом b = 3 (18 чисел у діапазоні [-3; 3])
        int[] arr = GenerateArray(n, 18, -3, 3);

        Console.WriteLine("Початковий масив:");
        PrintArray(arr);
        Console.WriteLine();

        // Завдання 1: Агрегація
        Console.WriteLine("--- Завдання 1: Агрегація за один прохід ---");
        Task1_Aggregation(arr);
        Console.WriteLine();

        // Завдання 2: Фільтрація (критерій a = 1: кратні (K % 4 + 2) = 5)
        Console.WriteLine("--- Завдання 2: Фільтрація (кратні 5) ---");
        int divisor = (k % 4) + 2; // 3 + 2 = 5
        int[] filteredArr = Task2_Filter(arr, x => x != 0 && x % divisor == 0);
        PrintArray(filteredArr);
        Console.WriteLine();

        // Завдання 3: Серії однакових елементів
        Console.WriteLine("--- Завдання 3: Найдовша серія однакових елементів ---");
        Task3_LongestSeries(arr);
        Console.WriteLine();

        // Завдання 4: Перестановка на місці (c = 2: циклічний зсув вправо на 1)
        Console.WriteLine("--- Завдання 4: Циклічний зсув вправо на місці ---");
        Console.WriteLine("До перестановки:");
        PrintArray(arr);
        Task4_ShiftRightInPlace(arr);
        Console.WriteLine("Після перестановки:");
        PrintArray(arr);
        Console.WriteLine();

        // Завдання 5: Матриця (b = 3 -> 6 рядків, 3 стовпці, діапазон [-3; 3])
        Console.WriteLine("--- Завдання 5: Обробка матриці (6x3) ---");
        Task5_MatrixProcessing(n, 6, 3, -3, 3);
    }

    // Генерація масиву через Random(seed)
    static int[] GenerateArray(int seed, int length, int minVal, int maxVal)
    {
        Random rand = new Random(seed);
        int[] arr = new int[length];
        for (int i = 0; i < length; i++)
        {
            arr[i] = rand.Next(minVal, maxVal + 1);
        }
        return arr;
    }

    // Виведення масиву
    static void PrintArray(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("[Масив порожній]");
            return;
        }
        Console.WriteLine(string.Join(", ", arr));
    }

    // Завдання 1: Агрегація за один прохід
    static void Task1_Aggregation(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("Результат не визначений: масив порожній.");
            return;
        }

        long sum = 0;
        int min = arr[0], minIndex = 0;
        int max = arr[0], maxIndex = 0;
        int zerosCount = 0;

        for (int i = 0; i < arr.Length; i++)
        {
            int val = arr[i];
            sum += val;

            if (val < min)
            {
                min = val;
                minIndex = i;
            }

            if (val > max)
            {
                max = val;
                maxIndex = i; // перше входження максимуму
            }

            if (val == 0)
            {
                zerosCount++;
            }
        }

        double average = (double)sum / arr.Length;

        Console.WriteLine($"Сума: {sum}");
        Console.WriteLine($"Середнє арифметичне: {average:F2}");
        Console.WriteLine($"Мінімум: {min} (індекс {minIndex})");
        Console.WriteLine($"Максимум: {max} (індекс {maxIndex})");
        Console.WriteLine($"Кількість нулів: {zerosCount}");
    }

    // Завдання 2: Фільтрація в новий масив точного розміру (два проходи)
    static int[] Task2_Filter(int[] arr, Func<int, bool> predicate)
    {
        if (arr == null || arr.Length == 0) return new int[0];

        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (predicate(arr[i])) count++;
        }

        int[] result = new int[count];
        int index = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (predicate(arr[i]))
            {
                result[index++] = arr[i];
            }
        }

        return result;
    }

    // Завдання 3: Найдовша серія однакових елементів
    static void Task3_LongestSeries(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("Результат не визначений: масив порожній.");
            return;
        }

        int bestValue = arr[0];
        int bestLength = 1;
        int bestStartIndex = 0;

        int currentVal = arr[0];
        int currentLength = 1;
        int currentStartIndex = 0;

        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] == currentVal)
            {
                currentLength++;
            }
            else
            {
                if (currentLength > bestLength)
                {
                    bestLength = currentLength;
                    bestValue = currentVal;
                    bestStartIndex = currentStartIndex;
                }
                currentVal = arr[i];
                currentLength = 1;
                currentStartIndex = i;
            }
        }

        // Перевірка серії, що закінчується на останньому елементі масиву
        if (currentLength > bestLength)
        {
            bestLength = currentLength;
            bestValue = currentVal;
            bestStartIndex = currentStartIndex;
        }

        Console.WriteLine($"Значення: {bestValue}, Довжина серії: {bestLength}, Індекс початку: {bestStartIndex}");
    }

    // Завдання 4: Перестановка на місці (циклічний зсув вправо на 1 позицію)
    static void Task4_ShiftRightInPlace(int[] arr)
    {
        if (arr == null || arr.Length <= 1) return;

        int last = arr[arr.Length - 1];
        for (int i = arr.Length - 1; i > 0; i--)
        {
            arr[i] = arr[i - 1];
        }
        arr[0] = last;
    }

    // Завдання 5: Обробка матриці за рядками та стовпцями
    static void Task5_MatrixProcessing(int seed, int rows, int cols, int minVal, int maxVal)
    {
        Random rand = new Random(seed);
        int[,] matrix = new int[rows, cols];

        Console.WriteLine("Матриця:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = rand.Next(minVal, maxVal + 1);
                Console.Write($"{matrix[i, j],4} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();

        // 1. Суми рядків та пошук рядка з найбільшою сумою
        int maxRowSum = int.MinValue;
        int maxRowIndex = 0;

        Console.WriteLine("Суми рядків:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            int rowSum = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                rowSum += matrix[i, j];
            }
            Console.WriteLine($"Рядок {i}: сума = {rowSum}");

            if (rowSum > maxRowSum)
            {
                maxRowSum = rowSum;
                maxRowIndex = i;
            }
        }
        Console.WriteLine($"=> Рядок з найбільшою сумою: №{maxRowIndex} (сума = {maxRowSum})\n");

        // 2. Максимуми стовпців
        Console.WriteLine("Максимуми стовпців:");
        for (int j = 0; j < matrix.GetLength(1); j++)
        {
            int colMax = matrix[0, j];
            for (int i = 1; i < matrix.GetLength(0); i++)
            {
                if (matrix[i, j] > colMax)
                {
                    colMax = matrix[i, j];
                }
            }
            Console.WriteLine($"Стовпець {j}: максимум = {colMax}");
        }
    }
}