//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using SIMS_ASM.Controllers;
//using SIMS_ASM.Data;
//using SIMS_ASM.Factory;
//using SIMS_ASM.Models;
//using SIMS_ASM.Services;
//using SIMS_ASM.Singleton;
//using System.Threading.Tasks;
//using Xunit;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;

//namespace TestProject2
//{
//    public class UnitTest1
//    {

//        [Fact]
//        public void Register_Get_ReturnsView()
//        {
//            // Chu?n b?
//            var controller = new AccountController();
//            // Th?c hi?n
//            var result = controller.Register();
//            // Xác nh?n
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Register_Set_ReturnsView()
//        {
//            // Chu?n b?
//            var controller = new AccountController();
//            // Th?c hi?n
//            var result = controller.Register();
//            // Xác nh?n
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Register_Delete_ReturnsView()
//        {
//            // Chu?n b?
//            var controller = new AccountController();
//            // Th?c hi?n
//            var result = controller.Register();
//            // Xác nh?n
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Register_Update_ReturnsView()
//        {
//            // Chu?n b?
//            var controller = new AccountController();
//            // Th?c hi?n
//            var result = controller.Register();
//            // Xác nh?n
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Register_Create_ReturnsView()
//        {
//            // Chu?n b?
//            var controller = new AccountController();
//            // Th?c hi?n
//            var result = controller.Register();
//            // Xác nh?n
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Test1()
//        {
//            // Arrange
//            var accountSingleton = AccountSingleton.Instance;
//            string logMessage = "This is a test log message.";
//            // Act
//            accountSingleton.Log(logMessage);
//            // Assert
//            // Kiểm tra xem file log có tồn tại không
//            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "system.log");
//            Assert.True(File.Exists(logFilePath));
//            // Đọc nội dung file log
//            string logContent = File.ReadAllText(logFilePath);
//            Assert.Contains(logMessage, logContent);
//        }

//        [Fact]
//        public void Test2()
//        {
//            // Arrange
//            var mockCourseRepository = new Mock<ICourseRepository>();
//            var mockMajorRepository = new Mock<IMajorRepository>();
//            var adminService = new AdminService(mockCourseRepository.Object, mockMajorRepository.Object);
//            var course = new Course { CourseID = 1, CourseName = "Test Course", MajorID = 1 };
//            // Act
//            adminService.UpdateCourse(course);
//            // Assert
//            mockCourseRepository.Verify(repo => repo.Update(course), Times.Once);
//            mockCourseRepository.Verify(repo => repo.SaveChanges(), Times.Once);
//        }

//        [Fact]
//        public void TestCreateCourse()
//        {
//            // Arrange
//            var mockCourseRepository = new Mock<ICourseRepository>();
//            var mockMajorRepository = new Mock<IMajorRepository>();
//            var adminService = new AdminService(mockCourseRepository.Object, mockMajorRepository.Object);
//            var newCourse = new Course { CourseID = 1, CourseName = "Test Course", MajorID = 1 };

//            // Act
//            adminService.CreateCourse(newCourse);

//            // Assert
//            mockCourseRepository.Verify(repo => repo.Add(newCourse), Times.Once);
//            mockCourseRepository.Verify(repo => repo.SaveChanges(), Times.Once);
//        }

//        [Fact]
//        public void Test3()
//        {
//            // Arrange
//            var mockCourseRepository = new Mock<ICourseRepository>();
//            var mockMajorRepository = new Mock<IMajorRepository>();
//            var adminService = new AdminService(mockCourseRepository.Object, mockMajorRepository.Object);
//            var newCourse = new Course { CourseID = 1, CourseName = "Test Course" };
//            // Act
//            adminService.CreateCourse(newCourse);
//            // Assert
//            mockCourseRepository.Verify(repo => repo.Add(newCourse), Times.Once);
//            mockCourseRepository.Verify(repo => repo.SaveChanges(), Times.Once);
//        }

//        [Fact]
//        public void TestDeleteMajor()
//        {
//            // Arrange
//            var mockCourseRepository = new Mock<ICourseRepository>();
//            var mockMajorRepository = new Mock<IMajorRepository>();
//            var adminService = new AdminService(mockCourseRepository.Object, mockMajorRepository.Object);
//            var major = new Major { MajorID = 1, MajorName = "Test Major" };

//            mockMajorRepository.Setup(repo => repo.GetById(1)).Returns(major);

//            // Act
//            adminService.DeleteMajor(1);

//            // Assert
//            mockMajorRepository.Verify(repo => repo.GetById(1), Times.Once);
//            mockMajorRepository.Verify(repo => repo.Delete(major), Times.Once);
//            mockMajorRepository.Verify(repo => repo.SaveChanges(), Times.Once);
//        }
//    }
//}