namespace SIMS_ASM.Singleton
{
    public class AccountSingleton
    {
        // Biến tĩnh để lưu trữ instance duy nhất
        private static AccountSingleton _instance;
        // Đối tượng lock để đảm bảo thread-safety khi tạo instance
        private static readonly object _lock = new object();

        // Đường dẫn file log
        private readonly string _logFilePath;

        // Constructor private để ngăn việc tạo instance từ bên ngoài
        private AccountSingleton()
        {
            // Đặt đường dẫn file log (ví dụ: trong thư mục Logs của dự án)
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "system.log");

            // Tạo thư mục Logs nếu chưa tồn tại
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        // Phương thức tĩnh để lấy instance duy nhất
        public static AccountSingleton Instance
        {
            get
            {
                // Kiểm tra nếu instance chưa được tạo
                if (_instance == null)
                {
                    // Sử dụng lock để đảm bảo chỉ có 1 thread được tạo instance
                    lock (_lock)
                    {
                        // Kiểm tra lần thứ hai sau khi có lock (double-check pattern)
                        if (_instance == null)
                        {
                            _instance = new AccountSingleton();
                        }
                    }
                }
                return _instance;
            }
        }

        // Phương thức ghi log
        public void Log(string message)
        {
            // Định dạng thông điệp log gồm thời gian và nội dung
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            // Ghi thông điệp vào cuối file log
            File.AppendAllText(_logFilePath, logEntry);
        }
    }
}

