document.addEventListener('DOMContentLoaded', function() {
    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        new bootstrap.Alert(alert);
    });
    const passwordInput = document.getElementById('passwordInput');
    const showPasswordCheckbox = document.getElementById('showPasswordCheckbox');
    showPasswordCheckbox.addEventListener('change', function() {
        passwordInput.type = this.checked ? 'text' : 'password';
    });
});