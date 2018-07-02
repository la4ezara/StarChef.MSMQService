namespace Fourth.Import.Exceptions
{
    public enum ExceptionType
    {
        None,
        MandatoryInTable,
        LookupMissing,
        DuplicateSupplierCode,
        DuplicateDistributorCode,
        DbSettingIsMandatory,
        DbSettingMinimumValue,
        DbSettingMaximumValue,
        DbSettingRegEx,
        DbSettingMaxLength,
        CategoryIsNotValid,
        NoRecordExistForUpdate,
        InvalidDateFormat,
        InvalidFutureDate,
        InvalidPConversionUnit,
        CategoryRequired
    }
}