using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThinkLib.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Default.Debug("");

            Bootstrapper.Current.DoneWithUnity();
        }
    }
}
