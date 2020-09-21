
using System.Data.SqlClient;

namespace StarChef.Common.Repository
{
	public class IngredientRepository : IIngredientRepository
	{
		private readonly IDatabaseManager _databaseManager;
		public IngredientRepository(IDatabaseManager databaseManager)
		{
			_databaseManager = databaseManager;
		}

		public void RunRankReorder(int product_id, string connectionString)
		{
			_databaseManager.Execute(connectionString, "sc_product_run_rankreorder", true, new SqlParameter("@product_id", product_id));
		}
	}
}
