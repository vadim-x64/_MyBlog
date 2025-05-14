$(document).ready(function() {
    // Початкове значення лічильників
    $('#contentCounter').text($('#PostInput_Content').val().length);

    // Встановлюємо обробники подій для оновлення лічильників
    $('#PostInput_Content').on('input', function() {
        $('#contentCounter').text($(this).val().length);
    });

    // Переключення між секціями фото відповідно до вибраного типу
    $('input[name="PostInput.UseLocalPhoto"]').change(function() {
        if ($(this).val() === 'true') {
            $('.local-photo-section').show();
            $('.remote-photo-section').hide();
            $('#imagePreview').hide();
        } else {
            $('.local-photo-section').hide();
            $('.remote-photo-section').show();
            loadImagePreview();
        }
    });

    // Функція для завантаження превью зображення
    function loadImagePreview() {
        var url = $('#remotePhotoUrl').val();
        if (url) {
            $('#imagePreview').attr('src', url).show();
        }
    }

    // Обробник зміни URL зображення
    $('#remotePhotoUrl').on('input', function() {
        var url = $(this).val();
        if (url) {
            $('#imagePreview').attr('src', url).show();
        } else {
            $('#imagePreview').hide();
        }
    });

    // Додаємо клієнтську валідацію для файлів, але без alert
    $('#Photo').change(function() {
        var fileInput = this;
        if (fileInput.files && fileInput.files[0]) {
            var fileName = fileInput.files[0].name;
            var extension = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
            var allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.jfif'];

            if (!allowedExtensions.includes(extension)) {
                // Видаляємо файл з поля вводу без alert
                $(this).val('');
                // Замість alert використовуємо валідаційне повідомлення
                $('span[data-valmsg-for="Photo"]').text('Обраний файл не є зображенням відповідного формату');
            }
            else {
                // Очищаємо валідаційне повідомлення, якщо файл відповідає вимогам
                $('span[data-valmsg-for="Photo"]').text('');
            }
        }
    });

    // Ініціалізація відображення секцій
    $('input[name="PostInput.UseLocalPhoto"]:checked').change();
});