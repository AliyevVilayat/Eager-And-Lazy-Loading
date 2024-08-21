namespace Loading.Lazy.Entities;
public class Group
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime FinishDate { get; set; }

    //Relational Property'lər üçün mütləq VIRTUAL keyword ilə declare olunmalıdır
    public virtual ICollection<Student> Students { get; set; }

}
