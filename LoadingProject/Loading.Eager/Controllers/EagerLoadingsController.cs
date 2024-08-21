using Loading.Eager.Contexts;
using Loading.Eager.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Loading.Eager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EagerLoadingsController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public EagerLoadingsController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStudentsWithGroup()
    {
        await SeedData();

        //Lazy Loading Sql Query generate olan zaman avtomatik şəkildə dataları gətirsə də Eager Loading isə Include method'u vasitəsi ilə dataları gətirir. Aşağıda yerləşən query execute olan zaman Group property null olaraq qayıdacaq.
        List<Student> students = await _appDbContext.Set<Student>().ToListAsync();

        //Aşağıda yerləşən query execute olan zaman isə Group property dolu olaraq qayıdacaq.
        students = await _appDbContext.Set<Student>().Include(s => s.Group).ToListAsync(); //Eager loading
       
        return Ok(students);
    }

    [NonAction]
    public async Task SeedData()
    {
        await _appDbContext.Database.MigrateAsync();

        if (_appDbContext.Set<Group>().Count() == 0)
        {
            await _appDbContext.Set<Group>().AddAsync(new Group() { Code = "VA222", StartedDate = DateTime.Now, FinishDate = DateTime.Now.AddMonths(6) });
            await _appDbContext.Set<Group>().AddAsync(new Group() { Code = "VA333", StartedDate = DateTime.Now, FinishDate = DateTime.Now.AddMonths(5) });
            await _appDbContext.Set<Group>().AddAsync(new Group() { Code = "VS888", StartedDate = DateTime.Now, FinishDate = DateTime.Now.AddMonths(5) });

            await _appDbContext.SaveChangesAsync();

        }

        if (_appDbContext.Set<Student>().Count() == 0)
        {
            await _appDbContext.Set<Student>().AddAsync(new Student() { Name = "TestName1", Surname = "TestSurname1", Age = 11, GroupId = 1 });
            await _appDbContext.Set<Student>().AddAsync(new Student() { Name = "TestName2", Surname = "TestSurname2", Age = 22, GroupId = 2 });
            await _appDbContext.Set<Student>().AddAsync(new Student() { Name = "TestName3", Surname = "TestSurname3", Age = 33, GroupId = 3 });

            await _appDbContext.SaveChangesAsync();

        }
    }
}
