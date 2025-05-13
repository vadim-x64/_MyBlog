$(document).ready(function () {
    var birthDateInput = $("#RegisterForm_BirthDate");

    var minDate = new Date();
    minDate.setFullYear(minDate.getFullYear() - 120);
    birthDateInput.attr("min", minDate.toISOString().split('T')[0]);

    var today = new Date();
    birthDateInput.attr("max", today.toISOString().split('T')[0]);

    const passwordInput = document.getElementById('passwordInput');
    const showPasswordCheckbox = document.getElementById('showPasswordCheckbox');

    showPasswordCheckbox.addEventListener('change', function () {
        passwordInput.type = this.checked ? 'text' : 'password';
    });

    const confirmPasswordInput = document.getElementById('confirmPasswordInput');
    const showConfirmPasswordCheckbox = document.getElementById('showConfirmPasswordCheckbox');

    showConfirmPasswordCheckbox.addEventListener('change', function () {
        confirmPasswordInput.type = this.checked ? 'text' : 'password';
    });
});

const aboutField = document.getElementById('aboutField');
const charCount = document.getElementById('charCount');
const pasteWarning = document.getElementById('pasteWarning');

if (aboutField && charCount) {
    charCount.textContent = `${aboutField.value.length}/150`;

    aboutField.addEventListener('input', function () {
        charCount.textContent = `${this.value.length}/150`;
    });

    aboutField.addEventListener('paste', function (e) {
        e.preventDefault();
        pasteWarning.classList.remove('d-none');

        setTimeout(() => {
            pasteWarning.classList.add('d-none');
        }, 3000);
    });

    aboutField.addEventListener('contextmenu', function (e) {
        e.preventDefault();
        return false;
    });
}