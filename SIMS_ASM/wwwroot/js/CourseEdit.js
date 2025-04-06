document.addEventListener('DOMContentLoaded', function () {
    // Bootstrap 5 validation
    (function () {
        'use strict';

        // Fetch all forms that need validation
        const forms = document.querySelectorAll('.needs-validation');

        // Loop over each form
        Array.from(forms).forEach(function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }

                form.classList.add('was-validated');
            }, false);

            // Add input event listeners for real-time validation
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(function (input) {
                input.addEventListener('input', function () {
                    if (input.checkValidity()) {
                        input.classList.remove('is-invalid');
                        input.classList.add('is-valid');
                    } else {
                        input.classList.remove('is-valid');
                        input.classList.add('is-invalid');
                    }
                });
            });
        });
    })();

    // Additional custom validation logic can be added here
});