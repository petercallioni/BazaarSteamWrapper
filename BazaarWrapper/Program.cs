namespace BazaarWrapper
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Usage: BazaarWrapper.exe <launcher exe>");
            }

            MainController mainController = new MainController(args[0]);
            mainController.Start();
        }
    }
}
