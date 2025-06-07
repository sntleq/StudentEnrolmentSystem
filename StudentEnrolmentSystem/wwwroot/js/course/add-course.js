$().ready(function () {
    const $checkboxes = $('input[name="CrsPreqIds"]');
    const $countSpan = $('#prereqCount');
    
    function updatePrereqCount() {
        const selectedCount = $checkboxes.filter(':checked').length;
        $countSpan.text(
            selectedCount + ' course' + (selectedCount !== 1 ? 's' : '') + ' selected'
        );
    }
    $checkboxes.on('change', updatePrereqCount);
    updatePrereqCount();

    const $categorySelect = $('#categorySelect');
    const $prereqItems = $('.prereq-item');

    function filterPrereqsByCategory() {
        const selectedCatId = $categorySelect.val(); // string or ""
        
        $prereqItems.each(function() {
            const $label      = $(this);
            const itemCatId   = $label.attr('data-catg-id'); // always a string
            const $checkbox   = $label.find('input[type="checkbox"]');

            // No category chosen: show everything
            if (!selectedCatId) {
                $label.show();
                return;
            }

            // Categories 1 and 2: General Education (GEC, GEE)
            if (selectedCatId === "1" || selectedCatId === "2") {
                if (itemCatId === "1" || itemCatId === "2") {
                    $label.show();
                } else {
                    if ($checkbox.prop('checked')) {
                        $checkbox.prop('checked', false);
                    }
                    $label.hide();
                }
                return;
            }

            // Categories 3-6: Program-specific courses (CC, PC, AP, PELEC)
            if (["3","4","5","6"].includes(selectedCatId)) {
                if (["3","4","5","6"].includes(itemCatId)) {
                    $label.show();
                } else {
                    if ($checkbox.prop('checked')) {
                        $checkbox.prop('checked', false);
                    }
                    $label.hide();
                }
                return;
            }

            // Category 7: PE
            if (selectedCatId === "7") {
                if (itemCatId === "7") {
                    $label.show();
                } else {
                    if ($checkbox.prop('checked')) {
                        $checkbox.prop('checked', false);
                    }
                    $label.hide();
                }
                return;
            }

            // Category 8: NSTP
            if (selectedCatId === "8") {
                if (itemCatId === "8") {
                    $label.show();
                } else {
                    if ($checkbox.prop('checked')) {
                        $checkbox.prop('checked', false);
                    }
                    $label.hide();
                }
                return;
            }

            // Fallback: hide everything else
            $label.hide();
            if ($checkbox.prop('checked')) {
                $checkbox.prop('checked', false);
            }
        });
        updatePrereqCount();
    }
    function toggleProgramSelect() {
        const selectedCatId = $categorySelect.val(); // string or ""
        const $programSelect = $('#programSelect');
        
        $programSelect.hide();
        if (["3","4","5","6"].includes(selectedCatId)) {
            $programSelect.show();
        }
    }
    $categorySelect.on('change', filterPrereqsByCategory);
    $categorySelect.on('change', toggleProgramSelect);
    filterPrereqsByCategory();
    toggleProgramSelect();
    
    const $form = $('#addCourseForm');
    const action = $form.data('action');

    $form.on('submit', function(e) {
        e.preventDefault();
        $('#formError').hide().text('');

        $.post(action, $form.serialize())
            .done(function(res) {
                if (res.success) {
                    Toast.fire({
                        icon: "success",
                        title: "Course added successfully",
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