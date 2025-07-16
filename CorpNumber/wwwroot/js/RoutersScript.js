//RoutersScript.js
$(function () {
    // Обработчик клика по строке таблицы
    $('#routerTableBody').on('click', '.router-row', function () {
        const data = $(this).data();

        // Отображение фото роутера по модели
        const model = data.model || "";
        const modelLower = model.toLowerCase();
        let photoSrc = '/images/default-router.jpg';

        const knownModels = ['e5373s', 'l02h', 'mf910', 'mf920'];
        for (let m of knownModels) {
            if (modelLower.includes(m)) {
                photoSrc = '/images/' + m.toUpperCase() + '.jpg';
                break;
            }
        }

        $('#routerPhoto').attr('src', photoSrc);

        $('#detailModel').text(data.model || '—');
        $('#detailIMEI').text(data.imei || '—');
        $('#detailSerial').text(data.serial || '—');
        $('#detailNumber').text(data.number || '—');
        $('#detailAccount').text(data.account || '—');
        $('#detailICCID').text(data.iccid || '—');
        $('#detailCategory').text(data.category || '—');

        $('.router-row').removeClass('table-primary');
        $(this).addClass('table-primary');
    });
});