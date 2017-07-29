using System;

namespace CatchBlock
{
    public class C
    {
        public int M()
        {
            try
            {
                int dividor = 0;
                return 5 / dividor;
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine(e);
                return 2;
            }
        }
    }
}