// TasksScript.js
$(function () {
    let selectedRow = null;
    let selectedId = null;

    // Выбор строки
    $(document).on('click', 'tbody tr', function () {
        $('tbody tr').removeClass('table-active');
        $(this).addClass('table-active');
        selectedRow = $(this);
        selectedId = $(this).data('id');
    });

    // Добавление
    $(document).on('click', '#btnAddTask', function () {
        clearModal();
        $('#taskModalLabel').text('Новая задача/напоминание');
        $('#taskModal').modal('show');
    });

    // Редактирование
    $(document).on('click', '#btnEditTask', function () {
        if (!selectedId) return alert('Выберите задачу.');

        $.get(`/Tasks/GetTask/${selectedId}`, function (data) {
            $('#CodeTask').val(data.codeTask);
            $('#CreateDate').val(data.createDate?.split('T')[0]);
            $('#TaskDate').val(data.taskDate?.split('T')[0]);
            $('#TaskText').val(data.taskText);
            $('#taskModalLabel').text('Редактирование задачи');
            $('#taskModal').modal('show');
        });
    });

    // Сохранить
    $(document).on('click', '#saveTaskBtn', function () {
        const task = {
            CodeTask: $('#CodeTask').val(),
            CreateDate: $('#CreateDate').val(),
            TaskDate: $('#TaskDate').val(),
            TaskText: $('#TaskText').val()
        };

        const url = task.CodeTask ? '/Tasks/Update' : '/Tasks/Create';

        $.post(url, task, function () {
            $('#taskModal').modal('hide');
            refreshTable();
        });
    });

    // Удаление
    $(document).on('click', '#btnDeleteTask', function () {
        if (!selectedId) return alert('Выберите задачу.');
        if (!confirm('Удалить задачу?')) return;

        $.post(`/Tasks/Delete/${selectedId}`, function () {
            refreshTable();
        });
    });

    // Переключение выполнения
    $(document).on('click', '#btnToggleComplete', function () {
        if (!selectedId) return alert('Выберите задачу.');
        $.post(`/Tasks/ToggleComplete/${selectedId}`, function () {
            refreshTable();
        });
    });

    function clearModal() {
        const today = new Date().toISOString().split('T')[0];
        $('#CodeTask').val('');
        $('#CreateDate').val(today);
        $('#TaskDate').val(today);
        $('#TaskText').val('');
    }

    function refreshTable() {
        $.get('/Tasks/TasksIndexPartial', function (html) {
            $('#taskTableContainer').html(html);
            selectedId = null;
        });
    }
});
