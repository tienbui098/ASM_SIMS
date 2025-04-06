$(document).ready(function () {
    var table = $('#classTable').DataTable({
        responsive: true,
        dom: '<"top"f>rt<"bottom"ip>',
        columnDefs: [
            {
                orderable: false,
                targets: [3] // Disable sorting for Actions column (index 3)
            },
            {
                targets: [2], // Major column (index 2)
                render: function (data, type, row) {
                    // Khi filter, sử dụng data-major thay vì text hiển thị
                    if (type === 'filter') {
                        return $(row[2]).data('major') || data;
                    }
                    return data;
                }
            }
        ],
        language: {
            info: "Showing _START_ to _END_ of _TOTAL_ classes",
            infoEmpty: "No classes found",
            emptyTable: "No class data available"
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

    // Reset Filters button
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