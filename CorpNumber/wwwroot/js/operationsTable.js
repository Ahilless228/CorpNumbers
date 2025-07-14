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

        let paginationHtml = '<div class="d-flex align-items-center justify-content-center flex-wrap gap-1">';

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

        paginationHtml += '</div>'; // Закрытие d-flex wrapper

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



    let isModalChanged = false;

    $(document).on('click', '#changeOperationInfo', function () {
        if (!selectedOperationId) {
            alert('Выберите строку для редактирования.');
            return;
        }

        $.get(`/Operations/GetOperationEditData/${selectedOperationId}`, function (data) {
            $('#edit-codeOperation').val(selectedOperationId);
            $('#edit-requestDate').val(data.requestDate || '');
            $('#edit-operDate').val(data.operDate || '');
            $('#edit-information').val(data.information || '');
            $('#edit-comments').val(data.comments || '');
            $('#edit-complete').prop('checked', data.complete);
            $('#edit-orderN').val(data.orderN || '');

            renderValueFields(data.codeOperType, data.oldValue, data.newValue);

            // Загружаем справочники и выставляем значения
            loadDropdownsAndSetDefaults(data.operatorCode, data.accountCode, data.codeOperType).then(() => {
                isModalChanged = false;
                const modal = new bootstrap.Modal(document.getElementById('editOperationModal'));
                modal.show();
            });
        });
    });


    // Слежение за изменениями
    $(document).on('change input', '#editOperationForm input, #editOperationForm select', function () {
        isModalChanged = true;
    });

    // Отмена
    $(document).on('click', '#cancelEdit', function () {
        if (isModalChanged && !confirm("Вы уверены, что хотите отменить изменения?")) return;

        const modal = bootstrap.Modal.getInstance(document.getElementById('editOperationModal'));
        modal.hide();
    });

    // Сохранение
    $(document).on('click', '#saveEdit', function () {
        if (!confirm("Сохранить изменения?")) return;

        // Используем массив, чтобы можно было делать push
        let formData = $('#editOperationForm').serializeArray();

        // Добавляем или обновляем поле Complete
        const isChecked = $('#edit-complete').is(':checked');

        // Удалим старую запись, если она уже есть
        formData = formData.filter(f => f.name !== 'Complete');

        // Добавим правильное значение
        formData.push({ name: 'Complete', value: isChecked });

        // Отправляем сериализованный массив как query string
        $.post('/Operations/UpdateOperation', $.param(formData), function () {
            alert("Операция обновлена.");
            isModalChanged = false;
            bootstrap.Modal.getInstance(document.getElementById('editOperationModal')).hide();
            loadTable(); // обновляем таблицу
        }).fail(function () {
            alert("Ошибка при сохранении изменений.");
        });
    });


    function loadDropdownsAndSetDefaults(selectedOperator, selectedAccount, selectedType) {
        return $.get('/Operations/GetDropdownData').then(function (data) {
            // Очищаем перед заполнением
            $('#edit-type').empty().append('<option value="">Выберите тип</option>');
            $('#edit-account').empty().append('<option value="">Выберите счёт</option>');
            $('#edit-operator').empty().append('<option value="">Выберите оператора</option>');

            data.types.forEach(t => {
                const option = new Option(t.type, t.codeOperType);
                $('#edit-type').append(option);
            });

            data.accounts.forEach(a => {
                const option = new Option(a.type, a.code);
                $('#edit-account').append(option);
            });

            data.operators.forEach(o => {
                const option = new Option(o.title, o.codeOperator);
                $('#edit-operator').append(option);
            });

            // Устанавливаем значения по умолчанию
            if (selectedType) $('#edit-type').val(selectedType);
            if (selectedAccount) $('#edit-account').val(selectedAccount);
            if (selectedOperator) $('#edit-operator').val(selectedOperator);
        });
    }


    function renderValueFields(codeOperType, oldValue, newValue) {
        const oldContainer = $('#edit-oldValue-container');
        const newContainer = $('#edit-newValue-container');

        // Очистка
        oldContainer.empty();
        newContainer.empty();

        const input = (name, value) => `<input type="text" class="form-control" name="${name}" value="${value || ''}" />`;

        const select = (name, value, options) => {
            let html = `<select class="form-select" name="${name}">`;
            options.forEach(opt => {
                const selected = opt.value == value ? 'selected' : '';
                html += `<option value="${opt.value}" ${selected}>${opt.text}</option>`;
            });
            html += `</select>`;
            return html;
        };

        // Логика по типу операции
        switch (codeOperType) {
            case 1:
            case 2: // Статусы
                oldContainer.html(input("Status_old", oldValue));
                newContainer.html(input("Status_new", newValue));
                break;

            case 3: // ICCID
                oldContainer.html(input("ICCID_old", oldValue));
                newContainer.html(input("ICCID_new", newValue));
                break;

            case 4:
            case 5: // Internet Service
                $.get('/Operations/GetInternetServices', function (services) {
                    const options = services.map(s => ({ value: s.codeServ, text: s.service }));
                    oldContainer.html(select("Internet_old", oldValue, options));
                    newContainer.html(select("Internet_new", newValue, options));
                });
                break;

            case 6:
            case 7: // Владельцы
                $.get('/Operations/GetOwnerOptions', function (owners) {
                    const options = owners.map(o => ({ value: o.codeOwner, text: o.display }));
                    oldContainer.html(select("Owner_old", oldValue, options));
                    newContainer.html(select("Owner_new", newValue, options));
                });
                break;

            case 8: // Лимит
                oldContainer.html(input("Limit_old", oldValue));
                newContainer.html(input("Limit_new", newValue));
                break;

            case 9:
            case 10:
            case 11:
            case 12: // Счета
                $.get('/Operations/GetAccountOptions', function (accounts) {
                    const options = accounts.map(a => ({ value: a.code, text: a.type }));
                    oldContainer.html(select("Account_old", oldValue, options));
                    newContainer.html(select("Account_new", newValue, options));
                });
                break;

            case 16: // Тарифы
                $.get('/Operations/GetTariffOptions', function (tariffs) {
                    const options = tariffs.map(t => ({ value: t.codeTariff, text: t.title }));
                    oldContainer.html(select("Tariff_old", oldValue, options));
                    newContainer.html(select("Tariff_new", newValue, options));
                });
                break;

            default:
                oldContainer.html(input("OldValue", oldValue));
                newContainer.html(input("NewValue", newValue));
                break;
        }
    }





});
