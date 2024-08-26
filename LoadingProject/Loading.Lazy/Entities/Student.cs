namespace Loading.Lazy.Entities;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public int GroupId { get; set; }

    public virtual Group Group { get; set; }
}


