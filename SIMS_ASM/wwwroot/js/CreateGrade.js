$(document).ready(function () {
    $('#ClassID').change(function () {
        var classId = $(this).val();
        if (classId) {
            $.getJSON('/Grade/GetEnrollmentsByClass', { classId: classId }, function (data) {
                var items = '<option value="">-- Select Enrollment --</option>';
                $.each(data, function (i, item) {
                    items += '<option value="' + item.value + '">' + item.text + '</option>';
                });
                $('#EnrollmentID').html(items);
            });
        } else {
            $('#EnrollmentID').html('<option value="">-- Select Enrollment --</option>');
        }
    });

    // Thêm xác nhận trước khi gửi form (tùy chọn)
    const form = document.querySelector('form');
    form.addEventListener('submit', function (e) {
        if (!confirm('Are you sure you want to create this grade?')) {
            e.preventDefault();
        }
    });
});