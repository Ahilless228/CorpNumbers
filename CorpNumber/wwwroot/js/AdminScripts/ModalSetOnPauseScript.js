
    let selectedPhoneId = null;
    let selectedPhoneStatus = null;

    // При клике на кнопку "⏸️ Установка на паузу"
    $(document).on("click", ".btn-set-pause", function () {
        const row = $("tr.selected-row");
    selectedPhoneId = row.data("id");
    selectedPhoneStatus = row.data("status");

    if (!selectedPhoneId) {
        alert("Выберите номер для установки на паузу.");
    return;
        }

    $.get('/Admin/GetPhoneInfo', {codePhone: selectedPhoneId }, function (model) {
        $("#CodePhone").val(model.codePhone);
    $("#Status_old").val(model.status);
    $("#pauseModal input[type='text']").first().val(model.number);
    $("#pauseModal input[type='text']").eq(1).val(model.statusText);

    $.get('/Operations/GetStatusOptions', function (statuses) {
                const statusSelect = $("#Status_new");
    statusSelect.empty();

                statuses.forEach(s => {
                    const selected = s.value === 4 ? 'selected' : '';
    statusSelect.append(`<option value="${s.value}" ${selected}>${s.text}</option>`);
                });

    $("#pauseModal").modal("show");
            });
        });
    });

    // Логика чекбокса
    $("#LossCheckbox").on("change", function () {
        const textArea = $("#Comments");
    const reason = "В связи с утерей (кражей)";
    if ($(this).is(":checked")) {
            if (!textArea.val().includes(reason)) {
        textArea.val(textArea.val() + "\n" + reason);
            }
        } else {
        textArea.val(textArea.val().replace(reason, '').trim());
        }
    });

    // Сохранение
    $("#savePauseBtn").on("click", function () {
        const comments = $("#Comments").val().trim();
    if (comments === "") {
        alert("Не указана причина установки на паузу в комментариях.");
    return;
        }

    const formData = {
        Number: selectedPhoneId,
        Status_old: $("#Status_old").val(),
        Status_new: $("#Status_new").val(),
        RequestDate: $("#RequestDate").val(),
        OperDate: $("#OperDate").val(),
        Comments: comments,
        CodeOperType: 1, // или другой код типа операции
        Complete: false
        };

    $.post("/Admin/CreatePauseOperation", formData, function () {
        $("#pauseModal").modal("hide");
            // можно обновить таблицу
        }).fail(function () {
        alert("Ошибка при сохранении операции.");
        });
    });

