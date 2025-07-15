$(function () {
    $('#showIncompleteOperationsModal').on('click', function () {
        $.ajax({
            url: '/Operations/GetIncompleteOperations',
            method: 'GET',
            success: function (data) {
                $('#incompleteOperationsTable tbody').html(data);
                const modal = new bootstrap.Modal(document.getElementById('incompleteOperationsModal'));
                modal.show();
            },
            error: function () {
                alert('Ошибка при загрузке данных.');
            }
        });
    });
});

$(function () {
    // Выделить все
    $('#selectAllCheckbox').on('change', function () {
        const checked = this.checked;
        $('.row-checkbox').prop('checked', checked);
        updateSelectedCount();
    });

    // Подсчет выбранных
    $(document).on('change', '.row-checkbox', updateSelectedCount);

    function updateSelectedCount() {
        const count = $('.row-checkbox:checked').length;
        $('#selectedCount').text(count);
    }
});

