$().ready(function () {
    const $checkboxes = $('.prog-filter');
    const $tableRows = $(".room");
    
    function filterRooms() {
        const checkedCats = $(".prog-filter:checked").map(function () {
            return $(this).val();
        }).get();

        $tableRows.each(function () {
            const rowCatId = $(this).attr("data-prog-id");

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
    $checkboxes.on('change', filterRooms);
    filterRooms();
});