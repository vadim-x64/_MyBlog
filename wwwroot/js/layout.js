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
    document.querySelectorAll('.dropdown-menu.custom-dropdown.show').forEach(function(menu) {
        if (menu !== button.nextElementSibling) {
            menu.classList.remove('show');
        }
    });
    const dropdown = button.nextElementSibling;
    dropdown.classList.toggle('show');
    document.addEventListener('click', function closeDropdown(e) {
        if (!button.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            document.removeEventListener('click', closeDropdown);
        }
    });
    event.stopPropagation();
}

document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.dropdown-menu.custom-dropdown.show').forEach(function(menu) {
            menu.classList.remove('show');
        });
    }
});

document.addEventListener('DOMContentLoaded', function() {
    if (!document.getElementById('backToTopBtn')) {
        const backToTopBtn = document.createElement('button');
        backToTopBtn.id = 'backToTopBtn';
        backToTopBtn.className = 'back-to-top-btn';
        backToTopBtn.title = 'Повернутися вгору';
        const img = document.createElement('img');
        img.src = 'URL_ДО_ВАШОЇ_ІКОНКИ';
        img.alt = 'Вгору';
        img.width = 50;
        img.height = 50;
        backToTopBtn.appendChild(img);
        document.body.appendChild(backToTopBtn);
    }

    const backToTopBtn = document.getElementById('backToTopBtn');
    
    function toggleBackToTopButton() {
        if (window.pageYOffset > 300) {
            if (!backToTopBtn.classList.contains('show')) {
                backToTopBtn.classList.add('show');
            }
        } else {
            if (backToTopBtn.classList.contains('show')) {
                backToTopBtn.classList.remove('show');
            }
        }
    }
    
    function scrollToTop() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
    
    window.addEventListener('scroll', toggleBackToTopButton);
    backToTopBtn.addEventListener('click', scrollToTop);
    toggleBackToTopButton();
});