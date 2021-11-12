using System;
using System.Collections.Generic;
using System.Text;

namespace ValutaAPI
{
    public class ValutaKurs
    {
        public ValutaKurs(int? id, string fromCurrency, string toCurrency, DateTime updatedAt, decimal rate)
        {
            Id = id;
            FromCurrency = fromCurrency;
            ToCurrency = toCurrency;
            UpdatedAt = updatedAt;
            Rate = rate;
        }
        public int? Id { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal Rate { get; set; }
    }
}
