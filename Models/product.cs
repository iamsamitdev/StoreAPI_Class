using System;
using System.Collections.Generic;

namespace StoreAPI.Models;

public partial class product
{
    public int product_id { get; set; }

    public int? category_id { get; set; }

    public string? product_name { get; set; }

    public decimal? unit_price { get; set; }

    public string? product_picture { get; set; }

    public int? unit_in_stock { get; set; }

    public DateTime? created_date { get; set; }

    public DateTime? modified_date { get; set; }

    public virtual category? category { get; set; }
}
