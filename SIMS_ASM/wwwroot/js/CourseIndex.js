document.addEventListener('DOMContentLoaded', function () {
    var table = $('#courseTable').DataTable({
        responsive: true,
        dom: '<"top"B>rt<"bottom"ip>',
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel me-1"></i> Excel',
                className: 'btn btn-success btn-sm',
                title: 'Courses_Export',
                exportOptions: {
                    columns: [0, 1, 2] // Export các cột 0,1,2 (bỏ cột Action)
                }
            }
        ],
        columnDefs: [
            {
                orderable: false,
                targets: [3] // Cột Action không sort được
            },
            {
                targets: [2], // Cột Major
                render: function (data, type, row) {
                    // Chỉ hiển thị tên Major, nhưng filter theo MajorID
                    if (type === 'filter') {
                        return $(row[2]).data('major') || data;
                    }
                    return data;
                }
            }
        ],
        language: {
            info: "Showing _START_ to _END_ of _TOTAL_ courses",
            infoEmpty: "No courses found",
            emptyTable: "No course data available"
        }
    });

    // Custom search
    $('#customSearch').on('keyup', function () {
        table.search(this.value).draw();
    });

    // Major filter - cải tiến
    $('#majorFilter').on('change', function () {
        var majorId = $(this).val();
        table.rows().every(function () {
            var rowMajor = $(this.node()).find('td[data-major]').data('major');
            $(this.node()).toggle(!majorId || rowMajor == majorId);
        });
    });

    // Export button
    $('#exportBtn').click(function () {
        table.button('.buttons-excel').trigger();
    });

    $('#resetFiltersBtn').on('click', function () {
        $(this).find('i').addClass('fa-spin');

        // Thực hiện reset
        $('#customSearch').val('');
        $('#majorFilter').val('').trigger('change');
        table.search('').columns().search('').draw();

        // Dừng hiệu ứng sau 1s
        setTimeout(() => {
            $(this).find('i').removeClass('fa-spin');
        }, 1000);
    });
});