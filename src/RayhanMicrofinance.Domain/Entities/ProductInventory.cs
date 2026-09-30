using RayhanMicrofinance.Domain.Common;

namespace RayhanMicrofinance.Domain.Entities;

public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? GstOrTaxNumber { get; set; }
}

public class Product : BaseEntity
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int CurrentStock { get; set; } = 0;
    public int MinStockAlert { get; set; } = 5;
    public string? Description { get; set; }
}

public class ProductPurchase : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
}

public class ProductStockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int FromBranchId { get; set; }
    public Branch? FromBranch { get; set; }

    public int ToBranchId { get; set; }
    public Branch? ToBranch { get; set; }

    public int Quantity { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Completed"; // Pending, Completed
    public string? Remarks { get; set; }
}
