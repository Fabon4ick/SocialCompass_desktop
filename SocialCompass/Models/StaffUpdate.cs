using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialCompass
{
    public class StaffUpdate
    {
        public int Id { get; set; }
        public byte[] Photo { get; set; } // Может быть null
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; } // Может быть null
        public string Birth { get; set; } // Дата в формате "yyyy-MM-dd"
        public string EmploymentDay { get; set; } // Дата в формате "yyyy-MM-dd"
        public string Bio { get; set; }
        public bool isVisible { get; set; }
        public double averageRating { get; set; }
    }
}
