const postsPerPage = 9;
let currentPage = 0;
let currentLayout = 'grid';
let allPosts = [];

document.addEventListener('DOMContentLoaded', function () {
    allPosts = document.querySelectorAll('.post-item');
    loadMorePosts();
});

function loadMorePosts() {
    const totalPosts = allPosts.length;
    const startIndex = currentPage * postsPerPage;
    const endIndex = Math.min(startIndex + postsPerPage, totalPosts);

    for (let i = startIndex; i < endIndex; i++) {
        allPosts[i].style.display = 'block';
    }

    currentPage++;

    if (endIndex >= totalPosts) {
        document.getElementById('load-more-button').disabled = true;
        document.getElementById('load-more-button').textContent = 'Всі пости завантажено';
    }

    updateLayout();
}

function setLayout(view) {
    currentLayout = view;
    updateLayout();
}

function updateLayout() {
    const visibleCards = document.querySelectorAll('.post-item[style="display: block;"]');

    visibleCards.forEach(card => {
        const cardInner = card.querySelector('.card');

        if (currentLayout === 'list') {
            card.classList.remove('col-md-6', 'col-lg-4');
            card.classList.add('col-12');
            cardInner.classList.add('card-horizontal');
        } else {
            card.classList.remove('col-12');
            card.classList.add('col-md-6', 'col-lg-4');
            cardInner.classList.remove('card-horizontal');
        }
    });
}