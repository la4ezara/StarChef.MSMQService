namespace Fourth.Import.Model
{
    public enum ImportStatus
    {
        FileUploaded = 1,
        Processing,
        ProcessedNewIngredient,
        FailedInvalidVersion,
        FailedInvalidTemplate,
        ProcessedPriceUpdate,
        ProcessedIntoleranceUpdate,
        ProcessedNutritionUpdate,
        ProcessedSuppIntoleranceUpdate,
        FailedInvalidFile
    }
}