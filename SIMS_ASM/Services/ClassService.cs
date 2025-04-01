using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class ClassService
    {
        // Khai báo RepositoryFactory và context để quản lý dữ liệu
        private RepositoryFactory _repositoryFactory; // Factory để tạo các repository
        private readonly ApplicationDbContex _context; // Context để truy cập cơ sở dữ liệu trực tiếp

        // Constructor: Inject ApplicationDbContex và khởi tạo RepositoryFactory
        public ClassService(ApplicationDbContex context)
        {
            _repositoryFactory = new RepositoryFactory(context); // Khởi tạo factory với context
            _context = context; // Gán context để sử dụng trực tiếp khi cần
        }

        // QUẢN LÝ LỚP HỌC (Class Management Methods)

        // Lấy danh sách tất cả lớp học
        public IEnumerable<Class> GetAllClasses()
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository(); // Lấy repository chuyên biệt cho Class
            return classRepo.GetAll()
                .Include(c => c.Major) // Bao gồm thông tin ngành học liên quan
                .ToList(); // Trả về danh sách lớp học dưới dạng List
        }

        // Lấy chi tiết lớp học theo ID
        public Class GetClassDetails(int classId)
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository(); // Lấy repository chuyên biệt cho Class
            return classRepo.GetById(classId); // Trả về thông tin lớp học theo ID
        }

        // Lấy danh sách lớp học theo MajorID
        public IEnumerable<Class> GetClassesByMajor(int majorId)
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository(); // Lấy repository chuyên biệt cho Class
            return classRepo.GetClassesByMajor(majorId); // Trả về các lớp học thuộc ngành học cụ thể
        }

        // Tạo lớp học mới
        public void CreateClass(Class newClass)
        {
            var classRepo = _repositoryFactory.GetClassRepository(); // Lấy repository cơ bản cho Class
            classRepo.Insert(newClass); // Thêm lớp học mới vào cơ sở dữ liệu
            classRepo.Save(); // Lưu thay đổi
        }

        // Cập nhật thông tin lớp học
        public void UpdateClass(Class updatedClass)
        {
            var classRepo = _repositoryFactory.GetClassRepository(); // Lấy repository cơ bản cho Class
            classRepo.Update(updatedClass); // Cập nhật thông tin lớp học
            classRepo.Save(); // Lưu thay đổi
        }

        // Xóa lớp học theo ID
        public void DeleteClass(int classId)
        {
            var classRepo = _repositoryFactory.GetClassRepository(); // Lấy repository cơ bản cho Class
            classRepo.Delete(classId); // Xóa lớp học theo ID
            classRepo.Save(); // Lưu thay đổi
        }

        // QUẢN LÝ NGÀNH HỌC (Major Management Methods)

        // Lấy danh sách tất cả ngành học
        public IEnumerable<Major> GetAllMajors()
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository(); // Lấy repository chuyên biệt cho Major
            return majorRepo.GetAll(); // Trả về tất cả ngành học
        }

        // Lấy danh sách StudentClass theo UserID (sinh viên)
        public async Task<List<StudentClass>> GetStudentClassesByUserIdAsync(int userId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.UserID == userId) // Lọc các bản ghi StudentClass theo UserID
                .Include(sc => sc.Class) // Bao gồm thông tin lớp học liên quan
                .ToListAsync(); // Trả về danh sách bất đồng bộ
        }
    }
}