using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialCompass
{
    public class StaffResponse
    {
        public int Id { get; set; }
        public byte[] Photo { get; set; } // Фото хранится в виде массива байтов
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime Birth { get; set; }
        public DateTime EmploymentDay { get; set; }
        public string Bio { get; set; }
        public string FullName => $"{Surname} {Name}";
        public bool isVisible { get; set; }
    }
}
