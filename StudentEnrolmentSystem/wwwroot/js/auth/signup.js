$().ready(function () {
    const $form = $('#signUpForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#signUpError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Signed up successfully",
                        didClose: () => {
                            window.location.href = '/';
                        }
                    });
                }
            })
            .fail(function(xhr) {
                $('#signUpError').show().text(xhr.responseJSON?.message);
            });
    });
});