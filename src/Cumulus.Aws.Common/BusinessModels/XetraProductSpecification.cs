using System.ComponentModel.DataAnnotations.Schema;

namespace Cumulus.Aws.Common.BusinessModels
{
    public class XetraProductSpecification
    {
        [Column("Market Segment Status")]
        public string MarketSegmentStatus { get; set; }

        [Column("Instrument Status")]
        public string InstrumentStatus { get; set; }

        [Column("instrument")]
        public string Instrument { get; set; }

        [Column("ISIN")]
        public string ISIN { get; set; }

        [Column("Product ID")]
        public string ProductID { get; set; }

        [Column("Instrument ID")]
        public string InstrumentID { get; set; }
    }
}
