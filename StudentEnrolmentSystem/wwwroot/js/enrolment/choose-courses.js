$().ready(function () {
    const $checkboxes   = $('input[name="CrsIds"]');
    const $selCount     = $('#selCount');
    const $selUnits     = $('#selUnits');
    const $selHours     = $('#selHours');

    function updateTotals() {
        let count = 0, units = 0, hours = 0;

        $checkboxes.filter(':checked').each(function() {
            count++;
            units += parseFloat(this.dataset.units)  || 0;
            hours += parseFloat(this.dataset.hours)  || 0;
        });

        $selCount.text(count);
        $selUnits.text(units);
        $selHours.text(hours);
    }

    $checkboxes.on('change', updateTotals);
    updateTotals();

    const $form = $('#chooseCoursesForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    window.location.href = res.redirectUrl;
                }
            })
            .fail(function(xhr) {
                $('#formError').show().text(xhr.responseJSON?.message);
            });
    });
});