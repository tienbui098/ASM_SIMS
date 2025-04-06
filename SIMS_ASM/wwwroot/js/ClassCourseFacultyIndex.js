$(document).ready(function () {
    var table = $('#ccfTable').DataTable({
        responsive: true,
        dom: '<"top"f>rt<"bottom"ip>',
        columnDefs: [
            { orderable: false, targets: [4] } // Disable sorting for Actions column
        ],
        language: {
            info: "Showing _START_ to _END_ of _TOTAL_ assignments",
            infoEmpty: "No assignments found",
            emptyTable: "No assignment data available"
        }
    });

    // Class filter
    $('#classFilter').on('change', function () {
        var classId = $(this).val();
        table.rows().every(function () {
            var row = this.node();
            $(row).toggle(!classId || $(row).data('class') == classId);
        });
    });

    // Course filter
    $('#courseFilter').on('change', function () {
        var courseId = $(this).val();
        table.rows().every(function () {
            var row = this.node();
            $(row).toggle(!courseId || $(row).data('course') == courseId);
        });
    });

    // Faculty filter
    $('#facultyFilter').on('change', function () {
        var facultyId = $(this).val();
        table.rows().every(function () {
            var row = this.node();
            $(row).toggle(!facultyId || $(row).data('faculty') == facultyId);
        });
    });

    // Reset Filters button
    $('#resetFiltersBtn').on('click', function () {
        $(this).find('i').addClass('fa-spin');

        // Thực hiện reset
        $('#classFilter, #courseFilter, #facultyFilter').val('');
        table.rows().every(function () {
            $(this.node()).show();
        });
        table.search('').draw();

        // Dừng hiệu ứng sau 1s
        setTimeout(() => {
            $(this).find('i').removeClass('fa-spin');
        }, 1000);
    });

    // Export button (nếu cần sử dụng trong tương lai)
    $('#exportBtn').click(function () {
        table.button('.buttons-excel').trigger();
    });
});