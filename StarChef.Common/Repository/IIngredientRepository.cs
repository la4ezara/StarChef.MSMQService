namespace StarChef.Common.Repository
{
	public interface IIngredientRepository
	{
		void RunRankReorder(int product_id, string connectionString);
	}
}
