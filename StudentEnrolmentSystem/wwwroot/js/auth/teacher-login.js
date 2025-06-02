$().ready(function () {
    const $form = $('#teacherLoginForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#loginError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Signed in successfully",
                        didClose: () => {
                            window.location.href = '/Teacher';
                        }
                    });
                }
            })
            .fail(function(xhr) {
                $('#loginError').show().text(xhr.responseJSON?.message);
            });
    });
});