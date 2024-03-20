using Microsoft.AspNetCore.Mvc;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ทดสอบเขียนฟังก์ชันการเชื่อมต่อ database
    [HttpGet("testconnect")]
    public void TestConnection()
    {
        // ถ้าเชื่อมต่อได้จะแสดงข้อความ "Connected"
        if (_context.Database.CanConnect())
        {
            Response.WriteAsync("Connected");
        }
        else
        {
            Response.WriteAsync("Not Connected");
        }
    }

    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าทั้งหมด
    // GET /api/Product
    [HttpGet]
    public ActionResult<product> GetProducts()
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ทั้งหมด
        var products = _context.products.ToList();

        // แบบอ่านที่มีเงื่อนไข
        // var products = _context.products.Where(p => p.unit_price > 45000).ToList();

        // แบบเชื่อมกับตารางอื่น products เชื่อมกับ categories
        // var products = _context.products
        //     .Join(
        //         _context.categories,
        //         p => p.category_id,
        //         c => c.category_id,
        //         (p, c) => new
        //         {
        //             p.product_id,
        //             p.product_name,
        //             p.unit_price,
        //             p.unit_in_stock,
        //             c.category_name
        //         }
        //     ).ToList();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(products);
    }

    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าตาม id
    // GET /api/Product/1
    [HttpGet("{id}")]
    public ActionResult<product> GetProduct(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ตาม id
        var product = _context.products.FirstOrDefault(p => p.product_id == id);

        // แบบเชื่อมกับตารางอื่น products เชื่อมกับ categories
        // var product = _context.products
        //     .Join(
        //         _context.categories,
        //         p => p.category_id,
        //         c => c.category_id,
        //         (p, c) => new
        //         {
        //             p.product_id,
        //             p.product_name,
        //             p.unit_price,
        //             p.unit_in_stock,
        //             c.category_name
        //         }
        //     )
        //     .FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการเพิ่มข้อมูลสินค้า และอัพโหลดรูปภาพ
    // POST /api/Product
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] product product, IFormFile file)
    {
        // ตรวจสอบว่ามีการอัพโหลดรูปภาพหรือไม่
        if (file != null)
        {
            // สร้างชื่อไฟล์ใหม่
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // กำหนด Path ให้กับ uploads folder
            string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // กำหนด Path ให้กับไฟล์
            var filePath = Path.Combine(uploads, fileName);

            // สร้าง uploads folder ถ้าไม่มี
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            // // บันทึกไฟล์ลงใน Path
            await using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // กำหนดชื่อไฟล์ให้กับ product_picture
            product.product_picture = fileName;
        }

        // เพิ่มข้อมูลลงในตาราง Products
        _context.products.Add(product);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);

    }
    

    // ฟังก์ชันสำหรับการแก้ไขข้อมูลสินค้า
    // PUT /api/Product/1
    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id, product product)
    {
        // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.products.FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.product_name = product.product_name;
        existingProduct.unit_price = product.unit_price;
        existingProduct.unit_in_stock = product.unit_in_stock;
        existingProduct.category_id = product.category_id;

        // บันทึกข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(existingProduct);
    }

    // ฟังก์ชันสำหรับการลบข้อมูลสินค้า
    // DELETE /api/Product/1
    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id)
    {
        // ดึงข้อมูลสินค้าตาม id
        var product = _context.products.FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ลบข้อมูล
        _context.products.Remove(product);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }
    
}