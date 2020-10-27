using StarChef.BackgroundServices.Common.Attributes;
using System;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    [CustomDisableConcurrentExecution(timeoutInSeconds: 300)]
    public interface ICreatePackageJob
    {
        void Process(int databaseId, int packageId, int userId, Guid uniqueIdent);
    }
}
