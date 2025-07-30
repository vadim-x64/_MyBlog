const editBtn = document.getElementById('editBtn');
const updateBtn = document.getElementById('updateBtn');
const inputs = document.querySelectorAll('input, textarea');
const newPasswordField = document.getElementById('newPasswordField');
const showPasswordCheckbox = document.getElementById('showPasswordCheckbox');

document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const closeEvent = new Event('click');
            const closeButton = alert.querySelector('.btn-close');
            if (closeButton) {
                closeButton.dispatchEvent(closeEvent);
            }
        }, 5000);
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const avatarImages = document.querySelectorAll('img[alt="Аватар користувача"]');
    avatarImages.forEach(img => {
        img.addEventListener('copy', e => e.preventDefault());
        img.addEventListener('cut', e => e.preventDefault());
        img.addEventListener('dragstart', e => e.preventDefault());
        img.addEventListener('selectstart', e => e.preventDefault());
        img.style.userSelect = 'none';
        img.style.webkitUserSelect = 'none';
        img.style.msUserSelect = 'none';
    });
});

showPasswordCheckbox.addEventListener('change', function () {
    if (this.checked) {
        newPasswordField.type = 'text';
    } else {
        newPasswordField.type = 'password';
    }
});

editBtn.addEventListener('click', () => {
    inputs.forEach(input => {
        if (!input.closest('#deleteAccountModal')) {
            input.disabled = false;
            input.dataset.initialValue = input.value;
        }
    });
    showPasswordCheckbox.disabled = false;
});

inputs.forEach(input => {
    input.addEventListener('input', () => {
        if (!input.closest('#deleteAccountModal')) {
            const hasChanges = Array.from(inputs).some(field => {
                return !field.closest('#deleteAccountModal') &&
                    field.dataset.initialValue !== undefined &&
                    field.value !== field.dataset.initialValue;
            });
            updateBtn.disabled = !hasChanges;
        }
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
}

function blockPaste(e) {
    e.preventDefault();
    pasteWarning.classList.remove('d-none');
    setTimeout(() => {
        pasteWarning.classList.add('d-none');
    }, 3000);
}

aboutField.addEventListener('paste', blockPaste);
aboutField.addEventListener('contextmenu', (e) => {
    e.preventDefault();
    return false;
});