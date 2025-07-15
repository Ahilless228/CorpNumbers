$(function () {
    $('#showIncompleteOperationsModal').on('click', function () {
        $.get('/Operations/GetIncompleteOperations', function (html) {
            // Если модальное окно уже есть, удаляем чтобы не было дубликатов
            $('#incompleteOperationsModal').remove();

            // Добавляем модальное окно в body
            $('body').append(html);

            // Показываем модальное окно
            const modal = new bootstrap.Modal(document.getElementById('incompleteOperationsModal'));
            modal.show();

            // После вставки HTML — вешаем обработчики
            bindModalHandlers();

            // Применяем фильтр по умолчанию — "Сегодня"
            $('#filterToday').prop('checked', true).trigger('change');
        });
    });

    function bindModalHandlers() {
        // Выделение всех
        $('#selectAllCheckbox').off('change').on('change', function () {
            const checked = this.checked;
            $('.row-checkbox').prop('checked', checked);
            updateSelectedCount();
        });

        // Подсчет выбранных
        $(document).off('change', '.row-checkbox').on('change', '.row-checkbox', updateSelectedCount);

        function updateSelectedCount() {
            const count = $('.row-checkbox:checked').length;
            $('#selectedCount').text(count);
        }

        // Переключение фильтров
        $('input[name="filter"]').off('change').on('change', function () {
            const filterId = $(this).attr('id');
            applyFilter(filterId);
        });

        // Фильтрация строк
        function applyFilter(filterId) {
            const rows = $('#incompleteOpsTable tbody tr');
            const today = new Date().toISOString().split('T')[0];
            const yesterday = new Date(Date.now() - 86400000).toISOString().split('T')[0];

            rows.each(function () {
                const dateStr = $(this).find('td:nth-child(2)').text(); // Дата заявки в формате "dd.MM.yyyy"
                if (!dateStr) {
                    $(this).hide();
                    return;
                }

                const date = dateStr.split('.').reverse().join('-'); // Преобразуем в "yyyy-MM-dd"

                const isToday = date === today;
                const isYesterday = date === yesterday;

                if (filterId === 'filterToday') {
                    $(this).toggle(isToday);
                } else if (filterId === 'filterYesterday') {
                    $(this).toggle(isYesterday);
                } else if (filterId === 'filterDeferred') {
                    // Пока логики нет, прячем все или показываем все - реши сам
                    $(this).hide();
                } else if (filterId === 'filterAll') {
                    $(this).show();
                } else {
                    $(this).show();
                }
            });
        }
    }
});
