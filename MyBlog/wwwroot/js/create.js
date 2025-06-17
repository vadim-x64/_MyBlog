$(document).ready(function() {
    $('#contentCounter').text($('#PostInput_Content').val().length);
    
    $('#PostInput_Content').on('input', function() {
        $('#contentCounter').text($(this).val().length);
    });
    
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
    
    function loadImagePreview() {
        var url = $('#remotePhotoUrl').val();
        if (url) {
            $('#imagePreview').attr('src', url).show();
        }
    }
    
    $('#remotePhotoUrl').on('input', function() {
        var url = $(this).val();
        if (url) {
            $('#imagePreview').attr('src', url).show();
        } else {
            $('#imagePreview').hide();
        }
    });

    $('#Photo').change(function() {
        var fileInput = this;
        if (fileInput.files && fileInput.files[0]) {
            var fileName = fileInput.files[0].name;
            var extension = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
            var allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.jfif'];
            if (!allowedExtensions.includes(extension)) {
                $(this).val('');
                $('span[data-valmsg-for="Photo"]').text('Обраний файл не є зображенням відповідного формату');
            }
            else {
                $('span[data-valmsg-for="Photo"]').text('');
            }
        }
    });
    $('input[name="PostInput.UseLocalPhoto"]:checked').change();
});