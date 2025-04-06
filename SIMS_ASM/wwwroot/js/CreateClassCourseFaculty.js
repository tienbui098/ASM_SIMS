document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');
    form.addEventListener('submit', function (e) {
        if (!confirm('Are you sure you want to create this class-course-faculty assignment?')) {
            e.preventDefault();
        }
    });
});