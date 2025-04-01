using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Services
{
    public class AdminService
    {
        // Khai báo RepositoryFactory để tạo các repository
        private RepositoryFactory _repositoryFactory;

        // Constructor: Inject ApplicationDbContex và khởi tạo RepositoryFactory
        public AdminService(ApplicationDbContex context)
        {
            _repositoryFactory = new RepositoryFactory(context); // Tạo factory với context được inject
        }

        // Lấy danh sách tất cả khóa học
        public IEnumerable<Course> GetAllCourses()
        {
            var courseRepo = _repositoryFactory.GetSpecificCourseRepository(); // Lấy repository chuyên biệt cho Course
            return courseRepo.GetAll(); // Trả về tất cả khóa học từ repository
        }

        // Lấy chi tiết khóa học theo ID
        public Course GetCourseDetails(int courseId)
        {
            var courseRepo = _repositoryFactory.GetSpecificCourseRepository(); // Lấy repository chuyên biệt cho Course
            return courseRepo.GetCourseWithDetails(courseId); // Trả về khóa học với chi tiết (có thể bao gồm thông tin liên quan)
        }

        // Tạo khóa học mới
        public void CreateCourse(Course course)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository(); // Lấy repository cơ bản cho Course
            courseRepo.Insert(course); // Thêm khóa học mới vào cơ sở dữ liệu
            courseRepo.Save(); // Lưu thay đổi
        }

        // Cập nhật thông tin khóa học
        public void UpdateCourse(Course course)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository(); // Lấy repository cơ bản cho Course
            courseRepo.Update(course); // Cập nhật thông tin khóa học
            courseRepo.Save(); // Lưu thay đổi
        }

        // Xóa khóa học theo ID
        public void DeleteCourse(int courseId)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository(); // Lấy repository cơ bản cho Course
            courseRepo.Delete(courseId); // Xóa khóa học theo ID
            courseRepo.Save(); // Lưu thay đổi
        }

        // QUẢN LÝ NGÀNH HỌC (Major Management)

        // Lấy chi tiết ngành học theo ID
        public Major GetMajorDetails(int majorId)
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository(); // Lấy repository chuyên biệt cho Major
            return majorRepo.GetById(majorId); // Trả về ngành học theo ID
        }

        // Lấy danh sách ngành học kèm khóa học liên quan
        public IEnumerable<Major> GetMajorsWithCourses()
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository(); // Lấy repository chuyên biệt cho Major
            return majorRepo.GetMajorsWithCourses(); // Trả về danh sách ngành học với thông tin khóa học liên quan
        }

        // Tạo ngành học mới
        public void CreateMajor(Major newMajor)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository(); // Lấy repository cơ bản cho Major
            majorRepo.Insert(newMajor); // Thêm ngành học mới vào cơ sở dữ liệu
            majorRepo.Save(); // Lưu thay đổi
        }

        // Cập nhật thông tin ngành học
        public void UpdateMajor(Major updatedMajor)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository(); // Lấy repository cơ bản cho Major
            majorRepo.Update(updatedMajor); // Cập nhật thông tin ngành học
            majorRepo.Save(); // Lưu thay đổi
        }

        // Xóa ngành học theo ID
        public void DeleteMajor(int majorId)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository(); // Lấy repository cơ bản cho Major
            majorRepo.Delete(majorId); // Xóa ngành học theo ID
            majorRepo.Save(); // Lưu thay đổi
        }
    }
}