(function () {
    let selectedPhoneId = null;
    let selectedPhoneStatus = null;
    let normalizedStatus = null;

    // При клике на кнопку "▶️ Снятие с паузы"
    $(document).on("click", ".btn-remove-pause", function () {
        const row = $("tr.selected-row");
        selectedPhoneId = row.data("id");
        selectedPhoneStatus = row.data("status");

        if (!selectedPhoneId) {
            alert("Выберите номер для снятия с паузы.");
            return;
        }

        normalizedStatus = (selectedPhoneStatus === null ||
            selectedPhoneStatus === "null" ||
            selectedPhoneStatus === "")
            ? null
            : parseInt(selectedPhoneStatus);

        if (normalizedStatus !== 4 && normalizedStatus !== null) {
            alert("Снять с паузы можно только номера со статусом 'Пауза'.");
            return;
        }

        $.get('/Admin/GetPhoneInfo', { codePhone: selectedPhoneId }, function (model) {
            $("#unpauseModal #CodePhone").val(model.codePhone);
            $("#unpauseModal #Status_old").val(model.status);
            $("#unpauseModal input[type='text']").first().val(model.number);
            $("#unpauseModal input[type='text']").eq(1).val(model.statusText);

            $.get('/Operations/GetStatusOptions', function (statuses) {
                const statusSelect = $("#unpauseModal #Status_new");
                statusSelect.empty();

                statuses.forEach(s => {
                    const selected = s.value === 1 ? 'selected' : '';
                    statusSelect.append(`<option value="${s.value}" ${selected}>${s.text}</option>`);
                });

                $("#unpauseModal").modal("show");
            });
        });
    });

    // Логика чекбокса восстановления
    $("#RestoreCheckbox").on("change", function () {
        const textArea = $("#unpauseModal #Comments");
        const reason = "В связи с восстановлением номера";

        if ($(this).is(":checked")) {
            if (!textArea.val().includes(reason)) {
                textArea.val((textArea.val() + "\n" + reason).trim());
            }
        } else {
            textArea.val(textArea.val().replace(reason, '').trim());
        }
    });

    // Сохранение операции снятия с паузы
    $("#saveUnpauseBtn").on("click", function () {
        const comments = $("#unpauseModal #Comments").val().trim();
        if (comments === "") {
            alert("Не указана причина снятия с паузы в комментариях.");
            return;
        }

        const formData = {
            Number: selectedPhoneId,
            Status_old: $("#unpauseModal #Status_old").val(),
            Status_new: $("#unpauseModal #Status_new").val(),
            RequestDate: $("#unpauseModal #RequestDate").val(),
            OperDate: $("#unpauseModal #OperDate").val(),
            Comments: comments,
            CodeOperType: 2, // Код операции "снятие с паузы"
            Complete: false
        };

        $.post("/Admin/CreateUnpauseOperation", formData, function () {
            $("#unpauseModal").modal("hide");
            // Обновление таблицы при необходимости
        }).fail(function () {
            alert("Ошибка при сохранении операции снятия с паузы.");
        });
    });
})();

