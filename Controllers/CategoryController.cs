using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Controllers;

[Authorize] // กำหนดให้ต้อง Login ก่อนเข้าถึง API นี้
[ApiController] // กำหนดให้ Class นี้เป็น API Controller
[Route("api/[controller]")] // กำหนด Route ของ API Controller
public class CategoryController: ControllerBase
{

    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // ฟังก์ชันสร้าง Constructor รับค่า ApplicationDbContext
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Category ทั้งหมด
    // GET /api/Category
    [HttpGet]
    public ActionResult<category> GetCategories()
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Categories ทั้งหมด
        var categories = _context.categories.ToList();

        // ส่งข้อมูลกลับเป็น JSON
        return Ok(categories);
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Category ตาม ID
    // GET /api/Category/1
    [HttpGet("{id}")]
    public ActionResult<category> GetCategory(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Categories ตาม ID
        var category = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะส่งข้อความกลับว่า "Not Found"
        if (category == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับเป็น JSON
        return Ok(category);
    }

    // ฟังก์ชันสำหรับการเพิ่มข้อมูล Category
    // POST /api/Category
    [HttpPost]
    public ActionResult<category> AddCategory([FromBody] category category)
    {
        // เพิ่มข้อมูลลงในตาราง Categories
        _context.categories.Add(category);
        _context.SaveChanges();

        // ส่งข้อมูลกลับเป็น JSON
        return Ok(category);
    }

    // ฟังก์ชันสำหรับการแก้ไขข้อมูล Category
    // PUT /api/Category/1
    [HttpPut("{id}")]
    public ActionResult<category> UpdateCategory(int id, [FromBody] category category)
    {
        // ค้นหาข้อมูลจากตาราง Categories ตาม ID
        var cat = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะส่งข้อความกลับว่า "Not Found"
        if (cat == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลในตาราง Categories
        cat.category_name = category.category_name;
        _context.SaveChanges();

        // ส่งข้อมูลกลับเป็น JSON
        return Ok(cat);
    }

    // ฟังก์ชันสำหรับการลบข้อมูล Category
    // DELETE /api/Category/1
    [HttpDelete("{id}")]
    public ActionResult<category> DeleteCategory(int id)
    {
        // ค้นหาข้อมูลจากตาราง Categories ตาม ID
        var cat = _context.categories.Find(id);

        // ถ้าไม่พบข้อมูลจะส่งข้อความกลับว่า "Not Found"
        if (cat == null)
        {
            return NotFound();
        }

        // ลบข้อมูลในตาราง Categories
        _context.categories.Remove(cat);
        _context.SaveChanges();

        // ส่งข้อมูลกลับเป็น JSON
        return Ok(cat);
    }


}