using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class ClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly IMajorRepository _majorRepository;
        private readonly ApplicationDbContex _context;

        // Constructor: Inject các repository và context cần thiết
        public ClassService(IClassRepository classRepository,
                          IMajorRepository majorRepository,
                          ApplicationDbContex context)
        {
            _classRepository = classRepository;
            _majorRepository = majorRepository;
            _context = context;
        }

        // QUẢN LÝ LỚP HỌC (Class Management Methods)

        // Lấy danh sách tất cả lớp học với thông tin Major
        public IEnumerable<Class> GetAllClasses()
        {
            return _classRepository.GetAllQueryable()
                .Include(c => c.Major)
                .ToList();
        }

        // Lấy chi tiết lớp học theo ID với thông tin Major
        public Class GetClassDetails(int classId)
        {
            return _classRepository.GetClassWithMajor(classId);
        }

        // Lấy danh sách lớp học theo MajorID
        public IEnumerable<Class> GetClassesByMajor(int majorId)
        {
            return _classRepository.GetClassesByMajor(majorId);
        }

        // Tạo lớp học mới
        public void CreateClass(Class newClass)
        {
            _classRepository.Add(newClass);
            _classRepository.SaveChanges();
        }

        // Cập nhật thông tin lớp học
        public void UpdateClass(Class updatedClass)
        {
            _classRepository.Update(updatedClass);
            _classRepository.SaveChanges();
        }

        // Xóa lớp học theo ID
        public void DeleteClass(int classId)
        {
            var classToDelete = _classRepository.GetById(classId);
            if (classToDelete != null)
            {
                _classRepository.Delete(classToDelete);
                _classRepository.SaveChanges();
            }
        }

        // QUẢN LÝ NGÀNH HỌC (Major Management Methods)

        // Lấy danh sách tất cả ngành học
        public IEnumerable<Major> GetAllMajors()
        {
            return _majorRepository.GetAll();
        }

        // Lấy danh sách StudentClass theo UserID (sinh viên)
        public async Task<List<StudentClass>> GetStudentClassesByUserIdAsync(int userId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.UserID == userId)
                .Include(sc => sc.Class)
                .ToListAsync();
        }
    }
}