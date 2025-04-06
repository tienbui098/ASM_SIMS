$(document).ready(function () {
    var table = $('#adminTable').DataTable({
        responsive: true,
        dom: '<"top"B>rt<"bottom"ip>', // Loại bỏ 'l' (length) và 'f' (filter)
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel me-1"></i> Excel',
                className: 'btn btn-success btn-sm',
                title: 'Admins_Export'
            }
        ],
        columnDefs: [
            { orderable: false, targets: [7] } // Actionsව

        language: {
                info: "Showing _START_ to _END_ of _TOTAL_ admins", // Customize info text
                infoEmpty: "No admins found",
                emptyTable: "No admin data available in table"
            }
    });

    // Áp dụng tìm kiếm từ search box tùy chỉnh
    $('#customSearch').keyup(function () {
        table.search(this.value).draw();
    });

    $('#exportBtn').click(function () {
        $('.buttons-excel').click();
    });
});