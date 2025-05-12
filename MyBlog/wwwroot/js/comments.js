document.addEventListener('DOMContentLoaded', function() {
    // Обробка натиснення на кнопку "Відповісти"
    document.addEventListener('click', function(event) {
        if (event.target.classList.contains('reply-button')) {
            const commentId = event.target.dataset.commentId;
            const authorName = event.target.dataset.authorName;
            const replyForm = document.getElementById(`reply-form-${commentId}`);

            if (replyForm) {
                // Сховаємо всі форми відповідей
                document.querySelectorAll('.reply-form').forEach(form => {
                    form.classList.add('d-none');
                });

                // Додаємо @username при відповіді
                replyForm.querySelector('textarea').value = `@${authorName} `;

                // Покажемо тільки потрібну форму
                replyForm.classList.remove('d-none');
                replyForm.querySelector('textarea').focus();
            }
        }
    });

    // Обробка кнопки "Скасувати"
    document.addEventListener('click', function(event) {
        if (event.target.classList.contains('cancel-reply')) {
            const commentId = event.target.dataset.commentId;
            const replyForm = document.getElementById(`reply-form-${commentId}`);

            if (replyForm) {
                replyForm.classList.add('d-none');
                replyForm.querySelector('textarea').value = '';
            }
        }
    });

    // Обробка кліків по посиланнях на батьківські коментарі
    document.addEventListener('click', function(event) {
        if (event.target.closest('.reply-indicator a')) {
            event.preventDefault(); // Запобігаємо стандартній поведінці посилання

            const link = event.target.closest('.reply-indicator a');
            const targetId = link.getAttribute('href');
            // Знаходимо коментар за його id
            const targetElement = document.querySelector(targetId);

            if (!targetElement) return;

            // Якщо цільовий елемент знаходиться в прихованому блоці відповідей
            const parentRepliesContainer = targetElement.closest('.replies-container, .nested-replies');
            if (parentRepliesContainer && parentRepliesContainer.classList.contains('d-none')) {
                // Показуємо контейнер з відповідями
                parentRepliesContainer.classList.remove('d-none');

                // Оновлюємо текст кнопки
                const commentId = parentRepliesContainer.id.replace('replies-', '');
                const toggleButton = document.querySelector(`.toggle-replies-btn[data-comment-id="${commentId}"]`);
                if (toggleButton) {
                    toggleButton.querySelector('.show-text').classList.add('d-none');
                    toggleButton.querySelector('.hide-text').classList.remove('d-none');
                }
            }

            // Видаляємо всі попередні підсвічування
            document.querySelectorAll('.highlight-comment').forEach(el => {
                el.classList.remove('highlight-comment');
            });

            // Знаходимо саме тіло коментаря для підсвічування (карточку)
            const commentCard = targetElement.querySelector('.card');
            if (commentCard) {
                // Прокручуємо до коментаря
                commentCard.scrollIntoView({ behavior: 'smooth', block: 'center' });

                // Додаємо тимчасове підсвічування тільки до карточки коментаря
                commentCard.classList.add('highlight-comment');

                // Видаляємо клас через 3 секунди
                setTimeout(() => {
                    commentCard.classList.remove('highlight-comment');
                }, 3000);
            }
        }
    });

    // Обробка кнопок показати/приховати відповіді
    document.addEventListener('click', function(event) {
        if (event.target.closest('.toggle-replies-btn')) {
            const button = event.target.closest('.toggle-replies-btn');
            const commentId = button.dataset.commentId;
            const repliesContainer = document.getElementById(`replies-${commentId}`);

            if (repliesContainer) {
                // Перемикаємо видимість контейнера з відповідями
                repliesContainer.classList.toggle('d-none');

                // Перемикаємо текст кнопки
                const showText = button.querySelector('.show-text');
                const hideText = button.querySelector('.hide-text');

                showText.classList.toggle('d-none');
                hideText.classList.toggle('d-none');
            }
        }
    });
});


// Додати в JavaScript файл (paste.txt)
document.addEventListener('DOMContentLoaded', function() {
    // Додаємо обробник для всіх textarea в коментарях
    document.querySelectorAll('textarea[name="replyContent"], textarea[name="CommentContent"]').forEach(function(textarea) {
        textarea.addEventListener('input', function() {
            if (this.value.length > 2000) {
                this.value = this.value.substring(0, 2000);
            }
        });
    });
});



document.addEventListener('DOMContentLoaded', function() {
    // Якщо в URL є параметр scrollToComment, прокручуємо до цього коментаря
    const urlParams = new URLSearchParams(window.location.search);
    const scrollToComment = urlParams.get('scrollToComment');

    if (scrollToComment) {
        const commentElement = document.getElementById(`comment-${scrollToComment}`);
        if (commentElement) {
            setTimeout(() => {
                commentElement.scrollIntoView({ behavior: 'smooth', block: 'center' });

                // Підсвічуємо коментар
                const commentCard = commentElement.querySelector('.card');
                if (commentCard) {
                    commentCard.classList.add('highlight-comment');
                    setTimeout(() => {
                        commentCard.classList.remove('highlight-comment');
                    }, 3000);
                }
            }, 500); // Невелика затримка для впевненості, що сторінка повністю завантажилась
        }
    }
});