$().ready(function () {
    
    // sessions generation
    const timeslots = window.timeslots || [];
    
    const $checkboxes = $('.slot-checkbox');
    const $sessionsContainer = $('#sessionsContainer');
    
    function rebuildSessions() {
        const checkedIds = $checkboxes
            .filter(':checked')
            .map(function () { return parseInt(this.value, 10); })
            .get();
        
        const total = checkedIds.length;

        const selected = timeslots
            .filter(s => checkedIds.includes(s.slotId))
            .sort((a, b) => {
                if (a.slotDay < b.slotDay) return -1;
                if (a.slotDay > b.slotDay) return 1;
                return a.slotId - b.slotId;
            });

        const sessions = [];
        let current = [];
        
        selected.forEach(slot => {
            if (
                current.length > 0 &&
                slot.slotDay === current[0].slotDay &&
                slot.slotId === current[current.length - 1].slotId + 1
            ) {
                current.push(slot);
            } else {
                if (current.length) sessions.push(current);
                current = [slot];
            }
        });
        if (current.length) sessions.push(current);
        
        $sessionsContainer.empty();
        if (!sessions.length) {
            $sessionsContainer.append(
                `<span class="text-sm text-gray-500"> No sessions created </span>`
            );
            return;
        }

        const dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        sessions.forEach((group) => {
            const day = dayNames[group[0].slotDay - 1];
            const start = formatTime(timeslots.find(s => s.slotId === group[0].slotId).slotTimeStart, false);
            const end = formatTime(timeslots.find(s => s.slotId === group[group.length - 1].slotId).slotTimeEnd, true);
            $sessionsContainer.append(
                `<input
                    type="text" value="${day} ${start}-${end}" disabled
                    class="mt-1 w-full rounded border-gray-300 bg-gray-100 sm:text-sm"
                />`
            );
        });

        const totalLabel = total === 1 ? '1 hour' : `${total} hours`;

        $sessionsContainer.append(
            `<div class="mt-1 flex items-center justify-between">
                <span class="text-sm text-gray-500"> Total: </span>
                <span class="text-sm text-gray-500"> ${totalLabel} </span>
            </div>`
        );

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
    
    $checkboxes.on('change', rebuildSessions);
    rebuildSessions();

    // teacher slot disable
    
    const teacherToSlots = window.teacherToSlots || {};
    const teacherSelect = $('#teacherSelect');
    
    function refreshDisabledSlots() {
        const tchrId = teacherSelect.val();
        const takenSlots = teacherToSlots[tchrId] || [];

        $('.slot-checkbox')
            .prop('disabled', false)
            .next()
                .removeClass('bg-gray-100')
                .removeClass('opacity-50');

        takenSlots.forEach(slotId => {
            const $cb = $(`#slot-${slotId}`);
            $cb.prop('checked', false)
                .prop('disabled', true)
                .next()
                    .addClass('bg-gray-100')
                    .addClass('opacity-50');
        });

        rebuildSessions();
    }
    
    teacherSelect.on('change', refreshDisabledSlots);
    refreshDisabledSlots();

    // form submission
    
    const $form = $('#addScheduleForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Schedule added successfully",
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
