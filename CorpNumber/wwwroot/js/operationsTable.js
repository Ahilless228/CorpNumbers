let selectedOperationId = null;

$(function () {
    function loadTable(page = 1) {
        const searchNumber = $('#searchNumber').val();
        const dateFrom = $('#dateFrom').val();
        const dateTo = $('#dateTo').val();
        const orderNumber = $('#orderNumber').val();
        const operationType = $('#operationType').val();

        $.ajax({
            url: '/Operations/GetFilteredOperations',
            type: 'GET',
            data: {
                searchNumber,
                orderNumber,
                operationType,
                dateFrom,
                dateTo,
                page
            },
            success: function (data) {
                $('#operationTable').html(data);
                updatePagination(page);

                selectedOperationId = null; // сбрасываем выбранную строку
                // ❗ Переинициализируем обработчик кликов после подгрузки
                console.log('Данные загружены, подключаем обработчики');
                
            }
        });
    }

    function updatePagination(currentPage) {
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        if (isNaN(totalPages) || totalPages < 1) {
            $('#paginationBlock').html('');
            return;
        }

        let paginationHtml = '';
        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="firstPage" ${currentPage === 1 ? 'disabled' : ''}>⏮ В начало</button>`;
        paginationHtml += `<button class="btn btn-outline-secondary btn-sm" id="prevPage" ${currentPage === 1 ? 'disabled' : ''}>← Назад</button>`;

        paginationHtml += '<div class="pagination-scroll d-inline-block" style="max-width: 600px; overflow-x: auto; white-space: nowrap;">';
        const maxPagesToShow = 10;
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);
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

        const startItem = parseInt($('#paginationBlock').data('start-item'));
        const endItem = parseInt($('#paginationBlock').data('end-item'));
        const totalItems = parseInt($('#paginationBlock').data('total-items'));
        $('#showingRange').text(`Показано: ${startItem}-${endItem} / ${totalItems}`);
    }

    // 🔁 Обновление таблицы при изменении фильтров
    $('#searchNumber, #dateFrom, #dateTo, #orderNumber, #operationType').on('input change', function () {
        loadTable(1);
    });

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        loadTable(page);
    });

    $(document).on('click', '#prevPage', function () {
        let currentPage = $('.page-btn.btn-primary').data('page');
        if (currentPage > 1) loadTable(currentPage - 1);
    });

    $(document).on('click', '#nextPage', function () {
        let currentPage = $('.page-btn.btn-primary').data('page');
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        if (currentPage < totalPages) loadTable(currentPage + 1);
    });

    $(document).on('click', '#firstPage', function () {
        loadTable(1);
    });

    $(document).on('click', '#lastPage', function () {
        const totalPages = parseInt($('#paginationBlock').data('total-pages'));
        loadTable(totalPages);
    });

    $('#resetFilters').on('click', function () {
        $('#searchNumber').val('');
        $('#dateFrom').val('');
        $('#dateTo').val('');
        $('#orderNumber').val('');
        $('#operationType').val('');
        loadTable(1);
    });

  
    // ✅ Одинарный клик — выделение строки
    $(document).on('click', '.post-row', function () {
        $('.post-row').removeClass('table-rowactive');
        $(this).addClass('table-rowactive');
        selectedOperationId = $(this).data('id');
        console.log('Выбрана строка с ID:', selectedOperationId);
    });

    // ✅ Двойной клик — открытие модального окна
    $(document).on('dblclick', '.post-row', function () {
        const operationId = $(this).data('id');
        if (!operationId) return;

        $.get(`/Operations/GetOperationInfo/${operationId}`, function (data) {
            $('#info-number').text(data.phoneNumber || '—');
            $('#info-operator').text(data.operatorName || '—');
            $('#info-account').text(data.account || '—');
            $('#info-requestDate').text(data.requestDate || '—');
            $('#info-operDate').text(data.operDate || '—');
            $('#info-type').text(data.type || '—');
            $('#info-information').text(data.information || '—');
            $('#info-comments').text(data.comments || '—');
            $('#info-oldValue').text(data.oldValue || '—');
            $('#info-newValue').text(data.newValue || '—');
            $('#info-complete').text(data.complete ? '✅' : '❌');
            $('#info-orderN').text(data.orderN || '—');

            const modal = new bootstrap.Modal(document.getElementById('infoModal'));
            modal.show();
        }).fail(function () {
            alert('Ошибка загрузки данных операции.');
        });
    });



    

    // Запуск первой загрузки
    loadTable(1);
});
