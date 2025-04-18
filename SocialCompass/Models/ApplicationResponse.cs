using System;
using System.Collections.Generic;
using SocialCompass;

public class ApplicationResponse
{
    public int ApplicationId { get; set; }
    public UserResponse User { get; set; }
    public string Service { get; set; }
    public string DateStart { get; set; }
    public string DateEnd { get; set; }
    public string IsHaveReabilitation { get; set; }
    public StaffResponse Staff { get; set; }
    public List<string> ExistingDiseases { get; set; }  // Список заболеваний
    public string ApplicationDuration { get; set; }
}

