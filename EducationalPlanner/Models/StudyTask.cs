using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationalPlanner
{
    public class StudyTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }

        public StudyTask() // Здесь хранятся параметры по умолчанию
        {
            Id = Guid.NewGuid().GetHashCode();
            Title = "Новая задача";
            Description = "Описание задачи";
            Category = "Общее";
            Priority = "Средний";
            Deadline = DateTime.Now.AddDays(7);
            IsCompleted = false;
        }
    }
}