$().ready(function () {
    const $form = $('#timeMachineForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Time set successfully",
                        didClose: () => {
                            window.location.href = '/Auth';
                        }
                    });
                }
            })
            .fail(function(xhr) {
                
            });
    });
});