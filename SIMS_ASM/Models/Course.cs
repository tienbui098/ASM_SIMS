using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_ASM.Models
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [ForeignKey("Major")]
        public int MajorID { get; set; }

        [StringLength(100)]
        public string CourseName { get; set; }

        public DateTime CourseStartDate { get; set; }
        public DateTime CourseEndDate { get; set; }

        // Navigation properties
        public virtual Major Major { get; set; }
        public virtual ICollection<ClassCourseFaculty> ClassCourseFaculties { get; set; }
    }

    // Custom validation attribute để kiểm tra EndDate > StartDate
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DateGreaterThanAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var endDate = (DateTime)value;
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);
            var startDate = (DateTime)otherPropertyInfo.GetValue(validationContext.ObjectInstance);

            if (endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }

}

