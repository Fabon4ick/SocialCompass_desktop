using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialCompass.Models
{
    public class FeedbackResponse
    {
        public int CommentId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public UserResponse User { get; set; }
        public StaffResponse Staff { get; set; }
    }
}
