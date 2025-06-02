$().ready(function () {
    const $checkboxes = $('.type-filter');
    const $tableRows = $(".faculty");
    
    function filterFaculty() {
        const checkedCats = $(".type-filter:checked").map(function () {
            return $(this).val();
        }).get();

        $tableRows.each(function () {
            const rowCatId = $(this).attr("data-type-id");

            if (checkedCats.indexOf(rowCatId) !== -1) {
                $(this).removeClass("hidden");
            } else {
                $(this).addClass("hidden");
            }
        });

        const visibleCount = $tableRows.filter(function () {
            return !$(this).hasClass("hidden");
        }).length;
        
        if (visibleCount === 0) {
            $(".no-results-msg").removeClass("hidden");
        } else {
            $(".no-results-msg").addClass("hidden");
        }
    }
    $checkboxes.on('change', filterFaculty);
    filterFaculty();
});