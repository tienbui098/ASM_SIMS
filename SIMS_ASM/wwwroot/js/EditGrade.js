$(document).ready(function () {
    // Set the selected class based on the current enrollment
    var initialClassId = '@(Model.Enrollment?.ClassCourseFaculty?.ClassID ?? 0)';
    if (initialClassId > 0) {
        $('#ClassID').val(initialClassId);
    }

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
});