using SIMS_ASM.Models;
using SIMS_ASM.Factory.Interfaces;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Factory.Implementations
{
    public class CourseFactory : ICourseFactory
    {
        private readonly ApplicationDbContex _context;

        public CourseFactory(ApplicationDbContex context)
        {
            _context = context;
        }

        public Course Create()
        {
            return new Course();
        }

        public bool Update(Course entity)
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                var course = _context.Courses.Find(id);
                if (course == null)
                    return false;

                _context.Courses.Remove(course);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Course GetById(int id)
        {
            return _context.Courses
                .Include(c => c.User)
                .Include(c => c.Grade)
                .FirstOrDefault(c => c.CourseID == id);
        }

        public IEnumerable<Course> GetAll()
        {
            return _context.Courses
                .Include(c => c.User)
                .Include(c => c.Grade)
                .ToList();
        }

        public bool AssignCourseToStudent(int courseId, int studentId)
        {
            try
            {
                var course = _context.Courses.Find(courseId);
                var student = _context.Users.Find(studentId);

                if (course == null || student == null || student.Role != "Student")
                    return false;

                // Kiểm tra xem học sinh đã được gán vào khóa học chưa qua bảng Grade
                var existingGrade = _context.Grades.FirstOrDefault(g =>
                    g.CourseID == courseId && g.UserID == studentId);

                if (existingGrade != null)
                    return true; // Đã được gán trước đó

                // Tạo grade mới để gán sinh viên vào khóa học
                var grade = new Grade
                {
                    CourseID = courseId,
                    UserID = studentId,
                    GradeValue = 0 // Giá trị mặc định
                };

                _context.Grades.Add(grade);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveStudentFromCourse(int courseId, int studentId)
        {
            try
            {
                var grade = _context.Grades.FirstOrDefault(g =>
                    g.CourseID == courseId && g.UserID == studentId);

                if (grade == null)
                    return false;

                _context.Grades.Remove(grade);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Course> GetCoursesByStudentId(int studentId)
        {
            return _context.Grades
                .Where(g => g.UserID == studentId)
                .Select(g => g.Course)
                .Include(c => c.User)
                .ToList();
        }

        public IEnumerable<User> GetStudentsByCourseId(int courseId)
        {
            return _context.Grades
                .Where(g => g.CourseID == courseId)
                .Select(g => g.User)
                .Where(u => u.Role == "Student")
                .ToList();
        }
    }
}
