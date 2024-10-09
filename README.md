# Loading Related Data

Bildiyimiz kimi Database-lə əlaqə qurub, datalar üzərində işləmək üçün ```ORM (Object Relational Mapping)``` istifadə olunur.
ORM vasitəsi ilə Database-lə bağlı əməliyyatları istər QUERY, istərsə də Method-lar vasitəsi ilə həyata keçirmək mümkündür. 
Bir sıra mənbələrdə ```DQL(Data Query Language)``` command, bəzi mənbələrdə isə ```DML(Data Manipulation Language)``` command olaraq göstərilən ```SELECT``` -də bura aiddir.

Entity Framework Core istifadə edərək SELECT sorğularını yaradılır və icra edilir. 

Table-lar arasında ```Relation``` mövcud olarsa Data-ların gətirilməsindən asılı olaraq bu proses bir qədər fərqlənir. 
Yəni, SELECT sorğusu yaradılarkən Related Data-ların da bu sorğuya əlavə olunma prosesi həm manual, həm də avtomatik ola bilər. Bu anlayış C#-da ```Loading``` olaraq adlandırılır.

Loading 3 yerə bölünür. Eager Loading, Explicit Loading və Lazy Loading. 


## Eager Loading

```Eager Loading``` generate olunan SELECT sorğusuna ```Related Data```-ların parça-parça əlavə edilməsini və bu prosesin `istəkli` şəkildə baş verməsini təmin edən bir yoldur.
Eager Loading ilə SELECT sorğusu həyata keçirilən zaman əlavə yüklənmənin qarşısı alınır. 
Əlavə yüklənmə dedikdə, Database-də yer alan Entity class'ın hər hansısa bir Table ilə Relation-ı varsa, bu zaman Entity üçün SELECT sorğusu yaradılarkən Related Data-lar gətirilməyəcək, bu prosesi bizim manual etməyimiz üçün şərait yaradılacaq.

Entity Framework Core işlədilən proyektdə yüklənmə by default Eager Loading-dir.

```csharp
List<TEntity> entityList = await _context.Set<TEntity>().ToListAsync();
```
Yuxarıda yerləşən ```LINQ Method Syntax``` ilə yaradılan sorğunun nəticəsi aşağıdaki kimi olacaq.
```sql
SELECT * FROM Entities
```

Sözügedən əlavə yüklənmənin qarşısı alınması nəticəsində göründüyü kimi, heç bir JOIN prosesi query-də yer almayıb. Bu da o deməkdir ki, Entity obyekt(lər) əldə olunan zaman Navigation property null dəyərə sahib olacaq.
Related data-lara ehtiyac yarandığı təqdirdə, bu JOIN(lər) bizim tərəfimizdən manual olaraq əlavə edilməlidir. Entity Framework Core-da JOIN-lər ```Include``` və ```ThenInclude``` method'lar vasitəsi ilə yaradılan sorğuya əlavə edilir.

Eager Loading-in istifadəsi aşağıdaki şəkildə olur.

```csharp
//LINQ Method Syntax
List<TEntity> entityListMethodSyntax = await _context.Set<TEntity>().Include(e=>e.TEntities2).ToListAsync();

// və ya

//LINQ Query Syntax
List<TEntity> entityListQuerySyntax = await (from e in _context.Set<TEntity>()
                        join e2 in _context.Set<TEntities2>() on e.Id equals e2.TEntityId
                        select e).ToListAsync();
```

Yuxarıda yerləşən LINQ sorğularının nəticəsi aşağıdaki şəkildə olacaq.

```sql
SELECT *
FROM [TEntity] AS [e]
INNER JOIN [TEntities2] AS [e2] ON [e].[Id] = [e2].[TEntityId]
```

>:bulb:**Not:** Query-ləri əldə etmək üçün IQueryable-a ToQueryString() method tətbiq edilir. Bu method nəticədə yaradılacaq Query-ni string tipində geriyə qaytarır.

## Explicit Loading

Yaradılan SELECT sorğusunda, Relational Data-ların gətirilib-gətirilməməsi prosesi daha sonradan şərtlərə və ya ehtiyaclara uyğun olaraq baş verməsi yanaşması ```Explicit Loading``` adlanır.

Explicit Loading tətbiq etmədən yuxarıda bəhs olunan yanaşma aşağıdakı şəkildə olacaqdır. 
```csharp
       TEntity2 tEntity2 = await _context.Set<TEntity2>().FindAsync(id);
       if(tEntity2.Prop2 == "TestValue")
       {
              TEntity tEntity = await _context.Set<TEntity>().Find(tEntity2.TEntityId);
       }
```
Yuxarıdaki kod blokunu yazmaq əvəzinə Explicit Loading tətbiq edərək daha oxunaqlı və düzgün şəkildə kod yaza bilərik.

### Reference
Explicit Loading tətbiq edilərkən data-ları əldə etmək istənilən Table-ın Navigation Property-si tək obyekt olarsa(yəni Collection deyilsə) bu zaman ```Reference``` method istifadə edilir.

```csharp
       TEntity2 tEntity2 = await _context.Set<TEntity2>().FindAsync(id);
       if(tEntity2.Prop2 == "TestValue")
       {
              _context.Entry(tEntity2).Reference(e2=>e2.TEntity).Load();
              //və ya
              await _context.Entry(tEntity2).Reference(e2=>e2.TEntity).LoadAsync();
       }
```

### Collection
Explicit Loading tətbiq edilərkən data-larını əldə etmək istənilən Table-ın Navigation Property-si kolleksiya olarsa bu zaman ```Collection``` method istifadə edilir.

```csharp
       TEntity tEntity = await _context.Set<TEntity>().FindAsync(id);
       if(tEntity.Prop1 == "TestValue")
       {
              _context.Entry(tEntity).Collection(e2=>e2.TEntity).Load();
              //və ya
              await _context.Entry(tEntity).Collection(e2=>e2.TEntity).LoadAsync();
       }
```

>:bulb:**Not:** Relational Datalar əldə edildikdən sonra əsas data-larla Context vasitəsilə əlaqələndirilərək saxlanılır. Çünki EntityFramework daha əvvəldə execute edilən sorğular nəticəsində get olunan data-ları daha sonraki proseslərdə istifadə edir.


Collection method istifadəsi zamanı biz Relational data-ları Query method ilə Queryable halına salıb həm Aggregate operatorları, həm də filterasiya tətbiq edə bilərik.


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

Həmçinin, bütün Entity-lər daxilində Relational Data-ları işarəliyən bütün property-lər ```Virtual``` olaraq qeyd olunmalıdır. 

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
## N+1 Problem (N+1 Selects Problem)

Lazy Loading enable olduqda, dataların navigation property-lərə müraciəti zamanı sorğu yaradaraq məlumatları gətirdiyini artıq bilirik. Bu müraciətlər döngü içərisində hər bir entity obyekt üçün baş verdikdə, dəfələrlə sorğu yaradılaraq database-ə göndərilir. Bu proses N+1 problemi adlanır. Çünki dəfələrlə sorğu göndərilməsi performans baxımından heç də yaxşı deyil və həmçinin bu, database connection-ın davamlı olaraq açıq qalmasına gətirib çıxarır. Eager Loading istifadəsi bu problemin qarşısını rahatlıqla alır.

Həmçinin N+1 Selects Problem olaraq da adlandılırmasının səbəbi abstract olaraq yox, bir başa əsas mexanizmi izzah etmək üçündür.

>:bulb:**Not:** Burada 1, Select sorğusu zamanı gətirilən əsas obyekti(ICollection). N isə əsas obyektə hər dəfə döngü ilə müraciət olunan zaman gətirilən Relational Data-ları təmsil edir.

## LinkedIn

[Vilayat Aliyev](https://www.linkedin.com/in/vilayataliyev/)









