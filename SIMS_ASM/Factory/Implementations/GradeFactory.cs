using SIMS_ASM.Models;
using SIMS_ASM.Data;
using SIMS_ASM.Factory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Factory.Implementations
{
    public class GradeFactory : IGradeFactory
    {
        private readonly ApplicationDbContex _context;

        public GradeFactory(ApplicationDbContex context)
        {
            _context = context;
        }

        public Grade Create()
        {
            return new Grade();
        }

        public bool Update (Grade entity)
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
                var grade = _context.Grades.Find(id);
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

        public Grade GetById(int id)
        {
            return _context.Grades
                .Include(g => g.Course)
                .Include(g => g.User)
                .FirstOrDefault(g => g.GradeID == id);
        }

        public IEnumerable<Grade> GetAll()
        {
            return _context.Grades
                .Include(g => g.Course)
                .Include(g => g.User)
                .ToList();
        }

        public IEnumerable<Grade> GetGradesByStudentId(int studentId)
        {
            return _context.Grades
                .Where(g => g.UserID == studentId)
                .Include(g => g.Course)
                .ToList();
        }

        public IEnumerable<Grade> GetGradesByCourseId(int courseId)
        {
            return _context.Grades
                .Where(g => g.CourseID == courseId)
                .Include(g => g.User)
                .ToList();
        }

        public Grade GetGradeByStudentAndCourse(int studentId, int courseId)
        {
            return _context.Grades
                .FirstOrDefault(g => g.UserID == studentId && g.CourseID == courseId);
        }
    }
}
