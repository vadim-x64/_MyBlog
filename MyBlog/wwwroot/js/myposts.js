function toggleDropdown(button) {
    const dropdown = button.nextElementSibling;
    dropdown.classList.toggle('show');

    document.querySelectorAll('.custom-dropdown.show').forEach(menu => {
        if (menu !== dropdown) {
            menu.classList.remove('show');
        }
    });

    document.addEventListener('click', function closeDropdown(e) {
        if (!button.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            document.removeEventListener('click', closeDropdown);
        }
    });

    dropdown.addEventListener('click', function(e) {
        e.stopPropagation();
    });

    checkDropdownPosition(dropdown);
}

function checkDropdownPosition(dropdown) {
    setTimeout(() => {
        if (window.innerWidth <= 768) {
            dropdown.style.right = 'auto';
            dropdown.style.left = '-100px';
        } else {
            const rect = dropdown.getBoundingClientRect();
            const windowWidth = window.innerWidth;

            if (rect.right > windowWidth) {
                dropdown.style.left = 'auto';
                dropdown.style.right = '0';
            }

            if (rect.left < 0) {
                dropdown.style.left = '0';
                dropdown.style.right = 'auto';
            }
        }
    }, 10);
}