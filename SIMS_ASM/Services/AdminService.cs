using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Services
{
    public class AdminService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMajorRepository _majorRepository;

        // Constructor: Inject các repository cần thiết
        public AdminService(ICourseRepository courseRepository, IMajorRepository majorRepository)
        {
            _courseRepository = courseRepository;
            _majorRepository = majorRepository;
        }

        // Lấy danh sách tất cả khóa học
        public IEnumerable<Course> GetAllCourses()
        {
            return _courseRepository.GetAll();
        }

        // Lấy chi tiết khóa học theo ID
        public Course GetCourseDetails(int courseId)
        {
            return _courseRepository.GetCourseWithDetails(courseId);
        }

        // Tạo khóa học mới
        public void CreateCourse(Course course)
        {
            _courseRepository.Add(course);
            _courseRepository.SaveChanges();
        }

        // Cập nhật thông tin khóa học
        public void UpdateCourse(Course course)
        {
            _courseRepository.Update(course);
            _courseRepository.SaveChanges();
        }

        // Xóa khóa học theo ID
        public void DeleteCourse(int courseId)
        {
            var course = _courseRepository.GetById(courseId);
            if (course != null)
            {
                _courseRepository.Delete(course);
                _courseRepository.SaveChanges();
            }
        }

        // QUẢN LÝ NGÀNH HỌC (Major Management)

        // Lấy chi tiết ngành học theo ID
        public Major GetMajorDetails(int majorId)
        {
            return _majorRepository.GetById(majorId);
        }

        // Lấy danh sách ngành học kèm khóa học liên quan
        public IEnumerable<Major> GetMajorsWithCourses()
        {
            return _majorRepository.GetMajorsWithCourses();
        }

        // Tạo ngành học mới
        public void CreateMajor(Major newMajor)
        {
            _majorRepository.Add(newMajor);
            _majorRepository.SaveChanges();
        }

        // Cập nhật thông tin ngành học
        public void UpdateMajor(Major updatedMajor)
        {
            _majorRepository.Update(updatedMajor);
            _majorRepository.SaveChanges();
        }

        // Xóa ngành học theo ID
        public void DeleteMajor(int majorId)
        {
            var major = _majorRepository.GetById(majorId);
            if (major != null)
            {
                _majorRepository.Delete(major);
                _majorRepository.SaveChanges();
            }
        }
    }
}