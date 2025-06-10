$().ready(function () {
    const sessions = window.sessions || [];
    const schedules = window.schedules || [];
    const courses = window.courses || [];
    const timeslots = window.timeslots || [];

    // Build a lookup from schedule ID to its sessions
    const sessionsBySched = sessions.reduce((map, sess) => {
        if (!map[sess.schedId]) map[sess.schedId] = [];
        map[sess.schedId].push(sess);
        return map;
    }, {});

    // Build a lookup from schedule ID to its description (and course if needed)
    const schedMap = schedules.reduce((map, sched) => {
        map[sched.schedId] = sched;
        return map;
    }, {});

    const courseMap = courses.reduce((map, course) => {
        map[course.crsId] = course.crsTitle;
        return map;
    }, {});

    function updateCalendar() {
        // Remove existing session blocks
        const $calendar = $('.calendar-grid');
        $calendar.find('.session-item').remove();

        // For each checked schedule, draw its sessions
        $('input[name="SchedIds"]').each(function () {
            const schedId = parseInt(this.value, 10);
            const sessList = sessionsBySched[schedId] || [];
            const sched = schedMap[schedId];
            const courseTitle = courseMap[sched.crsId];

            sessList.forEach(sess => {
                const colStart = Math.ceil(sess.startSlotId / 11);
                const slotIndex = (sess.startSlotId - 1) % 11;
                const rowStart = slotIndex + 2;
                const duration = sess.endSlotId - sess.startSlotId + 1;
                const rowEnd = rowStart + duration;

                // Create the session block
                const $block = $('<a>')
                    .addClass('session-item block rounded-lg p-4 bg-blue-600 text-white')
                    .css({
                        'grid-column-start': colStart,
                        'grid-row-start':    rowStart,
                        'grid-row-end':      rowEnd
                    });

                const start = formatTime(timeslots.find(s => s.slotId === sess.startSlotId).slotTimeStart, false);
                const end = formatTime(timeslots.find(s => s.slotId === sess.endSlotId).slotTimeEnd, true);

                $block.append($('<p>').addClass('text-sm leading-tight font-medium').text(courseTitle));
                $block.append($('<p>').addClass('mt-1 text-xs text-gray-100').text(`${start}-${end}`));

                // Append to calendar
                $calendar.append($block);
            });
        });

        function formatTime(timeSpanStr, hour12toggle) {
            const parts = timeSpanStr.split(':');
            const hours = parseInt(parts[0], 10);
            const minutes = parts.length > 1 ? parseInt(parts[1], 10) : 0;

            const d = new Date();
            d.setHours(hours, minutes, 0, 0);

            const options = { hour: 'numeric', minute: 'numeric', hour12: true };
            let string = new Date(d).toLocaleTimeString('en-US', options).replace(':00', '');
            if (hour12toggle)
                return string;
            else
                return string.replace(' AM', '').replace(' PM', '');
        }
    }
    
    $('input[name="SchedIds"]').on('change', updateCalendar);
    updateCalendar();

    const $form = $('#chooseSchedulesForm');
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
