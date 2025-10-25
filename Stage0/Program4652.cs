partial class Program
{
    private static void Main(string[] args)
    {
        Welcome4652();
        Welcome2851();
        Console.ReadKey();
    }
    
    static partial void Welcome2851();
    private static void Welcome4652()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        Console.Write("{0}, welcome to my first console application", name);
    }
}