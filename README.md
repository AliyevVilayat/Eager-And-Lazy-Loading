# Loading Related Data

Bildiyimiz kimi Database-lə əlaqə qurub, datalar üzərində işləmək üçün ```ORM (Object Relational Mapping)``` istifadə olunur. 
ORM-dən istifadə edərkən işləmə sürətinə görə fərqli şəkildə ```SELECT``` sorğuları həyata keçirilir. 
C# daxilində, Relation-lardan asılı olaraq SELECT sorğuları bir qədər fərqlənir. 
Yəni, ```JOIN```-lər SELECT sorğusuna həm manual, həm də avtomatik olaraq əlavə oluna bilər.


## Eager Loading

```Eager Loading``` generate olunan SELECT sorğuya ```Related Data```-ların parça-parça əlavə edilməsini və bu prosesin `istəkli` şəkildə baş verməsini təmin edən bir yoldur.
Eager Loading ilə SELECT sorğusu həyata keçirilən zaman əlavə yüklənmənin qarşısı alınır. 
Əlavə yüklənmə dedikdə, Entity class'ın daxilində hər hansısa Navigation property mövcuddursa, yəni hər hansısa bir table ilə relation varsa, bu zaman Entity dataları üçün SELECT sorğusu yaradılarkən JOIN-lər sorğuda yer almayacaq.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>().ToListAsync();
```
Yuxarıda yerləşən ```LINQ Method Syntax``` ilə yaradılan sorğunun nəticəsi aşağıdaki kimi olacaq
```sql
SELECT * FROM Entities
```

Sözügedən əlavə yüklənmənin qarşısı alınması nəticəsində göründüyü kimi, heç bir JOIN prosesi query-də yer almayıb. Bu da o deməkdir ki, Navigation olaraq qeyd olunan property null olaraq qayıdacaq.
Relational olan data(lar) ehtiyac yarandığı təqdirdə, bu JOIN(lər) bizim tərəfimizdən manual olaraq əlavə edilməlidir. ```Include``` və ```ThenInclude``` method'lar məhz bunun üçündür.
       
Əgər LINQ Method Syntax və ya LINQ Query Syntax vasitəsilə SELECT sorğusu yaradacağıqsa, by default Eager Loading işə düşəcək. 

Eager Loading əvəzinə Lazy Loading-in işə düşməsini istəyiriksə, bu zaman ```DbContext```-lə bağlı müəyyən konfiqurasiyalara ehtiyac var. 

## Include
SELECT sorğusu yaradılan zaman Relational Data-ların da əldə olunması üçün hər bir Navigation Property nəzərə alınaraq Include method tətbiq edilir beləliklə, sorğu nəticəsində Relation Data-lar da əldə edilir. Eager Loading-i həyata keçirmək üçün istifadə olunan bir method-dur. 

Aşağıda yerləşən query execute olunduqda, Navigation property null gəlmək əvəzinə, datalar yüklənmiş olaraq qayıdacaq.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>().Include(e => e.NavigationProperty).ToListAsync();

//və ya

List<TEntity> entityList = await _context.Set<TEntity>().Include(navigationPropertyName).ToListAsync();
```

Yuxarıda yerləşən LINQ Method Syntax ilə yaradılan sorğunun nəticəsi aşağıdaki kimi olacaq

```sql
SELECT *
FROM Entities e
LEFT JOIN RelationTable rt ON e.RelationalTableId = rt.Id      
```

## Filtered Include
Biz Include method-u tətbiq edən zaman, gələn datalar üzərində filterasiya həyata keçirə bilərik. Filterasiya yalnız ICollection tipində olan Navigation property-lərə şamil edilir. Filterasiyaya, Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Skip, Take method-lar aiddir.

Change Tracker-in aktiv olduğu situasiyalarda Filtered Include tətbiq edilən sorğu nəticəsində filterasiya olduğu halda gözlənilməyən data-lar gələ bilər. Bu daha əvvəl SELECT sorğusu nəticəsində gəlmiş və Change Tracker tərəfindən izlənilən və filterasiyadan keçməmiş datalara aiddir. Düzgün şəkildə Filtered Include prosesi üçün Change Tracker tərəfindən izlənməyən data-ların SELECT sorğusuna tətbiq edilməlidir.

## ThenInclude
SELECT sorğusu yaradılan zaman Relational Data-ları əldə etmək üçün Include method-u istifadə etməli olduğumuzu bilirik. Include edilən table daxilində də Relational data-lar yer alarsa həmçinin onları əldə etməyə ehtiyac duyula bilər.

Biz Include vasitəsi ilə bu funskiyasını həyata keçirə bilərik, lakin sorğu zamanı Include edilən Navigation property daxilində yer alan Navigation property ICollection tipində olarsa, Include tətbiq edərək istədiyimiz nəticəni əldə edə bilməyəcəyik.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>()
            .Include(e => e.NavigationProperty)
            .Include(e => e.NavigationProperty.InnerNavigationProperty)
            .ToListAsync(); 

```

Bunun yerinə ThenInclude method istifadə edilir və biz Inlclude method tətbiq etdikdən sonra göndərdiyimiz NavigationProperty-ni ThenInclude ilə əldə edib SELECT sorğusuna onun Relational Data-larını əldə etmək üçün JOIN edə bilərik.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>()
            .Include(e => e.NavigationProperty)
            .ThenInclude(rP => rP.InnerNavigationProperty)
            .ToListAsync(); 

```

Yuxarıda yerləşən LINQ Method Syntax ilə yaradılan sorğunun nəticəsi aşağıdaki kimi olacaq

```sql
SELECT *
FROM Entities e
LEFT JOIN RelationTable rt ON e.RelationalTableId = rt.Id
LEFT JOIN InnerRelationalTable irt ON rt.InnerRalationalTableId = irt.Id      
```


## Lazy Loading

```Lazy Loading```, yəni tənbəl yüklənmə ilə ```SELECT``` sorğusu həyata keçirilərkən ilk olaraq relation-lar nəzərə alınmadan datalar əldə edilir.
Navigation property-lərin datasının gətirilməsi isə bir qədər fərqlidir.

Navigation property'lərə ilk müraciət olunan zaman avtomatik olaraq SQL sorğusu yaradılır və beləliklə, datalar əldə edilir. 
Tənbəl yüklənmə adlandırılmasının səbəbi də məhz budur.

Əgər EntityFrameworkCore istifadə edəcəyiksə və Lazy Loading-in işə düşməsini istəyiriksə, DbContext üçün müəyyən konfiqurasiyalara ehtiyac vardır. 

İlk olaraq NuGet Package-dən ```Microsoft.EntityFrameworkCore.Proxies``` paketi yüklənməlidir.

Daha sonra, ```IoC``` üçün yazılan AddDbContext method daxilində ```UseLazyLoadingProxies()``` method-u aşağıdaki şəkildə call olunmalıdır.

```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
{

    opt.UseLazyLoadingProxies();
    opt.UseSqlServer(yourConnectionString);

});

```

Həmçinin, bütün Entity-lər daxilində Relation-u işarəliyən bütün property-lər ```Virtual``` olaraq qeyd olunmalıdır. 

Əks təqdirdə, ```System.InvalidOperationException``` ilə qarşılaşacağıq.

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
    public TEntity TEntity { get; set; } // Navigation Property Virutal olaraq təyin edilmədiyi üçün SELECT sorğusu icra olunduqda, yəni Lazy Loading işə düşdükdə, proxy-lər düzgün işləməyəcək və beləliklə exception alacağıq.
}

```

## Manual Lazy Loading(ILazyLoader)

Proxy-lər bütün platformalarda dəstəklənməyə bilər, buna görə də biz proxy-lər olmadan da Lazy Loading-i işə salmağı bacarmalıyıq.

İlk olaraq NuGet Package-dən ```Microsoft.EntityFrameworkCore.Abstractions``` paketi yüklənməlidir.

Manual olaraq həyata keçirəcəyimiz üçün Entity-lər daxilində yer alan Navigation property-lərin Virtual olaraq işarələnməsinə ehtiyac yoxdur.

Dependency Injection istifadə edilərək, ILazyLoader tipində parametr qəbul edən Constructor yazılmalı və bu parametr private olan Field’a mənimsədilməlidir. Bu proses Lazy Loading-in baş verməsini istədiyimiz bütün Entity-lərə tətbiq edirik.

Daha sonra Navigation property üçün Entity daxilində ayrı bir property yaradılır. Navigation property-in `Get` və `Set` method-ları override edilərək aşağıdaki şəkildə yazılır.

```csharp

public class TEntity
{
    private readonly ILazyLoader _lazyLoader;
    private TEntity2 _tEntity2;

    public int Id { get; set; }
    public string Prop1 { get; set; }

    public TEntity2 TEntity2
    {
        get
        {
            _lazyLoader.Load(this, ref _tEntity2);
        }
        set
        {
            _tEntity2 = value;
        }
    }

    public TEntity()
    {

    }
    public TEntity(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }

}

public class TEntity2
{
    public int Id { get; set; }
    public string Prop2 { get; set; }
    public int TEntityId { get; set; }
    public TEntity TEntity { get; set; }
}
```
## N+1 Selects Problem

Lazy Loading enable olduqda, dataların navigation property-lərə müraciəti zamanı sorğu yaradaraq məlumatları gətirdiyini artıq bilirik. Bu müraciətlər döngü içərisində hər bir entity obyekt üçün baş verdikdə, dəfələrlə sorğu yaradılaraq database-ə göndərilir. Bu proses N+1 problemi adlanır. Çünki dəfələrlə sorğu göndərilməsi performans baxımından heç də yaxşı deyil və həmçinin bu, database connection-ın davamlı olaraq açıq qalmasına gətirib çıxarır. Eager Loading istifadəsi bu problemin qarşısını rahatlıqla alır.



## LinkedIn

[Vilayat Aliyev](https://www.linkedin.com/in/vilayataliyev/)









