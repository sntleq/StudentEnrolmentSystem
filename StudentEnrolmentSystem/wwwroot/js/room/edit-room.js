$().ready(function () {
    const $form = $('#editRoomForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Room updated successfully",
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