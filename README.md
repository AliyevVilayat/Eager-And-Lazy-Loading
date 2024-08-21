# Loading

Bildiyimiz kimi Database’lə əlaqə qurub, datalar üzərində işləmək üçün ```ORM(Object Relational Mapping)```’ə ehtiyac duyulur. 
ORM istifadə edən zaman, işləmə sürətinə görə fərqli şəkildə ```SELECT``` sorğuları həyata keçirilir. 
C# daxilində, Relation’lardan asılı olaraq SELECT sorğuları bir qədər fərqlənirlər. 
Yəni ```JOIN```’lər SELECT sorğusuna həm manual olaraq, həm də avtomatik olaraq əlavə oluna bilər.


## Eager Loading

```Eager Loading``` ilə SELECT sorğusu həyata keçirilən zaman əlavə yüklənmənin qarşısı alınır. 
Əlavə yüklənmə dedikdə, Entity class'ın daxilində əgər hər hansısa Navigation property yer alarsa yəni hər hansısa bir table ilə relation mövcuddursa, bu zaman Entity dataları üçün SELECT sorğusunun generate olunması zamanı JOIN'lər sorğuda yer almayacaq.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>().ToListAsync();
```
Yuxarıda yerləşən ```LINQ Method Syntax``` ilə yaradılan sorğusu nəticəsi aşağıdaki kimi olacaq
```sql
SELECT * FROM Entities
```

Sözü gedən əlavə yüklənmənin qarşısı alınması nəticəsində, göründüyü kimi heç bir JOIN prosesi query'dər yer almayıb. Bu da o deməkdir ki, Navigation olaraq qeyd olunan property null olaraq qayıdacaq.
Relational olan data(lar) ehtiyac yarandığı təqdirdə bu JOIN(lər) bizim tərəfimizdən manual olaraq əlavə edilməlidir. ```Include``` və ```ThenInclude``` method'lar məhz bunun üçündür.

Aşağıda yerləşən query execute olan zaman Relation yaradan property null yerinə datalar yüklənmiş olaraq qayıdacaq.
```csharp
List<TEntity> entityList = await _context.Set<TEntity>().Include(e => e.RelationalProperty).ToListAsync();
```
        
LINQ Method Syntax və ya LINQ Query Syntax vasitəsi ilə SELECT sorğusu generate edəcəyiksə by default Eager Loading işə düşəcək. 

Eager Loading əvəzinə Lazy Loading'in işə düşməsini istəyiriksə bu zaman ```DbContext```'lə bağlı müəyyən Configuration’lara ehtiyac vardır. 


## Lazy Loading

```Lazy Loading```, yəni tənbəl yüklənmə ilə ```SELECT``` sorğusu həyata keçirilən zaman ilk olaraq relation’lar nəzərə alınmadan datalar əldə edilir.
Navigation property olan hər bir object'in datasının gətirilməsi isə bir qədır fərqlidir.

Həmin object'lərə ilk müraciət olunan zaman avtomatik olaraq həmin object üçün SQL sorğusu generate olunur. Beləliklə relational olan datalar əldə edilir. 
Tənbəl yüklənmə adlandırılmasının da səbəbi elə məhz budur.

EntityFrameworkCore istifadə edəcəyiksə və Lazy Loading'in işə düşməsini istəyiriksə DbContext üçün müəyyən Configuration’lara ehtiyac vardır. 

İlk olaraq NuGet Package’dən ```Microsoft.EntityFrameworkCore.Proxies``` package yüklənməlidir.

Daha sonra ```IoC``` üçün yazılan AddDbContext method daxilində ```UseLazyLoadingProxies()``` method aşağıdaki şəkildə call olunmalıdır.

```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
{

    opt.UseLazyLoadingProxies();
    opt.UseSqlServer(yourConnectionString);

});

```

Həmçinin bütün Entity’lər daxilində Relation’u işarəliyən bütün property’lər ```Virtual``` olaraq qeyd olunmalıdır. 

Əks təqdirdə ```System.InvalidOperationException``` exception qarşılaşacağıq.

> **Exception mesaj:**
'UseChangeTrackingProxies' requires all entity types to be public, unsealed, have virtual properties, and have a public or protected constructor.

```csharp
public class TEntity
{
    public int Id { get; set; }
    public string Prop1 { get; set; }

    public virtual ICollection<TEntity2> TEntities2 { get; set; }

}

public class TEntity2
{
    public int Id { get; set; }
    public string Prop2 { get; set; }

    public int TEntityId { get; set; }
    public TEntity TEntity { get; set; } // Navigation Property, Virutal olaraq təyin olunmadığı üçün SELECT sorğusü execute olan zaman yəni Lazy Loading işə düşən proxy'lər düzgün işləməyəcək və beləliklə exception alacağıq.
}

```



## LinkedIn

[Vilayat Aliyev](https://www.linkedin.com/in/vilayataliyev/)









