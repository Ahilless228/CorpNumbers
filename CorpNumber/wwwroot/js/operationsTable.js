$(function () {
    // Функция загрузки таблицы и пагинации с фильтрами и страницей
    function loadTable(page = 1) {
        const searchNumber = $('#searchNumber').val();
        const dateFrom = $('#dateFrom').val();
        const dateTo = $('#dateTo').val();

        $.ajax({
            url: '/Operations/GetFilteredOperations',
            type: 'GET',
            data: {
                searchNumber,
                dateFrom,
                dateTo,
                page
            },
            success: function (data) {
                $('#operationTable').html(data);

                // Обновим пагинацию отдельно, т.к. она в _OperationsTable
                updatePagination(page);
            }
        });
    }

    // Обновление пагинации под текущие параметры
    function updatePagination(currentPage) {
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        if (isNaN(totalPages) || totalPages < 1) {
            $('#paginationBlock').html('');
            return;
        }

        let paginationHtml = '';

        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="firstPage" ${currentPage === 1 ? 'disabled' : ''}>⏮ В начало</button>`;
        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="prevPage" ${currentPage === 1 ? 'disabled' : ''}>← Назад</button>`;


        // Контейнер с горизонтальной прокруткой
        paginationHtml += '<div class="pagination-scroll d-inline-block" style="max-width: 600px; overflow-x: auto; white-space: nowrap; vertical-align: middle;">';

        // Отображаем максимум 10 страниц вокруг текущей
        const maxPagesToShow = 10;
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        // Если на конце диапазона меньше страниц, сдвинем начало
        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `<button class="btn page-btn btn-sm ${i === currentPage ? 'btn-primary text-white' : 'btn-outline-secondary'}" data-page="${i}" style="margin-right:3px;">${i}</button>`;
        }
        paginationHtml += '</div>';

        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="nextPage" ${currentPage === totalPages ? 'disabled' : ''}>Вперёд →</button>`;
        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="lastPage" ${currentPage === totalPages ? 'disabled' : ''}>В конец ⏭</button>`;


        $('#paginationBlock').html(paginationHtml);

        // Показываем текущий диапазон строк
        const startItem = parseInt($('#paginationBlock').data('start-item'));
        const endItem = parseInt($('#paginationBlock').data('end-item'));
        const totalItems = parseInt($('#paginationBlock').data('total-items'));

        $('#showingRange').text(`Показано: ${startItem}-${endItem} / ${totalItems}`);
    }

    // Обработчики событий
    $('#searchNumber, #dateFrom, #dateTo').on('input change', function () {
        loadTable(1);
    });

    // Клик по кнопкам страниц
    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        loadTable(page);
    });

    // Кнопки Вперёд и Назад
    $(document).on('click', '#prevPage', function () {
        let currentPage = $('.page-btn.btn-primary').data('page');
        if (currentPage > 1) {
            loadTable(currentPage - 1);
        }
    });
    $(document).on('click', '#nextPage', function () {
        let currentPage = $('.page-btn.btn-primary').data('page');
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        if (currentPage < totalPages) {
            loadTable(currentPage + 1);
        }
    });
    // В начало
    $(document).on('click', '#firstPage', function () {
        loadTable(1);
    });

    // В конец
    $(document).on('click', '#lastPage', function () {
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        loadTable(totalPages);
    });


    // Кнопка сброса фильтров (добавим динамически)
    if ($('#resetFilters').length === 0) {
        $('.mb-3.d-flex').append('<button id="resetFilters" class="btn btn-secondary">Сбросить фильтры</button>');
    }
    $('#resetFilters').on('click', function () {
        $('#searchNumber').val('');
        $('#dateFrom').val('');
        $('#dateTo').val('');
        loadTable(1);
    });

    // Инициализация загрузки
    loadTable(1);
});
