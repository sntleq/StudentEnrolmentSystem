$().ready(function () {
    const $checkboxes = $('.catg-filter');
    const $tableRows = $("tbody tr[data-catg-id]");
    
    function filterCourses() {
        const checkedCats = $(".catg-filter:checked").map(function () {
            return $(this).val();
        }).get();

        $tableRows.each(function () {
            const rowCatId = $(this).attr("data-catg-id");

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
            $(".courses-container").addClass("hidden");
            $(".no-results-msg").removeClass("hidden");
        } else {
            $(".courses-container").removeClass("hidden");
            $(".no-results-msg").addClass("hidden");
        }
    }
    $checkboxes.on('change', filterCourses);
    filterCourses();
});