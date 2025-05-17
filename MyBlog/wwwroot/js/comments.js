document.addEventListener('DOMContentLoaded', function() {
    function handleFormSubmit(form, textarea) {
        const textareaValue = textarea.value.trim();
        if (textareaValue === '') {
            return false;
        }
        return true;
    }

    document.querySelectorAll('textarea[name="replyContent"], textarea[name="CommentContent"]').forEach(function(textarea) {
        const form = textarea.closest('form');
        if (form) {
            form.addEventListener('submit', function(event) {
                if (!handleFormSubmit(form, textarea)) {
                    event.preventDefault();
                    textarea.classList.add('is-invalid');
                    let errorSpan = form.querySelector('.invalid-feedback');
                    if (!errorSpan) {
                        errorSpan = document.createElement('span');
                        errorSpan.classList.add('invalid-feedback', 'd-block');
                        textarea.parentNode.insertBefore(errorSpan, textarea.nextSibling);
                    }
                    errorSpan.textContent = 'Коментар не може бути порожнім.';
                } else {
                    textarea.classList.remove('is-invalid');
                    let errorSpan = form.querySelector('.invalid-feedback');
                    if (errorSpan) {
                        errorSpan.remove();
                    }
                }
            });
        }

        textarea.addEventListener('input', function() {
            if (this.value.length > 2000) {
                this.value = this.value.substring(0, 2000);
            }
            if (this.value.trim() !== '') {
                this.classList.remove('is-invalid');
                let errorSpan = this.closest('form').querySelector('.invalid-feedback');
                if (errorSpan) {
                    errorSpan.textContent = '';
                }
            }
        });

        textarea.addEventListener('keydown', function(event) {
            if (event.key === 'Enter' && !event.shiftKey) {
                event.preventDefault();
                const currentForm = this.closest('form');
                if (currentForm && handleFormSubmit(currentForm, this)) {
                    currentForm.submit();
                } else if (currentForm) {
                    this.classList.add('is-invalid');
                    let errorSpan = currentForm.querySelector('.invalid-feedback');
                    if (!errorSpan) {
                        errorSpan = document.createElement('span');
                        errorSpan.classList.add('invalid-feedback', 'd-block');
                        this.parentNode.insertBefore(errorSpan, this.nextSibling);
                    }
                    errorSpan.textContent = 'Коментар не може бути порожнім.';
                }
            }
        });
    });

    document.addEventListener('click', function(event) {
        const replyButton = event.target.closest('.reply-button');
        if (replyButton) {
            const commentId = replyButton.dataset.commentId;
            const authorName = replyButton.dataset.authorName;
            const replyForm = document.getElementById(`reply-form-${commentId}`);

            if (replyForm) {
                document.querySelectorAll('.reply-form').forEach(form => {
                    if (form.id !== `reply-form-${commentId}`) {
                        form.classList.add('d-none');
                        form.querySelector('textarea').value = '';
                    }
                });

                replyForm.classList.toggle('d-none');
                const textarea = replyForm.querySelector('textarea');
                if (!replyForm.classList.contains('d-none')) {
                    textarea.value = `@${authorName} `;
                    textarea.focus();
                    const len = textarea.value.length;
                    textarea.setSelectionRange(len, len);
                } else {
                    textarea.value = '';
                }
            }
        }
    });

    document.addEventListener('click', function(event) {
        const cancelReplyButton = event.target.closest('.cancel-reply');
        if (cancelReplyButton) {
            const commentId = cancelReplyButton.dataset.commentId;
            const replyForm = document.getElementById(`reply-form-${commentId}`);

            if (replyForm) {
                replyForm.classList.add('d-none');
                replyForm.querySelector('textarea').value = '';
                replyForm.querySelector('textarea').classList.remove('is-invalid');
                let errorSpan = replyForm.querySelector('.invalid-feedback');
                if (errorSpan) {
                    errorSpan.remove();
                }
            }
        }
    });

    function highlightAndScroll(targetElement) {
        if (!targetElement) return;

        const commentCard = targetElement.querySelector('.card') || targetElement.closest('.card');
        if (commentCard) {
            commentCard.scrollIntoView({ behavior: 'smooth', block: 'center' });

            document.querySelectorAll('.highlight-comment').forEach(el => {
                el.classList.remove('highlight-comment');
                el.style.padding = '';
            });

            commentCard.classList.add('highlight-comment');

            setTimeout(() => {
                commentCard.classList.remove('highlight-comment');
                commentCard.style.padding = '';
            }, 3000);
        }
    }

    document.addEventListener('click', function(event) {
        const replyLink = event.target.closest('.reply-indicator a');
        if (replyLink) {
            event.preventDefault();
            const targetId = replyLink.getAttribute('href');
            const targetElement = document.querySelector(targetId);

            if (!targetElement) return;

            let current = targetElement;
            while(current && current.parentElement && current.parentElement !== document.body) {
                if (current.parentElement.classList.contains('replies-container') || current.parentElement.classList.contains('nested-replies')) {
                    if (current.parentElement.classList.contains('d-none')) {
                        current.parentElement.classList.remove('d-none');
                        const parentCommentId = current.parentElement.id.replace('replies-', '');
                        const toggleButton = document.querySelector(`.toggle-replies-btn[data-comment-id="${parentCommentId}"]`);
                        if (toggleButton) {
                            toggleButton.querySelector('.show-text').classList.add('d-none');
                            toggleButton.querySelector('.hide-text').classList.remove('d-none');
                        }
                    }
                }
                current = current.parentElement;
            }
            highlightAndScroll(targetElement);
        }
    });

    document.addEventListener('click', function(event) {
        const toggleButton = event.target.closest('.toggle-replies-btn');
        if (toggleButton) {
            const commentId = toggleButton.dataset.commentId;
            const repliesContainer = document.getElementById(`replies-${commentId}`);

            if (repliesContainer) {
                repliesContainer.classList.toggle('d-none');
                const showText = toggleButton.querySelector('.show-text');
                const hideText = toggleButton.querySelector('.hide-text');
                showText.classList.toggle('d-none');
                hideText.classList.toggle('d-none');
            }
        }
    });

    document.addEventListener('click', function(event) {
        const deleteBtn = event.target.closest('.delete-comment-btn');
        if (deleteBtn) {
            const commentId = deleteBtn.dataset.commentId;
            document.getElementById('commentIdToDelete').value = commentId;
        }
    });

    const urlParams = new URLSearchParams(window.location.search);
    const scrollToCommentId = urlParams.get('scrollToComment');

    if (scrollToCommentId) {
        const commentElement = document.getElementById(`comment-${scrollToCommentId}`);
        if (commentElement) {
            setTimeout(() => {
                let current = commentElement;
                while(current && current.parentElement && current.parentElement !== document.body) {
                    if (current.parentElement.classList.contains('replies-container') || current.parentElement.classList.contains('nested-replies')) {
                        if (current.parentElement.classList.contains('d-none')) {
                            current.parentElement.classList.remove('d-none');
                            const parentCommentId = current.parentElement.id.replace('replies-', '');
                            const toggleButton = document.querySelector(`.toggle-replies-btn[data-comment-id="${parentCommentId}"]`);
                            if (toggleButton) {
                                toggleButton.querySelector('.show-text').classList.add('d-none');
                                toggleButton.querySelector('.hide-text').classList.remove('d-none');
                            }
                        }
                    }
                    current = current.parentElement;
                }
                highlightAndScroll(commentElement);
            }, 300);
        }
    }

    var deleteModal = document.getElementById('deleteCommentModal');
    if(deleteModal) {
        deleteModal.addEventListener('shown.bs.modal', function () {
            var firstFocusableElement = deleteModal.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (firstFocusableElement) {
                // No specific focus needed here, default Bootstrap behavior is fine.
            }
        });
    }
});