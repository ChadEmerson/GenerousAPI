
namespace GenerousAPI.BusinessEntities
{
    /// <summary>
    /// Defines the various financial transaction types.
    /// NOTE: The TransactionMode, PaymentMode, and PaymentMethod enumerations are confusingly interchangeable with similarly named fields throughout
    /// the system and database. Refer to the Payment System Wiki for a disambiguation of these enumerations.
    /// https://docs.google.com/document/d/1qDAyeIcqIm5EZhqwu41Mh2ELeHfh0uNdBgKHTVl9SJU/edit
    /// </summary>
    public enum TransactionMode
    {
        Unknown,

        Debit,

        Credit,

        ACH,

        Adjustment,

        Cash,

        Check,

        Refund,

        Other
    }
}