using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Лабораторна робота №5: Алгоритми на стеку та черзі ===");
        Console.WriteLine("Варіант: N = 5, K = 3, a = 2, b = 1, c = 0\n");

        // --- Завдання 1: Збалансованість дужок ---
        Console.WriteLine("--- Завдання 1: Перевірка дужок ---");
        string[] testStrings = { "(a[b]{c})", "([)]", ")(", "((", "" };
        string customTest = "(((]";
        
        foreach (string s in testStrings)
        {
            var res = CheckBrackets(s);
            Console.WriteLine($"Рядок: \"{s}\" -> Збалансований: {res.isBalanced}, Індекс помилки: {res.errorIndex}");
        }
        var customRes = CheckBrackets(customTest);
        Console.WriteLine($"Рядок з K дужками: \"{customTest}\" -> Збалансований: {customRes.isBalanced}, Індекс помилки: {customRes.errorIndex}\n");

        // --- Завдання 2: Зворотний польський запис ---
        Console.WriteLine("--- Завдання 2: Зворотний польський запис (ЗПЗ) ---");
        string[] rpnTests = { "3 4 + 2 *", "5 3 -", "8 2 /", "2 3 4 * +", "5 0 /", "7 2 %" };
        foreach (string expr in rpnTests)
        {
            double val = EvaluateRPN(expr, out bool success, out string errMessage);
            if (success)
                Console.WriteLine($"Вираз: \"{expr}\" = {val}");
            else
                Console.WriteLine($"Вираз: \"{expr}\" -> Помилка: {errMessage}");
        }
        
        double myVal = EvaluateRPN("1 2 +", out _, out _);
        Console.WriteLine($"Власний вираз на K (1 2 +) -> результат: {myVal}\n");

        // --- Завдання 3: Модель обслуговування ---
        Console.WriteLine("--- Завдання 3: Модель обслуговування (1 обслуговувач) ---");
        RunServiceSimulation(5);
        Console.WriteLine();

        // --- Завдання 4: Власний стек поверх масиву ---
        Console.WriteLine("--- Завдання 4: Власний стек IntStack ---");
        IntStack stack = new IntStack(3 % 4 + 2);
        for (int i = 1; i <= (3 % 4 + 3); i++)
        {
            stack.Push(i * 10);
            Console.WriteLine($"Push({i * 10}) -> Count = {stack.Count}, Capacity = {stack.Capacity}");
        }

        CharStack charStack = new CharStack(10);
        bool charStackTest = CheckBracketsCustomStack("(a[b]{c})", charStack);
        Console.WriteLine($"Перевірка завдання 1 на власному стеку для \"(a[b]{{c}})\": {charStackTest}\n");

    }

    static (bool isBalanced, int errorIndex) CheckBrackets(string s)
    {
        Stack<char> stack = new Stack<char>();
        bool inQuotes = false;
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            if (ch == '"') { inQuotes = !inQuotes; continue; }
            if (inQuotes) continue;
            if (ch == '(' || ch == '[' || ch == '{') stack.Push(ch);
            else if (ch == ')' || ch == ']' || ch == '}')
            {
                if (stack.Count == 0) return (false, i);
                char top = stack.Pop();
                if ((ch == ')' && top != '(') || (ch == ']' && top != '[') || (ch == '}' && top != '{'))
                    return (false, i);
            }
        }
        if (stack.Count > 0) return (false, s.Length - 1);
        return (true, -1);
    }

    static double EvaluateRPN(string expression, out bool success, out string errorMessage)
    {
        success = true;
        errorMessage = "";
        Stack<double> stack = new Stack<double>();
        string[] tokens = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string token in tokens)
        {
            if (double.TryParse(token, out double number)) stack.Push(number);
            else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
            {
                if (stack.Count < 2) { success = false; errorMessage = "Замало операндів."; return 0; }
                double b = stack.Pop();
                double a = stack.Pop();
                double res = 0;
                if (token == "+") res = a + b;
                else if (token == "-") res = a - b;
                else if (token == "*") res = a * b;
                else if (token == "/")
                {
                    if (b == 0) { success = false; errorMessage = "Ділення на нуль."; return 0; }
                    res = a / b;
                }
                else if (token == "%")
                {
                    if (b == 0) { success = false; errorMessage = "Остача від ділення на нуль."; return 0; }
                    res = a % b;
                }
                stack.Push(res);
            }
        }
        if (stack.Count != 1) { success = false; errorMessage = "Некоректна кількість елементів."; return 0; }
        return stack.Pop();
    }

    static void RunServiceSimulation(int seed)
    {
        int totalRequests = 8 + 3 % 5;
        Random rnd = new Random(seed);
        Queue<Request> queue = new Queue<Request>();
        for (int i = 1; i <= totalRequests; i++) queue.Enqueue(new Request(i, rnd.Next(1, 10)));
        
        int currentTime = 0;
        int maxQueueLength = queue.Count;
        Console.WriteLine($"Номер\tВага\tПочаток\tКінець");
        while (queue.Count > 0)
        {
            if (queue.Count > maxQueueLength) maxQueueLength = queue.Count;
            Request req = queue.Dequeue();
            int startTime = currentTime;
            int endTime = startTime + req.WorkUnits;
            currentTime = endTime;
            Console.WriteLine($"{req.Id}\t{req.WorkUnits}\t{startTime}\t{endTime}");
        }
        Console.WriteLine($"Максимальна довжина черги: {maxQueueLength}");
    }

    static bool CheckBracketsCustomStack(string s, CharStack stack)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            if (ch == '(' || ch == '[' || ch == '{') stack.Push(ch);
            else if (ch == ')' || ch == ']' || ch == '}')
            {
                if (stack.Count == 0) return false;
                char top = stack.Pop();
                if ((ch == ')' && top != '(') || (ch == ']' && top != '[') || (ch == '}' && top != '{')) return false;
            }
        }
        return stack.Count == 0;
    }
}

class Request
{
    public int Id { get; }
    public int WorkUnits { get; }
    public Request(int id, int workUnits) { Id = id; WorkUnits = workUnits; }
}

class IntStack
{
    private int[] data;
    private int top = -1;
    public IntStack(int capacity) { data = new int[capacity]; }
    public void Push(int x)
    {
        if (top == data.Length - 1)
        {
            int[] newData = new int[data.Length * 2];
            Array.Copy(data, newData, data.Length);
            data = newData;
        }
        data[++top] = x;
    }
    public int Pop()
    {
        if (top == -1) throw new InvalidOperationException("Стек порожній.");
        return data[top--];
    }
    public int Count => top + 1;
    public int Capacity => data.Length;
}

class CharStack
{
    private char[] data;
    private int top = -1;
    public CharStack(int capacity) { data = new char[capacity]; }
    public void Push(char x)
    {
        if (top == data.Length - 1)
        {
            char[] newData = new char[data.Length * 2];
            Array.Copy(data, newData, data.Length);
            data = newData;
        }
        data[++top] = x;
    }
    public char Pop()
    {
        if (top == -1) throw new InvalidOperationException("Стек порожній.");
        return data[top--];
    }
    public int Count => top + 1;
}