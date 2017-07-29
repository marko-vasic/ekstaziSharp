using System;

namespace CatchFilters
{
    public class C
    {
        public void M()
        {
            try
            {
                throw new Exception("Exception");
            }
            catch (Exception e) when (e.Message == "Exception")
            {
                Console.WriteLine("Exception caught");
            }
        }
    }
}