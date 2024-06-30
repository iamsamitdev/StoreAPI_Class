using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? UnitinStock { get; set; }

    public string? ProductPicture { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}