using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models;

public partial class Category
{
    [Key]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int CategoryStatus { get; set; }
}
