using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Controllers;

[Authorize] // กำหนดให้ต้อง Login ก่อนเข้าถึง API นี้
[ApiController] // กำหนดให้ Class นี้เป็น API Controller
[Route("api/[controller]")] // กำหนด Route ของ API Controller
[EnableCors("MultipleOrigins")] // กำหนดให้สามารถเข้าถึง API นี้ได้จากหลายๆ Domain
public class ProductController: ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;
    
    // IWebHostEnvironment คืออะไร
    // IWebHostEnvironment เป็นอินเทอร์เฟซใน ASP.NET Core ที่ใช้สำหรับดึงข้อมูลเกี่ยวกับสภาพแวดล้อมการโฮสต์เว็บแอปพลิเคชัน
    // ContentRootPath: เส้นทางไปยังโฟลเดอร์รากของเว็บแอปพลิเคชัน
    // WebRootPath: เส้นทางไปยังโฟลเดอร์ wwwroot ของเว็บแอปพลิเคชัน
    private readonly IWebHostEnvironment _env;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // [Authorize]
    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าทั้งหมด
    // GET /api/Product
    [HttpGet]
    public ActionResult<Product> GetProducts([FromQuery] int page=1, [FromQuery] int limit=100, [FromQuery] string? searchQuery=null, [FromQuery] int? selectedCategory = null)
    {
        // int totalRecords = _context.products.Count();
        int skip = (page - 1) * limit;

        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ทั้งหมด
        // var products = _context.products.ToList();

        // แบบอ่านที่มีเงื่อนไข
        // var products = _context.products.Where(p => p.unit_price > 45000).ToList();

        // แบบเชื่อมกับตารางอื่น products เชื่อมกับ categories
        var query = _context.Products
            .Join(
                _context.Categories,
                p => p.CategoryId,
                c => c.CategoryId,
                (p, c) => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.UnitPrice,
                    p.UnitinStock,
                    p.ProductPicture,
                    p.CreatedDate,
                    p.ModifiedDate,
                    p.CategoryId,
                    c.CategoryName
                }
            );

        // Apply search query if it is not null or empty
        if (!string.IsNullOrEmpty(searchQuery))
        {
            // query = query.Where(p => p.product_name.Contains(searchQuery));
            query = query.Where(p => EF.Functions.ILike(p.ProductName!, $"%{searchQuery}%"));
        }

        // Apply category filter if it is not null
        if (selectedCategory.HasValue)
        {
            query = query.Where(p => p.CategoryId == selectedCategory.Value);
        }

        // query = query.Where(p => p.unit_in_stock > 100);

        var totalRecords = query.Count(); // Count after filtering

        var products = query
            .OrderByDescending(p => p.ProductId)
            .Skip(skip)
            .Take(limit)
            .ToList();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(new { Total = totalRecords, Products = products });
    }

    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าตาม id
    // GET /api/Product/1
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ตาม id
        // var product = _context.products.FirstOrDefault(p => p.product_id == id);

        // แบบเชื่อมกับตารางอื่น products เชื่อมกับ categories
        var product = _context.Products
            .Join(
                _context.Categories,
                p => p.CategoryId,
                c => c.CategoryId,
                (p, c) => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.UnitPrice,
                    p.UnitinStock,
                    p.ProductPicture,
                    p.CreatedDate,
                    p.ModifiedDate,
                    c.CategoryName
                }
            )
            .FirstOrDefault(p => p.ProductId == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการเพิ่มข้อมูลสินค้า
    // POST: /api/Product
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromForm] Product product, IFormFile? image)
    {
        // เพิ่มข้อมูลลงในตาราง Products
        _context.Products.Add(product);

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพ
            // string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads มีหรือไม่
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // บันทึกชื่อไฟล์รูปภาพลงในฐานข้อมูล
            product.ProductPicture = fileName;
        } else {
            product.ProductPicture = "noimg.jpg";
        }

        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }
    

    // ฟังก์ชันสำหรับการแก้ไขข้อมูลสินค้า
    // PUT /api/Product/1
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] Product product, IFormFile? image)
    {
        // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.Products.FirstOrDefault(p => p.ProductId == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.ProductName = product.ProductName;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.UnitinStock = product.UnitinStock;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.ModifiedDate = product.ModifiedDate;

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพ
            // string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads มีหรือไม่
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // ลบไฟล์รูปภาพเดิม ถ้ามีการอัพโหลดรูปภาพใหม่ และรูปภาพเดิมไม่ใช่ noimg.jpg
            if(existingProduct.ProductPicture != "noimg.jpg"){
                System.IO.File.Delete(Path.Combine(uploadFolder, existingProduct.ProductPicture!));
            }

            // บันทึกชื่อไฟล์รูปภาพลงในฐานข้อมูล
            existingProduct.ProductPicture = fileName;
        }

        // บันทึกข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(existingProduct);
    }

    // ฟังก์ชันสำหรับการลบข้อมูลสินค้า
    // DELETE /api/Product/1
    [HttpDelete("{id}")]
    public ActionResult<Product> DeleteProduct(int id)
    {
        // ดึงข้อมูลสินค้าตาม id
        var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ตรวจสอบว่ามีไฟล์รูปภาพหรือไม่
        if(product.ProductPicture != "noimg.jpg"){
            // string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ลบไฟล์รูปภาพ
            System.IO.File.Delete(Path.Combine(uploadFolder, product.ProductPicture!));
        }

        // ลบข้อมูล
        _context.Products.Remove(product);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }
    
}