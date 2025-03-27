using SIMS_ASM.Models;
using SIMS_ASM.Factory.Interfaces;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Factory.Implementations
{
    public class UserFactory : IUserFactory
    {


        //private readonly ApplicationDbContex _context;

        //public UserFactory(ApplicationDbContex context)
        //{
        //    _context = context;
        //}

        //public User Create()
        //{
        //    return new User();
        //}

        //public bool Update(User entity)
        //{
        //    try
        //    {
        //        _context.Entry(entity).State = EntityState.Modified;
        //        _context.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public bool Delete(int id)
        //{
        //    try
        //    {
        //        var user = _context.Users.Find(id);
        //        if (user == null)
        //            return false;

        //        _context.Users.Remove(user);
        //        _context.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public User GetById(int id)
        //{
        //    return _context.Users
        //        .Include(u => u.Courses)
        //        .Include(u => u.Grades)
        //        .FirstOrDefault(u => u.UserID == id);
        //}

        //public IEnumerable<User> GetAll()
        //{
        //    return _context.Users.ToList();
        //}

        //public IEnumerable<User> GetStudents()
        //{
        //    return _context.Users.Where(u => u.Role == "Student").ToList();
        //}

        //public IEnumerable<User> GetLecturers()
        //{
        //    return _context.Users.Where(u => u.Role == "Lecturer").ToList();
        //}

        //public IEnumerable<User> GetAdministrators()
        //{
        //    return _context.Users.Where(u => u.Role == "Administrator").ToList();
        //}
        public User Create()
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetAdministrators()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public User GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetLecturers()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetStudents()
        {
            throw new NotImplementedException();
        }

        public bool Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
