using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Demo2;

/// <summary>
/// This class is from Microsoft Semantic Kernel repo:
/// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/FunctionCalling/MultipleFunctionsVsParameters.cs
/// </summary>
public sealed class InvoiceSearchBy
{
    [KernelFunction]
    [Description("Search for invoices by customer name.")]
    public IEnumerable<Invoice> SearchByCustomerName([Description("The customer name.")] string customerName)
    {
        // Use a hardcoded list of invoices for demonstration purposes
        // In a real-world scenario, you would likely query a database or an external service
        return
        [
            new Invoice { CustomerName = customerName, PurchaseOrder = "PO123", VendorNumber = "VN123" },
            new Invoice { CustomerName = customerName, PurchaseOrder = "PO124", VendorNumber = "VN124" },
            new Invoice { CustomerName = customerName, PurchaseOrder = "PO125", VendorNumber = "VN125" },
        ];
    }

    [KernelFunction]
    [Description("Search for invoices by purchase order.")]
    public IEnumerable<Invoice> SearchByPurchaseOrder(
        [Description("The purchase order. Purchase orders begin with a PO prefix.")] string purchaseOrder)
    {
        return
        [
            new Invoice { CustomerName = "Customer1", PurchaseOrder = purchaseOrder, VendorNumber = "VN123" },
            new Invoice { CustomerName = "Customer2", PurchaseOrder = purchaseOrder, VendorNumber = "VN124" },
            new Invoice { CustomerName = "Customer3", PurchaseOrder = purchaseOrder, VendorNumber = "VN125" },
        ];
    }

    [KernelFunction]
    [Description("Search for invoices by vendor number")]
    public IEnumerable<Invoice> SearchByVendorNumber(
        [Description("The vendor number. Vendor numbers begin with a VN prefix.")] string vendorNumber)
    {
        return
        [
            new Invoice { CustomerName = "Customer1", PurchaseOrder = "PO123", VendorNumber = vendorNumber },
            new Invoice { CustomerName = "Customer2", PurchaseOrder = "PO124", VendorNumber = vendorNumber },
            new Invoice { CustomerName = "Customer3", PurchaseOrder = "PO125", VendorNumber = vendorNumber },
        ];
    }
}

/// <summary>
/// A plugin that provides methods to search for Invoices using different criteria.
/// </summary>
public sealed class InvoiceSearch
{
    [KernelFunction]
    [Description("Search for invoices by customer name or purchase order or vendor number.")]
    public IEnumerable<Invoice> Search(
        [Description(
            "The invoice search request. It must contain either a customer name or a purchase order or a vendor number")]
        InvoiceSearchRequest searchRequest)
    {
        return
        [
            new Invoice
            {
                CustomerName = searchRequest.CustomerName ?? "Customer1",
                PurchaseOrder = searchRequest.PurchaseOrder ?? "PO123",
                VendorNumber = searchRequest.VendorNumber ?? "VN123"
            },
            new Invoice
            {
                CustomerName = searchRequest.CustomerName ?? "Customer2",
                PurchaseOrder = searchRequest.PurchaseOrder ?? "PO124",
                VendorNumber = searchRequest.VendorNumber ?? "VN124"
            },
            new Invoice
            {
                CustomerName = searchRequest.CustomerName ?? "Customer3",
                PurchaseOrder = searchRequest.PurchaseOrder ?? "PO125",
                VendorNumber = searchRequest.VendorNumber ?? "VN125"
            },
        ];
    }
}

/// <summary>
/// Represents an invoice.
/// </summary>
public sealed class Invoice
{
    public string CustomerName { get; set; } = string.Empty;
    public string PurchaseOrder { get; set; } = string.Empty;
    public string VendorNumber { get; set; } = string.Empty;
}

/// <summary>
/// Represents an invoice search request.
/// </summary>
[Description("The invoice search request.")]
public sealed class InvoiceSearchRequest
{
    [Description("Optional, customer name.")]
    public string? CustomerName { get; set; }
    [Description("Optional, purchase order. Purchase orders begin with a PN prefix.")]
    public string? PurchaseOrder { get; set; }
    [Description("Optional, vendor number. Vendor numbers begin with a VN prefix.")]
    public string? VendorNumber { get; set; }
}