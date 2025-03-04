using System;
using System.Collections.Generic;

namespace SocialCompass
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Birthday { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string WhoGave { get; set; }
        public string WhenGet { get; set; }
        public string DepartmentCode { get; set; }
        public string Photo { get; set; }
        public string Address { get; set; }
        public string DisabilityCategory { get; set; }
        public string CivilCategory { get; set; }
        public decimal PensionAmount { get; set; }
        public string FamilyStatus { get; set; }
        public List<DiseaseResponse> Diseases { get; set; }
    }
}