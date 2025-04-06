$(document).ready(function () {
    var table = $('#userTable').DataTable({
        responsive: true,
        dom: '<"top"f>rt<"bottom"ip>',
        columnDefs: [
            { orderable: false, targets: [4] } // Disable sorting for Actions column
        ],
        language: {
            info: "Showing _START_ to _END_ of _TOTAL_ users",
            infoEmpty: "No users found",
            emptyTable: "No user data available"
        }
    });
});