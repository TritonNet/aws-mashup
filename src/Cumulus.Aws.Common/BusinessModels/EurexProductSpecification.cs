using System.ComponentModel.DataAnnotations.Schema;

namespace Cumulus.Aws.Common.BusinessModels
{
    public class EurexProductSpecification
    {
        [Column("PRODUCT_ID")]
        public string ProductID { get; set; }

        [Column("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [Column("PRODUCT_NAME")]
        public string ProductName { get; set; }

        [Column("PRODUCT_GROUP")]
        public string ProductGroup { get; set; }

        [Column("CURRENCY")]
        public string Currency { get; set; }

        [Column("PRODUCT_ISIN")]
        public string ProductISIN { get; set; }

        [Column("UNDERLYING_ISIN")]
        public string UnderlyingISIN { get; set; }

        [Column("SHARE_ISIN")]
        public string ShareISIN { get; set; }

        [Column("COUNTRY_CODE")]
        public string CountryCode { get; set; }

        [Column("CASH_MARKET_ID")]
        public string CashMarketID { get; set; }
    }
}
