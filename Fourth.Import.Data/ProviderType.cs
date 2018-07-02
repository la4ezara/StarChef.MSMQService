namespace Fourth.Import.Data
{
    public static class ProviderType
    {
        public static string Sql
        {
            get { return "System.Data.SqlClient"; }
        }

        public static string OleDb { get { return "System.Data.OleDb"; } }
    }
}