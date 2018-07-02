#region usings

using System;
using System.ServiceProcess;

#endregion

namespace Fourth.Starchef.Packages.Service
{
    internal static class Program
    {
        private static void Main()
        {
            if (Environment.UserInteractive)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new PackageService();
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
            else
            {
                ServiceBase[] servicesToRun = new ServiceBase[]
                {
                    new PackageService()
                };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}