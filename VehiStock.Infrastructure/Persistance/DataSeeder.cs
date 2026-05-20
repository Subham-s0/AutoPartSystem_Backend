using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Persistance;

public static class DataSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext dbContext)
    {
        // 1. Seed Part Categories
        var filtersCategory = await dbContext.PartCategories.FirstOrDefaultAsync(c => c.Name == "Filters");
        if (filtersCategory == null)
        {
            filtersCategory = new PartCategory
            {
                Name = "Filters",
                Description = "Filtering systems and components"
            };
            dbContext.PartCategories.Add(filtersCategory);
            await dbContext.SaveChangesAsync();
        }

        var generalCategory = await dbContext.PartCategories.FirstOrDefaultAsync(c => c.Name == "General");
        if (generalCategory == null)
        {
            generalCategory = new PartCategory
            {
                Name = "General",
                Description = "General vehicle components and accessories"
            };
            dbContext.PartCategories.Add(generalCategory);
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Vendors
        var vendorAlisa = await dbContext.Vendors.FirstOrDefaultAsync(v => v.VendorCode == "ACM-01");
        if (vendorAlisa == null)
        {
            vendorAlisa = new Vendor
            {
                VendorCode = "ACM-01",
                VendorName = "alisa",
                ContactPerson = "sushmita adhikari",
                Phone = "9813915588",
                Email = "adhikarisushmita812@gmail.com",
                Address = "Kathmandu",
                IsActive = true
            };
            dbContext.Vendors.Add(vendorAlisa);
        }
        else
        {
            vendorAlisa.VendorName = "alisa";
            vendorAlisa.ContactPerson = "sushmita adhikari";
            vendorAlisa.Phone = "9813915588";
            vendorAlisa.Email = "adhikarisushmita812@gmail.com";
        }

        var vendorSushmita = await dbContext.Vendors.FirstOrDefaultAsync(v => v.VendorCode == "100");
        if (vendorSushmita == null)
        {
            vendorSushmita = new Vendor
            {
                VendorCode = "100",
                VendorName = "sushmita adhikari",
                ContactPerson = "sushmita adhikari",
                Phone = "9749340467",
                Email = "sushmitaadhikari012@gmail.com",
                Address = "Kathmandu",
                IsActive = true
            };
            dbContext.Vendors.Add(vendorSushmita);
        }
        else
        {
            vendorSushmita.VendorName = "sushmita adhikari";
            vendorSushmita.ContactPerson = "sushmita adhikari";
            vendorSushmita.Phone = "9749340467";
            vendorSushmita.Email = "sushmitaadhikari012@gmail.com";
        }
        await dbContext.SaveChangesAsync();

        // 3. Seed Parts
        var existingOilFilters = await dbContext.Parts.Where(p => p.PartCode == "PRK-OIL").ToListAsync();
        if (existingOilFilters.Count < 2)
        {
            if (existingOilFilters.Any())
            {
                dbContext.Parts.RemoveRange(existingOilFilters);
                await dbContext.SaveChangesAsync();
            }

            var oilFilter1 = new Part
            {
                PartCategoryId = filtersCategory.PartCategoryId,
                PartCode = "PRK-OIL",
                PartName = "Oil-Filter",
                Brand = "BOSC",
                UnitCost = 1700m,
                UnitPrice = 2000m,
                StockQty = 0,
                MinimumStock = 2,
                IsActive = true
            };

            var oilFilter2 = new Part
            {
                PartCategoryId = filtersCategory.PartCategoryId,
                PartCode = "PRK-OIL",
                PartName = "Oil-Filter",
                Brand = "BOSC",
                UnitCost = 1700m,
                UnitPrice = 2000m,
                StockQty = 1,
                MinimumStock = 2,
                IsActive = true
            };

            dbContext.Parts.Add(oilFilter1);
            dbContext.Parts.Add(oilFilter2);
        }

        var bbbPart = await dbContext.Parts.FirstOrDefaultAsync(p => p.PartCode == "pkoo");
        if (bbbPart == null)
        {
            bbbPart = new Part
            {
                PartCategoryId = generalCategory.PartCategoryId,
                PartCode = "pkoo",
                PartName = "bbb",
                Brand = "bbbbbbbbb",
                UnitCost = 100m,
                UnitPrice = 1220m,
                StockQty = 2,
                MinimumStock = 3,
                IsActive = true
            };
            dbContext.Parts.Add(bbbPart);
        }
        else
        {
            bbbPart.PartName = "bbb";
            bbbPart.Brand = "bbbbbbbbb";
            bbbPart.UnitCost = 100m;
            bbbPart.UnitPrice = 1220m;
            bbbPart.StockQty = 2;
            bbbPart.MinimumStock = 3;
        }
        await dbContext.SaveChangesAsync();

        var seededBbbPart = await dbContext.Parts.FirstOrDefaultAsync(p => p.PartCode == "pkoo");

        // 4. Seed Purchase Invoice
        var existingInvoice = await dbContext.PurchaseInvoices.FirstOrDefaultAsync(i => i.InvoiceNo == "INVO001");
        if (existingInvoice == null)
        {
            var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@vehistock.com") 
                            ?? await dbContext.Users.FirstOrDefaultAsync();

            if (adminUser != null && seededBbbPart != null)
            {
                var invoice = new PurchaseInvoice
                {
                    VendorId = vendorSushmita.VendorId,
                    InvoiceNo = "INVO001",
                    PurchaseDate = new DateOnly(2026, 5, 19),
                    Subtotal = 1008m,
                    TaxAmount = 0m,
                    DiscountAmount = 0m,
                    TotalAmount = 1008m,
                    PaymentStatus = PaymentStatus.Paid,
                    CreatedByUserId = adminUser.Id,
                    Notes = "Initial purchase seed"
                };

                dbContext.PurchaseInvoices.Add(invoice);
                await dbContext.SaveChangesAsync();

                var invoiceItem = new PurchaseInvoiceItem
                {
                    PurchaseInvoiceId = invoice.PurchaseInvoiceId,
                    PartId = seededBbbPart.PartId,
                    Quantity = 1,
                    UnitCost = 1008m,
                    LineTotal = 1008m
                };

                dbContext.PurchaseInvoiceItems.Add(invoiceItem);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
