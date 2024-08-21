using Loading.Lazy.Contexts;
using Loading.Lazy.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Loading.Lazy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LazyLoadingsController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public LazyLoadingsController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStudentsWithGroup()
    {
        await SeedData();

        List<Student> students = await _appDbContext.Set<Student>().ToListAsync();
        return Ok(students);
    }


    [NonAction]
    public async Task SeedData()
    {
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
