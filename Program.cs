// See https://aka.ms/new-console-template for more information
using System.Collections.Concurrent;
using System.Diagnostics;


while (true)
{
    Console.WriteLine("Введите число элементов массива:");
    string? readNumber = Console.ReadLine();
    Console.WriteLine();


    int number;
    bool success = int.TryParse(readNumber, out number);
    if (success == false)
    {
        Console.WriteLine($"{readNumber} не число.");
    }
    else
    {
        int[] array = Enumerable.Range(0, number + 1).ToArray();

        Sum(array);

        LinqSum(array);

        PLinqSum(array);

        ParallelForSum(array);

        ParallelForEachSum(array);

        ParallelPartitionForEach(array);

        if (number <= 10000)
        {
            ThreadSum(array);
        }
        else
        {
            Console.WriteLine("Время выполнения опрерации с использованием List<Thread> слишком большое.");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Продолжить?  да - y, нет - любая другая клавиша");

    string? answer = Console.ReadLine();

    if (answer != "y")
    {
        return;
    }
}




static void Sum(int[] array)
{
    Stopwatch sw = Stopwatch.StartNew();
    long sum = 0;
    for (int i = 0; i < array.Length; i++) { sum += array[i]; }
    sw.Stop();
    Console.WriteLine($"Время выполнения в обычном цикле for:\t\t {sw.Elapsed} мс, сумма = {sum}");
}


static void ThreadSum(int[] array)
{
    Object lockObj = new Object();
    List<Thread> threads = new List<Thread>();

    Stopwatch sw = Stopwatch.StartNew();
    long sum = 0;

    for (int i = 0; i < array.Length; i++)
    {
        int index = i;
        Thread thread = new Thread(() =>
        {
            lock (lockObj)
            {
                sum += array[index];
            }
        });
        threads.Add(thread);
        thread.Start();
    }

    foreach (Thread thread in threads)
    { thread.Join(); }

    sw.Stop();
    Console.WriteLine($"Время выполнения в List<Thread>:\t\t {sw.Elapsed} мс, сумма = {sum}");
}


static void LinqSum(int[] array)
{
    Stopwatch sw = Stopwatch.StartNew();
    long sum = array.Sum(x => (long)x);
    sw.Stop();
    Console.WriteLine($"Время выполнения в Linq:\t\t\t {sw.Elapsed} мс, сумма = {sum}");
}


static void PLinqSum(int[] array)
{
    Stopwatch sw = Stopwatch.StartNew();
    long sum = array.AsParallel().Sum(x => (long)x);
    sw.Stop();
    Console.WriteLine($"Время выполнения в PLinq:\t\t\t {sw.Elapsed} мс, сумма = {sum}");
}




static void ParallelForSum(int[] array)
{
    Stopwatch sw = Stopwatch.StartNew();
    long sum = 0;

    Parallel.For(0, array.Length, () => 0L, (i, loopState, localSum) =>
    {
        localSum += array[i];
        return localSum;
    }, localSum => Interlocked.Add(ref sum, localSum));

    Console.WriteLine($"Время выполнения в Parallel.For:\t\t {sw.Elapsed} мс, сумма = {sum}");
}



static void ParallelForEachSum(int[] array)
{
    Stopwatch sw = Stopwatch.StartNew();
    long sum = 0;

    Parallel.ForEach(array, () => 0L, (i, loopState, localSum) =>
    {
        localSum += array[i];

        return localSum;
    }, localSum => Interlocked.Add(ref sum, localSum));

    Console.WriteLine($"Время выполнения в Parallel.ForEach:\t\t {sw.Elapsed} мс, сумма = {sum}");
}






static void ParallelPartitionForEach(int[] array)
{
    var rangePartitioner = Partitioner.Create(0, array.Length);

    Stopwatch sw = Stopwatch.StartNew();
    long sum = 0;

    Parallel.ForEach(rangePartitioner, () => 0L, (range, loopState, localSum) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
                localSum += array[i];

            return localSum;
        },
        localSum => Interlocked.Add(ref sum, localSum));

    Console.WriteLine($"Время выполнения в Parallel.ForEach(Partition):\t {sw.Elapsed} мс, сумма = {sum}");
}

