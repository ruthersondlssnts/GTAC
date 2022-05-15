using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Models.ViewModels
{
    public class StudentQuizzesViewModel
    {
        public Guid Id { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public bool isDone { get; set; }
        public User Author { get; set; }
        public bool isAnyStudents { get; set; }
    }
}
