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

    // ฟังก์ชันสำหรับการเพิ่มข้อมูลสินค้า
    // POST: /api/Product
    [HttpPost]
    public async Task<ActionResult<product>> CreateProduct([FromForm] product product, IFormFile image)
    {
        // เพิ่มข้อมูลลงในตาราง Products
        _context.products.Add(product);

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพ
            string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");

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
            product.product_picture = fileName;
        }

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