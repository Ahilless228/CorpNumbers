//файл employeeInfoModal.js
// Обработчик дабл клика и загрузка данных в мод окно с инфо
document.addEventListener('DOMContentLoaded', function () {
    const table = document.getElementById('employeeTable');

    if (!table) return;

    table.addEventListener('dblclick', function (e) {
        const row = e.target.closest('.employee-row');
        if (!row) return;

        const codeEmployee = row.dataset.id;

        fetch(`/Employee/GetEmployeeInfoModal?id=${codeEmployee}`)
            .then(res => res.json())
            .then(data => {
                if (!data || Object.keys(data).length === 0) {
                    alert("Сотрудник не найден.");
                    return;
                }

                // Заполнение данных
                $('#info_empCode').text(data.codeEmployee ?? '—');
                $('#info_empSurname').text(data.surname ?? '—');
                $('#info_empFirstname').text(data.firstname ?? '—');
                $('#info_empMidname').text(data.midname ?? '—');
                $('#info_empNameCh').text(data.nameCh ?? '—');
                $('#info_empTabNum').text(data.tabNum ?? '—');
                $('#info_empInputDate').text(data.inputDate?.substring(0, 10) ?? '—');
                $('#info_empPost').text(data.post ?? '—');
                $('#info_empPartTime').text(data.partTime ?? '—');
                $('#info_empDepartment').text(data.department ?? '—');
                $('#info_empSection').text(data.section ?? '—');
                $('#info_empQuota').text(data.quota ?? '—');
                $('#info_empOrg').text(data.org ?? '—');
                $('#info_empPhoto').attr('src', data.photo ?? '/images/default-profile.jpg');

                $('#info_empContractNumber').text(data.contractNumber ?? '—');
                $('#info_empContractDate').text(data.contractDate?.substring(0, 10) ?? '—');
                $('#info_empFired').prop('checked', data.fired === true);
                $('#info_empHazard').text(data.hazard != null ? data.hazard + '%' : '—');
                $('#info_empHazardDoc').text(data.hazardDocTitle ?? '—');
                $('#info_empFiringDate').text(data.firingDate?.substring(0, 10) ?? '—');

                $('#info_empSex').text(data.sexTitle ?? '—');
                $('#info_empBirthday').text(data.birthday?.substring(0, 10) ?? '—');
                $('#info_empCitizenship').text(data.citizenshipTitle ?? '—');
                $('#info_empPassport').text(data.passport ?? '—');
                $('#info_empNationality').text(data.nationalityTitle ?? '—');
                $('#info_empAddress').text(data.address ?? '—');
                $('#info_empDistrict').text(data.districtTitle ?? '—');

                // Загрузка телефонов
                fetch(`/Employee/GetRegisteredPhones?id=${data.codeEmployee}`)
                    .then(res => res.json())
                    .then(phoneData => {
                        $('#info_empPhoneCount').text(phoneData.count);
                        const tbody = $('#info_empPhoneTableBody');
                        tbody.empty();

                        if (phoneData.count === 0) {
                            tbody.append('<tr><td colspan="5" class="text-center text-muted">Нет зарегистрированных номеров</td></tr>');
                        } else {
                            phoneData.phones.forEach(p => {
                                tbody.append(`<tr>
                                    <td>${p.number}</td>
                                    <td>${p.operatorTitle}</td>
                                    <td>${p.accountType}</td>
                                    <td>${p.issueDate}</td>
                                    <td>
                                        <input type="checkbox" disabled class="custom-checkbox" ${p.corporative ? 'checked' : ''} style="accent-color: #0d6efd; opacity: 1; filter: none;" />
                                    </td>
                                </tr>`);
                            });
                        }
                    });

                // Активировать первую вкладку всегда
                const firstTab = document.querySelector('#employeeTab button[data-bs-target="#contract"]');
                const tab = new bootstrap.Tab(firstTab);
                tab.show();

                // Показать модальное окно
                const modalEl = document.getElementById('employeeInfoModal');
                const modal = new bootstrap.Modal(modalEl);
                modal.show();

                // Инициализация вкладок вручную после показа
                setTimeout(() => {
                    const triggerTabList = document.querySelectorAll('#employeeTab button[data-bs-toggle="tab"]');
                    triggerTabList.forEach(triggerEl => {
                        const tabTrigger = new bootstrap.Tab(triggerEl);

                        // Перехватываем клик, чтобы переключалось правильно
                        triggerEl.addEventListener('click', function (e) {
                            e.preventDefault();
                            tabTrigger.show();
                        });
                    });
                }, 300); // Небольшая задержка, чтобы DOM успел отрендериться

                // Очистка backdrop при закрытии
                modalEl.addEventListener('hidden.bs.modal', function () {
                    document.body.classList.remove('modal-open');
                    const backdrop = document.querySelector('.modal-backdrop');
                    if (backdrop) backdrop.remove();
                });
            });
    });
});
