$().ready(function () {
    const baseUnits = window._baseUnits || 0;
    const geeUnits = window._geeUnits || 0;
    const pelecUnits = window._pelecUnits || 0;
    
    const $geeInput   = $('#geeInput');
    const $pelecInput = $('#pelecInput');
    const $totalSpan  = $('#totalSpan');
    
    function recalcTotalRequirement() {
        let geeVal = parseInt($geeInput.val());
        if (isNaN(geeVal) || geeVal < 0) {
            geeVal = geeUnits;
        }

        // Parse PELEC override; if empty or invalid, fall back to 0
        let pelecVal = parseInt($pelecInput.val());
        if (isNaN(pelecVal) || pelecVal < 0) {
            pelecVal = pelecUnits;
        }

        // Sum everything up
        const total = baseUnits + geeVal + pelecVal;
        $totalSpan.text(total + ' units');
    }

    $geeInput.on('input change', recalcTotalRequirement);
    $pelecInput.on('input change', recalcTotalRequirement);
    recalcTotalRequirement();

    const $form = $('#editCurriculumForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Curriculum submitted successfully",
                        didClose: () => {
                            window.location.href = document.referrer;
                        }
                    });
                }
            })
            .fail(function(xhr) {
                $('#formError').show().text(xhr.responseJSON?.message);
            });
    });
});