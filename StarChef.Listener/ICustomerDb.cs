using System;
using System.Threading.Tasks;
using System.Xml;

namespace StarChef.Listener
{
    public interface ICustomerDb
    {
        Task<string> GetConnectionString(Guid organisationGuid);
        Task SaveDataToDb(string connectionString, XmlDocument xmlDoc);
    }
}