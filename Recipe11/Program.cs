using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe11
{
    class Program
    {
static void Main(string[] args)
{
    var t = new Thread(FaultyThread);
    t.Start();
    t.Join();

    try
    {
        t = new Thread(BadFaultyThread);
        t.Start();
        t.Join();
    }
    catch (Exception ex)
    {
        Console.WriteLine("We won't get here!");
    }
}

static void BadFaultyThread()
{
    Console.WriteLine("Starting a faulty thread...");
    Thread.Sleep(TimeSpan.FromSeconds(2));
    throw new Exception("Boom!");
}

static void FaultyThread()
{
    try
    {
        Console.WriteLine("Starting a faulty thread...");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        throw new Exception("Boom!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception handled: {ex.Message}");
    }
}
    }
}
