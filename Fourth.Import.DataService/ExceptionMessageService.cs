using Fourth.Import.Data;
using Fourth.Import.Model;
using System.Collections.Generic;
using System.Data;

namespace Fourth.Import.DataService
{
    public class ExceptionMessageService : DalBase
    {
        public ExceptionMessageService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }

        public List<ExceptionMessage> GetExceptionMessages()
        {
            List<ExceptionMessage> exceptionMessages = new List<ExceptionMessage>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_exception_message", CommandType.StoredProcedure))
                {
                    while (dr.Read())
                    {
                        exceptionMessages.Add(new ExceptionMessage
                        {
                            MappingName = dr.GetString(0),
                            ValidationName = dr.GetString(1),
                            Message = dr.GetString(2)
                        });
                    }
                }
            }
            return exceptionMessages;
        }
    }
}
