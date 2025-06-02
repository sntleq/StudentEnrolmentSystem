$().ready(function () {
    const $form = $('#deleteFacultyForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Faculty deleted successfully",
                        didClose: () => {
                            window.location.href = '/Admin/Faculty';
                        }
                    });
                }
            })
            .fail(function(xhr) {
                $('#formError').show().text(xhr.responseJSON?.message);
            });
    });
});