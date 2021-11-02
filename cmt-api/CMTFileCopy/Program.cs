using System.ServiceProcess;

namespace CMTFileCopy
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CMTFileCopy()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
