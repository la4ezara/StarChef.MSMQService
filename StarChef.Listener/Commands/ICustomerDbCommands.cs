using System;
using System.Threading.Tasks;
using System.Xml;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Commands
{
    public interface IDatabaseCommands
    {
        /// <summary>
        /// Save data to organization database
        /// </summary>
        /// <param name="organisationId">Organization Id</param>
        /// <param name="xmlDoc"></param>
        /// <exception cref="LoginDbNotFoundException">When connection string is not found Login Db</exception>
        /// <exception cref="CustomerDbNotFoundException">When connection string is not found for the given organization Id</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        Task SaveData(Guid organisationId, XmlDocument xmlDoc);
    }
}