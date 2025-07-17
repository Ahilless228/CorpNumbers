let selectedPhoneId = null;

$(document).on("click", ".btn-issue-phone", function () {
    const row = $("tr.selected-row");
    selectedPhoneId = row.data("id");
    const today = new Date().toISOString().slice(0, 10);
    $("#RequestDate").val(today);
    $("#OperDate").val(today);


    if (!selectedPhoneId) {
        alert("Выберите номер для выдачи.");
        return;
    }

    $.get('/Admin/GetPhoneInfo', { codePhone: selectedPhoneId }, function (model) {
        $("#issueNumberModal #CodePhone").val(model.codePhone);
        $("#issueNumberModal #PhoneNumber").val(model.number);
        $("#issueNumberModal #RequestDate").val(new Date().toISOString().slice(0, 10));
        $("#issueNumberModal #OperDate").val(new Date().toISOString().slice(0, 10));

        // Отобразим прежнего владельца (если он CodeCategory == 4)
        $.get('/Operations/GetOwnerCategory', { codePhone: selectedPhoneId }, function (owner) {
            $("#OldOwner").val(owner.categoryName ?? "Не найдено");
        });

        // Заполним список сотрудников (CodeCategory 1 или 6)
        $.get('/Operations/GetAvailableEmployees', function (list) {
            const select = $("#NewOwnerSelect");
            select.empty().append('<option value="">-- Выберите --</option>');
            list.forEach(emp => {
                select.append(`<option value="${emp.codeOwner}">${emp.fullName} (${emp.nameCh})</option>`);
            });
        });

        $("#issueNumberModal").modal("show");
    });
});

$("#NewOwnerSelect").on("change", function () {
    const ownerId = $(this).val();
    if (!ownerId) return;

    $.get('/Operations/GetEmployeeDetailsModal', { ownerId: ownerId }, function (data) {
        $("#TabNum").val(data.tabNum ?? "—");
        $("#Department").val(data.department ?? "—");
        $("#Post").val(data.post ?? "—");
    });
});

$("#EmploymentCheckbox").on("change", function () {
    const reason = "В связи с приемом на работу";
    const comments = $("#Comments");
    if (this.checked && !comments.val().includes(reason)) {
        comments.val((comments.val() + "\n" + reason).trim());
    } else {
        comments.val(comments.val().replace(reason, '').trim());
    }
});

$("#saveIssueBtn").on("click", function () {
    const formData = {
        Number: selectedPhoneId,
        Owner_new: $("#NewOwnerSelect").val(),
        RequestDate: $("#RequestDate").val(),
        OperDate: $("#OperDate").val(),
        Comments: $("#Comments").val().trim(),
        CodeOperType: 3, // Код "выдача"
        Complete: false
    };

    if (!formData.Owner_new || formData.Comments === "") {
        alert("Укажите нового владельца и комментарий.");
        return;
    }

    $.post("/Admin/CreateIssueOperation", formData, function () {
        $("#issuePhoneModal").modal("hide");
    }).fail(function () {
        alert("Ошибка при сохранении операции.");
    });
});
