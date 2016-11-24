using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThinkLib.Composition;

namespace ThinkLib.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            //LogManager.Default.Debug("starting...");

           var container = Bootstrapper.Current.DoneWithAutofac();

           var instance = container.Resolve<IObjectContainer>();

            Console.ReadKey();
        }
    }
}
