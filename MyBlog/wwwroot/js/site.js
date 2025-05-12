document.addEventListener('contextmenu', function(e) {
    e.preventDefault();
});

document.addEventListener('dragstart', function(e) {
    e.preventDefault();
});

document.addEventListener('drop', function(e) {
    e.preventDefault();
});





function toggleDropdown(button) {
    // Закриваємо всі відкриті меню
    document.querySelectorAll('.dropdown-menu.custom-dropdown.show').forEach(function(menu) {
        if (menu !== button.nextElementSibling) {
            menu.classList.remove('show');
        }
    });

    // Відкриваємо/закриваємо поточне меню
    const dropdown = button.nextElementSibling;
    dropdown.classList.toggle('show');

    // Додаємо обробник для закриття меню при кліку деінде
    document.addEventListener('click', function closeDropdown(e) {
        if (!button.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            document.removeEventListener('click', closeDropdown);
        }
    });

    // Запобігаємо спливанню події кліку
    event.stopPropagation();
}

// Закриваємо відкрите меню при натиснення клавіші Escape
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.dropdown-menu.custom-dropdown.show').forEach(function(menu) {
            menu.classList.remove('show');
        });
    }
});