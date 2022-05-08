using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTAC.Models.ViewModels
{
    public class InstructorScheduleViewModel
    {
        public Student SevenAMStudent { get; set; }
        public string DaySevenAMStudent { get; set; }
        public Student TenAMStudent { get; set; }
        public string DayTenAMStudent { get; set; }
        public Student OnePMStudent { get; set; }
        public string DayOnePMStudent { get; set; }
        public Student ThreePMStudent { get; set; }
        public string DayThreePMStudent { get; set; }
    }
}
