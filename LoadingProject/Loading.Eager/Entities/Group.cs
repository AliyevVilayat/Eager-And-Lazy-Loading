﻿namespace Loading.Eager.Entities;
public class Group
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime FinishDate { get; set; }

    public ICollection<Student> Students { get; set; }

}
